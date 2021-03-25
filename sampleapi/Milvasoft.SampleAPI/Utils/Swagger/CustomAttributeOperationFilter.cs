using Microsoft.OpenApi.Models;
using Milvasoft.SampleAPI.Utils.Attributes.ActionFilters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Reflection;

namespace Opsiyon.API.Helpers.Swagger
{
    /// <summary>
	/// <para><b>EN: </b>Swagger document creation utility class</para>
	/// <para><b>TR: </b>Swagger dokumanı oluşturmaya yardımcı sınıf</para>
	/// </summary>
    public class CustomAttributeOperationFilter : IOperationFilter
    {
        /// <summary>
        /// <para><b>EN: </b> Applies filter on swagger document.</para>
        /// <para><b>TR: </b> Swagger dökümantasyonuna istenilen filtrelemeyi uygular. </para>
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo?.IsDefined(typeof(OValidateStringParameterAttribute)) ?? false)
            {
                var defaultValue = Attribute.GetCustomAttribute(context.MethodInfo, typeof(OValidateStringParameterAttribute)) as OValidateStringParameterAttribute;

                foreach (var parameter in operation.Parameters)
                {
                    if(parameter.Schema?.Type == "string" )
                        parameter.Description += parameter.Description != null 
                                                                        ? $"<br /> Minimum Length : {defaultValue.MinimumLength}" + $"<br /> Maximum Length : {defaultValue.MaximumLength}"
                                                                        : $"Minimum Length : {defaultValue.MinimumLength}" + $"<br /> Maximum Length : {defaultValue.MaximumLength}";
                }
            }


        }
    }
}
