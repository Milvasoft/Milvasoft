using System;

namespace Milvasoft.Helpers.DataAccess.EfCore.Attributes;

/// <summary>
/// Attribute for adding default value in context level.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class MilvaDefaultValueAttribute : Attribute
{
    /// <summary>
    /// Specifies this property has a default value upon creation.
    /// </summary>
    /// <param name="defaultValue">The default value of the property.</param>
    public MilvaDefaultValueAttribute(object defaultValue)
    {
        DefaultValue = defaultValue;
    }

    /// <summary>
    /// Default value of tagged property.
    /// </summary>
    public object DefaultValue { get; private set; }
}
