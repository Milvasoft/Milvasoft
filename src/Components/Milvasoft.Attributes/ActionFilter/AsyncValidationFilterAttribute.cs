using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Reflection;

namespace Milvasoft.Attributes.ActionFilter;

/// <summary>
/// Throws <see cref="MilvaUserFriendlyException"/> if any error exists. Provides the attribute validation exclude opportunity.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AsyncValidationFilterAttribute : Attribute, IAsyncActionFilter
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
    /// <param name="next"></param>
    public virtual async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
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
                            if (entityProp.CustomAttributes.Any())
                                if (httpContext.Items[entityProp.Name] != null)
                                    errors.Remove(httpContext.Items[entityProp.Name].ToString());
                        }
                    }
                }
            }

            if (!properties.IsNullOrEmpty())
                foreach (var prop in properties)
                    if (httpContext.Items[prop] != null)
                        errors.Remove(httpContext.Items[prop].ToString());

            if (!errors.IsNullOrEmpty())
                throw new MilvaUserFriendlyException(string.Join("~", errors), MilvaException.Validation);
        }

        await next();
    }

    /// <summary>
    /// Gets properties.
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    private static List<string> GetProperties(string props)
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

        return prop.IsNullOrEmpty() ? null : [.. prop];
    }
}
