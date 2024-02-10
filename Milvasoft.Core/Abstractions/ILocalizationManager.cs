using Milvasoft.Types.Classes;

namespace Milvasoft.Core.Abstractions;

/// <summary>
/// It is the manager that provides access to localization resources. You can use this interface for creating new custom managers. Or you can use ready-made milva managers. 
/// <para> For ready made managers see here : https://github.com/Milvasoft/Milvasoft/tree/master/Localization/Milvasoft.Localization.Redis </para> 
/// </summary>
public interface ILocalizationManager
{
    #region Get Methods

    /// <summary>
    /// Gets the string resource with the given key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>The string resource as a <see cref="LocalizedValue"/>.</returns>
    LocalizedValue this[string key] { get; }

    /// <summary>
    /// Gets the string resource with the given key and formatted with the supplied arguments.
    /// </summary>
    /// <param name="key">The key of the string resource.</param>
    /// <param name="arguments">The values to format the string with.</param>
    /// <returns>The formatted string resource as a <see cref="LocalizedValue"/>.</returns>
    LocalizedValue this[string key, params object[] arguments] { get; }

    /// <summary>
    /// Gets all string resources.
    /// </summary>
    /// <param name="includeParentCultures">
    /// A <see cref="bool"/> indicating whether to include strings from parent cultures.
    /// </param>
    /// <returns>The strings.</returns>
    public IEnumerable<LocalizedValue> GetAllStrings(bool includeParentCultures);

    #endregion

    #region Update Methods

    /// <summary>
    /// Sets the given <paramref name="value"/> with given <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Set(string key, string value);

    /// <summary>
    /// Sets the given <paramref name="value"/> with given <paramref name="key"/> in given <paramref name="culture"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="culture"></param>
    public void Set(string key, string value, string culture);

    /// <summary>
    /// Removes value from resource with given <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    public void Remove(string key);

    /// <summary>
    /// Removes value from resource with given <paramref name="key"/> with given <paramref name="culture"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="culture"></param>
    public void Remove(string key, string culture);

    /// <summary>
    /// Sets the given <paramref name="value"/> with given <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Task SetAsync(string key, string value);

    /// <summary>
    /// Sets the given <paramref name="value"/> with given <paramref name="key"/> in given <paramref name="culture"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public Task SetAsync(string key, string value, string culture);

    /// <summary>
    /// Removes value from resource with given <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Task RemoveAsync(string key);

    /// <summary>
    /// Removes value from resource with given <paramref name="key"/> with given <paramref name="culture"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public Task RemoveAsync(string key, string culture);

    #endregion
}
