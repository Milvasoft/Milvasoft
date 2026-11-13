using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Cryptography.Abstract;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvasoft.DataAccess.EfCore.Utils.Converters;

/// <summary>
/// Encrypts string values with <see cref="IMilvaCryptographyProvider"/>. Stored values have the <c>enc::</c> prefix.
/// </summary>
/// <remarks>
/// <para>
/// <b>Encrypted columns are not queryable.</b> Every write uses a random IV, so the same plain text is stored differently each time:
/// equality filters, joins, unique constraints, indexes and sorting on an encrypted column do not work. Only read and write the value.
/// </para>
/// <para> Write: every non null value is encrypted, also values that already start with <c>enc::</c> (so user input is never taken for cipher text). </para>
/// <para>
/// Read: values without the <c>enc::</c> prefix (e.g. written before encryption was enabled) are returned as they are. A value with the prefix that cannot
/// be decrypted (corrupted data or another key) is never returned silently: if an <see cref="IMilvaLogger"/> is given, the failure is logged as an error and
/// the stored value is returned; otherwise a <see cref="CryptographicException"/> is thrown.
/// </para>
/// </remarks>
/// <param name="crypto"></param>
/// <param name="converterMappingHints"></param>
/// <param name="logger"> Logs values that cannot be decrypted. The converter is a part of the EF Core model, which is built once and cached, so pass a long lived (singleton) logger. </param>
public sealed class MilvaEncryptionConverter(IMilvaCryptographyProvider crypto, ConverterMappingHints converterMappingHints = null, IMilvaLogger logger = null)
    : ValueConverter<string, string>(s => EncryptedValue.Protect(s, crypto), s => EncryptedValue.Unprotect(s, crypto, logger), converterMappingHints)
{
}

/// <summary>
/// Encryption format of <see cref="MilvaEncryptionConverter"/> and <see cref="EncryptedJsonValueConverter{T}"/>.
/// </summary>
public static class EncryptedValue
{
    /// <summary>
    /// Prefix of encrypted values.
    /// </summary>
    public const string Prefix = "enc::";

    /// <summary>
    /// Encrypts <paramref name="value"/> and adds <see cref="Prefix"/>. Null stays null. Every other value is encrypted, also values that start with <see cref="Prefix"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="crypto"></param>
    /// <returns></returns>
    public static string Protect(string value, IMilvaCryptographyProvider crypto) => value is null ? null : Prefix + crypto.Encrypt(value);

    /// <summary>
    /// Decrypts a value written by <see cref="Protect(string, IMilvaCryptographyProvider)"/>. Values without <see cref="Prefix"/> are returned as they are.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="crypto"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    /// <exception cref="CryptographicException"> When a value with <see cref="Prefix"/> cannot be decrypted and <paramref name="logger"/> is null. </exception>
    public static string Unprotect(string value, IMilvaCryptographyProvider crypto, IMilvaLogger logger)
    {
        if (value is null || !value.StartsWith(Prefix, StringComparison.Ordinal))
            return value;

        try
        {
            return crypto.Decrypt(value[Prefix.Length..]);
        }
        catch (Exception ex)
        {
            const string message = "A value marked as encrypted ('" + Prefix + "' prefix) could not be decrypted. The data is corrupted or it was encrypted with another key.";

            if (logger is null)
                throw new CryptographicException(message, ex);

            // The value itself is not logged.
            logger.Error(ex, message + " The stored value is returned as it is.");

            return value;
        }
    }
}

/// <summary>
/// Json value comparer for EF Core.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class JsonValueComparer<T> : ValueComparer<T>
{
    private static readonly JsonSerializerOptions _jsonOpts = new(JsonSerializerDefaults.Web) { WriteIndented = false };

    /// <summary>
    /// Json value comparer for EF Core.
    /// </summary>
    public JsonValueComparer() : base((l, r) => JsonSerializer.Serialize(l, _jsonOpts) == JsonSerializer.Serialize(r, _jsonOpts),
                                      v => JsonSerializer.Serialize(v, _jsonOpts).GetHashCode(),
                                      v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, _jsonOpts), _jsonOpts)!)
    { }
}

