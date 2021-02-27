using System;

namespace Milvasoft.Helpers.DataAccess.Attributes
{
    /// <summary>
    /// Specifies that the value field value should be encrypted.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MilvaEncryptedAttribute : Attribute
    {
    }
}
