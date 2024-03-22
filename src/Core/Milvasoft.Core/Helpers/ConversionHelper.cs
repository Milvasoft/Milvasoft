using Milvasoft.Core.Utils.JsonConverters;
using System.Text.Json;

namespace Milvasoft.Core.Helpers;
public static partial class CommonHelper
{
    /// <summary>
    /// Converts an integer value to a GUID value.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The GUID value.</returns>
    public static Guid ToGuid(this int value)
    {
        var bytes = new byte[16];

        BitConverter.GetBytes(value).CopyTo(bytes, 0);

        return new Guid(bytes);
    }

    /// <summary>
    /// Converts an object to a json string.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <param name="jsonOptions">The json serialization options (optional).</param>
    /// <returns>The json string representation of the object.</returns>
    public static string ToJson(this object value, JsonSerializerOptions jsonOptions = null) => JsonSerializer.Serialize(value, jsonOptions ?? MilvaJsonConverterOptions.Current);

    /// <summary>
    /// Converts json string to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to convert to.</typeparam>
    /// <param name="value">The string value to convert.</param>
    /// <param name="jsonOptions">The json deserialization options (optional).</param>
    /// <returns>The deserialized object of type T.</returns>
    public static T ToObject<T>(this string value, JsonSerializerOptions jsonOptions = null) where T : class
        => string.IsNullOrWhiteSpace(value) ? null : JsonSerializer.Deserialize<T>(value, jsonOptions ?? MilvaJsonConverterOptions.Current);

    /// <summary>
    /// Converts a string to an object of the specified return type.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <param name="returnType">The return type of the object to convert to.</param>
    /// <param name="jsonOptions">The json deserialization options (optional).</param>
    /// <returns>The deserialized object of the specified return type.</returns>
    public static object ToObject(this string value, Type returnType, JsonSerializerOptions jsonOptions = null)
        => string.IsNullOrWhiteSpace(value) ? null : JsonSerializer.Deserialize(value, returnType, jsonOptions ?? MilvaJsonConverterOptions.Current);

    /// <summary>
    /// Deserializes a <see cref="JsonElement"/> to an object of the specified type.
    /// </summary>
    /// <param name="element">The json element to deserialize.</param>
    /// <param name="type">The type of the object to deserialize to.</param>
    /// <returns>The deserialized object of the specified type.</returns>
    public static object Deserialize(this JsonElement element, Type type)
    {
        var genericMethod = _deserializeMethod.MakeGenericMethod(type);

        var value = genericMethod.Invoke(null, [element, null]);

        return value;
    }
}
