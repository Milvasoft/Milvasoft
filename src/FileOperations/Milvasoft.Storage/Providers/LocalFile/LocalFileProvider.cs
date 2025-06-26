using Microsoft.AspNetCore.Http;
using Milvasoft.Core.Helpers;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Storage.Models;

namespace Milvasoft.Storage.Providers.LocalFile;

/// <summary>
/// Local file provider.
/// </summary>
public class LocalFileProvider(StorageProviderOptions options) : StorageProviderBase(options), ILocalFileProvider
{
    private readonly string _basePath = options.LocalFile.BasePath;
    private readonly string _routePrefix = options.LocalFile.BasePath;

    /// <inheritdoc/>
    public override async Task<FileOperationResult> UploadAsync(IFormFile file, string filePath, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
            return FileOperationResult.Failure(LocalizerKeys.FileNotFound);

        var fullFilePath = Path.Combine(_basePath, filePath);

        try
        {
            var directory = Path.GetDirectoryName(fullFilePath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using var fileStream = new FileStream(fullFilePath, FileMode.Create);

            await file.CopyToAsync(fileStream, cancellationToken);
        }
        catch (Exception ex)
        {
            if (File.Exists(fullFilePath))
                File.Delete(fullFilePath);

            return FileOperationResult.Failure(ex.Message);
        }

        return FileOperationResult.Success(fullPath: fullFilePath);
    }

    /// <inheritdoc/>
    public override async Task<FileOperationResult> NoBufferUploadAsync(HttpContext context, string filePath, CancellationToken cancellationToken = default)
    {
        var fullFilePath = Path.Combine(_basePath, filePath);

        try
        {
            var directory = Path.GetDirectoryName(fullFilePath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
            {
                await context.Request.Body.CopyToAsync(fileStream, cancellationToken);
            }

            return FileOperationResult.Success();
        }
        catch (Exception ex)
        {
            Directory.Delete(fullFilePath, true);
            return FileOperationResult.Failure(ex.Message);
        }
    }

    /// <inheritdoc/>
    public override async Task<FileDownloadResult> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullFilePath = Path.Combine(_basePath, filePath);

            if (!File.Exists(fullFilePath))
                return FileDownloadResult.Failure(LocalizerKeys.FileNotFound);

            var fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return FileDownloadResult.Success(fileStream);
        }
        catch (Exception ex)
        {
            return FileDownloadResult.Failure(ex.Message);
        }
    }

    /// <inheritdoc/>
    public override async Task<FileOperationResult> CopyAsync(string source, string destination, CancellationToken cancellationToken)
    {
        try
        {
            var fullSourcePath = Path.Combine(_basePath, source);
            var fullDestinationPath = Path.Combine(_basePath, destination);

            if (!File.Exists(fullSourcePath))
                return FileOperationResult.Failure(LocalizerKeys.FileNotFound);

            var destinationDirectory = Path.GetDirectoryName(fullDestinationPath);

            if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            await using var sourceStream = new FileStream(fullSourcePath, FileMode.Open, FileAccess.Read);

            await using var destinationStream = new FileStream(fullDestinationPath, FileMode.CreateNew, FileAccess.Write);

            await sourceStream.CopyToAsync(destinationStream, cancellationToken);

            return FileOperationResult.Success();
        }
        catch (Exception ex)
        {
            return FileOperationResult.Failure(ex.Message);
        }
    }

    /// <inheritdoc/>
    public override Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullFilePath = Path.Combine(_basePath, filePath);

            if (!File.Exists(fullFilePath))
                return Task.FromResult(FileOperationResult.Failure(LocalizerKeys.FileNotFound));

            File.Delete(fullFilePath);

            return Task.FromResult(FileOperationResult.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(FileOperationResult.Failure(ex.Message));
        }
    }

    /// <inheritdoc/>
    public override Task<FileOperationResult> DeleteAsync(List<string> filePaths, CancellationToken cancellationToken = default)
    {
        try
        {
            if (filePaths.IsNullOrEmpty())
                return Task.FromResult(FileOperationResult.Success());

            foreach (var filePath in filePaths)
                DeleteAsync(filePath, cancellationToken);

            return Task.FromResult(FileOperationResult.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(FileOperationResult.Failure(ex.Message));
        }
    }

    /// <inheritdoc/>
    public FileOperationResult ClearFileSource(CancellationToken cancellationToken = default)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(_basePath);

            if (!directoryInfo.Exists)
                return FileOperationResult.Success();

            directoryInfo.Delete(true);

            return FileOperationResult.Success();
        }
        catch (Exception ex)
        {
            return FileOperationResult.Failure(ex.Message);
        }
    }

    /// <inheritdoc/>
    public override string CreateAccessUrl(string filePath)
    {
        var relativePath = filePath.Replace(_basePath, "").Replace("\\", "/");

        return UrlCombine(_routePrefix, relativePath);
    }

    /// <inheritdoc/>
    public override string CreateCdnAccessUrl(string filePath) => CreateAccessUrl(filePath);
}