namespace Milvasoft.Storage.Providers.LocalFile;

/// <summary>
/// File configuration.
/// </summary>
public class LocalFileConfiguration
{
    /// <summary>
    /// Base path of the folder.
    /// </summary>
    public string BasePath { get; set; }

    /// <summary>
    /// Api route prefix for file serving.
    /// </summary>
    public string RoutePrefix { get; set; }
}