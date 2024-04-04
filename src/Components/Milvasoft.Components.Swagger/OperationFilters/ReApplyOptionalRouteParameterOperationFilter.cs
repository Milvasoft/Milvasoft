using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.RegularExpressions;

namespace Milvasoft.Components.Swagger.OperationFilters;

/// <summary>
/// Prevents optional parameters from appearing as required in swagger.
/// </summary>
public class ReApplyOptionalRouteParameterOperationFilter : IOperationFilter
{
    private const string _captureName = "routeParameter";

    /// <summary>
    /// Applies configuration.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var httpMethodAttributes = context.MethodInfo.GetCustomAttributes(true).OfType<HttpMethodAttribute>();

        var httpMethodWithOptional = httpMethodAttributes.FirstOrDefault(m => m.Template?.Contains('?') ?? false);

        if (httpMethodWithOptional == null)
            return;

        string regex = $"{{(?<{_captureName}>\\w+)\\?}}";

        var matches = Regex.Matches(httpMethodWithOptional.Template, regex, RegexOptions.NonBacktracking, TimeSpan.FromMilliseconds(100));

        foreach (Match match in matches.Cast<Match>())
        {
            var name = match.Groups[_captureName].Value;

            var parameter = operation.Parameters.FirstOrDefault(p => p.In == ParameterLocation.Path && p.Name == name);

            if (parameter != null)
            {
                parameter.AllowEmptyValue = true;
                parameter.Description = "Must check \"Send empty value\" or Swagger passes a comma for empty values otherwise";
                parameter.Required = false;
                parameter.Schema.Nullable = true;
            }
        }
    }
}
