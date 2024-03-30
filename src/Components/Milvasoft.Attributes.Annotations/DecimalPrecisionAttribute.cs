namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Contains decimal precision and scale information.
/// </summary>
/// <param name="precision"></param>
/// <param name="scale"></param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class DecimalPrecisionAttribute : Attribute
{
    /// <summary>
    /// Precision of decimal. Default is 18.
    /// </summary>
    public byte Precision { get; set; } = 18;

    /// <summary>
    /// Scale of decimal. Default is 2.
    /// </summary>
    public byte Scale { get; set; } = 2;

    public DecimalPrecision DecimalPrecision { get; set; }

    public DecimalPrecisionAttribute()
    {
        DecimalPrecision = new()
        {
            Scale = Scale,
            Precision = Precision,
        };
    }

    public DecimalPrecisionAttribute(byte scale)
    {
        Scale = scale;
        DecimalPrecision = new()
        {
            Scale = scale,
        };
    }

    public DecimalPrecisionAttribute(byte precision, byte scale)
    {
        Precision = precision;
        Scale = scale;
        DecimalPrecision = new()
        {
            Scale = scale,
            Precision = precision,
        };
    }
}

public class DecimalPrecision
{
    /// <summary>
    /// Precision of decimal. Default is 18.
    /// </summary>
    public byte Precision { get; set; } = 18;

    /// <summary>
    /// Scale of decimal. Default is 2.
    /// </summary>
    public byte Scale { get; set; } = 2;
}
