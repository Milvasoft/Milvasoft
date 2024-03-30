using Microsoft.AspNetCore.Http;
using Milvasoft.FileOperations.Enums;
using System.Text.RegularExpressions;

namespace Milvasoft.FileOperations.Concrete;

/// <summary>
/// <see cref="IFormFile"/> operations for .NET Core.
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

        try
        {
            //If there is no such folder in this path (folderPathOfItem), we created it.
            if (!Directory.Exists(folderPathOfItem))
                Directory.CreateDirectory(folderPathOfItem);
            else
            {
                Directory.Delete(folderPathOfItem, true);
                Directory.CreateDirectory(folderPathOfItem);
            }

            var fileNameWithExtension = $"{folderNameOfItem}{fileExtension}";
            var filePathOfItem = Path.Combine(folderPathOfItem, fileNameWithExtension);
            using (var fileStream = new FileStream(filePathOfItem, FileMode.Create))
            {
                await file.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            return filePathOfItem;
        }
        catch (Exception)
        {
            Directory.Delete(folderPathOfItem, true);
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
    public static async Task<List<string>> SaveFilesToPathAsync<TEntity>(this List<IFormFile> files,
                                                                         TEntity entity,
                                                                         string basePath,
                                                                         FilesFolderNameCreator folderNameCreator,
                                                                         string propertyName)
    {
        if (files.IsNullOrEmpty())
            return [];

        //Gets file extension.
        var fileExtension = Path.GetExtension(files.First().FileName);

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

        try
        {
            //If there is no such folder in this path (folderPathOfClass), we created it.
            if (!Directory.Exists(folderPathOfClass))
                Directory.CreateDirectory(folderPathOfClass);

            //If there is no such folder in this path (folderPathOfItem), we created it.
            if (!Directory.Exists(folderPathOfItem))
                Directory.CreateDirectory(folderPathOfItem);

            DirectoryInfo directory = new(folderPathOfItem);

            var directoryFiles = directory.GetFiles();

            int markerNo = directoryFiles.IsNullOrEmpty()
                            ? 1
                            : directoryFiles.Max(fileInDir => Convert.ToInt32(Path.GetFileNameWithoutExtension(fileInDir.FullName).Split('_')[1])) + 1;

            var folderPaths = new List<string>();

            foreach (var item in files)
            {
                var fileNameWithExtension = $"{folderNameOfItem}_{markerNo}{fileExtension}";
                var filePathOfItem = Path.Combine(folderPathOfItem, fileNameWithExtension);
                using (var fileStream = new FileStream(filePathOfItem, FileMode.Create))
                {
                    await item.CopyToAsync(fileStream).ConfigureAwait(false);
                }

                folderPaths.Add(filePathOfItem);
                markerNo++;
            }

            return folderPaths;
        }
        catch (Exception)
        {
            Directory.Delete(folderPathOfClass);
            Directory.Delete(folderPathOfItem);
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
    public static async Task<List<string>> SaveFilesToPathAsync<TEntity>(this IFormFileCollection files,
                                                                         TEntity entity,
                                                                         string basePath,
                                                                         FilesFolderNameCreator folderNameCreator,
                                                                         string propertyName)
    {
        if (files.IsNullOrEmpty())
            return [];

        //Gets file extension.
        var fileExtension = Path.GetExtension(files[0].FileName);

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

        try
        {
            //If there is no such folder in this path (folderPathOfClass), we created it.
            if (!Directory.Exists(folderPathOfClass))
                Directory.CreateDirectory(folderPathOfClass);

            //If there is no such folder in this path (folderPathOfItem), we created it.
            if (!Directory.Exists(folderPathOfItem))
                Directory.CreateDirectory(folderPathOfItem);

            DirectoryInfo directory = new(folderPathOfItem);

            var directoryFiles = directory.GetFiles();

            int markerNo = directoryFiles.IsNullOrEmpty()
                            ? 1
                            : directoryFiles.Max(fileInDir => Convert.ToInt32(Path.GetFileNameWithoutExtension(fileInDir.FullName).Split('_')[1])) + 1;

            var folderPaths = new List<string>();

            foreach (var item in files)
            {
                var fileNameWithExtension = $"{folderNameOfItem}_{markerNo}{fileExtension}";
                var filePathOfItem = Path.Combine(folderPathOfItem, fileNameWithExtension);
                using (var fileStream = new FileStream(filePathOfItem, FileMode.Create))
                {
                    await item.CopyToAsync(fileStream).ConfigureAwait(false);
                }

                folderPaths.Add(filePathOfItem);
                markerNo++;
            }

            return folderPaths;
        }
        catch (Exception)
        {
            Directory.Delete(folderPathOfClass);
            Directory.Delete(folderPathOfItem);
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
    /// <typeparam name="TFileDTO"></typeparam>
    /// <typeparam name="TFileEntity"></typeparam>
    /// <param name="fileDTOList"> Uploaded file in entity. </param>
    /// <param name="entity"></param>
    /// <param name="basePath"></param>
    /// <param name="folderNameCreator"></param>
    /// <param name="propertyName"></param>
    /// <returns> Completed Task </returns>
    public static async Task<List<TFileEntity>> SaveFilesToPathAsync<TEntity, TFileDTO, TFileEntity>(this List<TFileDTO> fileDTOList,
                                                                                                      TEntity entity,
                                                                                                      string basePath,
                                                                                                      FilesFolderNameCreator folderNameCreator,
                                                                                                      string propertyName)
    where TFileDTO : class, IFileDTO
    where TFileEntity : class, IFileEntity, new()
    {
        if (fileDTOList.IsNullOrEmpty())
            return [];

        var fileEntities = new List<TFileEntity>();

        foreach (var fileDTO in fileDTOList)
        {
            if (fileDTO == null)
                continue;

            var file = fileDTO.File;

            if (file.Length <= 0)
                continue;

            var path = await file.SaveFileToPathAsync(entity, basePath, folderNameCreator, propertyName).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(path))
                fileEntities.Add(new TFileEntity
                {
                    FileName = fileDTO.FileName,
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

        allowedFileExtensions = allowedFileExtensions.ConvertAll(d => d.ToLower());

        var fileExtension = Path.GetExtension(file.FileName);

        if (!allowedFileExtensions.Contains(fileExtension.ToLower()))
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

        var memory = new MemoryStream();

        using (var stream = new FileStream(path, FileMode.Open))
        {
            await stream.CopyToAsync(memory).ConfigureAwait(false);
        }

        memory.Position = 0;

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
    public static IFormFile ConvertToFormFile(string dataUriBase64)
    {
        var base64String = dataUriBase64?.Split(";base64,")[1];

        var regex = @"[^:]\w+\/[\w-+\d.]+(?=;|,)";

        var contentType = GetExtension(base64String, regex);

        var splittedContentType = contentType?.Split('/');

        var fileType = splittedContentType?[0];

        var fileExtension = splittedContentType?[1];

        var array = Convert.FromBase64String(base64String);

        using var memoryStream = new MemoryStream(array)
        {
            Position = 0
        };

        return new FormFile(memoryStream, 0, memoryStream.Length, fileType, $"File.{fileExtension}")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
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
            foreach (var filePath in filePaths)
                if (File.Exists(filePath))
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
    private static string GetExtension(string input, string regexString) => new Regex(regexString).Match(input).Captures?.FirstOrDefault()?.Value;

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
    private static List<string> GetDefaultFileExtensions(FileType fileType) => new Dictionary<FileType, string>
    {
       {FileType.Image, ".ai"},                {FileType.Video, ".3g2"},             {FileType.Audio, ".aif"},                       {FileType.EMail, ".email"},
       {FileType.Image, ".bmp"},               {FileType.Video, ".3gp"},             {FileType.Audio, ".cda"},                       {FileType.EMail, ".eml"},
       {FileType.Image, ".gif"},               {FileType.Video, ".h264"},            {FileType.Audio, ".mid"},                       {FileType.EMail, ".emlx"},
       {FileType.Image, ".ico"},               {FileType.Video, ".m4v"},             {FileType.Audio, ".mp3"},                       {FileType.EMail, ".msg"},
       {FileType.Image, ".jpeg"},              {FileType.Video, ".mkv"},             {FileType.Audio, ".mpa"},                       {FileType.EMail, ".oft"},
       {FileType.Image, ".jpg"},               {FileType.Video, ".mov"},             {FileType.Audio, ".ogg"},                       {FileType.EMail, ".ost"},
       {FileType.Image, ".png"},               {FileType.Video, ".mp4"},             {FileType.Audio, ".wav"},                       {FileType.EMail, ".pst"},
       {FileType.Image, ".ps"},                {FileType.Video, ".mpg"},             {FileType.Audio, ".wma"},                       {FileType.EMail, ".vcf"},
       {FileType.Image, ".svg"},               {FileType.Video, ".mpeg"},            {FileType.Audio, ".wpl"},
       {FileType.Image, ".tif"},               {FileType.Video, ".wmv"},
       {FileType.Image, ".tiff"},

       {FileType.Document, ".doc"},           {FileType.Compressed, ".arj"},         {FileType.InternetRelated, ".ai"},              {FileType.Font, ".fnt"},
       {FileType.Document, ".docx"},          {FileType.Compressed, ".deb"},         {FileType.InternetRelated, ".bmp"},             {FileType.Font, ".fon"},
       {FileType.Document, ".odt"},           {FileType.Compressed, ".pkg"},         {FileType.InternetRelated, ".gif"},             {FileType.Font, ".otf"},
       {FileType.Document, ".pdf"},           {FileType.Compressed, ".rar"},         {FileType.InternetRelated, ".ico"},             {FileType.Font, ".ttf"},
       {FileType.Document, ".rtf"},           {FileType.Compressed, ".rpm"},         {FileType.InternetRelated, ".jpeg"},
       {FileType.Document, ".tex"},           {FileType.Compressed, ".zip"},         {FileType.InternetRelated, ".jpg"},
       {FileType.Document, ".txt"},                                                  {FileType.InternetRelated, ".png"},
       {FileType.Document, ".wpd"},                                                  {FileType.InternetRelated, ".ps"},
       {FileType.Document, ".ods"},                                                  {FileType.InternetRelated, ".psd"},
       {FileType.Document, ".xls"},                                                  {FileType.InternetRelated, ".svg"},
       {FileType.Document, ".xlsm"},                                                 {FileType.InternetRelated, ".tif"},
       {FileType.Document, ".xlsx"},                                                 {FileType.InternetRelated, ".tiff"},
       {FileType.Document, ".key"},
       {FileType.Document, ".odp"},
       {FileType.Document, ".pps"},
       {FileType.Document, ".ppt"},
       {FileType.Document, ".pptx"},

    }.ToLookup(i => i.Key, i => i.Value).Where(i => i.Key == fileType).Select(i => i.First()).ToList();

    /// <summary>
    /// Basically a Path.Combine for URLs. Ensures exactly one '/' separates each segment,and exactly on '&amp;' separates each query parameter.
    ///	URL-encodes illegal characters but not reserved characters.
    /// </summary>
    /// <param name="parts">URL parts to combine.</param>
    private static string Combine(params string[] parts)
    {
        ArgumentNullException.ThrowIfNull(parts);

        string result = "";
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
    /// URL-encodes characters in a string that are neither reserved nor unreserved. 
    /// Avoids encoding reserved characters such as '/' and '?'. Avoids encoding '%' if it begins a %-hex-hex sequence (i.e. avoids double-encoding).
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

        // Uri.EscapeUriString mostly does what we want - encodes illegal characters only - but it has a quirk
        // in that % isn't illegal if it's the start of a %-encoded sequence https://stackoverflow.com/a/47636037/62600

        // no % characters, so avoid the regex overhead
        if (!s.Contains('%'))
            return Uri.EscapeDataString(s);

        // pick out all %-hex-hex matches and avoid double-encoding 
        return IllegalCharsRegex().Replace(s, c =>
        {
            var a = c.Groups[1].Value; // group 1 is a sequence with no %-encoding - encode illegal characters
            var b = c.Groups[2].Value; // group 2 is a valid 3-character %-encoded sequence - leave it alone!
            return Uri.EscapeDataString(a) + b;
        });
    }

    [GeneratedRegex("(.*?)((%[0-9A-Fa-f]{2})|$)")]
    private static partial Regex IllegalCharsRegex();

    #endregion
}
