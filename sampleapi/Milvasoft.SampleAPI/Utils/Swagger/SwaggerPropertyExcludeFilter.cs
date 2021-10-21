using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Milvasoft.SampleAPI.Utils.Swagger
{
    /// <summary>
    /// Deletes properties marked with the SwaggerExclude attribute from the swagger documentation.
    /// </summary>
    public class SwaggerPropertyExcludeFilter : ISchemaFilter
    {
        /// <summary>
        /// Applies configuration.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null)
            {
                return;
            }

            var excludedProperties = context.Type.GetProperties().Where(t => t.GetCustomAttribute<SwaggerExcludeAttribute>() != null);

            foreach (var excludedProperty in excludedProperties)
            {
                var propertyToRemove = schema.Properties.Keys.SingleOrDefault(x => x.ToLower() == excludedProperty.Name.ToLower());

                if (propertyToRemove != null)
                {
                    schema.Properties.Remove(propertyToRemove);
                }
            }
        }
    }
}
