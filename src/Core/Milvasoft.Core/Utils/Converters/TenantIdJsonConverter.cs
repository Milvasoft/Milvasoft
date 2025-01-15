using Milvasoft.Core.EntityBases.MultiTenancy;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvasoft.Core.Utils.Converters;

/// <summary>
/// Json converter for tenant id.
/// </summary>
public class TenantIdJsonConverter : JsonConverter<TenantId>
{
    /// <inheritdoc/>
    public override TenantId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var tenantIdString = reader.GetString();

            if (TenantId.TryParse(tenantIdString))
            {
                return new TenantId(tenantIdString);
            }
        }

        throw new JsonException($"Invalid value for {nameof(TenantId)}.");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TenantId value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}