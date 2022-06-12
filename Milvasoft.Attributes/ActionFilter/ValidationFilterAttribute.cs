﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Milvasoft.Core.Extensions;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Core.Utils.Models.Response;
using Newtonsoft.Json;
using System.Reflection;

namespace Milvasoft.Attributes.ActionFilter;

/// <summary>
/// Provides the attribute validation exclude opportunity.
/// </summary>
public class ValidationFilterAttribute : ActionFilterAttribute
{
    #region Properties

    /// <summary>
    /// Defines the features will be cancelled required.
    /// </summary>
    public string DisabledProperties { get; set; }

    /// <summary>
    /// Defines the features will be cancelled required. 
    /// </summary>
    public string DisabledNestedProperties { get; set; }

    /// <summary>
    /// Assembly type for disable nested type prop validations.
    /// </summary>
    public Type AssemblyTypeForNestedProps { get; set; }

    /// <summary>
    /// Folder assembly name for disable nested type prop validations.
    /// </summary>
    public string DTOFolderAssemblyName { get; set; }

    #endregion

    /// <summary>
    /// Performs when action executing.
    /// </summary>
    /// <param name="context"></param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        async Task<ActionExecutingContext> RewriteResponseAsync(string message)
        {
            var validationResponse = new ExceptionResponse
            {
                Success = false,
                Message = message,
                StatusCode = MilvaStatusCodes.Status600Exception,
                Result = new object(),
                ErrorCodes = new List<int>()
            };
            var json = JsonConvert.SerializeObject(validationResponse);

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Items.Add(new KeyValuePair<object, object>("StatusCode", MilvaStatusCodes.Status600Exception));
            context.HttpContext.Response.StatusCode = MilvaStatusCodes.Status200OK;
            await context.HttpContext.Response.WriteAsync(json).ConfigureAwait(false);

            context.Result = new OkResult();

            return context;
        };

        if (!context.ModelState.IsValid)
        {
            var httpContext = context.HttpContext;

            IEnumerable<ModelError> modelErrors = context.ModelState.Values.SelectMany(v => v.Errors);

            var errors = new HashSet<string> { };

            foreach (var item in modelErrors)
                errors.Add(item.ErrorMessage);

            var properties = GetProperties(DisabledProperties);
            var nestedProperties = GetProperties(DisabledNestedProperties);

            if (!nestedProperties.IsNullOrEmpty())
            {
                foreach (var nestedProp in nestedProperties)
                {
                    var assembly = Assembly.GetAssembly(AssemblyTypeForNestedProps);

                    var dtoType = assembly?.GetExportedTypes()?.ToList()?.FirstOrDefault(i => i.FullName.Contains(DTOFolderAssemblyName)
                                                                                                && (i.Name == $"{nestedProp}DTO"
                                                                                                     || i.Name == $"{nestedProp.Remove(nestedProp.Length - 1, 1)}DTO"));
                    if (dtoType != null)
                    {
                        var dtoProps = dtoType.GetProperties().ToList();

                        dtoProps.RemoveAll(i => i.Name.Contains("Id"));

                        foreach (var entityProp in dtoProps)
                        {
                            if (entityProp.CustomAttributes.Count() != 0)
                                if (httpContext.Items[entityProp.Name] != null)
                                {
                                    errors.Remove(httpContext.Items[entityProp.Name].ToString());
                                    httpContext.Items.Remove(entityProp.Name);
                                }
                        }
                    }
                }
            }

            if (!properties.IsNullOrEmpty())
                foreach (var prop in properties)
                    if (httpContext.Items[prop] != null)
                    {
                        errors.Remove(httpContext.Items[prop].ToString());
                        httpContext.Items.Remove(prop);
                    }

            if (!errors.IsNullOrEmpty())
                base.OnActionExecuting(RewriteResponseAsync(string.Join("~", errors)).Result);
        }
    }

    /// <summary>
    /// Gets properties.
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    private List<string> GetProperties(string props)
    {
        string[] prop = null;

        if (!string.IsNullOrWhiteSpace(props))
        {
            prop = props.Split(',');

            for (int i = 0; i < prop.Length; i++)
            {
                prop[i] = prop[i].Trim();
            }
        }

        return prop.IsNullOrEmpty() ? null : prop.ToList();
    }
}