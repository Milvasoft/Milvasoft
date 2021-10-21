using Microsoft.OpenApi.Models;
using Milvasoft.SampleAPI.Entity;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Milvasoft.SampleAPI.Utils.Swagger
{
    /// <summary>
	///Swagger document creation utility classs
	/// </summary>
    public class EntityDocumentFilter : IDocumentFilter
    {
        /// <summary>
        /// Applies filter on swagger document..
        /// </summary>
        /// <param name="swaggerDoc"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var entityAssembly = Assembly.GetAssembly(typeof(Student));

            //Sorting operations of schemas. DTOs comes first, Specs comes second.
            swaggerDoc.Components.Schemas = swaggerDoc.Components.Schemas.OrderByDescending(d => d.Key.Contains("DTO")).ThenBy(i => i.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
