using System.Globalization;

namespace Milvasoft.Localization;

/// <summary>
/// Switches the current culture to a specific culture then return to the original culture and dispose.
/// </summary>
/// <remarks>
/// Only the culture of the current execution context (<see cref="CultureInfo.CurrentCulture"/> and <see cref="CultureInfo.CurrentUICulture"/>) is changed;
/// process wide defaults (<see cref="CultureInfo.DefaultThreadCurrentCulture"/>) are never touched, so other requests are not affected.
/// Both cultures are restored to their original values on dispose. Use it with <c>using</c>.
/// </remarks>
public sealed class CultureSwitcher : IDisposable
{
    private readonly CultureInfo _originalCulture;
    private readonly CultureInfo _originalUICulture;

    /// <summary>
    /// Since Localizer.WithCultre has became obsolete,
    /// this is a solution to retrive a string from specific culture resource file
    /// </summary>
    public CultureSwitcher(string culture)
    {
        _originalCulture = CultureInfo.CurrentCulture;
        _originalUICulture = CultureInfo.CurrentUICulture;

        if (string.IsNullOrEmpty(culture))
            return;

        var cultureInfo = CultureInfo.GetCultureInfo(culture);

        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
    }

    /// <summary>
    /// Restores the original cultures.
    /// </summary>
    public void Dispose()
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUICulture;
    }
}
