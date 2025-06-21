namespace Milvasoft.Storage.Models;

/// <summary>
/// Azure blob storage provider configuration.
/// </summary>
public class AzureAbsConfiguration
{
    /// <summary>
    /// Azure storage connection string.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Container name.
    /// </summary>
    public string ContainerName { get; set; }

    /// <summary>
    /// Url for file access. 
    /// </summary>
    public string AccessUrl { get; set; }
}
