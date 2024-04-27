using System.Diagnostics.CodeAnalysis;

namespace Milvasoft.Types.Classes;

/// <summary>
/// A locale specific string value. 
/// </summary>
public class LocalizedValue
{
    /// <summary>
    /// The name of the string in the resource it was loaded from.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The actual string value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Whether the string was not found in a resource. If <c>false</c>, an alternate string value was used.
    /// </summary>
    public bool ResourceFound { get; }

    /// <summary>
    /// The location which was searched for a localization value.
    /// </summary>
    public string SearchedLocation { get; }

    /// <summary>
    /// Creates a new <see cref="LocalizedValue"/>.
    /// </summary>
    /// <param name="key">The name of the string in the resource it was loaded from.</param>
    /// <param name="value">The actual string.</param>
    public LocalizedValue(string key, string value) : this(key, value, resourceFound: value != null)
    {
    }

    /// <summary>
    /// Creates a new <see cref="LocalizedValue"/>.
    /// </summary>
    /// <param name="key">The name of the string in the resource it was loaded from.</param>
    /// <param name="value">The actual string.</param>
    /// <param name="resourceFound">Whether the string was not found in a resource. Set this to <c>false</c> to indicate an alternate string value was used.</param>
    public LocalizedValue(string key, string value, bool resourceFound) : this(key, value, resourceFound, searchedLocation: $"_{key}_")
    {
    }

    /// <summary>
    /// Creates a new <see cref="LocalizedValue"/>.
    /// </summary>
    /// <param name="key">The name of the string in the resource it was loaded from.</param>
    /// <param name="value">The actual string.</param>
    /// <param name="resourceFound">Whether the string was not found in a resource. Set this to <c>false</c> to indicate an alternate string value was used.</param>
    /// <param name="searchedLocation">The location which was searched for a localization value.</param>
    public LocalizedValue(string key, string value, bool resourceFound, string searchedLocation)
    {
        ArgumentNullException.ThrowIfNull(key);

        Key = key;
        Value = value;
        ResourceFound = resourceFound;
        SearchedLocation = searchedLocation;
    }

    /// <summary>
    /// Implicitly converts the <see cref="LocalizedValue"/> to a <see cref="string"/>.
    /// </summary>
    /// <param name="localizedValue">The string to be implicitly converted.</param>
    [return: NotNullIfNotNull(nameof(LocalizedValue))]
    public static implicit operator string(LocalizedValue localizedValue) => localizedValue?.Value;

    /// <summary>
    /// Returns the actual string.
    /// </summary>
    /// <returns>The actual string.</returns>
    public override string ToString() => Value;
}
