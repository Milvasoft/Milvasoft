using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvasoft.Core.Utils.Converters;

/// <summary>
/// Converts exceptions to JSON by serializing only the <see cref="Exception.Message"/> and <see cref="Exception.StackTrace"/> properties. Ignores the <see cref="Exception.TargetSite"/> property.
/// </summary>
/// <typeparam name="TExceptionType">The type of exception to convert.</typeparam>
public class ExceptionConverter<TExceptionType> : JsonConverter<TExceptionType> where TExceptionType : Exception
{
    /// <summary>
    /// Determines whether the specified type can be converted to <see cref="Exception"/>.
    /// </summary>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <returns><c>true</c> if the specified type can be converted to <see cref="Exception"/>; otherwise, <c>false</c>.</returns>
    public override bool CanConvert(Type typeToConvert) => typeof(Exception).IsAssignableFrom(typeToConvert);

    /// <summary>
    /// Deserializes the JSON representation of an exception.
    /// </summary>
    /// <param name="reader">The reader to use.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized exception.</returns>
    /// <exception cref="NotSupportedException">Thrown when deserialization is not supported.</exception>
    public override TExceptionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => JsonSerializer.Deserialize<TExceptionType>(ref reader);

    /// <summary>
    /// Serializes the specified exception to JSON.
    /// </summary>
    /// <param name="writer">The writer to use.</param>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, TExceptionType value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            return;
        }

        var serializableProperties = value.GetType()
                                          .GetProperties()
                                          .Select(uu => new { uu.Name, Value = uu.GetValue(value) })
                                          .Where(uu => uu.Name == nameof(Exception.Message) || uu.Name == nameof(Exception.StackTrace) || uu.Name == nameof(Exception.InnerException));

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
            if (prop.Name == nameof(Exception.InnerException))
            {
                writer.WritePropertyName(prop.Name);
                JsonSerializer.Serialize(writer, prop.Value, options);
            }
            else
            {
                writer.WritePropertyName(prop.Name);
                writer.WriteStringValue((string)prop.Value);
            }
        }

        writer.WriteEndObject();
    }
}