/// <summary>
/// Encrypted JSON value converter for EF Core.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Encrypted fields follow the rules of <see cref="MilvaEncryptionConverter"/> (not queryable, every value is encrypted on write, decryption failures are logged or thrown).
/// </remarks>
/// <param name="crypto"></param>
/// <param name="logger"> Logs values that cannot be decrypted. If null, a <see cref="CryptographicException"/> is thrown instead. </param>
public sealed class EncryptedJsonValueConverter<T>(IMilvaCryptographyProvider crypto, IMilvaLogger logger = null) : ValueConverter<T, string>(model => ToProvider(model, crypto), json => FromProvider(json, crypto, logger))
{

    private static readonly JsonSerializerOptions _jsonOpts = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    /// <summary>
    /// Cache: per-type property metadata (IsEncrypted + PropertyInfo)
    /// </summary>
    private static readonly ConcurrentDictionary<Type, PropertyMeta[]> _propCache = new();

    private sealed record PropertyMeta(string Name, PropertyInfo Info, bool IsEncrypted, Type PropType);

    private static string ToProvider(T model, IMilvaCryptographyProvider crypto)
    {
        if (model == null)
            return null!;

        var shaped = ShapeForEncryption(model, crypto);
        return JsonSerializer.Serialize(shaped, _jsonOpts);
    }

    private static T FromProvider(string json, IMilvaCryptographyProvider crypto, IMilvaLogger logger)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default!;

