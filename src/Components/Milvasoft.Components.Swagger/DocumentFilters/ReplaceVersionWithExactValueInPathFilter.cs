﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Milvasoft.Components.Swagger.DocumentFilters;

/// <summary>
/// Replaces version parameter.
/// </summary>
public class ReplaceVersionWithExactValueInPathFilter : IDocumentFilter
{
    /// <summary>
    /// Applies configuration.
    /// </summary>
    /// <param name="swaggerDoc"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = swaggerDoc.Paths;
        swaggerDoc.Paths = [];
        foreach (var path in paths)
        {
            var key = path.Key.Replace("v{version}", swaggerDoc.Info.Version);
            var value = path.Value;
            swaggerDoc.Paths.Add(key, value);
        }
    }
}
