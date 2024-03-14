namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Determines whether localization is done or not.
/// </summary>
/// <param name="name"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, AllowMultiple = false)]
public class TranslateAttribute : Attribute
{
    public string Key { get; set; }

    public TranslateAttribute()
    {
    }

    public TranslateAttribute(string key)
    {
        Key = key;
    }
}
