using System.Globalization;
using System.Text;

namespace Milvasoft.Helpers.FileOperations
{
    /// <summary>
    /// Json operations config for json file operations.
    /// </summary>
    public interface IJsonOperationsConfig
    {
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
    public class JsonOperationsConfig : IJsonOperationsConfig
    {
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
}
