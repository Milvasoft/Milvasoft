using Milvasoft.Types.Classes;

namespace Milvasoft.Localization;

/// <summary>
/// Provides an IMemoryCache service to save localized values in a memory cache
/// </summary>
public interface ILocalizationMemoryCache
{
    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    public LocalizedValue Get(string key);

    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="arguments"></param>
    public LocalizedValue Get(string key, params object[] arguments);

    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="culture"></param>
    public LocalizedValue Get(string key, string culture);

    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Set(string key, string value);

    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="culture"></param>
    public void Set(string key, string value, string culture);

    /// <summary>
    /// Remove entry from cache
    /// </summary>
    /// <param name="key"></param>
    public void Remove(string key);

    /// <summary>
    /// Remove entry from cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="culture"></param>
    public void Remove(string key, string culture);

    /// <summary>
    /// Get a cached item from the memory cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValue(string key, out string value);

    /// <summary>
    /// Remembers for a short time that <paramref name="key"/> has no resource, so it is not looked up again on every call.
    /// </summary>
    /// <param name="key"></param>
    public void SetMissing(string key) { }

    /// <summary>
    /// Whether <paramref name="key"/> is remembered as missing.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsMissing(string key) => false;
}