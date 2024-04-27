namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// It is used to hide authorization based property or column.
/// </summary>
/// <param name="roles">Gizlenecek roller</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class HideByRoleAttribute(params string[] roles) : Attribute
{
    /// <summary>
    /// Allowed roles.
    /// </summary>
    public string[] Roles { get; set; } = roles;
}
