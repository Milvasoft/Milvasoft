using Milvasoft.Core.Utils.JsonConverters;
using System.Text.Json;

namespace Milvasoft.Core.Helpers;
public static partial class CommonHelper
{
    /// <summary>
    /// This method return int value to guid value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Guid ToGuid(this int value)
    {
        var bytes = new byte[16];

        BitConverter.GetBytes(value).CopyTo(bytes, 0);

        return new Guid(bytes);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="string"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="jsonOptions"></param>
    /// <returns></returns>
    public static string ToJson(this object value, JsonSerializerOptions jsonOptions = null) => JsonSerializer.Serialize(value, jsonOptions ?? MilvaJsonConverterOptions.Current);

    /// <summary>
    /// Converts <paramref name="value"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="jsonOptions"></param>
    /// <returns></returns>
    public static T ToObject<T>(this string value, JsonSerializerOptions jsonOptions = null) where T : class
        => string.IsNullOrWhiteSpace(value) ? null : JsonSerializer.Deserialize<T>(value, jsonOptions ?? MilvaJsonConverterOptions.Current);

    /// <summary>
    /// Converts <paramref name="value"/> to <paramref name="returnType"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="returnType"></param>
    /// <param name="jsonOptions"></param>
    /// <returns></returns>
    public static object ToObject(this string value, Type returnType, JsonSerializerOptions jsonOptions = null)
        => string.IsNullOrWhiteSpace(value) ? null : JsonSerializer.Deserialize(value, returnType, jsonOptions ?? MilvaJsonConverterOptions.Current);

    /// <summary>
    /// Deserializes json element to <paramref name="type"/>. 
    /// </summary>
    /// <param name="element"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object Deserialize(this JsonElement element, Type type)
    {
        var genericMethod = _deserializeMethod.MakeGenericMethod(type);

        var value = genericMethod.Invoke(null, [element, null]);

        return value;
    }
}
