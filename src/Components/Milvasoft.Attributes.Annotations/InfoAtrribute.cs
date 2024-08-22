namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// It is used to determine the column info.
/// </summary>
/// <example></example>
/// <param name="info">Format example "{{Currency}}"</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class InfoAttribute(string info) : Attribute
{
    /// <summary>
    /// Column or property information.
    /// </summary>
    public string Info { get; set; } = info;
}
