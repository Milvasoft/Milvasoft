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

                //if (descriptor.ControllerName.StartsWith("Institution"))
                //{
                //    operation.Parameters.Add(new OpenApiParameter()
                //    {
                //        Name = "ApiKey",
                //        In = ParameterLocation.Header,
                //        Description = "Opsiyon API Key.",
                //        Required = false
                //    });
                //}

                //if (descriptor.ControllerName.StartsWith("Report"))
                //{
                //    operation.Parameters.Add(new OpenApiParameter()
                //    {
                //        Name = "PageIndex",
                //        In = ParameterLocation.Header,
                //        Description = "Requested page index.",
                //        Required = false
                //    });

                //    operation.Parameters.Add(new OpenApiParameter()
                //    {
                //        Name = "ItemCount",
                //        In = ParameterLocation.Header,
                //        Description = "Requested item count in page.",
                //        Required = false
                //    });
                //}

            }
        }
    }
}
