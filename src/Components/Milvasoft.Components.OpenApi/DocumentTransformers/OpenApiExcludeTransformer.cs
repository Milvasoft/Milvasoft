using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Milvasoft.Components.OpenApi.Attributes;

namespace Milvasoft.Components.OpenApi.DocumentTransformers;

/// <summary>
/// Deletes properties marked with the <see cref="OpenApiExcludeAttribute"/> attribute from the openapi documentation.
/// </summary>
public class OpenApiExcludeTransformer(IApiDescriptionGroupCollectionProvider apiDescriptionProvider) : IOpenApiDocumentTransformer
{
    private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionProvider = apiDescriptionProvider;

    ///  <inheritdoc/>
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        var apiDescriptions = _apiDescriptionProvider.ApiDescriptionGroups?.Items.SelectMany(g => g.Items) ?? [];

        foreach (var apiDescription in apiDescriptions)
        {
            // 1. Check if it is a Controller Action
            if (apiDescription.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
                continue;

            // 2. Check for the attribute on Controller or Action
            var controllerHasExclude = actionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(OpenApiExcludeAttribute), inherit: true).Length != 0;
            var actionHasExclude = actionDescriptor.MethodInfo.GetCustomAttributes(typeof(OpenApiExcludeAttribute), inherit: true).Length != 0;

            if (!controllerHasExclude && !actionHasExclude)
                continue;

            // 3. Normalize path key to match OpenAPI standard
            var relativePath = apiDescription.RelativePath ?? string.Empty;

            // Remove query string if present
            var pathWithoutQuery = relativePath.Split('?', 2)[0];

            // Ensure leading slash
            var key = "/" + pathWithoutQuery.TrimStart('/');

            // 4. Check if path exists in the document
            if (document.Paths.TryGetValue(key, out var pathItem))
            {
                // 5. Identify and remove the specific HTTP Operation
                if (apiDescription.HttpMethod != null)
                {
                    // Remove only the specific operation (e.g., remove POST, keep GET)
                    pathItem.Operations.Remove(new HttpMethod(apiDescription.HttpMethod));
                }

                // 6. If the path has no operations left, remove the path entirely
                if (pathItem.Operations.Count == 0)
                {
                    document.Paths.Remove(key);
                }
            }
        }

        return Task.CompletedTask;
    }
}
