namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// It is used to mask properties. It is used only in string data type.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class UpdatableIgnoreAttribute : Attribute
{
}
