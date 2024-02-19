using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvasoft.Core.Exceptions;

public class ExceptionConverter<TExceptionType> : JsonConverter<TExceptionType> where TExceptionType : Exception
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Exception).IsAssignableFrom(typeToConvert);
    }

    public override TExceptionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Deserializing exceptions is not allowed");
    }

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
