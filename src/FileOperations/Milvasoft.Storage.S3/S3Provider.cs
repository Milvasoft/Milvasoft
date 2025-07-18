﻿using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Helpers;
using Milvasoft.Core.Utils.Constants;
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
    private readonly AwsS3Configuration _s3Configuration = options.AwsS3;

    /// <inheritdoc/>
    public override async Task<FileOperationResult> UploadAsync(IFormFile file, string filePath, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _s3Configuration.BucketName,
            Key = filePath,
            InputStream = file.OpenReadStream(),
            ContentType = file.ContentType,
            CannedACL = S3CannedACL.BucketOwnerFullControl
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
    /// Uploads file to bucket.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="filePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A boolean value indicating the success or failure of the upload procedure.</returns>
    public override async Task<FileOperationResult> NoBufferUploadAsync(HttpContext context, string filePath, CancellationToken cancellationToken = default)
    {
        if (!MediaTypeHeaderValue.TryParse(context.Request.Headers.ContentType.ToString(), out var mediaTypeHeader))
            return FileOperationResult.Failure(LocalizerKeys.UnsupportedMediaType);

        var boundary = HeaderUtilities.RemoveQuotes(mediaTypeHeader.Boundary.Value).Value;

        var reader = new MultipartReader(boundary, context.Request.Body);

        var section = await reader.ReadNextSectionAsync(cancellationToken) ?? throw new MilvaUserFriendlyException(LocalizerKeys.FileShouldBeUploaded);

        var extension = string.Empty;
        var fileName = string.Empty;

        while (section != null)
        {
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

            if (!hasContentDispositionHeader)
                section = await reader.ReadNextSectionAsync(cancellationToken);

            if (contentDisposition.HasFileContentDisposition())
            {
                var trustedFileName = WebUtility.HtmlEncode(contentDisposition.FileName.Value);

                fileName = trustedFileName;

                extension = Path.GetExtension(trustedFileName).ToLowerInvariant();

                var isSupportedExtension = StorageHelper.FileSignatures.TryGetValue(extension, out _);

                if (!isSupportedExtension)
                    return FileOperationResult.Failure(LocalizerKeys.UnsupportedMediaType);

                using var output = new S3UploadStream(_client, _s3Configuration.BucketName, filePath + extension);

                // Send file part by part.
                await section.Body.CopyToAsync(output, cancellationToken);
            }
            else if (contentDisposition.HasFormDataContentDisposition())
                return FileOperationResult.Failure(LocalizerKeys.FileShouldBeUploaded);

            section = await reader.ReadNextSectionAsync(cancellationToken);
        }

        return FileOperationResult.Success(fullPath: filePath, fileName: fileName, fileExtension: extension);
    }

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
    public override async Task<FileOperationResult> DeleteAsync(List<string> filePaths, CancellationToken cancellationToken = default)
    {
        if (filePaths.IsNullOrEmpty())
            return FileOperationResult.Failure("File paths cannot be null or empty.");

        var keys = filePaths.Select(filePath => new KeyVersion { Key = filePath }).ToList();

        var deleteObjectsRequest = new DeleteObjectsRequest
        {
            BucketName = _s3Configuration.BucketName,
            Objects = keys
        };

        var response = await _client.DeleteObjectsAsync(deleteObjectsRequest, cancellationToken);

        if (response.HttpStatusCode != HttpStatusCode.OK)
            return FileOperationResult.Failure();

        return FileOperationResult.Success($"{response.DeletedObjects.Count} objects deleted.");
    }

    /// <inheritdoc/>
    public async Task<FileOperationResult> ClearBucketAsync(CancellationToken cancellationToken = default)
    {
        var request = new ListObjectsRequest
        {
            BucketName = _s3Configuration.BucketName
        };

        var response = await _client.ListObjectsAsync(request, cancellationToken);

        var keys = new List<KeyVersion>();

        foreach (var item in response.S3Objects)
        {
            // Here you can provide VersionId as well.
            keys.Add(new KeyVersion { Key = item.Key });
        }

        var multiObjectDeleteRequest = new DeleteObjectsRequest()
        {
            BucketName = _s3Configuration.BucketName,
            Objects = keys
        };

        var deleteObjectsResponse = await _client.DeleteObjectsAsync(multiObjectDeleteRequest, cancellationToken);

        return FileOperationResult.Success($"{deleteObjectsResponse.DeletedObjects.Count} objects deleted.");
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
    /// <returns>A string containing the pre-signed URL that can be used to upload the file.</returns>
    public string GeneratePreSignedUrl(string filePath, int expiresIn = 2, string cacheControlHeader = "public, max-age=86400")
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _s3Configuration.BucketName,
            Key = filePath,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(expiresIn),
            Headers =
            {
                CacheControl = cacheControlHeader
            }
        };

        return _client.GetPreSignedURL(request);
    }
}
