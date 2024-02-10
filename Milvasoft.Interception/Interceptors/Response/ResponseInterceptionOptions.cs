using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.Abstractions;

namespace Milvasoft.Interception.Interceptors.Logging;

public class ResponseInterceptionOptions : IResponseInterceptionOptions
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
    public Func<HideByRoleAttribute, bool> HideByRoleFunc { get; set; }

    /// <summary>
    /// It allows the values to be logged to be sent to the library, other than the values that the Interceptor logs by default.
    /// </summary>
    public Func<string, IMilvaLocalizer, Type, string, string> ApplyLocalizationFunc { get; set; }
}


public interface IResponseInterceptionOptions
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
    public Func<HideByRoleAttribute, bool> HideByRoleFunc { get; set; }

    /// <summary>
    /// It determines with which key pattern the data will be received from the ImilvaLocalizer in the Interceptor. 
    /// If it is not sent, localization is tried with the default pattern. The default pattern is <see cref="ResponseInterceptor.ApplyLocalization(string, IMilvaLocalizer, Type, string)"/>
    /// </summary>
    public Func<string, IMilvaLocalizer, Type, string, string> ApplyLocalizationFunc { get; set; }
}