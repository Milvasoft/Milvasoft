using Microsoft.Extensions.Caching.Memory;
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
    private readonly MemoryCacheEntryOptions _entryOps = localizationOptions.MemoryCacheEntryOptions ?? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = DefaultExpiration };

    /// <summary>
    /// Expiration of cached values when <see cref="ILocalizationOptions.MemoryCacheEntryOptions"/> is not set, so changed resources are picked up. Default is 10 minutes.
    /// </summary>
    public static TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Expiration of negative (missing key) cache entries. Default is 1 minute.
    /// </summary>
    public static TimeSpan MissingKeyExpiration { get; set; } = TimeSpan.FromMinutes(1);

    private const string _missingKeyFormat = "_MLN_{0}_{1}";

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
    public LocalizedValue Get(string key)
    {
        if (!_localizationOptions.UseInMemoryCache)
            return new LocalizedValue(key, null);

        var formattedKey = CreateFormattedKey(key);

        var value = _cache?.Get(formattedKey);

        return new LocalizedValue(key, value?.ToString(), value != null, $"loccache:{formattedKey}");
    }

    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="arguments"></param>
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

        return new LocalizedValue(key, value?.ToString(), value != null, $"loccache:{formattedKey}");
    }

    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="culture"></param>
    public LocalizedValue Get(string key, string culture)
    {
        if (!_localizationOptions.UseInMemoryCache)
            return new LocalizedValue(key, null);

        using var cultureSwitcher = new CultureSwitcher(culture);

        return Get(key);
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
            _cache.Remove(string.Format(_missingKeyFormat, CultureInfo.CurrentUICulture.Name, key));
        }
    }

    /// <inheritdoc/>
    public void SetMissing(string key)
    {
        if (_localizationOptions.UseInMemoryCache)
            _cache.Set(string.Format(_missingKeyFormat, CultureInfo.CurrentUICulture.Name, key), true, MissingKeyExpiration);
    }

    /// <inheritdoc/>
    public bool IsMissing(string key)
        => _localizationOptions.UseInMemoryCache && _cache.TryGetValue(string.Format(_missingKeyFormat, CultureInfo.CurrentUICulture.Name, key), out _);

    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="culture"></param>
    public void Set(string key, string value, string culture)
    {
        using var cultureSwitcher = new CultureSwitcher(culture);

        Set(key, value);
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
            _cache.Remove(string.Format(_missingKeyFormat, CultureInfo.CurrentUICulture.Name, key));
        }
    }

    /// <summary>
    /// Remove entry from cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="culture"></param>
    public void Remove(string key, string culture)
    {
        using var cultureSwitcher = new CultureSwitcher(culture);

        Remove(key);
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

    /// <summary>
    /// Localization depends on the UI culture (the language), not on the formatting culture.
    /// </summary>
    private string CreateFormattedKey(string key) => string.Format(_keyFormat, CultureInfo.CurrentUICulture.Name, key);
}