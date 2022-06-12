using System;

namespace Milvasoft.DataAccess.EfCore.Attributes;

/// <summary>
/// Attribute for adding precision in context level.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class MilvaPrecisionAttribute : Attribute
{
    /// <summary>
    /// Precision of tagged property.
    /// </summary>
    public int Precision { get; private set; }

    /// <summary>
    /// Scale of tagged property.
    /// </summary>
    public int Scale { get; private set; }

    /// <summary>
    /// Specifies this property has a precision upon creation.
    /// </summary>
    /// <param name="precision"></param>
    /// <param name="scale"></param>
    public MilvaPrecisionAttribute(int precision, int scale)
    {
        Scale = scale;
        Precision = precision;
    }
}
