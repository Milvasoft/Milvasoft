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
    /// Deserializes the JSON representation of an exception with the default serializer (only settable properties such as HelpLink and Source are restored).
    /// </summary>
    /// <param name="reader">The reader to use.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options. They are not passed on, because they contain this converter.</param>
    /// <returns>The deserialized exception.</returns>
    public override TExceptionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => JsonSerializer.Deserialize<TExceptionType>(ref reader);

    /// <summary>
    /// Serializes the specified exception to JSON. Only Message, InnerException and StackTrace are written; other property getters are not invoked.
    /// </summary>
    /// <param name="writer">The writer to use.</param>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, TExceptionType value, JsonSerializerOptions options)
    {
        if (value == null)
            return;

        var skipNulls = options?.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull;

        writer.WriteStartObject();

        writer.WriteString(nameof(Exception.Message), value.Message);

        if (!skipNulls || value.InnerException != null)
        {
            writer.WritePropertyName(nameof(Exception.InnerException));
            JsonSerializer.Serialize(writer, (object)value.InnerException, options);
        }

        var stackTrace = value.StackTrace;

        if (!skipNulls || stackTrace != null)
            writer.WriteString(nameof(Exception.StackTrace), stackTrace);

        writer.WriteEndObject();
    }
}
