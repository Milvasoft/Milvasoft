using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Milvasoft.Components.Swagger.OperationFilters;

/// <summary>
/// Provides file upload in swagger ui.
/// </summary>
public class SwaggerFileOperationFilter : IOperationFilter
{
    /// <summary>
    /// Modifies operation.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileUploadMime = "multipart/form-data";

        if (operation.RequestBody == null || !operation.RequestBody.Content.Any(x => x.Key.Equals(fileUploadMime, StringComparison.InvariantCultureIgnoreCase)))
            return;

        var fileParams = context.MethodInfo.GetParameters().Where(p => p.ParameterType == typeof(IFormFile));

        operation.RequestBody.Content[fileUploadMime].Schema.Properties = fileParams.ToDictionary(k => k.Name, v => new OpenApiSchema()
        {
            Type = "string",
            Format = "binary",
            Description = "File to be upload."
        });
    }
}
