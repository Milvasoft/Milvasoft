using Microsoft.Extensions.Localization;
using Milvasoft.Types;

namespace Milvasoft.Localization.Resx;

public class ResxLocalizationManager<TResource>(IStringLocalizer<TResource> stringLocalizer) : ILocalizationManager
{
    private readonly IStringLocalizer<TResource> _stringLocalizer = stringLocalizer;

    /// <summary>
    /// Gets the string resource with the given key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>The string resource as a <see cref="LocalizedValue"/>.</returns>
    public virtual LocalizedValue this[string key]
    {
        get
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var value = _stringLocalizer[key];

            return new LocalizedValue(key, value);
        }
    }

    /// <summary>
    /// Gets the string resource with the given key and formatted with the supplied arguments.
    /// </summary>
    /// <param name="key">The key of the string resource.</param>
    /// <param name="arguments">The values to format the string with.</param>
    /// <returns>The formatted string resource as a <see cref="LocalizedValue"/>.</returns>
    public virtual LocalizedValue this[string key, params object[] arguments]
    {
        get
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var value = _stringLocalizer[key, arguments];

            return new LocalizedValue(key, value);
        }
    }

    /// <summary>
    /// Gets all string resources.
    /// </summary>
    /// <param name="includeParentCultures">
    /// A <see cref="bool"/> indicating whether to include strings from parent cultures.
    /// </param>
    /// <returns>The strings.</returns>
    public IEnumerable<LocalizedValue> GetAllStrings(bool includeParentCultures)
    {
        var values = _stringLocalizer.GetAllStrings(includeParentCultures);

        return values.Select(value => new LocalizedValue(value, value));
    }
}
