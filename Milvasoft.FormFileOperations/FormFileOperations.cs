using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Milvasoft.FormFileOperations.Enums;
using Milvasoft.FormFileOperations.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Milvasoft.FormFileOperations
{
    public static class FormFileOperations
    {
        public delegate string FilesFolderNameCreator(Type type);

        /// <summary>
        /// <para> Allowed file extensions for media files. </para>
        /// </summary>
        public static ILookup<FileTypes, string> DefaultAllowedExtensions { get; } = new Dictionary<FileTypes, string>
        {
           {FileTypes.Image, ".ai"},                {FileTypes.Video, ".3g2"},             {FileTypes.Audio, ".aif"},                       {FileTypes.EMail, ".email"},
           {FileTypes.Image, ".bmp"},               {FileTypes.Video, ".3gp"},             {FileTypes.Audio, ".cda"},                       {FileTypes.EMail, ".eml"},
           {FileTypes.Image, ".gif"},               {FileTypes.Video, ".avi"},             {FileTypes.Audio, ".mid"},                       {FileTypes.EMail, ".emlx"},
           {FileTypes.Image, ".ico"},               {FileTypes.Video, ".avi"},             {FileTypes.Audio, ".mp3"},                       {FileTypes.EMail, ".msg"},
           {FileTypes.Image, ".jpeg"},              {FileTypes.Video, ".h264"},            {FileTypes.Audio, ".mpa"},                       {FileTypes.EMail, ".oft"},
           {FileTypes.Image, ".jpg"},               {FileTypes.Video, ".m4v"},             {FileTypes.Audio, ".ogg"},                       {FileTypes.EMail, ".ost"},
           {FileTypes.Image, ".png"},               {FileTypes.Video, ".mkv"},             {FileTypes.Audio, ".wav"},                       {FileTypes.EMail, ".pst"},
           {FileTypes.Image, ".ps"},                {FileTypes.Video, ".mov"},             {FileTypes.Audio, ".wma"},                       {FileTypes.EMail, ".vcf"},
           {FileTypes.Image, ".svg"},               {FileTypes.Video, ".mp4"},             {FileTypes.Audio, ".wpl"},
           {FileTypes.Image, ".tif"},               {FileTypes.Video, ".mpg"},
           {FileTypes.Image, ".tiff"},              {FileTypes.Video, ".mpeg"},
                                                    {FileTypes.Video, ".wmv"},



           {FileTypes.Document, ".doc"},           {FileTypes.Compressed, ".7z"},          {FileTypes.InternetRelated, ".ai"},              {FileTypes.Font, ".fnt"},
           {FileTypes.Document, ".docx"},          {FileTypes.Compressed, ".arj"},         {FileTypes.InternetRelated, ".bmp"},             {FileTypes.Font, ".fon"},
           {FileTypes.Document, ".odt"},           {FileTypes.Compressed, ".deb"},         {FileTypes.InternetRelated, ".gif"},             {FileTypes.Font, ".otf"},
           {FileTypes.Document, ".pdf"},           {FileTypes.Compressed, ".pkg"},         {FileTypes.InternetRelated, ".ico"},             {FileTypes.Font, ".ttf"},
           {FileTypes.Document, ".rtf"},           {FileTypes.Compressed, ".rar"},         {FileTypes.InternetRelated, ".jpeg"},
           {FileTypes.Document, ".tex"},           {FileTypes.Compressed, ".rpm"},         {FileTypes.InternetRelated, ".jpg"},
           {FileTypes.Document, ".txt"},           {FileTypes.Compressed, ".zip"},         {FileTypes.InternetRelated, ".png"},
           {FileTypes.Document, ".wpd"},                                                   {FileTypes.InternetRelated, ".ps"},
           {FileTypes.Document, ".ods"},                                                   {FileTypes.InternetRelated, ".psd"},
           {FileTypes.Document, ".xls"},                                                   {FileTypes.InternetRelated, ".svg"},
           {FileTypes.Document, ".xlsm"},                                                  {FileTypes.InternetRelated, ".tif"},
           {FileTypes.Document, ".xlsx"},                                                  {FileTypes.InternetRelated, ".tiff"},
           {FileTypes.Document, ".key"},
           {FileTypes.Document, ".odp"},
           {FileTypes.Document, ".pps"},
           {FileTypes.Document, ".ppt"},
           {FileTypes.Document, ".pptx"},
        }.ToLookup(i => i.Key, i => i.Value);


        /// <summary>
        /// <para><b>EN: </b>Save uploaded IFormFile file to physical file path. Target Path will be : "<paramref name ="basePath"></paramref>/<b><paramref name="folderNameCreator"/>()</b>/<paramref name="entity"></paramref>.<paramref name="propertyName"/>"</para>
        /// <para><b>TR: </b>Yüklenen IFormFile dosyasını fiziksel bir dosya yoluna kaydedin. Hedef Yol: <paramref name ="basePath"></paramref>/<b><paramref name="folderNameCreator"/>()</b>/<paramref name ="entity"></paramref>.<paramref name="propertyName"/>" olacaktır</para>
        /// </summary>
        /// 
        /// <para><b>Remarks:</b></para>
        /// 
        /// <remarks>
        /// 
        /// <para> Don't forget validate file with <see cref="ValidateFile(IFormFile, FileTypes, long, List{string})"/>, before use this method.</para>
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="file"> Uploaded file in entity. </param>
        /// <param name="entity"></param>
        /// <param name="basePath"></param>
        /// <param name="folderNameCreator"></param>
        /// <param name="propertyName"></param>
        /// <param name="maxFileLength"></param>
        /// <param name="fileTypeEnum"></param>
        /// <returns> Completed Task </returns>
        public static async Task<string> SaveFileToPathAsync<TEntity>(this IFormFile file,
                                                                                TEntity entity,
                                                                                    string basePath,
                                                                                        FilesFolderNameCreator folderNameCreator,
                                                                                            string propertyName)
        {
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

        /// <summary>
        /// <para><b>EN: </b>Checks that the file compatible the upload rules.</para>
        /// <para><b>TR: </b>Dosyanın yükleme kurallarıyla uyumlu olup olmadığını kontrol eder.</para>
        /// </summary>
        /// <param name="file"> Uploaded file. </param>
        /// <param name="fileType"> Uploaded file type. (e.g image,video,sound..) </param>
        /// <param name="maxFileSize"> Maximum file size in bytes of uploaded file. </param>
        public static FileValidationResult ValidateFile(this IFormFile file, FileTypes fileType, long maxFileSize, List<string> allowedFileExtensions)
        {
            if (file == null) return FileValidationResult.NullFile;

            if (allowedFileExtensions == null)
                allowedFileExtensions = GetDefaultFileExtensions(fileType);

            var fileExtension = Path.GetExtension(file.FileName);

            if (!allowedFileExtensions.Contains(fileExtension))
                return FileValidationResult.FileSizeTooBig;

            // Get length of file in bytes
            long fileSizeInBytes = file.Length;

            if (fileSizeInBytes > maxFileSize)
                return FileValidationResult.InvalidFileExtension;

            return FileValidationResult.Valid;
        }

        /// <summary>
        /// <para><b>EN: </b>Returns the path of the uploaded file.</para>
        /// <para><b>TR: </b>Yüklenen dosyanın yolunu döndürür.</para>
        /// </summary>
        /// <param name="originalFilePath"> Uploaded file. </param>
        /// <param name="requestPath"> Request path section. (e.g. api/ImageLibrary) </param>
        /// <returns> "api/ImageLibrary/1/1.jpeg" </returns>
        public static string GetFileUrlPathSectionFromFilePath(string originalFilePath, string requestPath)
        {
            if (string.IsNullOrEmpty(originalFilePath))
                return string.Empty;

            var fileNameWithExtension = Path.GetFileName(originalFilePath);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFilePath);
            var parentFolderNameOfOriginalFile = Directory.GetParent(originalFilePath).Parent.Name;

            return Combine(requestPath, parentFolderNameOfOriginalFile, fileNameWithoutExtension, fileNameWithExtension);
        }

        /// <summary>
        /// <para><b>EN: </b>Gets image file in requested path.</para>
        /// <para><b>TR: </b>İstenen yolda görüntü dosyasını alır.</para>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<IFormFile> GetFileFromPathAsync(string path,FileTypes fileType)
        {
            var memory = new MemoryStream();
            if (string.IsNullOrEmpty(path))
                return null;
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
        /// <para><b>EN: </b>Removes the folder the image is in.</para> 
        /// <para><b>TR: </b>Görüntünün bulunduğu klasörü kaldırır.</para> 
        /// </summary>
        /// <param name="filePath"></param>
        public static void RemoveDirectoryFileIsIn(string filePath)
        {
            if (File.Exists(filePath)) Directory.Delete(Path.GetDirectoryName(filePath), true);
        }

        /// <summary>
        /// <para><b>EN: </b>It is used to change the folder and name information of the uploaded file.</para>
        /// <para><b>TR: </b>Yüklenen dosyanın klasör ve isim bilgisini değiştirmeye yarar</para>
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
        /// <para><b>EN: </b>Allows learning of the file type.</para>
        /// <para><b>TR: </b>Dosya tipinin ögrenilmesini saglar.</para>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        /// <summary>
        /// <para><b>EN: </b>File types to be accepted.</para>
        /// <para><b>TR: </b>Kabul edilecn dosya tipleri.</para>
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
        private static List<string> GetDefaultFileExtensions(FileTypes fileType)
        {
            return DefaultAllowedExtensions.Where(i => i.Key == fileType).Select(i => i.First()).ToList();
        }

        /// <summary>
        /// Checks <paramref name="propertyName"/> exists in <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static bool PropertyExists<T>(string propertyName)
        {
            return typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase |
                                                       BindingFlags.Public | BindingFlags.Instance) != null;
        }

        /// <summary>
		/// <para>Basically a Path.Combine for URLs. Ensures exactly one '/' separates each segment,and exactly on '&amp;' separates each query parameter.
		///		  URL-encodes illegal characters but not reserved characters.</para>
		/// 
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
        /// <para> URL-encodes characters in a string that are neither reserved nor unreserved. Avoids encoding reserved characters such as '/' and '?'. Avoids encoding '%' if it begins a %-hex-hex sequence (i.e. avoids double-encoding).</para>
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
            return Regex.Replace(s, "(.*?)((%[0-9A-Fa-f]{2})|$)", c => {
                var a = c.Groups[1].Value; // group 1 is a sequence with no %-encoding - encode illegal characters
                var b = c.Groups[2].Value; // group 2 is a valid 3-character %-encoded sequence - leave it alone!
                return Uri.EscapeUriString(a) + b;
            });
        }

        #endregion
    }
}
