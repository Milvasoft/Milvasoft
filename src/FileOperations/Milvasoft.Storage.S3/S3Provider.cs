using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Milvasoft.Core.Helpers;
using Milvasoft.Storage.Models;
using Milvasoft.Storage.Providers;
using System.Net;

namespace Milvasoft.Storage.S3;

/// <summary>
/// Aws S3 provider.
/// </summary>
public class S3Provider(IAmazonS3 client, StorageProviderOptions options) : StorageProviderBase(options), IS3Provider
{
    private readonly IAmazonS3 _client = client;
    private const int _maxKeysPerDeleteRequest = 1000;

    private readonly AwsS3Configuration _s3Configuration = options.AwsS3;

    /// <inheritdoc/>
    public IAmazonS3 GetClient() => _client;

    /// <inheritdoc/>
    public override async Task<FileOperationResult> UploadAsync(IFormFile file, string filePath, CancellationToken cancellationToken = default)
    {
        await using var inputStream = file.OpenReadStream();

        var request = new PutObjectRequest
        {
            BucketName = _s3Configuration.BucketName,
            Key = filePath,
            InputStream = inputStream,
            AutoCloseStream = false,
            ContentType = StorageHelper.GetSafeContentType(string.IsNullOrEmpty(file.FileName) ? filePath : file.FileName, file.ContentType),
            CannedACL = S3CannedACL.BucketOwnerFullControl,
        };

        var response = await _client.PutObjectAsync(request, cancellationToken);

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            return FileOperationResult.Success($"Successfully uploaded {filePath} to {_s3Configuration.BucketName}.", filePath);
        }
        else
        {
            return FileOperationResult.Failure($"Could not upload {filePath} to {_s3Configuration.BucketName}.");
        }
    }

    /// <summary>
    /// Streams the file in the multipart/form-data request body to the bucket without buffering the whole file.
    /// The extension must be allowed (see <see cref="StorageProviderOptions.AllowedFileExtensions"/>), the size must not exceed
    /// <see cref="StorageProviderOptions.MaxFileSize"/> and the content must match the registered file signature of the extension.
    /// The object is committed only when the whole body was received; otherwise the multipart upload is aborted.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="filePath">Object key without extension. The extension of the uploaded file is appended.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A value indicating the success or failure of the upload procedure.</returns>
    public override Task<FileOperationResult> NoBufferUploadAsync(HttpContext context, string filePath, CancellationToken cancellationToken = default)
        => ReadMultipartFilesAsync(context, filePath, async (key, contentType, copyToAsync) =>
        {
            await using var output = new S3UploadStream(_client, _s3Configuration.BucketName, key, contentType);

            await copyToAsync(output);

            await output.CompleteAsync(cancellationToken);
        }, cancellationToken);

    /// <inheritdoc/>
    public override async Task<FileDownloadResult> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = _s3Configuration.BucketName,
            Key = filePath,
        };

        try
        {
            // Issue request and remember to dispose of the response
            GetObjectResponse response = await _client.GetObjectAsync(request, cancellationToken);

            if (response.HttpStatusCode != HttpStatusCode.OK)
                return FileDownloadResult.Failure(string.Empty);

            return FileDownloadResult.Success(response.ResponseStream);
        }
        catch (Exception)
        {
            return FileDownloadResult.Failure(string.Empty);
        }
    }

    /// <inheritdoc/>
    public override async Task<FileOperationResult> CopyAsync(string source, string destination, CancellationToken cancellationToken)
    {
        var request = new CopyObjectRequest
        {
            SourceBucket = _s3Configuration.BucketName,
            DestinationBucket = _s3Configuration.BucketName,
            SourceKey = source,
            DestinationKey = destination,
            CannedACL = S3CannedACL.BucketOwnerFullControl
        };

        var res = await _client.CopyObjectAsync(request, cancellationToken);

        if (res.HttpStatusCode != HttpStatusCode.OK)
            return FileOperationResult.Failure(res.ResponseMetadata.RequestId);

        return FileOperationResult.Success();
    }

    /// <inheritdoc/>
    public override async Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var deleteObjectRequest = new DeleteObjectRequest
        {
            BucketName = _s3Configuration.BucketName,
            Key = filePath,
        };

        var response = await _client.DeleteObjectAsync(deleteObjectRequest, cancellationToken);

        if (response.HttpStatusCode != HttpStatusCode.OK)
            return FileOperationResult.Failure();

        return FileOperationResult.Success();
    }

    /// <inheritdoc/>
    /// <remarks>Keys are deleted in batches of 1000 (the S3 limit). Fails if any key could not be deleted.</remarks>
    public override async Task<FileOperationResult> DeleteAsync(List<string> filePaths, CancellationToken cancellationToken = default)
    {
        if (filePaths.IsNullOrEmpty())
            return FileOperationResult.Failure("File paths cannot be null or empty.");

        var (deleted, failed) = await DeleteKeysAsync(filePaths, cancellationToken);

        if (failed > 0)
            return FileOperationResult.Failure($"{deleted} objects deleted, {failed} objects could not be deleted.");

        return FileOperationResult.Success($"{deleted} objects deleted.");
    }

    /// <inheritdoc/>
    public async Task<FileOperationResult> DeleteFolderAsync(string prefix, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return FileOperationResult.Failure("Prefix cannot be empty.");

        prefix = EnsureSlashSuffix(prefix.Trim().TrimStart('/'));

        return await DeleteByPrefixAsync(prefix, cancellationToken);

        static string EnsureSlashSuffix(string s) => string.IsNullOrEmpty(s) ? s : (s.EndsWith('/') ? s : s + "/");
    }

    /// <inheritdoc/>
    /// <remarks>Lists and deletes every object page by page, so buckets with more than 1000 objects are cleared completely.</remarks>
    public Task<FileOperationResult> ClearBucketAsync(CancellationToken cancellationToken = default) => DeleteByPrefixAsync(prefix: null, cancellationToken);

    private async Task<FileOperationResult> DeleteByPrefixAsync(string prefix, CancellationToken cancellationToken)
    {
        var listReq = new ListObjectsV2Request
        {
            BucketName = _s3Configuration.BucketName,
            Prefix = prefix,
            MaxKeys = _maxKeysPerDeleteRequest,
            FetchOwner = false
        };

        var totalDeleted = 0;
        var totalFailed = 0;

        try
        {
            ListObjectsV2Response listRes;

            do
            {
                listRes = await _client.ListObjectsV2Async(listReq, cancellationToken);

                if (listRes.S3Objects.IsNullOrEmpty())
                    break;

                var (deleted, failed) = await DeleteKeysAsync(listRes.S3Objects.Select(o => o.Key), cancellationToken);

                totalDeleted += deleted;
                totalFailed += failed;

                listReq.ContinuationToken = listRes.NextContinuationToken;
            }
            while (listRes.IsTruncated ?? false);
        }
        catch (AmazonS3Exception)
        {
            return FileOperationResult.Failure($"{totalDeleted} objects deleted before the operation failed.");
        }

        if (totalFailed > 0)
            return FileOperationResult.Failure($"{totalDeleted} objects deleted, {totalFailed} objects could not be deleted.");

        return FileOperationResult.Success($"{totalDeleted} objects deleted.");
    }

    private async Task<(int Deleted, int Failed)> DeleteKeysAsync(IEnumerable<string> keys, CancellationToken cancellationToken)
    {
        var deleted = 0;
        var failed = 0;

        foreach (var chunk in keys.Chunk(_maxKeysPerDeleteRequest))
        {
            var request = new DeleteObjectsRequest
            {
                BucketName = _s3Configuration.BucketName,
                Quiet = true,
                Objects = [.. chunk.Select(k => new KeyVersion { Key = k })]
            };

            int errorCount;

            try
            {
                var response = await _client.DeleteObjectsAsync(request, cancellationToken);

                errorCount = response.DeleteErrors?.Count ?? 0;
            }
            catch (DeleteObjectsException ex)
            {
                errorCount = ex.Response?.DeleteErrors?.Count ?? chunk.Length;
            }

            failed += errorCount;
            deleted += chunk.Length - errorCount;
        }

        return (deleted, failed);
    }

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
    /// <param name="contentLength">
    /// When set, the <c>Content-Length</c> header is included in the signature, so S3 rejects (403) any PUT whose body
    /// size differs from this value. Use it to enforce a declared upload size server-side.
    /// </param>
    /// <returns>A string containing the pre-signed URL that can be used to upload the file.</returns>
    public string GeneratePreSignedUrl(string filePath,
                                       int expiresIn = 2,
                                       string cacheControlHeader = "public, max-age=86400",
                                       string contentType = null,
                                       Dictionary<string, string> metadatas = null,
                                       long? contentLength = null)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _s3Configuration.BucketName,
            Key = filePath,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(expiresIn),
            ContentType = contentType,
            Headers =
            {
                CacheControl = cacheControlHeader
            }
        };

        // Signing the length pins the upload to exactly this many bytes.
        if (contentLength.HasValue)
            request.Headers.ContentLength = contentLength.Value;

        // Add custom metadata
        if (metadatas != null)
        {
            foreach (var kvp in metadatas)
            {
                // SDK automatically prefixes with "x-amz-meta-" when sending
                request.Metadata.Add(kvp.Key, kvp.Value);
            }
        }

        return _client.GetPreSignedURL(request);
    }
}
