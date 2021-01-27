﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Milvasoft.FormFileOperations.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Milvasoft.FormFileOperations
{
    /// <summary>
    /// <see cref="IFormFile"/> operations for .NET Core.
    /// </summary>
    public static class FormFileOperations
    {
        //TODO save file to path by filename methods will be added later.

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
        /// <para> Don't forget validate file with <see cref="ValidateFile(IFormFile, FileType, long, List{string})"/>, before use this method.</para>
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
            if (file.Length <= 0) return "";

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
            var folderNameOfItem = PropertyExists<TEntity>(propertyName) ? entity.GetType().GetProperty(propertyName).GetValue(entity, null).ToString() : throw new Exception($"{propertyName} özelliği bu sınıfta bulunmamaktadır.");

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
        /// Saves uploaded IFormFile files to physical file path. If file list is null or empty returns empty <see cref="List{string}"/> 
        /// Target Path will be : "<paramref name ="basePath"></paramref>/<b><paramref name="folderNameCreator"/>()</b>/<paramref name="entity"></paramref>.<paramref name="propertyName"/>"
        /// </summary>
        /// 
        /// <para><b>Remarks:</b></para>
        /// 
        /// <remarks>
        /// 
        /// <para> Don't forget validate files with <see cref="ValidateFile(IFormFile, FileType, long, List{string})"/>, before use this method.</para>
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
            if (files.IsNullOrEmpty()) return new List<string>();

            //Gets file extension.
            var fileExtension = Path.GetExtension(files.First().FileName);

            //Gets the class name. E.g. If class is ProductDTO then sets the value of this variable as "Product".
            var folderNameOfClass = folderNameCreator.Invoke(entity.GetType());

            //We combined the name of this class (folderNameOfClass) with the path of the basePath folder. So we created the path of the folder belonging to this class.
            var folderPathOfClass = Path.Combine(basePath, folderNameOfClass);

            //Since each data belonging to this class (folderNameOfClass) will have a separate folder, we received the Id of the data sent.
            var folderNameOfItem = PropertyExists<TEntity>(propertyName) ? entity.GetType().GetProperty(propertyName).GetValue(entity, null).ToString() : throw new ArgumentException($"Type of {entity.GetType().Name}'s properties doesn't contain '{propertyName}'.");

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

                DirectoryInfo directory = new DirectoryInfo(folderPathOfItem);

                var directoryFiles = directory.GetFiles();
                int markerNo = directoryFiles.IsNullOrEmpty() ? 1 : directoryFiles.Max(fileInDir => Convert.ToInt32(Path.GetFileNameWithoutExtension(fileInDir.FullName).Split('_')[1]));

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
        /// Saves uploaded IFormFile files to physical file path. If file list is null or empty returns empty <see cref="List{string}"/> 
        /// Target Path will be : "<paramref name ="basePath"></paramref>/<b><paramref name="folderNameCreator"/>()</b>/<paramref name="entity"></paramref>.<paramref name="propertyName"/>"
        /// </summary>
        /// 
        /// <para><b>Remarks:</b></para>
        /// 
        /// <remarks>
        /// 
        /// <para> Don't forget validate files with <see cref="ValidateFile(IFormFile, FileType, long, List{string})"/>, before use this method.</para>
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
            if (files.IsNullOrEmpty()) return new List<string>();
            //Gets file extension.
            var fileExtension = Path.GetExtension(files.First().FileName);

            //Gets the class name. E.g. If class is ProductDTO then sets the value of this variable as "Product".
            var folderNameOfClass = folderNameCreator.Invoke(entity.GetType());

            //We combined the name of this class (folderNameOfClass) with the path of the basePath folder. So we created the path of the folder belonging to this class.
            var folderPathOfClass = Path.Combine(basePath, folderNameOfClass);

            //Since each data belonging to this class (folderNameOfClass) will have a separate folder, we received the Id of the data sent.
            var folderNameOfItem = PropertyExists<TEntity>(propertyName) ? entity.GetType().GetProperty(propertyName).GetValue(entity, null).ToString() : throw new ArgumentException($"Type of {entity.GetType().Name}'s properties doesn't contain '{propertyName}'.");

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

                DirectoryInfo directory = new DirectoryInfo(folderPathOfItem);

                var directoryFiles = directory.GetFiles();
                int markerNo = directoryFiles.IsNullOrEmpty() ? 1 : directoryFiles.Max(fileInDir => Convert.ToInt32(Path.GetFileNameWithoutExtension(fileInDir.FullName).Split('_')[1]));

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
        /// Checks that the file compatible the upload rules.
        /// </summary>
        /// <param name="file"> Uploaded file. </param>
        /// <param name="fileType"> Uploaded file type. (e.g image,video,sound..) </param>
        /// <param name="maxFileSize"> Maximum file size in bytes of uploaded file. </param>
        /// <param name="allowedFileExtensions"> Allowed file extensions for <paramref name="fileType"/>. </param>
        public static FileValidationResult ValidateFile(this IFormFile file, FileType fileType, long maxFileSize, List<string> allowedFileExtensions)
        {
            if (file == null) return FileValidationResult.NullFile;

            if (allowedFileExtensions == null)
                allowedFileExtensions = GetDefaultFileExtensions(fileType);

            var fileExtension = Path.GetExtension(file.FileName);

            if (!allowedFileExtensions.Contains(fileExtension))
                return FileValidationResult.InvalidFileExtension;

            // Get length of file in bytes
            long fileSizeInBytes = file.Length;

            if (fileSizeInBytes > maxFileSize)
                return FileValidationResult.FileSizeTooBig;

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
            if (string.IsNullOrEmpty(originalFilePath))
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
            if (string.IsNullOrEmpty(path))
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
        /// <param name="milvaBase64"></param>
        /// <returns></returns>
        public static IFormFile ConvertToFormFile(string milvaBase64)
        {
            var base64String = milvaBase64.Split(";base64,")?[1];

            var regex = @"[^:]\w+\/[\w-+\d.]+(?=;|,)";

            var contentType = GetExtension(base64String, regex);

            var splittedContentType = contentType.Split('/');

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

        /// <summary>
        /// Removes the folder the file is in.
        /// </summary>
        /// <param name="filePath"></param>
        public static void RemoveDirectoryFileIsIn(string filePath)
        {
            if (File.Exists(filePath)) Directory.Delete(Path.GetDirectoryName(filePath), true);
        }

        /// <summary>
        /// Removes file.
        /// </summary>
        /// <param name="filePath"></param>
        public static void RemoveFileByPath(string filePath)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        /// <summary>
        /// Removes file.
        /// </summary>
        /// <param name="filePaths"></param>
        public static void RemoveFilesByPath(List<string> filePaths)
        {
            if (!filePaths.IsNullOrEmpty())
                foreach (var filePath in filePaths)
                    if (File.Exists(filePath)) File.Delete(filePath);
        }

        /// <summary>
        /// Delete all files in directory on <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="folderPath"></param>
        public static void RemoveFilesInFolder(string folderPath)
        {
            DirectoryInfo directory = new DirectoryInfo(folderPath);

            var files = directory != null ? directory.EnumerateFiles() : null;

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
            DirectoryInfo directory = new DirectoryInfo(folderPath);

            var files = directory != null ? directory.EnumerateFiles() : null;

            if (!files.IsNullOrEmpty())
                foreach (FileInfo file in files)
                    if (!fileNames.IsNullOrEmpty())
                        if (fileNames.Contains(file.Name))
                            file.Delete();
        }

        /// <summary>
        /// Delete all directories in directory on <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="folderPath"></param>
        public static void RemoveDirectoriesInFolder(string folderPath)
        {
            DirectoryInfo directory = new DirectoryInfo(folderPath);

            var directories = directory != null ? directory.EnumerateDirectories() : null;

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
            DirectoryInfo directory = new DirectoryInfo(folderPath);
            var directories = directory != null ? directory.EnumerateDirectories() : null;

            if (!directories.IsNullOrEmpty())
                foreach (DirectoryInfo dir in directory.EnumerateDirectories())
                    if (!directoryNames.IsNullOrEmpty())
                        if (directoryNames.Contains(dir.Name))
                            dir.Delete(true);
        }

        /// <summary>
        /// Delete all files and directories in directory on <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="folderPath"></param>
        public static void RemoveDirectoriesAndFolder(string folderPath)
        {
            DirectoryInfo directory = new DirectoryInfo(folderPath);

            if (directory != null)
            {
                foreach (FileInfo file in directory.EnumerateFiles())
                    file.Delete();

                foreach (DirectoryInfo dir in directory.EnumerateDirectories())
                    dir.Delete(true);
            }

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


        #region Private Helper Methods

        /// <summary>
        /// Allows learning of the file type.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetContentType(string path) => GetMimeTypes()[Path.GetExtension(path).ToLowerInvariant()];

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
        private static Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".css", "text/css"},
                {".avi", "video/x-msvideo"},
                {".css", "text/css"},
                {".css", "text/css"},
                {".css", "text/css"},
                {".mp3", "audio/mpeg"},
                {".mpkg", "application/vnd.apple.installer+xml"},
                {".ppt", "application/vnd.ms-powerpoint"},
                {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
                {".rar", "application/vnd.rar"},
                {".svg", "image/svg+xml"},
                {".tif", "image/tiff"},
                {".tiff", "image/tiff"},
                {".webm", "video/webm"},
                {".wav", "audio/wav"},
                {".weba", "audio/webm"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".zip", "application/zip"},
                {".7z", "application/x-7z-compressed"},
            };
        }

        /// <summary>
        /// Gets default defined file extensions by <paramref name="fileType"/>.
        /// </summary>
        /// <param name="fileType"></param>
        /// <returns></returns>
        private static List<string> GetDefaultFileExtensions(FileType fileType)
        {
            return new Dictionary<FileType, string>
            {
               {FileType.Image, ".ai"},                {FileType.Video, ".3g2"},             {FileType.Audio, ".aif"},                       {FileType.EMail, ".email"},
               {FileType.Image, ".bmp"},               {FileType.Video, ".3gp"},             {FileType.Audio, ".cda"},                       {FileType.EMail, ".eml"},
               {FileType.Image, ".gif"},               {FileType.Video, ".avi"},             {FileType.Audio, ".mid"},                       {FileType.EMail, ".emlx"},
               {FileType.Image, ".ico"},               {FileType.Video, ".avi"},             {FileType.Audio, ".mp3"},                       {FileType.EMail, ".msg"},
               {FileType.Image, ".jpeg"},              {FileType.Video, ".h264"},            {FileType.Audio, ".mpa"},                       {FileType.EMail, ".oft"},
               {FileType.Image, ".jpg"},               {FileType.Video, ".m4v"},             {FileType.Audio, ".ogg"},                       {FileType.EMail, ".ost"},
               {FileType.Image, ".png"},               {FileType.Video, ".mkv"},             {FileType.Audio, ".wav"},                       {FileType.EMail, ".pst"},
               {FileType.Image, ".ps"},                {FileType.Video, ".mov"},             {FileType.Audio, ".wma"},                       {FileType.EMail, ".vcf"},
               {FileType.Image, ".svg"},               {FileType.Video, ".mp4"},             {FileType.Audio, ".wpl"},
               {FileType.Image, ".tif"},               {FileType.Video, ".mpg"},
               {FileType.Image, ".tiff"},              {FileType.Video, ".mpeg"},
                                                        {FileType.Video, ".wmv"},



               {FileType.Document, ".doc"},           {FileType.Compressed, ".7z"},          {FileType.InternetRelated, ".ai"},              {FileType.Font, ".fnt"},
               {FileType.Document, ".docx"},          {FileType.Compressed, ".arj"},         {FileType.InternetRelated, ".bmp"},             {FileType.Font, ".fon"},
               {FileType.Document, ".odt"},           {FileType.Compressed, ".deb"},         {FileType.InternetRelated, ".gif"},             {FileType.Font, ".otf"},
               {FileType.Document, ".pdf"},           {FileType.Compressed, ".pkg"},         {FileType.InternetRelated, ".ico"},             {FileType.Font, ".ttf"},
               {FileType.Document, ".rtf"},           {FileType.Compressed, ".rar"},         {FileType.InternetRelated, ".jpeg"},
               {FileType.Document, ".tex"},           {FileType.Compressed, ".rpm"},         {FileType.InternetRelated, ".jpg"},
               {FileType.Document, ".txt"},           {FileType.Compressed, ".zip"},         {FileType.InternetRelated, ".png"},
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
        }

        /// <summary>
        /// Checks <paramref name="propertyName"/> exists in <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static bool PropertyExists<T>(string propertyName) => typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase |
                                                                                                          BindingFlags.Public |
                                                                                                          BindingFlags.Instance) != null;

        /// <summary>
		/// Basically a Path.Combine for URLs. Ensures exactly one '/' separates each segment,and exactly on '&amp;' separates each query parameter.
		///	URL-encodes illegal characters but not reserved characters.
		/// </summary>
		/// <param name="parts">URL parts to combine.</param>
		private static string Combine(params string[] parts)
        {
            if (parts == null)
                throw new ArgumentNullException(nameof(parts));

            string result = "";
            bool inQuery = false, inFragment = false;

            string CombineEnsureSingleSeparator(string a, string b, char separator)
            {
                if (string.IsNullOrEmpty(a)) return b;
                if (string.IsNullOrEmpty(b)) return a;
                return a.TrimEnd(separator) + separator + b.TrimStart(separator);
            }

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part))
                    continue;

                if (result.EndsWith("?") || part.StartsWith("?"))
                    result = CombineEnsureSingleSeparator(result, part, '?');
                else if (result.EndsWith("#") || part.StartsWith("#"))
                    result = CombineEnsureSingleSeparator(result, part, '#');
                else if (inFragment)
                    result += part;
                else if (inQuery)
                    result = CombineEnsureSingleSeparator(result, part, '&');
                else
                    result = CombineEnsureSingleSeparator(result, part, '/');

                if (part.Contains("#"))
                {
                    inQuery = false;
                    inFragment = true;
                }
                else if (!inFragment && part.Contains("?"))
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
            if (string.IsNullOrEmpty(s))
                return s;

            if (encodeSpaceAsPlus)
                s = s.Replace(" ", "+");

            // Uri.EscapeUriString mostly does what we want - encodes illegal characters only - but it has a quirk
            // in that % isn't illegal if it's the start of a %-encoded sequence https://stackoverflow.com/a/47636037/62600

            // no % characters, so avoid the regex overhead
            if (!s.Contains("%"))
                return Uri.EscapeUriString(s);

            // pick out all %-hex-hex matches and avoid double-encoding 
            return Regex.Replace(s, "(.*?)((%[0-9A-Fa-f]{2})|$)", c =>
            {
                var a = c.Groups[1].Value; // group 1 is a sequence with no %-encoding - encode illegal characters
                var b = c.Groups[2].Value; // group 2 is a valid 3-character %-encoded sequence - leave it alone!
                return Uri.EscapeUriString(a) + b;
            });
        }

        /// <summary>
        /// Checks whether or not collection is null or empty. Assumes collection can be safely enumerated multiple times.
        /// </summary>
        private static bool IsNullOrEmpty(this IEnumerable @this) => @this == null || @this.GetEnumerator().MoveNext() == false;

        #endregion
    }
}
