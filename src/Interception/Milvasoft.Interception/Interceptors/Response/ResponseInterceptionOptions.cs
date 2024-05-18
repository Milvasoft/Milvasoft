using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Interception.Builder;

namespace Milvasoft.Interception.Interceptors.Response;

/// <summary>
/// Represents the options for <see cref="Components.Rest.MilvaResponse.Response"/> interception.
/// </summary>
public class ResponseInterceptionOptions : IResponseInterceptionOptions
{
    /// <inheritdoc/>
    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Interception:Response";

    /// <inheritdoc/>
    public ServiceLifetime InterceptorLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <inheritdoc/>
    public bool ApplyMetadataRules { get; set; } = true;

    /// <inheritdoc/>
    public bool MetadataCreationEnabled { get; set; } = true;

    /// <inheritdoc/>
    public bool TranslateResultMessages { get; set; } = true;

    /// <inheritdoc/>
    public bool TranslateMetadata { get; set; } = true;

    /// <inheritdoc/>
    public Func<IServiceProvider, HideByRoleAttribute, bool> HideByRoleFunc { get; set; }

    /// <inheritdoc/>
    public Func<IServiceProvider, MaskByRoleAttribute, bool> MaskByRoleFunc { get; set; }

    /// <inheritdoc/>
    public Func<string, IMilvaLocalizer, Type, string, string> ApplyLocalizationFunc { get; set; }
}

/// <summary>
/// Represents the options for <see cref="Components.Rest.MilvaResponse.Response"/> interception.
/// </summary>
public interface IResponseInterceptionOptions : IInterceptionOptions
{
    /// <summary>
    /// If it is true <see cref="ResponseInterceptor"/> apply metadata rules to response data. Default is true.
    /// </summary>
    public bool ApplyMetadataRules { get; set; }

    /// <summary>
    /// If it is true <see cref="ResponseInterceptor"/> create metadata for response. Default is true.
    /// </summary>
    public bool MetadataCreationEnabled { get; set; }

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
    /// This property represents a function that takes an array of role names as input and determines whether masking should be performed based on role-based authorization checks.
    /// The function evaluates these roles to decide whether the item should be hidden. 
    /// If the item should be masked, the function returns true; otherwise, it returns false. This method can be utilized to provide dynamic access control based on user roles.
    /// 
    /// <remarks>
    /// Function parameter contains not allowed roles.
    /// </remarks>
    /// </summary>
    public Func<IServiceProvider, MaskByRoleAttribute, bool> MaskByRoleFunc { get; set; }

    /// <summary>
    /// It determines with which key pattern the data will be received from the ImilvaLocalizer in the Interceptor. 
    /// If it is not sent, localization is tried with the default pattern. The default pattern is <see cref="ResponseInterceptor.ApplyLocalization(string, IMilvaLocalizer, Type, string)"/>
    /// </summary>
    public Func<string, IMilvaLocalizer, Type, string, string> ApplyLocalizationFunc { get; set; }
}