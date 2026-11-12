using Microsoft.AspNetCore.Http;
using Milvasoft.Core.Helpers;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Storage.Models;

namespace Milvasoft.Storage.Providers.LocalFile;

/// <summary>
/// Local file provider.
/// </summary>
/// <remarks>
/// Every path is resolved under <see cref="LocalFileConfiguration.BasePath"/>. Paths that resolve outside of it
/// (for example with <c>../</c> segments or a rooted path to another folder) are rejected with <see cref="InvalidFilePath"/>.
/// Rooted paths inside the base path (such as <see cref="FileOperationResult.FullPath"/> returned by an upload) are accepted.
/// </remarks>
public class LocalFileProvider(StorageProviderOptions options) : StorageProviderBase(options), ILocalFileProvider
{
    /// <summary>
    /// Failure message for paths that resolve outside of the base path.
    /// </summary>
    public const string InvalidFilePath = nameof(InvalidFilePath);

    private static readonly StringComparison _pathComparison = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
                                                                    ? StringComparison.OrdinalIgnoreCase
                                                                    : StringComparison.Ordinal;

    private readonly string _configuredBasePath = options.LocalFile.BasePath;
    private readonly string _basePath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(options.LocalFile.BasePath));
    private readonly string _routePrefix = options.LocalFile.RoutePrefix ?? options.LocalFile.BasePath;

    /// <inheritdoc/>
    public override async Task<FileOperationResult> UploadAsync(IFormFile file, string filePath, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
            return FileOperationResult.Failure(LocalizerKeys.FileNotFound);

        if (!TryGetFullPath(filePath, out var fullFilePath))
            return FileOperationResult.Failure(InvalidFilePath);

        try
        {
            await WriteFileAtomicallyAsync(fullFilePath, destination => file.CopyToAsync(destination, cancellationToken), cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return FileOperationResult.Failure(ex.Message);
        }

        return FileOperationResult.Success(fullPath: Path.Combine(_configuredBasePath, filePath));
    }

    /// <summary>
    /// Streams the file in the multipart/form-data request body to the base path without buffering the whole file.
    /// The extension must be allowed (see <see cref="StorageProviderOptions.AllowedFileExtensions"/>), the size must not exceed
    /// <see cref="StorageProviderOptions.MaxFileSize"/> and the content must match the registered file signature of the extension.
    /// The file is written to a temporary file and moved into place only when the whole body was received.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="filePath">File path relative to the base path, without extension. The extension of the uploaded file is appended.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A value indicating the success or failure of the upload procedure.</returns>
    public override async Task<FileOperationResult> NoBufferUploadAsync(HttpContext context, string filePath, CancellationToken cancellationToken = default)
    {
        if (!TryGetFullPath(filePath, out _))
            return FileOperationResult.Failure(InvalidFilePath);

        return await ReadMultipartFilesAsync(context, filePath, async (path, _, copyToAsync) =>
        {
            if (!TryGetFullPath(path, out var fullFilePath))
                throw new InvalidDataException(InvalidFilePath);

            await WriteFileAtomicallyAsync(fullFilePath, copyToAsync, cancellationToken);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<FileDownloadResult> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!TryGetFullPath(filePath, out var fullFilePath))
                return Task.FromResult(FileDownloadResult.Failure(InvalidFilePath));

            if (!File.Exists(fullFilePath))
                return Task.FromResult(FileDownloadResult.Failure(LocalizerKeys.FileNotFound));

            var fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

            return Task.FromResult(FileDownloadResult.Success(fileStream));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Task.FromResult(FileDownloadResult.Failure(ex.Message));
        }
    }

    /// <inheritdoc/>
    public override async Task<FileOperationResult> CopyAsync(string source, string destination, CancellationToken cancellationToken)
    {
        if (!TryGetFullPath(source, out var fullSourcePath) || !TryGetFullPath(destination, out var fullDestinationPath))
            return FileOperationResult.Failure(InvalidFilePath);

        try
        {
            if (!File.Exists(fullSourcePath))
                return FileOperationResult.Failure(LocalizerKeys.FileNotFound);

            var destinationDirectory = Path.GetDirectoryName(fullDestinationPath);

            Directory.CreateDirectory(destinationDirectory);

            await using var sourceStream = new FileStream(fullSourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

            await using var destinationStream = new FileStream(fullDestinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true);

            await sourceStream.CopyToAsync(destinationStream, cancellationToken);

            return FileOperationResult.Success();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return FileOperationResult.Failure(ex.Message);
        }
    }

    /// <inheritdoc/>
    public override Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
        => Task.FromResult(DeleteFile(filePath));

    /// <inheritdoc/>
    public override Task<FileOperationResult> DeleteAsync(List<string> filePaths, CancellationToken cancellationToken = default)
    {
        if (filePaths.IsNullOrEmpty())
            return Task.FromResult(FileOperationResult.Success());

        var failed = filePaths.Count(filePath => !DeleteFile(filePath).IsSuccess);

        if (failed > 0)
            return Task.FromResult(FileOperationResult.Failure($"{filePaths.Count - failed} files deleted, {failed} files could not be deleted."));

        return Task.FromResult(FileOperationResult.Success());
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
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return FileOperationResult.Failure(ex.Message);
        }
    }

    /// <inheritdoc/>
    public override string CreateAccessUrl(string filePath)
    {
        var relativePath = TryGetFullPath(filePath, out var fullPath) ? Path.GetRelativePath(_basePath, fullPath) : filePath;

        return UrlCombine(_routePrefix?.TrimEnd('/', '\\'), relativePath.Replace('\\', '/').TrimStart('/'));
    }

    /// <inheritdoc/>
    public override string CreateCdnAccessUrl(string filePath) => CreateAccessUrl(filePath);

    /// <summary>
    /// Resolves <paramref name="filePath"/> under the base path. Returns false if it is empty or resolves outside of the base path.
    /// </summary>
    /// <param name="filePath">Path relative to the base path, or a rooted path inside it.</param>
    /// <param name="fullPath">Resolved absolute path.</param>
    public bool TryGetFullPath(string filePath, out string fullPath)
    {
        fullPath = null;

        if (string.IsNullOrWhiteSpace(filePath) || filePath.Contains('\0'))
            return false;

        // Paths returned as FullPath by uploads start with the configured (possibly relative) base path.
        if (!Path.IsPathRooted(_configuredBasePath)
            && filePath.Length > _configuredBasePath.Length
            && filePath.StartsWith(_configuredBasePath, _pathComparison)
            && filePath[_configuredBasePath.Length] is '/' or '\\')
            filePath = filePath[(_configuredBasePath.Length + 1)..];

        string candidate;

        try
        {
            candidate = Path.GetFullPath(Path.Combine(_basePath, filePath));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }

        var basePathWithSeparator = Path.EndsInDirectorySeparator(_basePath) ? _basePath : _basePath + Path.DirectorySeparatorChar;

        if (!candidate.StartsWith(basePathWithSeparator, _pathComparison))
            return false;

        fullPath = candidate;

        return true;
    }

    private FileOperationResult DeleteFile(string filePath)
    {
        if (!TryGetFullPath(filePath, out var fullFilePath))
            return FileOperationResult.Failure(InvalidFilePath);

        try
        {
            if (!File.Exists(fullFilePath))
                return FileOperationResult.Failure(LocalizerKeys.FileNotFound);

            File.Delete(fullFilePath);

            return FileOperationResult.Success();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return FileOperationResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Writes to a temporary file in the target directory and replaces the target only after the write completed,
    /// so a failed or cancelled write never leaves a partial file or destroys the previous one.
    /// </summary>
    private static async Task WriteFileAtomicallyAsync(string fullFilePath, Func<Stream, Task> writeAsync, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(fullFilePath);

        Directory.CreateDirectory(directory);

        var tempFilePath = Path.Combine(directory, $".{Path.GetFileName(fullFilePath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var fileStream = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await writeAsync(fileStream);
                await fileStream.FlushAsync(cancellationToken);
            }

            File.Move(tempFilePath, fullFilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
}
