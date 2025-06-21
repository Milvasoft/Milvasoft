using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Storage.Providers.LocalFile;
using System.Text;

namespace Milvasoft.Storage.Models;

/// <summary>
/// Storage provider options.
/// </summary>
public class StorageProviderOptions : IMilvaOptions
{
    /// <summary>
    /// Storage provider configuration section name.
    /// </summary>
    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Storage";

    /// <summary>
    /// Base url or path for file access. 
    /// </summary>
    public string AccessUrl { get; set; }

    /// <summary>
    /// Base CDN url for file access. 
    /// </summary>
    public string CdnAccessUrl { get; set; }

    /// <summary>
    /// Encryption key as byte array for obscure.
    /// </summary>
    public string EncryptionKeySecret { get; set; }

    /// <summary>
    /// Encryption key as byte array.
    /// </summary>
    public byte[] EncryptionKey { get => Encoding.UTF8.GetBytes(EncryptionKeySecret); }

    /// <summary>
    /// Local file configuration.
    /// </summary>
    public LocalFileConfiguration LocalFile { get; set; }

    /// <summary>
    /// Aws s3 configuration.
    /// </summary>
    public AwsS3Configuration AwsS3 { get; set; }

    /// <summary>
    /// Azure blob storage configuration.
    /// </summary>
    public AzureAbsConfiguration AzureBlob { get; set; }
}
