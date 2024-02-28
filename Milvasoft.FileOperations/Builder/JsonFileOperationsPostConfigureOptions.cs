using System.Globalization;
using System.Text;

namespace Milvasoft.FileOperations.Builder;

/// <summary>
/// Json operations config for json file operations.
/// </summary>
public class JsonFileOperationsPostConfigureOptions
{
    /// <summary>
    /// Gets or sets base path. If sets combines all file paths with this.
    /// </summary>
    public string BasePath { get; set; }

    /// <summary>
    /// Culture info to be used when access file. Default is en-US.
    /// </summary>
    public CultureInfo CultureInfo { get; set; }

    /// <summary>
    /// Encoding to be used when access file. Default is UTF8.
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;
}
