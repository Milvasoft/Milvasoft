using System.Collections;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Reflection;

namespace Milvasoft.BulkExtensions.Internal;

/// <summary>
/// Reads values from a <see cref="DbDataReader"/> as a given CLR type.
/// </summary>
internal static class TypedReader
{
    private static readonly MethodInfo _readMethod = typeof(TypedReader).GetMethod(nameof(ReadAs), BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly ConcurrentDictionary<Type, Func<DbDataReader, int, object>> _readers = new();

    /// <summary>
    /// Gets a delegate that reads a non null value of <paramref name="type"/>. Callers cache it per column.
    /// </summary>
    public static Func<DbDataReader, int, object> Create(Type type)
        => _readers.GetOrAdd(type, static t => _readMethod.MakeGenericMethod(t).CreateDelegate<Func<DbDataReader, int, object>>());

    private static object ReadAs<T>(DbDataReader reader, int ordinal) => reader.GetFieldValue<T>(ordinal);
}

/// <summary>
/// Compares key value arrays by value. Arrays inside keys (such as <see cref="byte"/> arrays) are compared element by element.
/// </summary>
internal sealed class KeyComparer : IEqualityComparer<object[]>
{
    public static KeyComparer Instance { get; } = new();

    public bool Equals(object[] x, object[] y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null || x.Length != y.Length)
            return false;

        for (int i = 0; i < x.Length; i++)
            if (!ValueEquals(x[i], y[i]))
                return false;

        return true;
    }

    public int GetHashCode(object[] obj)
    {
        var hash = new HashCode();

        foreach (var value in obj)
            hash.Add(value is IStructuralEquatable structural ? structural.GetHashCode(EqualityComparer<object>.Default) : value?.GetHashCode() ?? 0);

        return hash.ToHashCode();
    }

    private static bool ValueEquals(object x, object y)
    {
        if (x is IStructuralEquatable structural)
            return structural.Equals(y, EqualityComparer<object>.Default);

        return Equals(x, y);
    }
}

/// <summary>
/// Changes to entities that are applied only after the operation has succeeded, so a failed or retried operation leaves entities as they were.
/// Changes are applied in the order they were added.
/// </summary>
internal sealed class PendingWrites
{
    private readonly List<PendingWrite> _writes = [];

    public void Add(Action action) => _writes.Add(new PendingWrite(action, null, 0, null, null));

    /// <summary>
    /// Adds a value to write to a row. Stored without a closure, since there is one per returned value.
    /// </summary>
    public void Add(RowSource source, int row, ColumnBinding binding, object value) => _writes.Add(new PendingWrite(null, source, row, binding, value));

    public void Apply()
    {
        foreach (var write in _writes)
        {
            if (write.Action is not null)
                write.Action();
            else
                write.Source.Write(write.Row, write.Binding, write.Value);
        }

        _writes.Clear();
    }

    private readonly record struct PendingWrite(Action Action, RowSource Source, int Row, ColumnBinding Binding, object Value);
}