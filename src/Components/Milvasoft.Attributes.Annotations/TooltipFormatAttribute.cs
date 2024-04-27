namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// It is used to determine the tooltip format of the data returned to the table to be displayed on the front end.
/// </summary>
/// <example></example>
/// <param name="format">Format example "{{Currency}}"</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class TooltipFormatAttribute(string format) : Attribute
{
    /// <summary>
    /// Tooltip format.
    /// </summary>
    public string Format { get; set; } = format;
}
