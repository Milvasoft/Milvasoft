using Microsoft.OpenApi.Models;
using Milvasoft.Helpers.Attributes.Validation;
using Milvasoft.Helpers.Utils;
using Milvasoft.SampleAPI.AppStartup;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Reflection;

namespace Opsiyon.API.Helpers.Swagger
{
    /// <summary>
	/// <para><b>EN: </b>Swagger document creation utility class</para>
	/// <para><b>TR: </b>Swagger dokumanı oluşturmaya yardımcı sınıf</para>
	/// </summary>
    public class CustomAttributeSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// <para><b>EN: </b> Applies filter on swagger document.</para>
        /// <para><b>TR: </b> Swagger dökümantasyonuna istenilen filtrelemeyi uygular. </para>
        /// </summary>
        /// <param name="swaggerSchema"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiSchema swaggerSchema, SchemaFilterContext context)
        {
            if (context.MemberInfo?.IsDefined(typeof(OValidateStringAttribute)) ?? false)
            {
                var defaultValue = Attribute.GetCustomAttribute(context.MemberInfo, typeof(OValidateStringAttribute)) as OValidateStringAttribute;

                swaggerSchema.MaxLength = defaultValue.MaximumLength;
                swaggerSchema.MinLength = defaultValue.MinimumLength;
            }

            if (context.MemberInfo?.IsDefined(typeof(OValidateDecimalAttribute)) ?? false)
            {
                var defaultValue = Attribute.GetCustomAttribute(context.MemberInfo, typeof(OValidateDecimalAttribute)) as OValidateDecimalAttribute;

                swaggerSchema.Minimum = defaultValue.MinValue == -1 ? 0 : defaultValue.MinValue;
            }

            if (context.MemberInfo?.IsDefined(typeof(MilvaRegexAttribute)) ?? false)
            {
                swaggerSchema.Pattern = Startup.SharedStringLocalizer?[LocalizerKeys.RegexExample + context.MemberInfo.Name];
            }


        }
    }
}
