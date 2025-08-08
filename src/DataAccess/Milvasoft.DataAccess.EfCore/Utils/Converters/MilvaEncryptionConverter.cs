using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Cryptography.Abstract;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvasoft.DataAccess.EfCore.Utils.Converters;

/// <summary>
/// Defines the internal encryption converter for string values.
/// </summary>
/// <remarks>
/// Creates a new <see cref="MilvaEncryptionConverter"/> instance.
/// </remarks>
/// <param name="crypto"></param>
/// <param name="converterMappingHints"></param>

public sealed class MilvaEncryptionConverter(IMilvaCryptographyProvider crypto, ConverterMappingHints converterMappingHints = null) : ValueConverter<string, string>(
        s => s == null ? null
                 : (s.StartsWith(_prefix, StringComparison.Ordinal) ? s : _prefix + crypto.Encrypt(s)),
        s => s == null ? null
                 : (s.StartsWith(_prefix, StringComparison.Ordinal) ? SafeDecrypt(s, crypto) : s), converterMappingHints)
{
    private const string _prefix = "enc::";

    private static string SafeDecrypt(string marked, IMilvaCryptographyProvider crypto)
    {
        var payload = marked[_prefix.Length..];

        try
        {
            return crypto.Decrypt(payload);
        }
        catch
        {
            // If data is corrupted or broken, do not throw an exception, just return the original marked value
            return marked;
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
/// <param name="crypto"></param>
public sealed class EncryptedJsonValueConverter<T>(IMilvaCryptographyProvider crypto) : ValueConverter<T, string>(model => ToProvider(model, crypto), json => FromProvider(json, crypto))
{
    private const string _prefix = "enc::";

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

    private static T FromProvider(string json, IMilvaCryptographyProvider crypto)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default!;

        using var doc = JsonDocument.Parse(json);
        var root = DecryptElement(doc.RootElement, typeof(T), crypto);
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
                                                                   .Where(p => p.CanRead)
                                                                   .Select(p => new PropertyMeta(p.Name,
                                                                                                 p,
                                                                                                 p.GetCustomAttribute<EncryptedAttribute>() is not null,
                                                                                                 p.PropertyType))]);

    private static string EncryptIfNeeded(string v, bool shouldEncrypt, IMilvaCryptographyProvider crypto)
    {
        if (!shouldEncrypt)
            return v;

        if (string.IsNullOrEmpty(v))
            return v;

        if (v.StartsWith(_prefix, StringComparison.Ordinal))
            return v; // already marked

        return _prefix + crypto.Encrypt(v);
    }

    private static string DecryptIfNeeded(string v, bool shouldDecrypt, IMilvaCryptographyProvider crypto)
    {
        if (!shouldDecrypt)
            return v;

        if (string.IsNullOrEmpty(v))
            return v;

        // tolerate legacy/plain values
        if (!v.StartsWith(_prefix, StringComparison.Ordinal))
            return v;

        var payload = v[_prefix.Length..];

        // Be tolerant: if base64/cipher is broken, just return original
        try
        {
            return crypto.Decrypt(payload);
        }
        catch
        {
            // optionally log here
            return v;
        }
    }

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

        // IEnumerable (but not IDictionary)
        if (obj is IEnumerable enumerable && obj is not IDictionary)
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
                dict[meta.Name] = EncryptIfNeeded((string)value, shouldEncrypt: true, crypto);
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
    /// <returns></returns>
    private static JsonElement DecryptElement(JsonElement element, Type targetType, IMilvaCryptographyProvider crypto)
    {
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
                        var plain = DecryptIfNeeded(str, shouldDecrypt: true, crypto);
                        writer.WriteStringValue(plain);
                        continue;
                    }

                    if (jp.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        var child = DecryptElement(jp.Value, meta.PropType, crypto);
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
                        DecryptElement(item, elemType, crypto).WriteTo(writer);
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
}
