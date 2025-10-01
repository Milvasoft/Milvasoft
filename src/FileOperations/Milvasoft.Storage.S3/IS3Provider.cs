using Amazon.S3;
using Milvasoft.Storage.Abstract;
using Milvasoft.Storage.Models;

namespace Milvasoft.Storage.S3;

/// <summary>
/// Aws S3 provider.
/// </summary>
public interface IS3Provider : IStorageProvider
{
    /// <summary>
    /// Exposes the S3 client for direct access to S3 operations.
    /// </summary>
    /// <returns></returns>
    public IAmazonS3 GetClient();

    /// <summary>
    /// Remove all files in bucket.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<FileOperationResult> ClearBucketAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a files recursively from the S3 bucket with specified prefix.
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<FileOperationResult> DeleteFolderAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a pre-signed URL for uploading a file to an S3 bucket.
    /// </summary>
    /// <remarks>The generated pre-signed URL allows the caller to perform an HTTP PUT operation to upload 
    /// the specified file to the S3 bucket. The URL is valid for the duration specified by  <paramref
    /// name="expiresIn"/>.</remarks>
    /// <param name="filePath">The path of the file within the S3 bucket. This is used as the key to identify the file.</param>
    /// <param name="expiresIn">The expiration time, in minutes, for the pre-signed URL. Defaults to 2 minutes. Must be a positive integer.</param>
    /// <param name="cacheControlHeader">The value of the Cache-Control header to be included in the request. Defaults to  "public, max-age=86400".</param>
    /// <param name="contentType"></param>
    /// <param name="metadatas"></param>
    /// <returns>A string containing the pre-signed URL that can be used to upload the file.</returns>
    public string GeneratePreSignedUrl(string filePath,
                                       int expiresIn = 2,
                                       string cacheControlHeader = "public, max-age=86400",
                                       string contentType = null,
                                       Dictionary<string, string> metadatas = null);
}
