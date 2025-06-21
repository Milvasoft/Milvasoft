using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Storage.Models;
using Milvasoft.Storage.Providers;
using System.Net;

namespace Milvasoft.Storage.Abs.AzureAbs;

/// <summary>
/// Azure blob storage provider.
/// </summary>
public class AbsProvider(BlobServiceClient blobServiceClient, StorageProviderOptions options) : StorageProviderBase(options), IAbsProvider
{
    private readonly BlobContainerClient _containerClient = blobServiceClient.GetBlobContainerClient(options.AzureBlob.ContainerName);

    /// <inheritdoc/>
    public override async Task<FileOperationResult> UploadAsync(IFormFile file, string filePath, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(filePath);

        try
        {
            await blobClient.UploadAsync(file.OpenReadStream(), overwrite: true, cancellationToken);

            return FileOperationResult.Success(fullPath: filePath);
        }
        catch (Exception)
        {
            return FileOperationResult.Failure();
        }
    }

    /// <inheritdoc/>
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

                try
                {
                    var blobClient = _containerClient.GetBlockBlobClient(filePath + extension);

                    using var output = new AbsUploadStream(blobClient, section.ContentType, filePath + extension);

                    // Sends part by part.
                    await section.Body.CopyToAsync(output, cancellationToken);
                }
                catch (Exception ex)
                {
                    return FileOperationResult.Failure(ex.Message);
                }
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
        var blobClient = _containerClient.GetBlobClient(filePath);

        if (!await blobClient.ExistsAsync(cancellationToken))
            return FileDownloadResult.Failure();

        var blobStream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);

        return FileDownloadResult.Success(blobStream);
    }

    /// <inheritdoc/>
    public override async Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(filePath);

        var isSuccess = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

        if (!isSuccess)
            return FileOperationResult.Failure();

        return FileOperationResult.Success();
    }

    /// <inheritdoc/>
    public async Task<FileOperationResult> RemoveAllAsync(CancellationToken cancellationToken = default)
    {
        var blobs = _containerClient.GetBlobsAsync(cancellationToken: cancellationToken);

        var counter = 0;

        await foreach (var blobItem in blobs)
        {
            var blobClient = _containerClient.GetBlobClient(blobItem.Name);

            var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

            if (response.Value)
                counter++;
        }

        return FileOperationResult.Success($"{counter} objects deleted.");
    }

    /// <inheritdoc/>
    public override Task<FileOperationResult> CopyAsync(string source, string destination, CancellationToken cancellationToken) => throw new NotImplementedException();
}
