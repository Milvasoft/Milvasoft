namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// It is used to mask properties. It is used only in string data type.
/// </summary>
/// <param name="roles"></param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class MaskAttribute() : Attribute
{
}
