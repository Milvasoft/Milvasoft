using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Globalization;
using System.Text.Json.Nodes;

namespace Milvasoft.Components.OpenApi.SchemaTransformers;

/// <summary>
/// Documents enum members: adds a "Value = Name" list to the schema description (e.g. "1 = Active, 2 = Passive")
/// and, for integer enums, the member names as the <c>x-enum-varnames</c> extension.
/// </summary>
/// <remarks>
/// The schema type and values are kept as generated, so the schema still matches what the API serializes
/// (numbers by default, names when a string enum converter is used) and generated clients keep working.
/// All underlying types are supported, including <see cref="ulong"/>.
/// </remarks>
public class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    private const string _enumNamesExtension = "x-enum-varnames";

    /// <inheritdoc/>
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        type = type == null ? null : Nullable.GetUnderlyingType(type) ?? type;

        if (type == null || !type.IsEnum)
            return Task.CompletedTask;

        var names = Enum.GetNames(type);
        var values = Enum.GetValuesAsUnderlyingType(type);

        var mapping = string.Join(", ", names.Select((name, index) => $"{Convert.ToString(values.GetValue(index), CultureInfo.InvariantCulture)} = {name}"));

        if (string.IsNullOrWhiteSpace(schema.Description))
            schema.Description = mapping;
        else if (!schema.Description.Contains(mapping, StringComparison.Ordinal))
            schema.Description = $"{schema.Description} ({mapping})";

        if (schema.Type is JsonSchemaType schemaType && schemaType.HasFlag(JsonSchemaType.Integer))
        {
            schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            schema.Extensions[_enumNamesExtension] = new JsonNodeExtension(new JsonArray([.. names.Select(name => (JsonNode)name)]));
        }

        return Task.CompletedTask;
    }
}
