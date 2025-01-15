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
            else
                return TenantId.Empty;
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            return TenantId.Empty;
        }

        throw new MilvaDeveloperException($"Invalid value for {nameof(TenantId)}.");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TenantId value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}