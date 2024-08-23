namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Linked with metadata formatter.
/// </summary>
public interface ILinkedWithFormatter
{
    /// <summary>
    /// Formats the value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public object Format(object value);
}
