using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace Milvasoft.SampleAPI.Utils.Swagger
{
    /// <summary>
    /// Operation filter to add the requirement of the custom header
    /// </summary>
    public class RequestHeaderFilter : IOperationFilter
    {
        /// <summary>
        /// Applies configuration.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null) operation.Parameters = new List<OpenApiParameter>();

            var descriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;

            if (descriptor != null && !descriptor.ControllerName.StartsWith("Weather"))
            {
                var versionParameter = operation.Parameters.SingleOrDefault(p => p.Name == "version");
                if (versionParameter != null) operation.Parameters.Remove(versionParameter);

                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "Accept-Language",
                    In = ParameterLocation.Header,
                    Description = "The lang iso code of system. (e.g. tr-TR)",
                    Required = false
                });
            }
        }
    }
}
