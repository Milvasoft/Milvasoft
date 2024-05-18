using Milvasoft.Attributes.Annotations;

namespace Milvasoft.Interception.Interceptors.Response;

/// <summary>
/// If options are made from the configuration file, the class that allows options that cannot be made from the configuration file.
/// </summary>
public class ResponseInterceptionPostConfigureOptions
{
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
    /// It allows the values to be logged to be sent to the library, other than the values that the Interceptor logs by default.
    /// </summary>
    public Func<string, IMilvaLocalizer, Type, string, string> ApplyLocalizationFunc { get; set; }
}