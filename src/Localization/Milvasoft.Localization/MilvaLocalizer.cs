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

            LocalizedValue localizedValue = null;

            //Get value from cache if cache is used.
            if (_localizationOptions.UseInMemoryCache)
                localizedValue = _cache.Get(key);

            if (localizedValue == null || !localizedValue.ResourceFound)
            {
                localizedValue = _localizationManager[key];

                if (_localizationOptions.UseInMemoryCache && localizedValue.ResourceFound)
                    _cache.Set(key, localizedValue.Value);
            }

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

            LocalizedValue localizedValue = null;

            //Get value from cache if cache is used.
            if (_localizationOptions.UseInMemoryCache)
                localizedValue = _cache.Get(key, arguments);

            if (localizedValue == null || !localizedValue.ResourceFound)
            {
                localizedValue = _localizationManager[key];

                if (_localizationOptions.UseInMemoryCache && localizedValue.ResourceFound)
                    _cache.Set(key, localizedValue.Value);
            }

            return localizedValue;
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
