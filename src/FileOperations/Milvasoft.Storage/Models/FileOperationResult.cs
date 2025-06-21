using Milvasoft.Core.Utils.Constants;

namespace Milvasoft.Storage.Models;

/// <summary>
/// Represents the result of a file operation, including success status, message, and file details.
/// </summary>
public class FileOperationResult
{
    /// <summary>
    /// Indicates whether the file operation was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Message describing the result of the operation.
    /// </summary>
    public string Message { get; set; } = LocalizerKeys.Successful;

    /// <summary>
    /// The full path of the file involved in the operation.
    /// </summary>
    public string FullPath { get; set; }

    /// <summary>
    /// The name of the file involved in the operation.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// The file extension of the file involved in the operation.
    /// </summary>
    public string FileExtension { get; set; }

    /// <summary>
    /// Creates a successful <see cref="FileOperationResult"/> instance.
    /// </summary>
    /// <param name="message">Optional success message.</param>
    /// <param name="fullPath">Optional full file path.</param>
    /// <param name="fileName">Optional file name.</param>
    /// <param name="fileExtension">Optional file extension.</param>
    /// <returns>A successful <see cref="FileOperationResult"/>.</returns>
    public static FileOperationResult Success(string message = null, string fullPath = null, string fileName = null, string fileExtension = null) => new()
    {
        IsSuccess = true,
        Message = message ?? LocalizerKeys.Successful,
        FullPath = fullPath,
        FileName = fileName,
        FileExtension = fileExtension
    };

    /// <summary>
    /// Creates a failed <see cref="FileOperationResult"/> instance.
    /// </summary>
    /// <param name="message">Optional failure message.</param>
    /// <param name="fullPath">Optional full file path.</param>
    /// <param name="fileName">Optional file name.</param>
    /// <param name="fileExtension">Optional file extension.</param>
    /// <returns>A failed <see cref="FileOperationResult"/>.</returns>
    public static FileOperationResult Failure(string message = null, string fullPath = null, string fileName = null, string fileExtension = null) => new()
    {
        IsSuccess = false,
        Message = message ?? LocalizerKeys.Failed,
        FullPath = fullPath,
        FileName = fileName,
        FileExtension = fileExtension
    };
}