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

    /// <inheritdoc/>
    public ServiceLifetime Lifetime { get; set; }

    /// <inheritdoc/>
    public string Key { get; set; }

    /// <inheritdoc/>
    public CipherMode Cipher { get; set; } = CipherMode.CBC;

    /// <inheritdoc/>
    public PaddingMode Padding { get; set; } = PaddingMode.PKCS7;
}

/// <summary>
/// Cryptography options.
/// </summary>
public interface IMilvaCryptographyOptions
{
    /// <summary>
    /// Lifetime of the <see cref="Abstract.IMilvaCryptographyProvider"/> registration.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; }

    /// <summary>
    /// Encryption key, validated when the services are registered. Either <c>"base64:"</c> followed by the Base64 encoding of 32 random bytes
    /// (recommended for new deployments, used directly as the AES-256-GCM key; see <see cref="Concrete.MilvaCryptographyProvider.GenerateKey"/>),
    /// or a random secret of 16, 24 or 32 bytes (UTF-8) as used by existing deployments. Do not convert an existing key to the other form:
    /// values encrypted with the old key would no longer decrypt.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Block cipher mode of values encrypted by older versions (legacy AES-CBC format). Only used for decrypting them;
    /// new values are always encrypted with AES-256-GCM. Default is <see cref="CipherMode.CBC"/>.
    /// </summary>
    public CipherMode Cipher { get; set; }

    /// <summary>
    /// Padding mode of values encrypted by older versions (legacy AES-CBC format). Only used for decrypting them;
    /// new values are always encrypted with AES-256-GCM. Default is <see cref="PaddingMode.PKCS7"/>.
    /// </summary>
    public PaddingMode Padding { get; set; }
}
