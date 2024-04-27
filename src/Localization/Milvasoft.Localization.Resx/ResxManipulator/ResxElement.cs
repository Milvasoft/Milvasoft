using System.Xml.Linq;

namespace Milvasoft.Localization.Resx.ResxManipulator;

/// <summary>
/// Model for exporting DB entities to Resx file
/// </summary>
public sealed class ResxElement : IEquatable<ResxElement>
{
    /// <summary>
    /// Initialize a new instance of ResxElement
    /// </summary>
    public ResxElement()
    {

    }

    /// <summary>
    /// Initializes a new instance of ResxElement from XElement
    /// </summary>
    /// <param name="element"></param>
    public ResxElement(XElement element)
    {
        Key = element.Attribute("name").Value;
        Value = element.Element("value").Value;
        Comment = element.Element("comment").Value;
    }

    /// <summary>
    /// Resource key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Resource value
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Comment...
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// Converts current ResxElement to XElement (resource file element)
    /// </summary>
    public XElement ToXElement() => new("data",
                                        new XAttribute("name", Key),
                                        new XAttribute($"{XNamespace.Xml + "space"}", "preserve"),
                                        new XElement("value", Value),
                                        new XElement("comment", Comment));

    /// <summary>
    /// Determine if two elements are equal
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(ResxElement other) => Key.Equals(other.Key, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object obj) => Equals(obj as ResxElement);

    /// <inheritdoc/>
    public override int GetHashCode() => throw new NotImplementedException();
}
