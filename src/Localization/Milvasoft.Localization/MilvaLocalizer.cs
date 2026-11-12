using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Types.Classes;

namespace Milvasoft.Localization;

/// <summary>
/// Represents a service that provides localized strings.
/// </summary>
/// <remarks>
/// Creates a new <see cref="MilvaLocalizer"/>.
/// </remarks>
public class MilvaLocalizer : IMilvaLocalizer
{
    private readonly ILocalizationManager _localizationManager;
    private readonly ILocalizationOptions _localizationOptions;
    private readonly ILocalizationMemoryCache _cache;

    /// <summary>
    /// Initializes new instance with <paramref name="serviceProvider"/>
    /// </summary>
    /// <param name="serviceProvider"></param>
    public MilvaLocalizer(IServiceProvider serviceProvider)
    {
        _localizationManager = serviceProvider.GetRequiredService<ILocalizationManager>();
        _localizationOptions = serviceProvider.GetRequiredService<ILocalizationOptions>();

        if (_localizationOptions.UseInMemoryCache)
            _cache = serviceProvider.GetService<ILocalizationMemoryCache>();
    }

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

            if (_cache == null)
                return _localizationManager[key];

            //Get value from cache if cache is used.
            var localizedValue = _cache.Get(key);

            if (localizedValue?.ResourceFound == true)
                return localizedValue;

            // Known missing keys are not looked up again until the negative cache entry expires.
            if (_cache.IsMissing(key))
                return new LocalizedValue(key, key, resourceFound: false);

            localizedValue = _localizationManager[key];

            if (localizedValue.ResourceFound)
                _cache.Set(key, localizedValue.Value);
            else
                _cache.SetMissing(key);

            return localizedValue;
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

            // Without cache the manager formats the value (arguments were dropped before).
            if (_cache == null)
                return _localizationManager[key, arguments];

            //Get value from cache if cache is used.
            var localizedValue = _cache.Get(key, arguments);

            if (localizedValue?.ResourceFound == true)
                return localizedValue;

            if (_cache.IsMissing(key))
                return _localizationManager[key, arguments];

            var rawValue = _localizationManager[key];

            if (!rawValue.ResourceFound)
            {
                _cache.SetMissing(key);

                return _localizationManager[key, arguments];
            }

            _cache.Set(key, rawValue.Value);

            // Assign again for formatted values etc. bla bla {0} bla bla
            return _cache.Get(key, arguments);
        }
    }

    /// <summary>
    /// Gets all string resources.
    /// </summary>
    /// <param name="includeParentCultures">
    /// A <see cref="System.Boolean"/> indicating whether to include strings from parent cultures.
    /// </param>
    /// <returns>The strings.</returns>
    public IEnumerable<LocalizedValue> GetAllStrings(bool includeParentCultures) => _localizationManager.GetAllStrings(includeParentCultures);

    /// <summary>
    /// Gets string resource with given <paramref name="key"/> with given <paramref name="culture"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="culture"></param>
    /// <returns>The string.</returns>
    public LocalizedValue GetWithCulture(string key, string culture)
    {
        using var cultureSwitcher = new CultureSwitcher(culture);

        return this[key];
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
        using var cultureSwitcher = new CultureSwitcher(culture);

        return this[key, arguments];
    }
}
