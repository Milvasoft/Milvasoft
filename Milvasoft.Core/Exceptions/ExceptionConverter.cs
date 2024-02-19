using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvasoft.Core.Exceptions;

/// <summary>
/// System.Text.Json exception converter. Ignores <see cref="Exception.TargetSite"/> property. Only serializes <see cref="Exception.Message"/> and <see cref="Exception.StackTrace"/> properties.
/// </summary>
/// <typeparam name="TExceptionType"></typeparam>
public class ExceptionConverter<TExceptionType> : JsonConverter<TExceptionType> where TExceptionType : Exception
{
    /// <summary>
    /// Determnies whether <typeparamref name="TExceptionType"/> is convertible to <see cref="Exception"/> or not.
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <returns></returns>
    public override bool CanConvert(Type typeToConvert) => typeof(Exception).IsAssignableFrom(typeToConvert);

    /// <summary>
    /// Deserialization operation.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public override TExceptionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException("Deserializing exceptions is not allowed");

    /// <summary>
    /// Serialization operation.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, TExceptionType value, JsonSerializerOptions options)
    {
        var serializableProperties = value.GetType()
            .GetProperties()
            .Select(uu => new { uu.Name, Value = uu.GetValue(value) })
            .Where(uu => uu.Name == nameof(Exception.Message) || uu.Name == nameof(Exception.StackTrace));

        if (options?.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
        {
            serializableProperties = serializableProperties.Where(uu => uu.Value != null);
        }

        var propList = serializableProperties.ToList();

        // Nothing to write
        if (propList.Count == 0)
            return;

        writer.WriteStartObject();

        foreach (var prop in propList)
        {
            if (prop.Name == nameof(Exception.StackTrace))
            {
                writer.WritePropertyName(prop.Name);
                JsonSerializer.Serialize(writer, prop.Value, options);
            }
            else
            {
                writer.WritePropertyName(prop.Name);
                JsonSerializer.Serialize(writer, prop.Value, options);
            }
        }

        writer.WriteEndObject();
    }
}
