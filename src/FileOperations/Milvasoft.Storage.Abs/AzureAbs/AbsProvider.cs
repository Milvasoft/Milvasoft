using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Http;
using Milvasoft.Core.Helpers;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Storage.Models;
using Milvasoft.Storage.Providers;

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
            await using var inputStream = file.OpenReadStream();

            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = StorageHelper.GetSafeContentType(string.IsNullOrEmpty(file.FileName) ? filePath : file.FileName, file.ContentType)
                }
            };

            await blobClient.UploadAsync(inputStream, options, cancellationToken);

            return FileOperationResult.Success(fullPath: filePath);
        }
        catch (RequestFailedException)
        {
            return FileOperationResult.Failure(LocalizerKeys.UploadFailed);
        }
    }

    /// <summary>
    /// Streams the file in the multipart/form-data request body to the container without buffering the whole file.
    /// The extension must be allowed (see <see cref="StorageProviderOptions.AllowedFileExtensions"/>), the size must not exceed
    /// <see cref="StorageProviderOptions.MaxFileSize"/> and the content must match the registered file signature of the extension.
    /// The blob is committed only when the whole body was received.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="filePath">Blob name without extension. The extension of the uploaded file is appended.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A value indicating the success or failure of the upload procedure.</returns>
    public override Task<FileOperationResult> NoBufferUploadAsync(HttpContext context, string filePath, CancellationToken cancellationToken = default)
        => ReadMultipartFilesAsync(context, filePath, async (blobName, contentType, copyToAsync) =>
        {
            var blobClient = _containerClient.GetBlockBlobClient(blobName);

            await using var output = new AbsUploadStream(blobClient, contentType);

            await copyToAsync(output);

            await output.CompleteAsync(cancellationToken);
        }, cancellationToken);

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
    public override async Task<FileOperationResult> DeleteAsync(List<string> filePaths, CancellationToken cancellationToken = default)
    {
        if (filePaths.IsNullOrEmpty())
            return FileOperationResult.Failure("File paths cannot be null or empty.");

        var failed = 0;

        foreach (var filePath in filePaths)
        {
            try
            {
                var result = await DeleteAsync(filePath, cancellationToken);

                if (!result.IsSuccess)
                    failed++;
            }
            catch (RequestFailedException)
            {
                failed++;
            }
        }

        if (failed > 0)
            return FileOperationResult.Failure($"{filePaths.Count - failed} files deleted, {failed} files could not be deleted.");

        return FileOperationResult.Success();
    }

    /// <inheritdoc/>
    public async Task<FileOperationResult> ClearContainerAsync(CancellationToken cancellationToken = default)
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
    public override async Task<FileOperationResult> CopyAsync(string source, string destination, CancellationToken cancellationToken)
    {
        var sourceClient = _containerClient.GetBlobClient(source);

        if (!await sourceClient.ExistsAsync(cancellationToken))
            return FileOperationResult.Failure(LocalizerKeys.FileNotFound);

        try
        {
            var destinationClient = _containerClient.GetBlobClient(destination);

            var operation = await destinationClient.StartCopyFromUriAsync(sourceClient.Uri, cancellationToken: cancellationToken);

            await operation.WaitForCompletionAsync(cancellationToken);

            return FileOperationResult.Success(fullPath: destination);
        }
        catch (RequestFailedException)
        {
            return FileOperationResult.Failure();
        }
    }
}
