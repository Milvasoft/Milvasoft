using Microsoft.AspNetCore.Http;
using Milvasoft.Storage.Models;
using System.Text;

namespace Milvasoft.Storage.Providers;

/// <summary>
/// Provides a base class for storage providers, offering common functionality such as URL combination and file path obscuration.
/// </summary>
/// <param name="storageProviderOptions"></param>
public abstract class StorageProviderBase(StorageProviderOptions storageProviderOptions)
{
    private readonly StorageProviderOptions _storageProviderOptions = storageProviderOptions;

    /// <summary>
    /// Shows how to upload a file from the local computer to an provider.
    /// </summary>
    /// <param name="filePath">The object to upload.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="file">The file to upload.</param>
    /// <returns>A boolean value indicating the success or failure of the upload procedure.</returns>
    public abstract Task<FileOperationResult> UploadAsync(IFormFile file, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads file to provider.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="filePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A boolean value indicating the success or failure of the upload procedure.</returns>
    public abstract Task<FileOperationResult> NoBufferUploadAsync(HttpContext context, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads file from provider.
    /// </summary>
    /// <param name="filePath">The name of the object to download.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A boolean value indicating the success or failure of the download process.</returns>
    public abstract Task<FileDownloadResult> DownloadAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies provider file from <paramref name="sourceFilePath"/> to <paramref name="destinationFilePath"/>.
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="destinationFilePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public abstract Task<FileOperationResult> CopyAsync(string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes file from provider.
    /// </summary>
    /// <param name="filePath">The name of the object to delete.</param>
    /// <param name="cancellationToken"></param>
    public abstract Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes files from provider.
    /// </summary>
    /// <param name="filePaths">The names of the objects to delete.</param>
    /// <param name="cancellationToken"></param>
    public abstract Task<FileOperationResult> DeleteAsync(List<string> filePaths, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets file url for <paramref name="filePath"/> according to the provider.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public virtual string CreateAccessUrl(string filePath) => Path.Combine(_storageProviderOptions.AccessUrl, filePath);

    /// <summary>
    /// Creates access path to file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public virtual string CreateCdnAccessUrl(string filePath) => UrlCombine(_storageProviderOptions.CdnAccessUrl, filePath);

    /// <summary>
    /// Combines <paramref name="filePaths"/> with '/' seperator.
    /// </summary>
    /// <param name="filePaths"></param>
    /// <returns></returns>
    public virtual string UrlCombine(params string[] filePaths) => string.Join('/', filePaths);

    /// <summary>
    /// Creates obscured path.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public virtual string Obscure(string filePath)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(_storageProviderOptions.EncryptionKey);

        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(filePath));

        // URL-safe base64, first 16 char
        return Convert.ToBase64String(hashBytes).Replace("+", "-").Replace("/", "_").Replace("=", "")[..16];
    }
}
