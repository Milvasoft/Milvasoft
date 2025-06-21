using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Storage.Models;
using System.Text;

namespace Milvasoft.Storage;

/// <summary>
/// Provides file-related utility methods such as content type detection, file extension parsing, and boundary/multipart handling.
/// </summary>
public static class StorageHelper
{
    /// <summary>
    /// Dictionary containing file extensions and their corresponding byte signatures (magic numbers).
    /// Useful for validating file contents against extensions.
    /// </summary>
    public static Dictionary<string, List<byte[]>> FileSignatures { get; private set; } = [];

    /// <summary>
    /// Registers a file signature for a specific extension (e.g., ".apk", ".jpg").
    /// </summary>
    public static void RegisterFileSignature(string extension, byte[] signature)
    {
        if (!extension.StartsWith('.'))
            extension = '.' + extension;

        if (!FileSignatures.ContainsKey(extension))
            FileSignatures[extension] = [];

        FileSignatures[extension].Add(signature);
    }

    /// <summary>
    /// Gets boundary from content type.
    /// </summary>
    public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
            throw new InvalidDataException("Missing content-type boundary.");

        if (boundary.Length > lengthLimit)
            throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");

        return boundary;
    }

    /// <summary>
    /// Gets encoding from multipart section.
    /// Defaults to UTF-8 if invalid or insecure encoding like UTF-7 is found.
    /// </summary>
    public static Encoding GetEncoding(MultipartSection section)
    {
        var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

#pragma warning disable SYSLIB0001 // UTF7 obsolete
        if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            return Encoding.UTF8;
#pragma warning restore SYSLIB0001

        return mediaType.Encoding;
    }

    /// <summary>
    /// Checks if the content disposition header contains a file.
    /// </summary>
    public static bool HasFileContentDisposition(this ContentDispositionHeaderValue contentDisposition)
        => contentDisposition != null &&
           contentDisposition.DispositionType.Equals("form-data", StringComparison.OrdinalIgnoreCase) &&
           (!string.IsNullOrEmpty(contentDisposition.FileName.Value) || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));

    /// <summary>
    /// Checks if the content disposition is form-data without a file.
    /// </summary>
    public static bool HasFormDataContentDisposition(this ContentDispositionHeaderValue contentDisposition)
        => contentDisposition != null &&
           contentDisposition.DispositionType.Equals("form-data", StringComparison.OrdinalIgnoreCase) &&
           string.IsNullOrEmpty(contentDisposition.FileName.Value) &&
           string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);

    /// <summary>
    /// Gets the file extension from a file path.
    /// </summary>
    public static string GetFileExtension(string filePath)
        => string.IsNullOrEmpty(filePath) ? string.Empty : Path.GetExtension(filePath);

    /// <summary>
    /// Gets the file name without its extension.
    /// </summary>
    public static string GetFileNameWithoutExtension(string filePath)
        => string.IsNullOrEmpty(filePath) ? string.Empty : Path.GetFileNameWithoutExtension(filePath);

    /// <summary>
    /// Gets the file name with extension from a full file path.
    /// </summary>
    public static string GetFileName(string filePath)
        => string.IsNullOrEmpty(filePath) ? string.Empty : Path.GetFileName(filePath);

    /// <summary>
    /// Validates whether the MIME type of the specified file matches the expected MIME type for the given file
    /// extension.
    /// </summary>
    /// <remarks>This method performs a case-insensitive comparison of the file's MIME type and the expected
    /// MIME type. Ensure that the <paramref name="file"/> parameter contains a valid <see
    /// cref="IFormFile.ContentType"/> value.</remarks>
    /// <param name="file">The file to validate. Must not be null.</param>
    /// <param name="fileExtension">The file extension used to determine the expected MIME type. Must not be null or empty.</param>
    /// <returns><see langword="true"/> if the MIME type of the file matches the expected MIME type for the specified file
    /// extension;  otherwise, <see langword="false"/>.</returns>
    public static bool ValidateMimeType(IFormFile file, string fileExtension)
    {
        var contentType = file.ContentType?.ToLowerInvariant();
        var expectedMime = MimeTypeHelper.GetMimeType(fileExtension);

        return !string.IsNullOrWhiteSpace(contentType) &&
               !string.IsNullOrWhiteSpace(expectedMime) &&
               contentType.Equals(expectedMime, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates the file signature against known signatures for the specified file extension.
    /// </summary>
    /// <remarks>This method checks the file's header bytes against a predefined set of signatures associated with
    /// the given file extension. If no signatures are defined for the specified extension, the method returns <see
    /// langword="true"/>.</remarks>
    /// <param name="file">The file to validate. Must not be null.</param>
    /// <param name="fileExtension">The file extension used to determine the expected signatures. Must not be null or empty.</param>
    /// <returns><see langword="true"/> if the file signature matches any known signature for the specified file extension,  or if
    /// no signatures are defined for the extension; otherwise, <see langword="false"/>.</returns>
    public static bool ValidateFileSignature(IFormFile file, string fileExtension)
    {
        if (!FileSignatures.TryGetValue(fileExtension, out var signatures))
            return true;

        using var stream = file.OpenReadStream();
        var header = new byte[8];

        stream.Read(header, 0, header.Length);

        return signatures.Any(sig => header.Take(sig.Length).SequenceEqual(sig));
    }

    /// <summary>
    /// Validates the uploaded file against strict criteria, including file size, extension, MIME type, and file
    /// signature.
    /// </summary>
    /// <remarks>This method performs multiple validation checks: <list type="bullet">
    /// <item><description>Ensures the file is not <see langword="null"/>.</description></item>
    /// <item><description>Validates the file extension against the allowed extensions.</description></item>
    /// <item><description>Checks that the file size does not exceed <paramref name="maxFileSize"/>.</description></item> <item><description>Verifies the MIME type matches the file
    /// extension.</description></item> <item><description>Validates the file signature to ensure the file is not
    /// corrupted or tampered with.</description></item> </list></remarks>
    /// <param name="file">The uploaded file to validate. Cannot be <see langword="null"/>.</param>
    /// <param name="maxFileSize">The maximum allowed file size, in bytes. Must be greater than zero.</param>
    /// <param name="allowedFileExtensions">A list of allowed file extensions (e.g., ".pdf", ".docx"). If <see langword="null"/>, default extensions for the
    /// specified <paramref name="fileType"/> are used.</param>
    /// <param name="fileType">The type of file being validated, used to determine default allowed extensions if <paramref name="allowedFileExtensions"/> is <see langword="null"/>. Defaults to <see cref="FileType.Document"/>.</param>
    /// <returns>A <see cref="FileOperationResult"/> indicating the success or failure of the validation. If validation fails,
    /// the result contains an error message describing the reason.</returns>
    public static FileOperationResult ValidateFileStrict(this IFormFile file, long maxFileSize, List<string> allowedFileExtensions, FileType fileType = FileType.Document)
    {
        if (file == null)
            return FileOperationResult.Failure(LocalizerKeys.FileShouldBeUploaded);

        allowedFileExtensions ??= GetDefaultExtensions(fileType);
        allowedFileExtensions = allowedFileExtensions.ConvertAll(e => e.ToLowerInvariant());

        var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(fileExtension) || !allowedFileExtensions.Contains(fileExtension))
            return FileOperationResult.Failure("InvalidFileExtension");

        if (file.Length > maxFileSize)
            return FileOperationResult.Failure("FileSizeTooBig");

        if (!ValidateMimeType(file, fileExtension))
            return FileOperationResult.Failure("InvalidMimeType");

        if (!ValidateFileSignature(file, fileExtension))
            return FileOperationResult.Failure("InvalidSignature");

        return FileOperationResult.Success();
    }

    internal static List<string> GetDefaultExtensions(FileType type) => type switch
    {
        FileType.Image => [".jpg", ".jpeg", ".png", ".gif", ".tiff", ".bmp"],
        FileType.Video => [".mp4", ".mov", ".avi", ".webm", ".mkv"],
        FileType.Audio => [".mp3", ".wav", ".ogg", ".aac"],
        FileType.Document => [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".txt"],
        FileType.Compressed => [".zip", ".rar", ".tar", ".gz", ".7z"],
        FileType.Font => [".ttf", ".otf", ".woff", ".woff2"],
        FileType.EMail => [".eml", ".msg"],
        FileType.InternetRelated => [".html", ".css", ".js", ".json", ".xml"],
        FileType.ARModel => [".glb", ".gltf", ".usdz"],
        _ => []
    };

    /// <summary>
    /// Provides utilities for working with MIME types.
    /// </summary>
    public static class MimeTypeHelper
    {
        private static readonly FileExtensionContentTypeProvider _provider = new();

        /// <summary>
        /// Gets all extension-MIME mappings.
        /// </summary>
        public static IDictionary<string, string> ExtensionMimeTypePairs => _provider.Mappings;

        /// <summary>
        /// Registers a custom MIME type mapping.
        /// </summary>
        public static void RegisterMimeType(string extension, string mimeType)
        {
            if (!extension.StartsWith('.'))
                extension = '.' + extension;

            _provider.Mappings[extension] = mimeType;
        }

        /// <summary>
        /// Gets MIME type from file extension. Defaults to "application/octet-stream".
        /// </summary>
        public static string GetMimeType(string fileExtension)
        {
            if (!fileExtension.StartsWith('.'))
                fileExtension = '.' + fileExtension;

            return _provider.TryGetContentType(fileExtension, out var contentType) ? contentType : "application/octet-stream";
        }
    }
}