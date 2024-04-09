using Fody;
using Microsoft.Extensions.Localization;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Localization.Resx.ResxManipulator;
using Milvasoft.Types.Classes;
using System.Globalization;

namespace Milvasoft.Localization.Resx;

/// <summary>
/// Provides localization operations with resx files with <see cref="IStringLocalizer{TResource}"/>.
/// </summary>
/// <typeparam name="TResource"></typeparam>
/// <param name="stringLocalizer"></param>
/// <param name="localizationOptions"></param>
[ConfigureAwait(false)]
public class ResxLocalizationManager<TResource>(IStringLocalizer<TResource> stringLocalizer, ILocalizationOptions localizationOptions) : ILocalizationManager
{
    private readonly IStringLocalizer<TResource> _stringLocalizer = stringLocalizer;
    private readonly ResxLocalizationOptions _localizationOptions = (ResxLocalizationOptions)localizationOptions;

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

            key = _localizationOptions.KeyFormatMethod.Invoke(key);

            var value = _stringLocalizer[key];

            return new LocalizedValue(key, value, !value.ResourceNotFound, value.SearchedLocation);
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

            key = _localizationOptions.KeyFormatMethod.Invoke(key);

            var value = _stringLocalizer[key, arguments];

            return new LocalizedValue(key, value, !value.ResourceNotFound, value.SearchedLocation);
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

        return values.Select(value => new LocalizedValue(value.Name, value.Value, !value.ResourceNotFound, value.SearchedLocation));
    }

    #region Update Methods

    /// <summary>
    /// Sets the given <paramref name="value"/> with given <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetAsync(string key, string value)
    {
        ThrowIfKeyIsInvalid(key);

        var resxWriter = new ResxWriter(typeof(TResource), CultureInfo.CurrentCulture.Name, _localizationOptions.ResourcesFolderPath, null);

        await resxWriter.AddAsync(new ResxElement
        {
            Key = key,
            Value = value
        }, true);
    }

    /// <summary>
    /// Sets the given <paramref name="value"/> with given <paramref name="key"/> in given <paramref name="culture"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public async Task SetAsync(string key, string value, string culture)
    {
        var cultureSwitcher = new CultureSwitcher(culture);

        await SetAsync(key, value);

        cultureSwitcher.Dispose();
    }

    /// <summary>
    /// Sets the given <paramref name="value"/> with given <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Set(string key, string value)
    {
        ThrowIfKeyIsInvalid(key);

        SetAsync(key, value).Wait();
    }

    /// <summary>
    /// Sets the given <paramref name="value"/> with given <paramref name="key"/> in given <paramref name="culture"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="culture"></param>
    public void Set(string key, string value, string culture)
    {
        var cultureSwitcher = new CultureSwitcher(culture);

        Set(key, value);

        cultureSwitcher.Dispose();
    }

    /// <summary>
    /// Removes value from resource with given <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task RemoveAsync(string key)
    {
        ThrowIfKeyIsInvalid(key);

        var resxWriter = new ResxWriter(typeof(TResource), CultureInfo.CurrentCulture.Name, _localizationOptions.ResourcesFolderPath, null);

        await resxWriter.RemoveAsync(new ResxElement
        {
            Key = key
        });
    }

    /// <summary>
    /// Removes value from resource with given <paramref name="key"/> with given <paramref name="culture"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public async Task RemoveAsync(string key, string culture)
    {
        var cultureSwitcher = new CultureSwitcher(culture);

        await RemoveAsync(key);

        cultureSwitcher.Dispose();
    }

    /// <summary>
    /// Removes value from resource with given <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    public void Remove(string key)
    {
        ThrowIfKeyIsInvalid(key);

        RemoveAsync(key).Wait();
    }

    /// <summary>
    /// Removes value from resource with given <paramref name="key"/> with given <paramref name="culture"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="culture"></param>
    public void Remove(string key, string culture)
    {
        var cultureSwitcher = new CultureSwitcher(culture);

        Remove(key);

        cultureSwitcher.Dispose();
    }

    #endregion

    /// <summary>
    /// Checks the key is null or empty.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static void ThrowIfKeyIsInvalid(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
    }
}
