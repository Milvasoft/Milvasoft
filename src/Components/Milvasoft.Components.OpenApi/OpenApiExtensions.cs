using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.OpenApi.DocumentTransformers;
using Milvasoft.Components.OpenApi.OperationTransformers;
using Milvasoft.Components.OpenApi.SchemaTransformers;
using System.Reflection;

namespace Milvasoft.Components.OpenApi;

/// <summary>
/// OpenApi extension methods.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Adds XML documentation files for OpenApi components. You must ensure that XML documentation files are generated during the build process.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IServiceCollection AddXmlComponentsForOpenApi(this IServiceCollection services, params Assembly[] assemblies)
    {
        var xmlFiles = new List<string>();

        foreach (var assembly in assemblies)
        {
            // 1. Get the assembly name
            var assemblyName = assembly.GetName().Name;
            var xmlFileName = $"{assemblyName}.xml";
            string xmlPath = null;

            // 2. METHOD A: First, check the physical folder where the assembly is located (most reliable)
            // Note: In Single File Publish mode assembly.Location can be empty, so Try/Catch or checks are required.
            try
            {
                if (!string.IsNullOrEmpty(assembly.Location))
                {
                    var assemblyDirectory = Path.GetDirectoryName(assembly.Location);

                    if (!string.IsNullOrEmpty(assemblyDirectory))
                    {
                        var pathCandidate = Path.Combine(assemblyDirectory, xmlFileName);

                        if (File.Exists(pathCandidate))
                            xmlPath = pathCandidate;
                    }
                }
            }
            catch
            {
                // If accessing Assembly.Location fails, swallow the exception and proceed to plan B
            }

            // 3. METHOD B: If not found next to the assembly, look in the application's base directory (AppContext.BaseDirectory)
            // This usually works in Docker or standard IIS deployments.
            if (xmlPath == null)
            {
                var baseDirectoryCandidate = Path.Combine(AppContext.BaseDirectory, xmlFileName);

                if (File.Exists(baseDirectoryCandidate))
                    xmlPath = baseDirectoryCandidate;
            }

            // 4. If the file is found, add it to the list
            if (xmlPath != null)
                xmlFiles.Add(xmlPath);
        }

        if (xmlFiles.Count > 0)
            services.AddSingleton(new XmlDocumentationService(xmlFiles));

        return services;
    }

    /// <summary>
    /// Adds Milva OpenApi transformers.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static OpenApiOptions AddMilvaTransformers(this OpenApiOptions options)
    {
        // Document
        options.AddDocumentTransformer<OpenApiExcludeTransformer>();
        options.AddDocumentTransformer<BearerSecuritySchemaTransformer>();

        // Operation
        options.AddOperationTransformer<XmlCommentsTransformer>();
        options.AddOperationTransformer<RequestHeaderTransformer>();

        // Schema
        options.AddSchemaTransformer<XmlCommentsTransformer>();
        options.AddSchemaTransformer<EnumSchemaTransformer>();

        return options;
    }
}
