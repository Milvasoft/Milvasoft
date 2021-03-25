using Microsoft.OpenApi.Models;
using Milvasoft.SampleAPI.Entity;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Milvasoft.SampleAPI.Utils.Swagger
{
    /// <summary>
	/// <para><b>EN: </b>Swagger document creation utility class</para>
	/// <para><b>TR: </b>Swagger dokumanı oluşturmaya yardımcı sınıf</para>
	/// </summary>
    public class EntityDocumentFilter : IDocumentFilter
    {
        /// <summary>
        /// <para><b>EN: </b> Applies filter on swagger document.</para>
        /// <para><b>TR: </b> Swagger dökümantasyonuna istenilen filtrelemeyi uygular. </para>
        /// </summary>
        /// <param name="swaggerDoc"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var entityAssembly = Assembly.GetAssembly(typeof(AppUser));

            //Removing entities from document.
            foreach (var name in entityAssembly.DefinedTypes.Select(dt => dt.Name))
            {
                if (name != "EducationStatus")
                {
                    context.SchemaRepository.Schemas.Remove(name);
                    swaggerDoc.Components.Schemas.Remove(name);
                }
            }

            //foreach (var item in swaggerDoc.Components.Schemas)
            //{

            //    item.Value.Properties.Clear();
            //    //foreach (var prop in item.Value.Properties)
            //    //{
            //    //    prop.Value.Properties = null;
            //    //}
            //}


            //foreach (var item in context.SchemaRepository.Schemas)
            //{
            //    item.Value.Properties.Clear();
            //    //foreach (var prop in item.Value.Properties)
            //    //{
            //    //    prop.Value.Properties = null;
            //    //}
            //}

            //swaggerDoc.Components.Examples.Clear();

            //Sorting operations of schemas. DTOs comes first, Specs comes second.
            swaggerDoc.Components.Schemas = swaggerDoc.Components.Schemas.OrderByDescending(d => d.Key.Contains("DTO")).ThenBy(i => i.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
