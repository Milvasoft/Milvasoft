namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Contains decimal precision and scale information. 
/// </summary>
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

    /// <summary>
    /// Precision of decimal.
    /// </summary>
    public DecimalPrecision DecimalPrecision { get; set; }

    /// <summary>
    /// Initializes new instance with default values.
    /// </summary>
    public DecimalPrecisionAttribute()
    {
        DecimalPrecision = new()
        {
            Scale = Scale,
            Precision = Precision,
        };
    }

    /// <summary>
    /// Initializes new instance with <paramref name="scale"/>.
    /// </summary>
    /// <param name="scale"></param>
    public DecimalPrecisionAttribute(byte scale)
    {
        Scale = scale;
        DecimalPrecision = new()
        {
            Scale = scale,
        };
    }

    /// <summary>
    /// Initializes new instance with <paramref name="precision"/> and <paramref name="scale"/>.
    /// </summary>
    /// <param name="precision"></param>
    /// <param name="scale"></param>
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

/// <summary>
/// Precision and scale.
/// </summary>
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
