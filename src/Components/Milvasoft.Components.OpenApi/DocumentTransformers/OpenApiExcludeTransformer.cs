using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Milvasoft.Components.OpenApi.Attributes;
using System.Text.RegularExpressions;

namespace Milvasoft.Components.OpenApi.DocumentTransformers;

/// <summary>
/// Deletes operations marked with the <see cref="OpenApiExcludeAttribute"/> attribute (on the controller, the action or minimal API endpoint metadata)
/// from the openapi documentation.
/// </summary>
/// <remarks>
/// <see cref="OpenApiExtensions.AddMilvaTransformers(OpenApiOptions)"/> also excludes these endpoints with <see cref="OpenApiOptions.ShouldInclude"/>,
/// so they are not generated at all; this transformer removes anything that was generated anyway.
/// </remarks>
public partial class OpenApiExcludeTransformer(IApiDescriptionGroupCollectionProvider apiDescriptionProvider) : IOpenApiDocumentTransformer
{
    private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionProvider = apiDescriptionProvider;

    ///  <inheritdoc/>
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.Paths == null)
            return Task.CompletedTask;

        var apiDescriptions = context?.DescriptionGroups?.SelectMany(g => g.Items)
                              ?? _apiDescriptionProvider.ApiDescriptionGroups?.Items.SelectMany(g => g.Items)
                              ?? [];

        foreach (var apiDescription in apiDescriptions)
        {
            if (!IsExcluded(apiDescription))
                continue;

            var key = NormalizePath(apiDescription.RelativePath);

            if (!document.Paths.TryGetValue(key, out var pathItem) || pathItem.Operations == null)
                continue;

            // Remove only the specific operation (e.g., remove POST, keep GET)
            if (apiDescription.HttpMethod != null)
                pathItem.Operations.Remove(new HttpMethod(apiDescription.HttpMethod));

            // If the path has no operations left, remove the path entirely
            if (pathItem.Operations.Count == 0)
                document.Paths.Remove(key);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Whether the endpoint is marked with <see cref="OpenApiExcludeAttribute"/> (controller, action or endpoint metadata).
    /// </summary>
    /// <param name="apiDescription"></param>
    /// <returns></returns>
    public static bool IsExcluded(ApiDescription apiDescription)
        => apiDescription?.ActionDescriptor?.EndpointMetadata?.OfType<OpenApiExcludeAttribute>().Any() == true;

    /// <summary>
    /// Converts a route template to an OpenAPI path key: leading slash, no query string, no constraints, defaults or optional/catch-all markers.
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    public static string NormalizePath(string relativePath)
    {
        // Route parameters first: an optional parameter ({id?}) contains '?' too.
        var path = RouteParameterRegex().Replace(relativePath ?? string.Empty, match => $"{{{match.Groups["name"].Value}}}");

        path = path.Split('?', 2)[0];

        return "/" + path.TrimStart('/');
    }

    [GeneratedRegex(@"\{\**(?<name>[^}:=?]+)[^}]*\}")]
    private static partial Regex RouteParameterRegex();
}
