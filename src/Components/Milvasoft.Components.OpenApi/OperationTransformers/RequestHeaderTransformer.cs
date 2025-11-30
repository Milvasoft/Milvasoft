using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Milvasoft.Components.OpenApi.OperationTransformers;

/// <summary>
/// Adds the "Accept-Language" header requirement and removes the "version" parameter for non-Weather controllers.
/// </summary>
public class RequestHeaderTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // Ensure parameters list is initialized
        operation.Parameters ??= [];

        // Check if the action belongs to a controller
        if (context.Description.ActionDescriptor is ControllerActionDescriptor descriptor)
        {
            // Skip logic if the controller name starts with "Weather"
            if (descriptor.ControllerName.StartsWith("Weather", StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            // 1. Remove "version" parameter if it exists
            var versionParameter = operation.Parameters.FirstOrDefault(p => p.Name == "version");

            if (versionParameter != null)
                operation.Parameters.Remove(versionParameter);

            // 2. Add "Accept-Language" header parameter
            // Check if it already exists to avoid duplicates
            if (!operation.Parameters.Any(p => p.Name == "Accept-Language" && p.In == ParameterLocation.Header))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Accept-Language",
                    In = ParameterLocation.Header,
                    Description = "The lang iso code of system. (e.g. tr-TR)",
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Default = "tr-TR"
                    }
                });
            }
        }

        return Task.CompletedTask;
    }
}