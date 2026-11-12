using Npgsql;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Milvasoft.BulkExtensions.PostgreSql;

/// <summary>
/// Writes boxed values to a binary COPY with the generic, typed Npgsql methods.
/// </summary>
internal static partial class NpgsqlValueWriter
{
    private static readonly MethodInfo _writeSync = typeof(NpgsqlValueWriter).GetMethod(nameof(WriteTyped), BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly MethodInfo _writeAsync = typeof(NpgsqlValueWriter).GetMethod(nameof(WriteTypedAsync), BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly ConcurrentDictionary<Type, Writer> _writers = new();
    private static readonly ConcurrentDictionary<string, string> _dataTypeNames = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the writer for values of <paramref name="type"/>. Callers cache it per column, since a column almost always holds one type.
    /// </summary>
    public static Writer GetWriter(Type type) => _writers.GetOrAdd(type, static t => new Writer(
        t,
        _writeSync.MakeGenericMethod(t).CreateDelegate<Action<NpgsqlBinaryImporter, object, string>>(),
        _writeAsync.MakeGenericMethod(t).CreateDelegate<Func<NpgsqlBinaryImporter, object, string, CancellationToken, Task>>()));

    /// <summary>
    /// Turns a store type of the EF Core model into the data type name Npgsql expects:
    /// type modifiers are removed (<c>character varying(100)</c> becomes <c>character varying</c>, <c>geometry(Point,4326)</c> becomes
    /// <c>geometry</c>) and identifier quotes are dropped.
    /// The binary format of a type does not depend on its modifiers.
    /// </summary>
    public static string GetDataTypeName(string storeType) => _dataTypeNames.GetOrAdd(storeType, static s =>
    {
        // "char" (with quotes) is a distinct internal PostgreSQL type, not character(1).
        if (s.StartsWith("\"char\"", StringComparison.Ordinal))
            return s;

        var name = TypeModifier().Replace(s, string.Empty).Replace("\"", string.Empty);
        var builder = new StringBuilder(name.Length);

        foreach (var part in name.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (builder.Length > 0 && !part.StartsWith('['))
                builder.Append(' ');

            builder.Append(part);
        }

        return builder.ToString();
    });

    private static void WriteTyped<T>(NpgsqlBinaryImporter importer, object value, string dataTypeName) => importer.Write((T)value, dataTypeName);

    private static Task WriteTypedAsync<T>(NpgsqlBinaryImporter importer, object value, string dataTypeName, CancellationToken cancellationToken)
        => importer.WriteAsync((T)value, dataTypeName, cancellationToken);

    [GeneratedRegex(@"\([^()]*\)")]
    private static partial Regex TypeModifier();

    /// <summary>
    /// Writes values of one CLR type, which must not be null.
    /// </summary>
    internal sealed record Writer(Type Type, Action<NpgsqlBinaryImporter, object, string> Sync, Func<NpgsqlBinaryImporter, object, string, CancellationToken, Task> Async)
    {
        public Task WriteAsync(NpgsqlBinaryImporter importer, object value, string dataTypeName, bool async, CancellationToken cancellationToken)
        {
            if (async)
                return Async(importer, value, dataTypeName, cancellationToken);

            Sync(importer, value, dataTypeName);

            return Task.CompletedTask;
        }
    }
}