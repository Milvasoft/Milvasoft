using Milvasoft.Types;

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
    /// <param name="value"></param>
    public LocalizedValue Get(string key);

    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public LocalizedValue Get(string key, params object[] arguments);

    /// <summary>
    /// Add new entry to the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
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
    public void Remove(string key, string culture);

    /// <summary>
    /// Get a cached item from the memory cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValue(string key, out string value);
}