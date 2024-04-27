using Milvasoft.Core.Utils.Constants;
using Milvasoft.Localization.Builder;

namespace Milvasoft.Localization.Resx;

/// <summary>
/// Resx manager spesific options.
/// </summary>
public class ResxLocalizationOptions : LocalizationOptions
{
    /// <inheritdoc/>
    public new static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Localization:Resx";

    /// <summary>
    /// Key format.
    /// 
    /// {0}: key
    /// 
    /// full key sample : Success
    /// </summary>
    public new string KeyFormat { get; set; } = "{0}";

    /// <summary>
    /// Resource relative path of resx files for .net localization infrastructure.
    /// 
    /// <para> etc. Path.Combine("LocalizationResources", "Resources") </para>
    /// </summary>
    public string ResourcesPath { get; set; }

    /// <summary>
    /// Full relative path of resource folder path which contains resx files.
    /// 
    /// <para> etc. Path.Combine(Environment.CurrentDirectory, "LocalizationResources", "Resources") </para>
    /// </summary>
    public string ResourcesFolderPath { get; set; }
}
