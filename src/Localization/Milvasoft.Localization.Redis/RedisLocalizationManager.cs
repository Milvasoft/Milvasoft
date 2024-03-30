using Milvasoft.Caching.Redis;
using Milvasoft.Caching.Redis.Options;
using Milvasoft.Core.Abstractions.Cache;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Types.Classes;
using System.Globalization;

namespace Milvasoft.Localization.Redis;

/// <summary>
/// Provides localization operations with rediswith <see cref="IStringLocalizer{TResource}"/>.
/// </summary>
/// <param name="cacheAccessor"></param>
/// <param name="redisCacheServiceOptions"></param>
/// <param name="localizationOptions"></param>
public class RedisLocalizationManager(ICacheAccessor<RedisAccessor> cacheAccessor, ICacheOptions<RedisCachingOptions> redisCacheServiceOptions, ILocalizationOptions localizationOptions) : ILocalizationManager
{
    private readonly RedisAccessor _cacheAccessor = (RedisAccessor)cacheAccessor;
    private readonly RedisCachingOptions _redisCacheServiceOptions = (RedisCachingOptions)redisCacheServiceOptions;
    private readonly RedisLocalizationOptions _localizationOptions = (RedisLocalizationOptions)localizationOptions;

    #region Get Methods

    /// <summary>
    /// Gets the string resource with the given key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>The string resource as a <see cref="LocalizedValue"/>.</returns>
    public virtual LocalizedValue this[string key]
    {
        get
        {
            var formattedKey = CheckAndFormatKey(key);

            var value = _cacheAccessor.Get(formattedKey);

            return new LocalizedValue(formattedKey, value, value != null, GetSearchLocation(formattedKey));
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
            var formattedKey = CheckAndFormatKey(key);

            var value = _cacheAccessor.Get(formattedKey);

            if (value != null)
            {
                value = string.Format(value, arguments);
            }

            return new LocalizedValue(formattedKey, value, value != null, GetSearchLocation(formattedKey));
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
        var server = _cacheAccessor.GetServer(_redisCacheServiceOptions.ConfigurationOptions.EndPoints[0]);

        var keys = server.Keys().Select(k => k.ToString());
        var values = _cacheAccessor.Get(keys);

        return values.Select(v => new LocalizedValue(v, v, v.HasValue, GetSearchLocation("keys[]")));
    }

    #endregion

    #region Update Methods

    /// <summary>
    /// Sets the given <paramref name="value"/> with given <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetAsync(string key, string value)
    {
        var formattedKey = CheckAndFormatKey(key);

        await _cacheAccessor.SetAsync(formattedKey, value);
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
        var formattedKey = CheckAndFormatKey(key);

        _cacheAccessor.Set(formattedKey, value);
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
        var formattedKey = CheckAndFormatKey(key);

        await _cacheAccessor.RemoveAsync(formattedKey);
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
        var formattedKey = CheckAndFormatKey(key);

        _cacheAccessor.Remove(formattedKey);
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
    /// Gets search location for redis.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string GetSearchLocation(string key) => $"{_redisCacheServiceOptions.ConfigurationOptions.EndPoints[0]}:{key}";

    /// <summary>
    /// Checks the key is null or empty, if key is valid; formats key with <see cref="RedisLocalizationOptions.KeyFormatMethod"/> and returns it.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private string CheckAndFormatKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        key = _localizationOptions.KeyFormatMethod.Invoke(key, CultureInfo.CurrentCulture.Name);

        return key;
    }
}
