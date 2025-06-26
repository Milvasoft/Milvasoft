using Microsoft.AspNetCore.Http;
using Milvasoft.Storage.Models;

namespace Milvasoft.Storage.Abstract;

/// <summary>
/// Storage provider.
/// </summary>
public interface IStorageProvider
{
    /// <summary>
    /// Uploads file to provider.
    /// </summary>
    /// <param name="filePath">The file to upload.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="file">The file to upload.</param>
    /// <returns>A boolean value indicating the success or failure of the upload procedure.</returns>
    Task<FileOperationResult> UploadAsync(IFormFile file, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads file to provider.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="filePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A boolean value indicating the success or failure of the upload procedure.</returns>
    Task<FileOperationResult> NoBufferUploadAsync(HttpContext context, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads file from provider.
    /// </summary>
    /// <param name="filePath">The name of the file to download.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A boolean value indicating the success or failure of the download process.</returns>
    Task<FileDownloadResult> DownloadAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies file from <paramref name="source"/> to <paramref name="destination"/>.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FileOperationResult> CopyAsync(string source, string destination, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes file from the provider.
    /// </summary>
    /// <param name="filePath">The name of the file to delete.</param>
    /// <param name="cancellationToken"></param>
    Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes files from the provider.
    /// </summary>
    /// <param name="filePaths">The names of the files to delete.</param>
    /// <param name="cancellationToken"></param>
    Task<FileOperationResult> DeleteAsync(List<string> filePaths, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets file url for <paramref name="filePath"/> according to the provider.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public string CreateAccessUrl(string filePath);

    /// <summary>
    /// Creates access path to file.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string CreateCdnAccessUrl(string path);

    /// <summary>
    /// Combines <paramref name="paths"/> with '/' seperator.
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    public string UrlCombine(params string[] paths);

    /// <summary>
    /// Creates obscured path.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public string Obscure(string filePath);
}
