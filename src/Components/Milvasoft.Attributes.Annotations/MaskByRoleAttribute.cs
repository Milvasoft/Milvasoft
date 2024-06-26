﻿namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// It is used to mask role-related properties. It is used only in string data type.
/// </summary>
/// <param name="roles"></param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class MaskByRoleAttribute(params string[] roles) : Attribute
{
    /// <summary>
    /// Allowed roles.
    /// </summary>
    public string[] Roles { get; set; } = roles;
}
