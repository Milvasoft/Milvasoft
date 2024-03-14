using Microsoft.Extensions.Caching.Memory;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Types.Classes;
using System.Globalization;

namespace Milvasoft.Localization;

/// <summary>
/// Provides an IMemoryCache service to save localized values in a memory cache
/// </summary>
public class LocalizationMemoryCache(IMemoryCache cache, ILocalizationOptions localizationOptions) : ILocalizationMemoryCache
{
    private readonly IMemoryCache _cache = cache;
    private readonly ILocalizationOptions _localizationOptions = localizationOptions;
    private readonly MemoryCacheEntryOptions _entryOps = localizationOptions.MemoryCacheEntryOptions;

    /// <summary>
    /// _ML_ : Milva Localizer
    /// <para>{0}: culture name</para>
    /// <para>{1}: key</para>
    /// 
    /// full key sample :  _ML_en-US_Success
    /// </summary>
    private readonly string _keyFormat = "_ML_{0}_{1}";

    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public LocalizedValue Get(string key)
    {
        if (!_localizationOptions.UseInMemoryCache)
            return new LocalizedValue(key, null);

        var formattedKey = CreateFormattedKey(key);

        var value = _cache?.Get(formattedKey);

        return new LocalizedValue(formattedKey, value?.ToString(), value != null, $"loccache:{formattedKey}");
    }

    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public LocalizedValue Get(string key, params object[] arguments)
    {
        if (!_localizationOptions.UseInMemoryCache)
            return new LocalizedValue(key, null);

        var formattedKey = CreateFormattedKey(key);

        var value = _cache.Get(formattedKey);

        if (value != null)
        {
            value = string.Format(value.ToString(), arguments);
        }

        return new LocalizedValue(formattedKey, value?.ToString(), value != null, $"loccache:{formattedKey}");
    }

    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public LocalizedValue Get(string key, string culture)
    {
        if (!_localizationOptions.UseInMemoryCache)
            return new LocalizedValue(key, null);

        var cultureSwitcher = new CultureSwitcher(culture);

        var localizedValue = Get(key);

        cultureSwitcher.Dispose();

        return localizedValue;
    }

    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Set(string key, string value)
    {
        if (_localizationOptions.UseInMemoryCache)
        {
            var formattedKey = CreateFormattedKey(key);

            _cache.Set(formattedKey, value, _entryOps);
        }
    }

    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Set(string key, string value, string culture)
    {
        var cultureSwitcher = new CultureSwitcher(culture);

        Set(key, value);

        cultureSwitcher.Dispose();
    }

    /// <summary>
    /// Remove entry from cache
    /// </summary>
    /// <param name="key"></param>
    public void Remove(string key)
    {
        if (_localizationOptions.UseInMemoryCache)
        {
            var formattedKey = CreateFormattedKey(key);

            _cache.Remove(formattedKey);
        }
    }

    /// <summary>
    /// Remove entry from cache
    /// </summary>
    /// <param name="key"></param>
    public void Remove(string key, string culture)
    {
        var cultureSwitcher = new CultureSwitcher(culture);

        Remove(key);

        cultureSwitcher.Dispose();
    }

    /// <summary>
    /// Get a cached item from the memory cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValue(string key, out string value)
    {
        if (_localizationOptions.UseInMemoryCache)
        {
            var formattedKey = CreateFormattedKey(key);

            return _cache.TryGetValue(formattedKey, out value);
        }

        value = key;

        return false;
    }

    private string CreateFormattedKey(string key)
    {
        return string.Format(_keyFormat, CultureInfo.CurrentCulture.Name, key);
    }
}