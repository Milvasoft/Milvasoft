﻿using Milvasoft.Types.Classes;

namespace Milvasoft.Core.Abstractions.Localization;

/// <summary>
/// Represents a service that provides localized strings.
/// </summary>
public interface IMilvaLocalizer
{
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

    /// <summary>
    /// Gets string resource with given <paramref name="key"/> with given <paramref name="culture"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="culture"></param>
    /// <returns>The string.</returns>
    public LocalizedValue GetWithCulture(string key, string culture);

    /// <summary>
    /// Gets string resource with given <paramref name="key"/> with given <paramref name="culture"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="culture"></param>
    /// <param name="arguments"></param>
    /// <returns>The string.</returns>
    public LocalizedValue GetWithCulture(string key, string culture, params object[] arguments);

}
