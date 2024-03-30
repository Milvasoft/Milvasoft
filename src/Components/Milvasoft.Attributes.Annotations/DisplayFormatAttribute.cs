namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// It is used to determine the format of the data returned to the table to be displayed in the frontend.
/// </summary>
/// <example></example>
/// <param name="format">Format example "{{Amount}} {{Currency}}"</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class DisplayFormatAttribute(string format) : Attribute
{
    public string Format { get; set; } = format;
}
