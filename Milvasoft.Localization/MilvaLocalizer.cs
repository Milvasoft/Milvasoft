using Milvasoft.Core.Abstractions;
using Milvasoft.Types;

namespace Milvasoft.Localization;

/// <summary>
/// Represents a service that provides localized strings.
/// </summary>
/// <remarks>
/// Creates a new <see cref="MilvaLocalizer"/>.
/// </remarks>
/// <param name="manager">The <see cref="ILocalizationManager"/> to use.</param>
public class MilvaLocalizer(ILocalizationManager localizationManager) : IMilvaLocalizer
{
    private readonly ILocalizationManager _localizationManager = localizationManager;

    /// <summary>
    /// Gets the string resource with the given key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>The string resource as a <see cref="LocalizedValue"/>.</returns>
    public LocalizedValue this[string key]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(key);

            return _localizationManager[key];
        }
    }

    /// <summary>
    /// Gets the string resource with the given key and formatted with the supplied arguments.
    /// </summary>
    /// <param name="key">The key of the string resource.</param>
    /// <param name="arguments">The values to format the string with.</param>
    /// <returns>The formatted string resource as a <see cref="LocalizedValue"/>.</returns>
    public LocalizedValue this[string key, params object[] arguments]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(key);

            return _localizationManager[key, arguments];
        }
    }

    /// <summary>
    /// Gets all string resources.
    /// </summary>
    /// <param name="includeParentCultures">
    /// A <see cref="System.Boolean"/> indicating whether to include strings from parent cultures.
    /// </param>
    /// <returns>The strings.</returns>
    public IEnumerable<LocalizedValue> GetAllStrings(bool includeParentCultures) =>
        _localizationManager.GetAllStrings(includeParentCultures);

    /// <summary>
    /// Gets string resource with given <paramref name="key"/> with given <paramref name="culture"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="culture"></param>
    /// <returns>The string.</returns>
    public LocalizedValue GetWithCulture(string key, string culture)
    {
        var cultureSwitcher = new CultureSwitcher(culture);

        var value = this[key];

        cultureSwitcher.Dispose();

        return value;
    }

    /// <summary>
    /// Gets string resource with given <paramref name="key"/> with given <paramref name="culture"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="culture"></param>
    /// <param name="arguments"></param>
    /// <returns>The string.</returns>
    public LocalizedValue GetWithCulture(string key, string culture, params object[] arguments)
    {
        var cultureSwitcher = new CultureSwitcher(culture);

        var value = this[key, arguments];

        cultureSwitcher.Dispose();

        return value;
    }
}
