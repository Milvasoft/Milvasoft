namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Determines whether localization is done or not.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, AllowMultiple = false)]
public class TranslateAttribute : Attribute
{
    /// <summary>
    /// Resource key.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Initializes new instance.
    /// </summary>
    public TranslateAttribute()
    {
    }

    /// <summary>
    /// Initializes new instance with <paramref name="key"/>
    /// </summary>
    /// <param name="key"></param>
    public TranslateAttribute(string key)
    {
        Key = key;
    }
}
