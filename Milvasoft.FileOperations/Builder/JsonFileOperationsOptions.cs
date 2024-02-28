using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Extensions;
using System.Globalization;
using System.Text;

namespace Milvasoft.FileOperations.Builder;

/// <summary>
/// Json operations config for json file operations.
/// </summary>
public interface IJsonFileOperationOptions : IMilvaOptions
{
    /// <summary>
    /// Gets or sets base path. If sets combines all file paths with this.
    /// </summary>
    ServiceLifetime Lifetime { get; set; }

    /// <summary>
    /// Gets or sets base path. If sets combines all file paths with this.
    /// </summary>
    string BasePath { get; set; }

    /// <summary>
    /// Gets or sets encryption key.
    /// </summary>
    string EncryptionKey { get; set; }

    /// <summary>
    /// Culture info to be used when access file.
    /// </summary>
    CultureInfo CultureInfo { get; set; }

    /// <summary>
    /// Encoding to be used when access file.
    /// </summary>
    Encoding Encoding { get; set; }
}

/// <summary>
/// Json operations config for json file operations.
/// </summary>
public class JsonFileOperationsOptions : IJsonFileOperationOptions
{
    /// <summary>
    /// Configuration file section path.
    /// </summary>
    public static string SectionName { get; } = $"{MilvaOptionsExtensions.ParentSectionName}:FileOperations:Json";

    /// <summary>
    /// Gets or sets base path. If sets combines all file paths with this.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Singleton;

    /// <summary>
    /// Gets or sets base path. If sets combines all file paths with this.
    /// </summary>
    public string BasePath { get; set; }

    /// <summary>
    /// Gets or sets encryption key.
    /// </summary>
    public string EncryptionKey { get; set; }

    /// <summary>
    /// Culture info to be used when access file. Default is en-US.
    /// </summary>
    public CultureInfo CultureInfo { get; set; } = new CultureInfo("en-US");

    /// <summary>
    /// Encoding to be used when access file. Default is UTF8.
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;
}
