using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Milvasoft.Storage.Models;

/// <summary>
/// Represents an file upload data transfer object.
/// </summary>
public record FileUploadRequest
{
    private readonly IFormFile _formFile = null;
    private const string _base64Flag = ";base64,";

    /// <summary>
    /// If this value is null it means image changed. If not, sometimes only Order or AltText property is changed, so this property is used to determine if the image itself has changed.
    /// </summary>
    public Guid? Id { get; set; }

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
    public byte[] File { get => !string.IsNullOrWhiteSpace(FileAsBase64) ? DataUriToPlainText() : []; }

    /// <summary>
    /// File of the content.
    /// </summary>
    [JsonIgnore]
    public IFormFile MediaAsFormFile { get => !string.IsNullOrWhiteSpace(FileAsBase64) ? _formFile ?? ConvertToFormFile() : null; }

    /// <summary>
    /// Validates the file extension against allowed extensions for the specified file type.
    /// </summary>
    /// <param name="fileType"></param>
    /// <param name="maxFileSize"></param>
    /// <param name="allowedFileExtensions"></param>
    /// <returns></returns>
    public FileOperationResult ValidateExtension(FileType fileType, List<string> allowedFileExtensions = null, long maxFileSize = 1024 * 1024)
    {
        if (MediaAsFormFile != null)
            return MediaAsFormFile.ValidateFileStrict(maxFileSize, allowedFileExtensions, fileType);

        allowedFileExtensions ??= StorageHelper.GetDefaultExtensions(fileType);
        allowedFileExtensions = allowedFileExtensions.ConvertAll(e => e.ToLowerInvariant());

        if (string.IsNullOrWhiteSpace(FileExtension) || !allowedFileExtensions.Contains(FileExtension))
            return FileOperationResult.Failure("InvalidFileExtension");

        return FileOperationResult.Success();
    }

    /// <summary>
    /// Converts data uri base64 string to plain text base64 string.
    /// </summary>
    /// <returns></returns>
    private byte[] DataUriToPlainText()
    {
        if (string.IsNullOrEmpty(FileAsBase64))
            return [];

        var splitted = FileAsBase64.Split(_base64Flag);

        if (splitted.Length != 2)
            return [];

        var base64String = splitted[1];

        var array = Convert.FromBase64String(base64String);

        return array;
    }

    /// <summary>
    /// Converts data URI formatted base64 string to IFormFile.
    /// </summary>
    /// <returns></returns>
    private IFormFile ConvertToFormFile()
    {
        if (string.IsNullOrEmpty(FileAsBase64))
            return default;

        var base64String = FileAsBase64?.Split(_base64Flag)[1];

        var contentType = GetExtension(FileAsBase64);

        var splittedContentType = contentType?.Split('/');

        var fileType = splittedContentType?[0];

        var fileExtension = splittedContentType?[1];

        var array = Convert.FromBase64String(base64String);

        var memoryStream = new MemoryStream(array)
        {
            Position = 0
        };

        return new FormFile(memoryStream, 0, memoryStream.Length, fileType, $"{FileName ?? "file"}.{fileExtension}")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static string GetExtension(string input) => new Regex(@"[^:]\w+\/[\w-+\d.]+(?=;|,)", RegexOptions.None, TimeSpan.FromSeconds(60)).Match(input).Captures?.FirstOrDefault()?.Value;
}
