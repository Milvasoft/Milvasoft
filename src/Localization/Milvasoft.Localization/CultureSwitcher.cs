using System.Globalization;

namespace Milvasoft.Localization;

/// <summary>
/// Switches the current culture to a specific culture then return to the original culture and dispose.
/// </summary>
public sealed class CultureSwitcher : IDisposable
{
    private readonly CultureInfo _originalCulture;

    /// <summary>
    /// Since Localizer.WithCultre has became obsolete, 
    /// this is a solution to retrive a string from specific culture resource file
    /// </summary>
    public CultureSwitcher(string culture)
    {
        if (!Equals(CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture))
            throw new InvalidOperationException("Different CurrentCulture and CurrentUICulture culture is not supported.");

        _originalCulture = CultureInfo.CurrentCulture;

        var cultureInfo = string.IsNullOrEmpty(culture) ? CultureInfo.CurrentCulture : new CultureInfo(culture);

        SetCulture(cultureInfo);
    }

    private static void SetCulture(CultureInfo cultureInfo)
    {
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
    }

    /// <summary>
    /// dispose...
    /// </summary>
    public void Dispose() => SetCulture(_originalCulture);
}