        using var doc = JsonDocument.Parse(json);
        var root = DecryptElement(doc.RootElement, typeof(T), crypto, logger);
        return JsonSerializer.Deserialize<T>(root.GetRawText(), _jsonOpts)!;
    }

    private static bool IsSimpleType(Type t)
    {
        // unwrap Nullable<T>
        if (Nullable.GetUnderlyingType(t) is Type u)
            t = u;

        if (t.IsEnum)
            return true;

        return t.IsPrimitive
            || t == typeof(string)
            || t == typeof(decimal)
            || t == typeof(DateTime)
            || t == typeof(DateTimeOffset)
            || t == typeof(TimeSpan)
#if NET6_0_OR_GREATER
            || t == typeof(DateOnly)
            || t == typeof(TimeOnly)
#endif
            || t == typeof(Guid);
    }

    private static PropertyMeta[] GetProps(Type type) => _propCache.GetOrAdd(type, t => [.. t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                                   // Indexers (e.g. Dictionary.Item) can not be read without an index.
                                                                   .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                                                                   .Select(p => new PropertyMeta(p.Name,
                                                                                                 p,
                                                                                                 p.GetCustomAttribute<EncryptedAttribute>() is not null,
                                                                                                 p.PropertyType))]);

    /// <summary>
    /// Model -> shape (object graph) with field-level encryption applied
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="crypto"></param>
    /// <returns></returns>
    private static object ShapeForEncryption(object obj, IMilvaCryptographyProvider crypto)
    {
        if (obj is null)
            return null;

        var type = obj.GetType();

        // primitives & string directly
        if (IsSimpleType(type))
            return obj;

        // Dictionary: keep keys, shape values (they can contain encrypted properties).
        if (obj is IDictionary dictionary)
        {
            var shapedDictionary = new Dictionary<string, object>(dictionary.Count);

            foreach (DictionaryEntry entry in dictionary)
                shapedDictionary[Convert.ToString(entry.Key, System.Globalization.CultureInfo.InvariantCulture)!] = ShapeForEncryption(entry.Value, crypto);

            return shapedDictionary;
        }

        // IEnumerable (but not IDictionary)
        if (obj is IEnumerable enumerable)
        {
            var list = new List<object>();
            foreach (var item in enumerable)
                list.Add(ShapeForEncryption(item, crypto));
            return list;
        }

        // Complex object
        var dict = new Dictionary<string, object>();
        var metas = GetProps(type);

        foreach (var meta in metas)
        {
            var value = meta.Info.GetValue(obj);

            if (value is null)
            {
                dict[meta.Name] = null;
                continue;
            }

            if (meta.IsEncrypted && meta.PropType == typeof(string))
            {
                dict[meta.Name] = EncryptedValue.Protect((string)value, crypto);
            }
            else
            {
                dict[meta.Name] = ShapeForEncryption(value, crypto);
            }
        }

        return dict;
    }

    /// <summary>
    /// JSON -> JSON (decrypt only encrypted string fields, preserve shape)
    /// </summary>
    /// <param name="element"></param>
    /// <param name="targetType"></param>
    /// <param name="crypto"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    private static JsonElement DecryptElement(JsonElement element, Type targetType, IMilvaCryptographyProvider crypto, IMilvaLogger logger)
    {
        if (element.ValueKind == JsonValueKind.Object && GetDictionaryValueType(targetType) is Type dictionaryValueType)
        {
            using var buffer = new MemoryStream();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                writer.WriteStartObject();

                foreach (var jp in element.EnumerateObject())
                {
                    writer.WritePropertyName(jp.Name);

                    if (jp.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                        DecryptElement(jp.Value, dictionaryValueType, crypto, logger).WriteTo(writer);
                    else
                        jp.Value.WriteTo(writer);
                }

                writer.WriteEndObject();
            }

            buffer.Position = 0;
            using var reParsed = JsonDocument.Parse(buffer);
            return reParsed.RootElement.Clone();
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            using var buffer = new MemoryStream();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                writer.WriteStartObject();

                var metas = GetProps(targetType)
                    .ToDictionary(m => m.Name, m => m, StringComparer.Ordinal);

                foreach (var jp in element.EnumerateObject())
                {
                    writer.WritePropertyName(jp.Name);

                    if (!metas.TryGetValue(jp.Name, out var meta))
                    {
                        // not mapped to CLR -> write as-is
                        jp.Value.WriteTo(writer);
                        continue;
                    }

                    if (meta.IsEncrypted && meta.PropType == typeof(string) && jp.Value.ValueKind == JsonValueKind.String)
                    {
                        var str = jp.Value.GetString();
                        var plain = EncryptedValue.Unprotect(str, crypto, logger);
                        writer.WriteStringValue(plain);
                        continue;
                    }

                    if (jp.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        var child = DecryptElement(jp.Value, meta.PropType, crypto, logger);
                        child.WriteTo(writer);
                    }
                    else
                    {
                        jp.Value.WriteTo(writer);
                    }
                }

                writer.WriteEndObject();
            }

            buffer.Position = 0;
            using var reParsed = JsonDocument.Parse(buffer);
            return reParsed.RootElement.Clone();
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            // resolve element type for arrays/lists
            Type elemType =
                targetType.IsArray
                    ? (targetType.GetElementType() ?? typeof(object))
                    : (targetType.IsGenericType
                        ? (targetType.GetGenericArguments().FirstOrDefault() ?? typeof(object))
                        : typeof(object));

            using var buffer = new MemoryStream();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                writer.WriteStartArray();

                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                        DecryptElement(item, elemType, crypto, logger).WriteTo(writer);
                    else
                        item.WriteTo(writer);
                }

                writer.WriteEndArray();
            }

            buffer.Position = 0;
            using var reParsed = JsonDocument.Parse(buffer);
            return reParsed.RootElement.Clone();
        }

        // primitive
        return element.Clone();
    }

    /// <summary>
    /// Returns value type of a dictionary type (IDictionary&lt;TKey, TValue&gt; or IReadOnlyDictionary&lt;TKey, TValue&gt;), otherwise null.
    /// </summary>
    private static Type GetDictionaryValueType(Type type)
        => Array.Find([type, .. type.GetInterfaces()], i => i.IsGenericType
                                                         && (i.GetGenericTypeDefinition() == typeof(IDictionary<,>) || i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)))
               ?.GetGenericArguments()[1];
}
