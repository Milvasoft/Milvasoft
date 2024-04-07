using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Annotations;

namespace Milvasoft.Interception.Interceptors.Response;

/// <summary>
/// Represents the options for <see cref="Components.Rest.MilvaResponse.Response"/> interception.
/// </summary>
public class ResponseInterceptionOptions : IResponseInterceptionOptions
{
    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Interception:Response";

    public ServiceLifetime InterceptorLifetime { get; set; } = ServiceLifetime.Scoped;

    public bool TranslateResultMessages { get; set; } = true;

    public bool TranslateMetadata { get; set; } = true;

    public Func<IServiceProvider, HideByRoleAttribute, bool> HideByRoleFunc { get; set; }

    public Func<string, IMilvaLocalizer, Type, string, string> ApplyLocalizationFunc { get; set; }
}

/// <summary>
/// Represents the options for <see cref="Components.Rest.MilvaResponse.Response"/> interception.
/// </summary>
public interface IResponseInterceptionOptions : IMilvaOptions
{
    /// <summary>
    /// Response interceptor lifetime.
    /// </summary>
    public ServiceLifetime InterceptorLifetime { get; set; }

    /// <summary>
    /// If it is true <see cref="ResponseInterceptor"/> translate IResponseTyped response's result messages. Default is true.
    /// </summary>
    public bool TranslateResultMessages { get; set; }

    /// <summary>
    /// If it is true <see cref="ResponseInterceptor"/> translate IResponseTyped response's metadata prop name. Default is true.
    /// </summary>
    public bool TranslateMetadata { get; set; }

    /// <summary>
    /// This property represents a function that takes an array of role names as input and determines whether hiding should be performed based on role-based authorization checks.
    /// The function evaluates these roles to decide whether the item should be hidden. 
    /// If the item should be hidden, the function returns true; otherwise, it returns false. This method can be utilized to provide dynamic access control based on user roles.
    /// 
    /// <remarks>
    /// Function parameter contains not allowed roles.
    /// </remarks>
    /// </summary>
    public Func<IServiceProvider, HideByRoleAttribute, bool> HideByRoleFunc { get; set; }

    /// <summary>
    /// It determines with which key pattern the data will be received from the ImilvaLocalizer in the Interceptor. 
    /// If it is not sent, localization is tried with the default pattern. The default pattern is <see cref="ResponseInterceptor.ApplyLocalization(string, IMilvaLocalizer, Type, string)"/>
    /// </summary>
    public Func<string, IMilvaLocalizer, Type, string, string> ApplyLocalizationFunc { get; set; }
}