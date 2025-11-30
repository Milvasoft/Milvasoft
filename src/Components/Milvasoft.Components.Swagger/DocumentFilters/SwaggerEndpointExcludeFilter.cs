using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Milvasoft.Components.Swagger.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Milvasoft.Components.Swagger.DocumentFilters;

/// <summary>
/// Deletes properties marked with the SwaggerExclude attribute from the swagger documentation.
/// </summary>
public class SwaggerEndpointExcludeFilter : IDocumentFilter
{
    /// <summary>
    /// Applies configuration to swagger document.
    /// </summary>
    /// <param name="swaggerDoc"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var contextApiDescription in context.ApiDescriptions)
        {
            var actionDescriptor = (ControllerActionDescriptor)contextApiDescription.ActionDescriptor;

            if (actionDescriptor.ControllerTypeInfo.GetCustomAttributes<SwaggerExcludeAttribute>().Any() || actionDescriptor.MethodInfo.GetCustomAttributes<SwaggerExcludeAttribute>().Any())
            {
                var key = string.Concat("/", contextApiDescription.RelativePath);

                swaggerDoc.Paths.Remove(key);
            }
        }
    }
}
