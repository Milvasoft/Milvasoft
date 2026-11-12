using Microsoft.AspNetCore.Http;
using Milvasoft.FileOperations.Enums;
using Milvasoft.Storage.Models;
using System.Text.RegularExpressions;

namespace Milvasoft.FileOperations.Concrete;

/// <summary>
/// <see cref="IFormFile"/> operations for .NET 3.1+. 
/// </summary>
public static partial class FormFileOperations
{
    #region Public Properties

    /// <summary>
    /// File name creator delegate.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public delegate string FilesFolderNameCreator(Type type);

    #endregion

    /// <summary>
    /// Saves uploaded IFormFile file to physical file path. If <paramref name="file"/>.Lenght is lower or equal than 0 then returns empty string.
    /// Target Path will be : "<paramref name ="basePath"></paramref>/<b><paramref name="folderNameCreator"/>()</b>/<paramref name="entity"></paramref>.<paramref name="propertyName"/>"
    /// </summary>
    /// 
    /// <para><b>Remarks:</b></para>
    /// 
    /// <remarks>
    /// 
    /// <para> Don't forget validate file with <see cref="ValidateFile(IFormFile, long, List{string}, FileType)"/>, before use this method.</para>
    /// 
    /// </remarks>
    /// 
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="file"> Uploaded file in entity. </param>
    /// <param name="entity"></param>
    /// <param name="basePath"></param>
    /// <param name="folderNameCreator"></param>
    /// <param name="propertyName"></param>
    /// <returns> Completed Task </returns>
    public static async Task<string> SaveFileToPathAsync<TEntity>(this IFormFile file,
                                                                  TEntity entity,
                                                                  string basePath,
                                                                  FilesFolderNameCreator folderNameCreator,
                                                                  string propertyName)
    {
        if (file.Length <= 0)
            return "";

        //Gets file extension.
        var fileExtension = Path.GetExtension(file.FileName);

        //Gets the class name. E.g. If class is ProductDTO then sets the value of this variable as "Product".
        var folderNameOfClass = folderNameCreator.Invoke(entity.GetType());

        //We combined the name of this class (folderNameOfClass) with the path of the basePath folder. So we created the path of the folder belonging to this class.
        var folderPathOfClass = Path.Combine(basePath, folderNameOfClass);

        //If there is no such folder in this path (folderPathOfClass), we created it.
        if (!Directory.Exists(folderPathOfClass))
            Directory.CreateDirectory(folderPathOfClass);

        //Since each data belonging to this class (folderNameOfClass) will have a separate folder, we received the Id of the data sent.
        var folderNameOfItem = CommonHelper.PropertyExists<TEntity>(propertyName)
                                ? entity.GetType().GetProperty(propertyName).GetValue(entity, null).ToString()
                                : throw new MilvaDeveloperException("PropertyNotExists");

        //We created the path to the folder of this Id (folderNameOfItem).
        var folderPathOfItem = Path.Combine(folderPathOfClass, folderNameOfItem);

        Directory.CreateDirectory(folderPathOfItem);

        var fileNameWithExtension = $"{folderNameOfItem}{fileExtension}";
        var filePathOfItem = Path.Combine(folderPathOfItem, fileNameWithExtension);
        var tempFilePath = Path.Combine(folderPathOfItem, $".{Guid.NewGuid():N}.tmp");

        try
        {
            // Write the new file first so a failed upload never destroys the previous file.
            await using (var fileStream = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await file.CopyToAsync(fileStream);
            }

            // The folder holds a single file per item; remove the previous one (it may have another extension).
            foreach (var oldFile in Directory.EnumerateFiles(folderPathOfItem).Where(f => f != tempFilePath && f != filePathOfItem).ToList())
                File.Delete(oldFile);

            File.Move(tempFilePath, filePathOfItem, overwrite: true);

            return filePathOfItem;
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    /// <summary>
    /// Saves uploaded IFormFile files to physical file path. If file list is null or empty returns empty <see cref="List{String}"/> 
    /// Target Path will be : "<paramref name ="basePath"></paramref>/<b><paramref name="folderNameCreator"/>()</b>/<paramref name="entity"></paramref>.<paramref name="propertyName"/>"
    /// </summary>
    /// 
    /// <para><b>Remarks:</b></para>
    /// 
    /// <remarks>
    /// 
    /// <para> Don't forget validate files with <see cref="ValidateFile(IFormFile, long, List{string}, FileType)"/>, before use this method.</para>
    /// 
    /// </remarks>
    /// 
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="files"> Uploaded file in entity. </param>
    /// <param name="entity"></param>
    /// <param name="basePath"></param>
    /// <param name="folderNameCreator"></param>
    /// <param name="propertyName"></param>
    /// <returns> Completed Task </returns>
    public static async Task<List<string>> SaveFilesToPathAsync<TEntity>(this List<IFormFile> files,
                                                                         TEntity entity,
                                                                         string basePath,
                                                                         FilesFolderNameCreator folderNameCreator,
                                                                         string propertyName)
    {
        if (files.IsNullOrEmpty())
            return [];

        //Gets the class name. E.g. If class is ProductDTO then sets the value of this variable as "Product".
        var folderNameOfClass = folderNameCreator.Invoke(entity.GetType());

        //We combined the name of this class (folderNameOfClass) with the path of the basePath folder. So we created the path of the folder belonging to this class.
        var folderPathOfClass = Path.Combine(basePath, folderNameOfClass);

        //Since each data belonging to this class (folderNameOfClass) will have a separate folder, we received the Id of the data sent.
        var folderNameOfItem = CommonHelper.PropertyExists<TEntity>(propertyName)
                                ? entity.GetType().GetProperty(propertyName).GetValue(entity, null).ToString()
                                : throw new MilvaDeveloperException("PropertyNotExists");

        //We created the path to the folder of this Id (folderNameOfItem).
        var folderPathOfItem = Path.Combine(folderPathOfClass, folderNameOfItem);

        Directory.CreateDirectory(folderPathOfItem);

        // Existing files are named "{item}_{marker}{extension}". Files with other names (e.g. a GUID) are ignored.
        var markerNo = Directory.EnumerateFiles(folderPathOfItem)
                                .Select(f => int.TryParse(Path.GetFileNameWithoutExtension(f).Split('_')[^1], out var marker) ? marker : 0)
                                .DefaultIfEmpty(0)
                                .Max() + 1;

        var folderPaths = new List<string>(files.Count);

        try
        {
            foreach (var item in files)
            {
                var fileNameWithExtension = $"{folderNameOfItem}_{markerNo}{Path.GetExtension(item.FileName)}";
                var filePathOfItem = Path.Combine(folderPathOfItem, fileNameWithExtension);

                await using (var fileStream = new FileStream(filePathOfItem, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    folderPaths.Add(filePathOfItem);

                    await item.CopyToAsync(fileStream);
                }

                markerNo++;
            }

            return folderPaths;
        }
        catch (Exception)
        {
            // Remove only the files written by this call; other files of the item stay untouched.
            foreach (var writtenFile in folderPaths.Where(File.Exists))
                File.Delete(writtenFile);

            throw;
        }
    }

    /// <summary>
    /// Saves uploaded IFormFile files to physical file path. If file list is null or empty returns empty <see cref="List{String}"/> 
    /// Target Path will be : "<paramref name ="basePath"></paramref>/<b><paramref name="folderNameCreator"/>()</b>/<paramref name="entity"></paramref>.<paramref name="propertyName"/>"
    /// </summary>
    /// 
    /// <para><b>Remarks:</b></para>
    /// 
    /// <remarks>
    /// 
    /// <para> Don't forget validate files with <see cref="ValidateFile(IFormFile, long, List{string}, FileType)"/>, before use this method.</para>
    /// 
    /// </remarks>
    /// 
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="files"> Uploaded file in entity. </param>
    /// <param name="entity"></param>
    /// <param name="basePath"></param>
    /// <param name="folderNameCreator"></param>
    /// <param name="propertyName"></param>
    /// <returns> Completed Task </returns>
    public static Task<List<string>> SaveFilesToPathAsync<TEntity>(this IFormFileCollection files,
                                                                   TEntity entity,
                                                                   string basePath,
                                                                   FilesFolderNameCreator folderNameCreator,
                                                                   string propertyName)
        => SaveFilesToPathAsync([.. files], entity, basePath, folderNameCreator, propertyName);

    /// <summary>
    /// Saves uploaded IFormFile files to physical file path. If file list is null or empty returns empty <see cref="List{String}"/> 
    /// Target Path will be : "<paramref name ="basePath"></paramref>/<b><paramref name="folderNameCreator"/>()</b>/<paramref name="entity"></paramref>.<paramref name="propertyName"/>"
    /// </summary>
    /// 
    /// <para><b>Remarks:</b></para>
    /// 
    /// <remarks>
    /// 
    /// <para> Don't forget validate files with <see cref="ValidateFile(IFormFile, long, List{string}, FileType)"/>, before use this method.</para>
    /// 
    /// </remarks>
    /// 
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TFileDTO"></typeparam>
    /// <typeparam name="TFileEntity"></typeparam>
    /// <param name="fileDtoList"> Uploaded file in entity. </param>
    /// <param name="entity"></param>
    /// <param name="basePath"></param>
    /// <param name="folderNameCreator"></param>
    /// <param name="propertyName"></param>
    /// <returns> Completed Task </returns>
    public static async Task<List<TFileEntity>> SaveFilesToPathAsync<TEntity, TFileDTO, TFileEntity>(this List<TFileDTO> fileDtoList,
                                                                                                      TEntity entity,
                                                                                                      string basePath,
                                                                                                      FilesFolderNameCreator folderNameCreator,
                                                                                                      string propertyName)
    where TFileDTO : class, IFileDto
    where TFileEntity : class, IFileEntity, new()
    {
        if (fileDtoList.IsNullOrEmpty())
            return [];

        var fileEntities = new List<TFileEntity>();

        foreach (var fileDto in fileDtoList)
        {
            if (fileDto == null)
                continue;

            var file = fileDto.File;

            if (file.Length <= 0)
                continue;

            var path = await file.SaveFileToPathAsync(entity, basePath, folderNameCreator, propertyName);

            if (!string.IsNullOrWhiteSpace(path))
                fileEntities.Add(new TFileEntity
                {
                    FileName = fileDto.FileName,
                    FilePath = path
                });
        }

        return fileEntities;
    }

    /// <summary>
    /// Checks that the file compatible the upload rules. 
    /// File extension validation is not case sensitive.
    /// </summary>
    /// <param name="file"> Uploaded file. </param>
    /// <param name="fileType"> Uploaded file type. (e.g image,video,sound..) </param>
    /// <param name="maxFileSize"> Maximum file size in bytes of uploaded file. </param>
    /// <param name="allowedFileExtensions"> Allowed file extensions for <paramref name="fileType"/>. </param>
    public static FileValidationResult ValidateFile(this IFormFile file, long maxFileSize, List<string> allowedFileExtensions, FileType fileType = FileType.Document)
    {
        if (file == null)
            return FileValidationResult.NullFile;

        allowedFileExtensions ??= GetDefaultFileExtensions(fileType);

        var fileExtension = Path.GetExtension(file.FileName);

        // Ordinal, culture-independent comparison: "GIF".ToLower() is "gıf" on tr-TR.
        if (string.IsNullOrEmpty(fileExtension) || !allowedFileExtensions.Exists(e => string.Equals(e, fileExtension, StringComparison.OrdinalIgnoreCase)))
            return FileValidationResult.InvalidFileExtension;

        // Get length of file in bytes
        long fileSizeInBytes = file.Length;

        if (fileSizeInBytes > maxFileSize)
            return FileValidationResult.FileSizeTooBig;

        return FileValidationResult.Valid;
    }

    /// <summary>
    /// Checks that the files compatible the upload rules. 
    /// File extension validation is not case sensitive.
    /// </summary>
    /// <param name="files"> Uploaded files. </param>
    /// <param name="fileType"> Uploaded file type. (e.g image,video,sound..) </param>
    /// <param name="maxFileSize"> Maximum file size in bytes of uploaded file. </param>
    /// <param name="allowedFileExtensions"> Allowed file extensions for <paramref name="fileType"/>. </param>
    public static FileValidationResult ValidateFiles(this List<IFormFile> files, long maxFileSize, List<string> allowedFileExtensions, FileType fileType = FileType.Document)
    {
        if (files.IsNullOrEmpty())
            return FileValidationResult.NullFile;

        foreach (var file in files)
        {
            var validationResult = file.ValidateFile(maxFileSize, allowedFileExtensions, fileType);

            if (validationResult != FileValidationResult.Valid)
                return validationResult;
        }

        return FileValidationResult.Valid;
    }

    /// <summary>
    /// Returns the path of the uploaded file.
    /// </summary>
    /// <param name="originalFilePath"> Uploaded file. </param>
    /// <param name="requestPath"> Request path section. (e.g. api/ImageLibrary) </param>
    /// <returns> "api/ImageLibrary/1/1.jpeg" </returns>
    public static string GetFileUrlPathSectionFromFilePath(string originalFilePath, string requestPath)
    {
        if (string.IsNullOrWhiteSpace(originalFilePath))
            return string.Empty;

        var fileNameWithExtension = Path.GetFileName(originalFilePath);

        var parentDirectory = Directory.GetParent(originalFilePath);

        var fileNameWithoutExtension = parentDirectory.Name;

        var parentFolderNameOfOriginalFile = parentDirectory.Parent.Name;

        return Combine(requestPath, parentFolderNameOfOriginalFile, fileNameWithoutExtension, fileNameWithExtension);
    }

    /// <summary>
    /// Gets file as <see cref="IFormFile"/> from requested path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileType"></param>
    /// <returns></returns>
    public static async Task<IFormFile> GetFileFromPathAsync(string path, FileType fileType)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        // The returned FormFile reads from this stream later, so it must not be disposed here.
        var memory = new MemoryStream(await File.ReadAllBytesAsync(path), writable: false);

        var file = new FormFile(memory, 0, memory.Length, fileType.ToString(), Path.GetFileName(path))
        {
            Headers = new HeaderDictionary(),
            ContentType = GetContentType(path)
        };

        return file;
    }

    /// <summary>
    /// Converts data URI formatted base64 string to IFormFile.
    /// </summary>
    /// <param name="dataUriBase64"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">If <paramref name="dataUriBase64"/> is not a base64 data URI.</exception>
    public static IFormFile ConvertToFormFile(string dataUriBase64)
    {
        var parts = dataUriBase64?.Split(";base64,");

        if (parts == null || parts.Length != 2)
            throw new ArgumentException("Value is not a base64 data URI.", nameof(dataUriBase64));

        var base64String = parts[1];

        var regex = @"[^:]\w+\/[\w-+\d.]+(?=;|,)";

        var contentType = GetExtension(dataUriBase64, regex);

        var splittedContentType = contentType?.Split('/');

        var fileType = splittedContentType?[0];

        var fileExtension = splittedContentType?.Length > 1 ? splittedContentType[1] : string.Empty;

        var array = Convert.FromBase64String(base64String);

        var memoryStream = new MemoryStream(array)
        {
            Position = 0
        };

        return new FormFile(memoryStream, 0, memoryStream.Length, fileType, $"File.{fileExtension}")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    /// <summary>
    /// Converts form file to byte array.
    /// </summary>
    /// <param name="formFile"></param>
    /// <returns></returns>
    public static async Task<byte[]> ConvertFormFileToByteArrayAsync(IFormFile formFile)
    {
        using var memoryStream = new MemoryStream();

        await formFile.CopyToAsync(memoryStream);

        return memoryStream.ToArray();
    }

    #region File & Folder Remove Operations

    /// <summary>
    /// Removes the folder the file is in.
    /// </summary>
    /// <param name="filePath"></param>
    public static void RemoveDirectoryFileIsIn(string filePath)
    {
        if (File.Exists(filePath))
            Directory.Delete(Path.GetDirectoryName(filePath), true);
    }

    /// <summary>
    /// Removes file.
    /// </summary>
    /// <param name="filePath"></param>
    public static void RemoveFileByPath(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    /// <summary>
    /// Removes file.
    /// </summary>
    /// <param name="filePaths"></param>
    public static void RemoveFilesByPath(List<string> filePaths)
    {
        if (!filePaths.IsNullOrEmpty())
            foreach (var filePath in filePaths.Where(File.Exists))
                File.Delete(filePath);
    }

    /// <summary>
    /// Delete all files in directory on <paramref name="folderPath"/>.
    /// </summary>
    /// <param name="folderPath"></param>
    public static void RemoveFilesInFolder(string folderPath)
    {
        DirectoryInfo directory = new(folderPath);

        var files = directory.EnumerateFiles();

        if (!files.IsNullOrEmpty())
            foreach (FileInfo file in files)
                file.Delete();
    }

    /// <summary>
    /// Delete matching files in directory on <paramref name="folderPath"/>.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="fileNames"> File names should contains extension. </param>
    public static void RemoveFilesInFolder(string folderPath, List<string> fileNames)
    {
        DirectoryInfo directory = new(folderPath);

        var files = directory.EnumerateFiles();

        if (!files.IsNullOrEmpty())
            foreach (FileInfo file in files)
                if (!fileNames.IsNullOrEmpty() && fileNames.Contains(file.Name))
                    file.Delete();
    }

    /// <summary>
    /// Delete all directories in directory on <paramref name="folderPath"/>.
    /// </summary>
    /// <param name="folderPath"></param>
    public static void RemoveDirectoriesInFolder(string folderPath)
    {
        DirectoryInfo directory = new(folderPath);

        var directories = directory.EnumerateDirectories();

        if (!directories.IsNullOrEmpty())
            foreach (DirectoryInfo dir in directories)
                dir.Delete(true);
    }

    /// <summary>
    /// Delete matching directories in directory on <paramref name="folderPath"/>.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="directoryNames"></param>
    public static void RemoveDirectoriesInFolder(string folderPath, List<string> directoryNames)
    {
        DirectoryInfo directory = new(folderPath);

        var directories = directory.EnumerateDirectories();

        if (!directories.IsNullOrEmpty())
            foreach (DirectoryInfo dir in directory.EnumerateDirectories())
                if (!directoryNames.IsNullOrEmpty() && directoryNames.Contains(dir.Name))
                    dir.Delete(true);
    }

    /// <summary>
    /// Delete all files and directories in directory on <paramref name="folderPath"/>.
    /// </summary>
    /// <param name="folderPath"></param>
    public static void RemoveDirectoriesAndFolder(string folderPath)
    {
        DirectoryInfo directory = new(folderPath);

        foreach (FileInfo file in directory.EnumerateFiles())
            file.Delete();

        foreach (DirectoryInfo dir in directory.EnumerateDirectories())
            dir.Delete(true);
    }

    /// <summary>
    /// It is used to change the folder and name information of the uploaded file.
    /// </summary>
    /// <param name="oldPath"> Old file path. </param>
    /// <param name="newId"> New id of item. </param>
    public static string RenameFolderAndFileName(string oldPath, Guid newId)
    {
        var oldFolderPath = Directory.GetParent(oldPath).FullName;

        if (!Directory.Exists(oldFolderPath))
            return "";

        var parentFolderPathOfOriginalFile = Directory.GetParent(oldPath).Parent.FullName;

        var newFolderPath = Path.Combine(parentFolderPathOfOriginalFile, $"{newId}");

        var newFilePath = Path.Combine(newFolderPath, $"{newId}{Path.GetExtension(oldPath)}");

        Directory.CreateDirectory(newFolderPath);

        File.Move(oldPath, newFilePath);

        Directory.Delete(oldFolderPath, true);

        return newFilePath;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Allows learning of the file type.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static string GetContentType(string path) => GetMimeTypes(Path.GetExtension(path).ToLowerInvariant());

    /// <summary>
    /// Checks if <paramref name="input"/> matches <paramref name="regexString"/>.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="regexString"></param>
    /// <returns></returns>
    private static string GetExtension(string input, string regexString) => new Regex(regexString, RegexOptions.None, TimeSpan.FromSeconds(1)).Match(input[..Math.Min(input.Length, 256)]).Captures?.FirstOrDefault()?.Value;

    /// <summary>
    /// File types to be accepted.
    /// </summary>
    private static string GetMimeTypes(string extension) => extension switch
    {
        ".txt" => MimeTypeNames.TextPlain,
        ".pdf" => MimeTypeNames.ApplicationPdf,
        ".doc" => MimeTypeNames.ApplicationMsword,
        ".docx" => MimeTypeNames.ApplicationMsword,
        ".xls" => MimeTypeNames.ApplicationVndMsExcel,
        ".png" => MimeTypeNames.ImagePng,
        ".jpg" => MimeTypeNames.ImageJpeg,
        ".jpeg" => MimeTypeNames.ImageJpeg,
        ".gif" => MimeTypeNames.ImageGif,
        ".csv" => MimeTypeNames.TextCsv,
        ".css" => MimeTypeNames.TextCss,
        ".mp3" => MimeTypeNames.AudioMpeg,
        ".ppt" => MimeTypeNames.ApplicationVndMsPowerpoint,
        ".pptx" => MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentPresentationmlPresentation,
        ".rar" => MimeTypeNames.ApplicationXRarCompressed,
        ".svg" => MimeTypeNames.ImageSvgXml,
        ".tif" => MimeTypeNames.ImageTiff,
        ".tiff" => MimeTypeNames.ImageTiff,
        ".webm" => MimeTypeNames.VideoWebm,
        ".weba" => MimeTypeNames.AudioWebm,
        ".xlsx" => MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet,
        ".zip" => MimeTypeNames.ApplicationZip,
        _ => "",
    };

    /// <summary>
    /// Gets default defined file extensions by <paramref name="fileType"/>.
    /// </summary>
    /// <param name="fileType"></param>
    /// <returns></returns>
    private static List<string> GetDefaultFileExtensions(FileType fileType) => _defaultFileExtensions.TryGetValue(fileType, out var extensions) ? [.. extensions] : [];

    private static readonly Dictionary<FileType, string[]> _defaultFileExtensions = new()
    {
        [FileType.Image] = [".ai", ".bmp", ".gif", ".ico", ".jpeg", ".jpg", ".png", ".ps", ".svg", ".tif", ".tiff"],
        [FileType.Video] = [".3g2", ".3gp", ".h264", ".m4v", ".mkv", ".mov", ".mp4", ".mpg", ".mpeg", ".wmv"],
        [FileType.Audio] = [".aif", ".cda", ".mid", ".mp3", ".mpa", ".ogg", ".wav", ".wma", ".wpl"],
        [FileType.EMail] = [".email", ".eml", ".emlx", ".msg", ".oft", ".ost", ".pst", ".vcf"],
        [FileType.Document] = [".doc", ".docx", ".odt", ".pdf", ".rtf", ".tex", ".txt", ".wpd", ".ods", ".xls", ".xlsm", ".xlsx", ".key", ".odp", ".pps", ".ppt", ".pptx"],
        [FileType.Compressed] = [".arj", ".deb", ".pkg", ".rar", ".rpm", ".zip"],
        [FileType.InternetRelated] = [".ai", ".bmp", ".gif", ".ico", ".jpeg", ".jpg", ".png", ".ps", ".psd", ".svg", ".tif", ".tiff"],
        [FileType.Font] = [".fnt", ".fon", ".otf", ".ttf"],
    };

    /// <summary>
    /// URL-encodes illegal characters but not reserved characters.
    /// </summary>
    /// <param name="parts">URL parts to combine.</param>
    private static string Combine(params string[] parts)
    {
        ArgumentNullException.ThrowIfNull(parts);

        var result = string.Empty;
        bool inQuery = false, inFragment = false;

        static string CombineEnsureSingleSeparator(string a, string b, char separator)
        {
            if (string.IsNullOrWhiteSpace(a))
                return b;
            if (string.IsNullOrWhiteSpace(b))
                return a;
            return a.TrimEnd(separator) + separator + b.TrimStart(separator);
        }

        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
                continue;

            // Each step yields the whole combined string, so it replaces the previous result (appending duplicated it).
            if (result.EndsWith('?') || part.StartsWith('?'))
                result = CombineEnsureSingleSeparator(result, part, '?');
            else if (result.EndsWith('#') || part.StartsWith('#'))
                result = CombineEnsureSingleSeparator(result, part, '#');
            else if (inFragment)
                result += part;
            else if (inQuery)
                result = CombineEnsureSingleSeparator(result, part, '&');
            else
                result = CombineEnsureSingleSeparator(result, part, '/');

            if (part.Contains('#'))
            {
                inQuery = false;
                inFragment = true;
            }
            else if (!inFragment && part.Contains('?'))
            {
                inQuery = true;
            }
        }

        return EncodeIllegalCharacters(result);
    }

    /// <summary>
    /// URL-encodes characters in a string that are neither reserved nor unreserved (RFC 3986).
    /// Reserved characters such as '/' and '?' are kept. '%' is kept if it begins a %-hex-hex sequence (avoids double-encoding).
    /// </summary>
    /// <param name="s">The string to encode.</param>
    /// <param name="encodeSpaceAsPlus">If true, spaces will be encoded as + signs. Otherwise, they'll be encoded as %20.</param>
    /// <returns>The encoded URL.</returns>
    private static string EncodeIllegalCharacters(string s, bool encodeSpaceAsPlus = false)
    {
        if (string.IsNullOrWhiteSpace(s))
            return s;

        if (encodeSpaceAsPlus)
            s = s.Replace(" ", "+");

        var builder = new StringBuilder(s.Length);
        Span<byte> utf8 = stackalloc byte[4];

        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];

            if (IsUnreservedOrReserved(c) || c == '%' && i + 2 < s.Length && Uri.IsHexDigit(s[i + 1]) && Uri.IsHexDigit(s[i + 2]))
            {
                builder.Append(c);
                continue;
            }

            var charCount = char.IsHighSurrogate(c) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]) ? 2 : 1;
            var byteCount = Encoding.UTF8.GetBytes(s.AsSpan(i, charCount), utf8);

            foreach (var b in utf8[..byteCount])
                builder.Append('%').Append(b.ToString("X2", System.Globalization.CultureInfo.InvariantCulture));

            i += charCount - 1;
        }

        return builder.ToString();

        static bool IsUnreservedOrReserved(char c) => char.IsAsciiLetterOrDigit(c) || "-._~:/?#[]@!$&'()*+,;=".Contains(c);
    }

    #endregion
}
