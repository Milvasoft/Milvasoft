using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Milvasoft.Components.OpenApi.SchemaTransformers;

/// <summary>
/// Modifies Enum schemas to display values in "Value -> Name" format (e.g., "1 -> Active").
/// </summary>
public class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        // Ensure the type is an Enum
        if (type != null && type.IsEnum)
        {
            schema.Enum ??= [];

            // Clear the default generated values
            schema.Enum?.Clear();

            // Since we are formatting the output as "Value -> Name", the data type must be string
            schema.Type = JsonSchemaType.String;
            schema.Format = null; // Clear specific formats like 'int32'

            foreach (var name in Enum.GetNames(type))
            {
                var value = Convert.ToInt64(Enum.Parse(type, name));

                // Add the formatted string to the enum list
                schema.Enum.Add($"{value} -> {name}");
            }
        }

        return Task.CompletedTask;
    }
}