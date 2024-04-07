using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;

namespace Milvasoft.Cryptography.Builder;

/// <summary>
/// Cryptography options.
/// </summary>
public class MilvaCryptographyOptions : IMilvaCryptographyOptions
{
    /// <summary>
    /// Configuration file section path.
    /// </summary>
    public static string SectionName { get; } = $"Milvasoft:Cryptography";

    public ServiceLifetime Lifetime { get; set; }

    public string Key { get; set; }

    public CipherMode Cipher { get; set; } = CipherMode.CBC;

    public PaddingMode Padding { get; set; } = PaddingMode.PKCS7;
}

/// <summary>
/// Cryptography options.
/// </summary>
public interface IMilvaCryptographyOptions
{
    /// <summary>
    /// Gets or sets base path. If sets combines all file paths with this.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; }

    /// <summary>
    /// Encryption key.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Specifies the block cipher mode to use for encryption. Default is  Default is <see cref="CipherMode.CBC"/>
    /// </summary>
    public CipherMode Cipher { get; set; }

    /// <summary>
    /// Specifies the type of padding to apply when the message data block is shorter than the full number of bytes needed for a cryptographic operation.
    /// Default is <see cref="PaddingMode.PKCS7"/>
    /// </summary>
    public PaddingMode Padding { get; set; }
}
