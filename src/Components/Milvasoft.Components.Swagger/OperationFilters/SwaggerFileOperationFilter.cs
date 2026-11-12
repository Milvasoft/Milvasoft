using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Milvasoft.Components.Swagger.OperationFilters;

/// <summary>
/// Provides file upload in swagger ui.
/// </summary>
public class SwaggerFileOperationFilter : IOperationFilter
{
    private const string _fileUploadMime = "multipart/form-data";

    /// <summary>
    /// Modifies operation.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var mediaType = operation.RequestBody?.Content?.FirstOrDefault(x => x.Key.Equals(_fileUploadMime, StringComparison.InvariantCultureIgnoreCase)).Value;

        // Referenced schemas cannot be changed here; only inline multipart schemas are extended.
        if (mediaType?.Schema is not OpenApiSchema schema || context.MethodInfo == null)
            return;

        var fileParams = context.MethodInfo.GetParameters().Where(p => ContainsFileType(p.ParameterType, []));

        foreach (var fileParam in fileParams)
        {
            schema.Properties ??= new Dictionary<string, IOpenApiSchema>();

            // Swashbuckle may have described the parameter already; never add it twice.
            if (fileParam.Name == null || schema.Properties.ContainsKey(fileParam.Name))
                continue;

            schema.Properties[fileParam.Name] = new OpenApiSchema()
            {
                Type = JsonSchemaType.String,
                Format = "binary",
                Description = "File to be upload."
            };
        }
    }

    /// <summary>
    /// Whether <paramref name="type"/> is or contains (in its properties) an <see cref="IFormFile"/>.
    /// Visited types are tracked and framework types are not inspected, so self referencing types (e.g. DateTime.Date) cannot recurse forever.
    /// </summary>
    private static bool ContainsFileType(Type type, HashSet<Type> visited)
    {
        if (type == null)
            return false;

        if (typeof(IFormFile).IsAssignableFrom(type) || typeof(IFormFileCollection).IsAssignableFrom(type))
            return true;

        if (type.IsPrimitive || type.IsEnum || type.IsPointer || type == typeof(string) || !visited.Add(type))
            return false;

        if (type.HasElementType)
            return ContainsFileType(type.GetElementType(), visited);

        // Collections and other generic framework types (List<IFormFile>, ...) contain files through their type arguments.
        if (type.IsGenericType && Array.Exists(type.GetGenericArguments(), argument => ContainsFileType(argument, visited)))
            return true;

        if (IsFrameworkType(type))
            return false;

        return Array.Exists(type.GetProperties(), property => property.GetIndexParameters().Length == 0 && ContainsFileType(property.PropertyType, visited));
    }

    private static bool IsFrameworkType(Type type)
    {
        var typeNamespace = type.Namespace;

        return typeNamespace != null
               && (typeNamespace == "System" || typeNamespace.StartsWith("System.", StringComparison.Ordinal) || typeNamespace.StartsWith("Microsoft.", StringComparison.Ordinal));
    }
}
