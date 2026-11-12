using Microsoft.AspNetCore.Http;
using Milvasoft.Core.Utils.Constants;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Milvasoft.Storage.Models;

/// <summary>
/// Represents an file upload data transfer object.
/// </summary>
public record FileUploadRequest
{
    private const string _base64Flag = ";base64,";
    private const string _dataUriPrefix = "data:";


    /// <summary>
    /// Decoded content cache. Kept outside of the record's fields so record equality is not affected.
    /// </summary>
    private static readonly ConditionalWeakTable<FileUploadRequest, DecodedFile> _decodedFiles = new();

    private sealed record DecodedFile(string Source, byte[] Bytes, string ContentType);

    /// <summary>
    /// Id of file.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Determines whether the file has been updated or not.
    /// </summary>
    public bool FileChanged { get; set; }

    /// <summary>
    /// File url.
    /// </summary>
    public string FileExtension { get; set; }

    /// <summary>
    /// File name.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Order of display of file. Lowest comes first.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Alt text of file.
    /// </summary>
    public string AltText { get; set; }

    /// <summary>
    /// File of the content as data URI formatted base64 string.
    /// </summary>
    public string FileAsBase64 { get; set; }

    /// <summary>
    /// File of the content.
    /// </summary>
    [JsonIgnore]
    public byte[] File { get => Decode()?.Bytes ?? []; }

    /// <summary>
    /// File of the content.
    /// </summary>
    /// <remarks>Returns null if <see cref="FileAsBase64"/> is empty or is not a valid base64 data URI.</remarks>
    [JsonIgnore]
    public IFormFile MediaAsFormFile { get => Decode() is { } decoded ? ConvertToFormFile(decoded) : null; }

    /// <summary>
    /// Validates the file extension against allowed extensions for the specified file type.
    /// </summary>
    /// <param name="fileType"></param>
    /// <param name="maxFileSize"></param>
    /// <param name="allowedFileExtensions"></param>
    /// <returns></returns>
    public FileOperationResult ValidateExtension(FileType fileType, List<string> allowedFileExtensions = null, long maxFileSize = 1024 * 1024)
    {
        if (!string.IsNullOrWhiteSpace(FileAsBase64))
        {
            var formFile = MediaAsFormFile;

            if (formFile == null)
                return FileOperationResult.Failure(LocalizerKeys.UnsupportedMediaType);

            return formFile.ValidateFileStrict(maxFileSize, allowedFileExtensions, fileType);
        }

        allowedFileExtensions ??= StorageHelper.GetDefaultExtensions(fileType);
        allowedFileExtensions = allowedFileExtensions.ConvertAll(e => e.ToLowerInvariant());

        if (string.IsNullOrWhiteSpace(FileExtension) || !allowedFileExtensions.Contains(FileExtension))
            return FileOperationResult.Failure(LocalizerKeys.InvalidFileExtension);

        return FileOperationResult.Success();
    }

    /// <summary>
    /// Decodes <see cref="FileAsBase64"/> once and caches the result until the property changes.
    /// </summary>
    /// <returns>Decoded file, or null if the value is empty or not a valid base64 data URI.</returns>
    private DecodedFile Decode()
    {
        var source = FileAsBase64;

        if (string.IsNullOrWhiteSpace(source))
            return null;

        if (_decodedFiles.TryGetValue(this, out var cached) && ReferenceEquals(cached.Source, source))
            return cached.Bytes != null ? cached : null;

        var decoded = DecodeDataUri(source);

        _decodedFiles.AddOrUpdate(this, decoded);

        return decoded.Bytes != null ? decoded : null;
    }

    private static DecodedFile DecodeDataUri(string source)
    {
        var flagIndex = source.IndexOf(_base64Flag, StringComparison.Ordinal);

        if (flagIndex < 0)
            return new DecodedFile(source, null, null);

        var header = source[..flagIndex];

        if (header.StartsWith(_dataUriPrefix, StringComparison.OrdinalIgnoreCase))
            header = header[_dataUriPrefix.Length..];

        // Content type is the media type before any ";param=value" part.
        var parameterIndex = header.IndexOf(';');

        var contentType = (parameterIndex >= 0 ? header[..parameterIndex] : header).Trim();

        try
        {
            return new DecodedFile(source, Convert.FromBase64String(source[(flagIndex + _base64Flag.Length)..]), contentType);
        }
        catch (FormatException)
        {
            return new DecodedFile(source, null, null);
        }
    }

    /// <summary>
    /// Converts the decoded data URI to <see cref="IFormFile"/>.
    /// </summary>
    private FormFile ConvertToFormFile(DecodedFile decoded)
    {
        var splittedContentType = decoded.ContentType?.Split('/');

        var fileType = splittedContentType?[0];

        var fileExtension = splittedContentType?.Length > 1 ? splittedContentType[1] : string.Empty;

        var memoryStream = new MemoryStream(decoded.Bytes, writable: false);

        return new FormFile(memoryStream, 0, memoryStream.Length, fileType, $"{FileName ?? "file"}.{fileExtension}")
        {
            Headers = new HeaderDictionary(),
            ContentType = decoded.ContentType
        };
    }
}
