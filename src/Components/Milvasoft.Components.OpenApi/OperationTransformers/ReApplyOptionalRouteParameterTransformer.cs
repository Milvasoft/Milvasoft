using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.RegularExpressions;

namespace Milvasoft.Components.Swagger.OperationFilters;

/// <summary>
/// Adds a warning description to optional route parameters (e.g., "{id?}") to guide API clients.
/// </summary>
public partial class ReApplyOptionalRouteParameterTransformer : IOpenApiOperationTransformer
{
    private const string _captureName = "routeParameter";

    // Regex is compiled for performance
    private static readonly Regex _routeParamRegex = RouteParamRegex();

    /// <inheritdoc/>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // 1. Get the MethodInfo from ActionDescriptor
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            return Task.CompletedTask;
        }

        var methodInfo = actionDescriptor.MethodInfo;

        // 2. Find HttpMethodAttribute (HttpGet, HttpPost etc.) that contains a '?' in the template
        var httpMethodAttributes = methodInfo.GetCustomAttributes(true).OfType<HttpMethodAttribute>();
        var httpMethodWithOptional = httpMethodAttributes.FirstOrDefault(m => m.Template?.Contains('?') ?? false);

        if (httpMethodWithOptional == null || httpMethodWithOptional.Template == null)
        {
            return Task.CompletedTask;
        }

        // 3. Match the optional parameters in the route template
        var matches = _routeParamRegex.Matches(httpMethodWithOptional.Template);

        foreach (Match match in matches.Cast<Match>())
        {
            var name = match.Groups[_captureName].Value;

            // 4. Find the corresponding parameter in the OpenAPI operation
            var parameter = operation.Parameters?.FirstOrDefault(p => p.In == ParameterLocation.Path && p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (parameter != null)
            {
                // 5. Append the warning message
                var warningMsg = " (Must check \"Send empty value\" or client may pass a comma/invalid data for empty values)";

                if (string.IsNullOrEmpty(parameter.Description))
                {
                    parameter.Description = warningMsg.TrimStart();
                }
                else if (!parameter.Description.Contains(warningMsg))
                {
                    parameter.Description += warningMsg;
                }

                // Optional: Explicitly allow empty value if needed
                // parameter.AllowEmptyValue = true;
            }
        }

        return Task.CompletedTask;
    }

    [GeneratedRegex(@"{(?<routeParameter>\w+)\?}", RegexOptions.Compiled)]
    private static partial Regex RouteParamRegex();
}