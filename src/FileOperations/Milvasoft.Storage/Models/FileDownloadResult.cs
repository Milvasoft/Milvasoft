using Milvasoft.Core.Utils.Constants;

namespace Milvasoft.Storage.Models;

/// <summary>
/// Represents the result of a file download operation, including the file stream and related metadata.
/// </summary>
public class FileDownloadResult : FileOperationResult
{
    /// <summary>
    /// Gets or sets the stream containing the downloaded file data.
    /// </summary>
    public Stream Stream { get; set; }

    /// <summary>
    /// Creates a successful <see cref="FileDownloadResult"/> with the specified stream and optional metadata.
    /// </summary>
    /// <param name="stream">The stream containing the file data.</param>
    /// <param name="message">An optional success message.</param>
    /// <param name="fullPath">The full path of the downloaded file.</param>
    /// <param name="fileName">The name of the downloaded file.</param>
    /// <param name="fileExtension">The extension of the downloaded file.</param>
    /// <returns>A successful <see cref="FileDownloadResult"/> instance.</returns>
    public static FileDownloadResult Success(Stream stream, string message = null, string fullPath = null, string fileName = null, string fileExtension = null) => new()
    {
        IsSuccess = true,
        Message = message ?? LocalizerKeys.Successful,
        Stream = stream,
        FullPath = fullPath,
        FileName = fileName,
        FileExtension = fileExtension
    };

    /// <summary>
    /// Creates a failed <see cref="FileDownloadResult"/> with an optional error message and metadata.
    /// </summary>
    /// <param name="message">An optional error message.</param>
    /// <param name="fullPath">The full path of the file.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="fileExtension">The extension of the file.</param>
    /// <returns>A failed <see cref="FileDownloadResult"/> instance.</returns>
    public static new FileDownloadResult Failure(string message = null, string fullPath = null, string fileName = null, string fileExtension = null) => new()
    {
        IsSuccess = false,
        Message = message ?? LocalizerKeys.Failed,
        FullPath = fullPath,
        FileName = fileName,
        FileExtension = fileExtension
    };
}
