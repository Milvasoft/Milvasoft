namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Specifies that the value field value should be encrypted.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class EncryptedAttribute : Attribute
{
}
