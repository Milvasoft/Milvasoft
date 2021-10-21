using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.FileOperations.Concrete;
using Milvasoft.Helpers.FileOperations.Enums;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.Utils;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Utils
{
    /// <summary>
    /// Helper extensions methods for Ops!yon Project.
    /// </summary>
    public static class HelperExtensions
    {

        #region IFormFile Helpers

        /// <summary>
        /// <para><b>EN: </b>Save uploaded IFormFile file to server. Target Path will be : ".../wwwroot/Media Library/Image Library/<paramref name="entity"></paramref>.Id"</para>
        /// <para><b>TR: </b>Yüklenen IFormFile dosyasını sunucuya kaydedin. Hedef Yol: "... / wwwroot / Media Library / Image Library / <paramref name =" entity "> </paramref> .Id" olacaktır</para>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="file"> Uploaded file in entity. </param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static async Task<string> SaveImageToServerAsync<TEntity, TKey>(this IFormFile file, TEntity entity) where TEntity : AuditableEntity<AppUser, Guid, TKey>
                                                                                                                                                      where TKey : struct, IEquatable<TKey>
        {
            string basePath = GlobalConstants.ImageLibraryPath;

            FormFileOperations.FilesFolderNameCreator imagesFolderNameCreator = CreateImageFolderNameFromDTO;

            string propertyName = "Id";

            int maxFileLength = 14000000;

            var allowedFileExtensions = GlobalConstants.AllowedFileExtensions.Find(i => i.FileType == FileType.Image.ToString()).AllowedExtensions;

            var validationResult = file.ValidateFile(maxFileLength, allowedFileExtensions, FileType.Image);

            switch (validationResult)
            {
                case FileValidationResult.Valid:
                    break;
                case FileValidationResult.FileSizeTooBig:
                    // Get length of file in bytes
                    long fileSizeInBytes = file.Length;
                    // Convert the bytes to Kilobytes (1 KB = 1024 Bytes)
                    double fileSizeInKB = fileSizeInBytes / 1024;
                    // Convert the KB to MegaBytes (1 MB = 1024 KBytes)
                    double fileSizeInMB = fileSizeInKB / 1024;
                    throw new MilvaUserFriendlyException("FileIsTooBigMessage", fileSizeInMB.ToString("0.#"));
                case FileValidationResult.InvalidFileExtension:
                    var stringBuilder = new StringBuilder();
                    throw new MilvaUserFriendlyException("UnsupportedFileTypeMessage", stringBuilder.AppendJoin(", ", allowedFileExtensions));
                case FileValidationResult.NullFile:
                    return "";
            }

            var path = await file.SaveFileToPathAsync(entity, basePath, imagesFolderNameCreator, propertyName).ConfigureAwait(false);

            await file.OpenReadStream().DisposeAsync().ConfigureAwait(false);

            return path;
        }

        /// <summary>
        /// <para><b>EN: </b>Save uploaded IFormFile file to server. Target Path will be : ".../wwwroot/Media Library/Image Library/<paramref name="entity"></paramref>.Id"</para>
        /// <para><b>TR: </b>Yüklenen IFormFile dosyasını sunucuya kaydedin. Hedef Yol: "... / wwwroot / Media Library / Image Library / <paramref name =" entity "> </paramref> .Id" olacaktır</para>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="file"> Uploaded file in entity. </param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static async Task<string> SaveVideoToServerAsync<TEntity, TKey>(this IFormFile file, TEntity entity)
        {
            string basePath = GlobalConstants.VideoLibraryPath;

            FormFileOperations.FilesFolderNameCreator imagesFolderNameCreator = CreateVideoFolderNameFromDTO;

            string propertyName = "Id";

            int maxFileLength = 140000000;

            var allowedFileExtensions = GlobalConstants.AllowedFileExtensions.Find(i => i.FileType == FileType.Video.ToString()).AllowedExtensions;

            var validationResult = file.ValidateFile(maxFileLength, allowedFileExtensions, FileType.Video);

            switch (validationResult)
            {
                case FileValidationResult.Valid:
                    break;
                case FileValidationResult.FileSizeTooBig:
                    // Get length of file in bytes
                    long fileSizeInBytes = file.Length;
                    // Convert the bytes to Kilobytes (1 KB = 1024 Bytes)
                    double fileSizeInKB = fileSizeInBytes / 1024;
                    // Convert the KB to MegaBytes (1 MB = 1024 KBytes)
                    double fileSizeInMB = fileSizeInKB / 1024;
                    throw new MilvaUserFriendlyException("FileIsTooBigMessage", fileSizeInMB.ToString("0.#"));
                case FileValidationResult.InvalidFileExtension:
                    var stringBuilder = new StringBuilder();
                    throw new MilvaUserFriendlyException("UnsupportedFileTypeMessage", stringBuilder.AppendJoin(", ", allowedFileExtensions));
                case FileValidationResult.NullFile:
                    return "";
            }

            var path = await file.SaveFileToPathAsync(entity, basePath, imagesFolderNameCreator, propertyName).ConfigureAwait(false);

            await file.OpenReadStream().DisposeAsync().ConfigureAwait(false);

            return path;
        }

        /// <summary>
        /// <para><b>EN: </b>Returns the path of the uploaded file.</para>
        /// <para><b>TR: </b>Yüklenen dosyanın yolunu döndürür.</para>
        /// </summary>
        /// <param name="originalImagePath"> Uploaded file. </param>
        /// <param name="fileType"> Uploaded file type. (e.g image,video,sound) </param>
        public static string GetFileUrlFromPath(string originalImagePath, FileType fileType)
        {
            string libraryType = string.Empty;
            switch (fileType)
            {
                case FileType.Image:
                    libraryType = "api/ImageLibrary";
                    break;
                case FileType.Video:
                    libraryType = "api/VideoLibrary";
                    break;
                case FileType.ARModel:
                    libraryType = "api/ARModelLibrary";
                    break;
                case FileType.Audio:
                    libraryType = "api/AudioLibrary";
                    break;
                case FileType.Document:
                    libraryType = "api/DocumentLibrary";
                    break;
                default:
                    break;
            }
            return FormFileOperations.GetFileUrlPathSectionFromFilePath(originalImagePath, libraryType);
        }

        /// <summary>
        /// Converts data URI formatted base64 string to IFormFile.
        /// </summary>
        /// <param name="milvaBase64"></param>
        /// <returns></returns>
        public static IFormFile ConvertToFormFile(string milvaBase64)
        {
            var splittedBase64String = milvaBase64.Split(";base64,");
            var base64String = splittedBase64String?[1];

            var contentType = splittedBase64String[0].Split(':')[1];

            var splittedContentType = contentType.Split('/');

            var fileType = splittedContentType?[0];

            var fileExtension = splittedContentType?[1];

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

        private static string CreateImageFolderNameFromDTO(Type type)
        {
            return type.Name.Split("DTO")[0] + "Images";
        }
        private static string CreateVideoFolderNameFromDTO(Type type)
        {
            return type.Name + "Videos";
        }

        #endregion


        #region IEnumerable Helpers

        /// <summary>
        /// Checks guid list. If list is null or empty return default(<typeparamref name="TDTO"/>). Otherwise invoke <paramref name="returnFunc"/>.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TDTO"></typeparam>
        /// <param name="toBeCheckedList"></param>
        /// <param name="returnFunc"></param>
        /// <returns></returns>
        public static List<TDTO> CheckList<TEntity, TDTO>(this IEnumerable<TEntity> toBeCheckedList, Func<IEnumerable<TEntity>, IEnumerable<TDTO>> returnFunc)
         where TDTO : new()
         where TEntity : class, IBaseEntity<Guid>
         => toBeCheckedList.IsNullOrEmpty() ? new List<TDTO>() : returnFunc.Invoke(toBeCheckedList).ToList();


        /// <summary>
        /// Checks guid object. If is null return default(<typeparamref name="TDTO"/>). Otherwise invoke <paramref name="returnFunc"/>.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TDTO"></typeparam>
        /// <param name="toBeCheckedObject"></param>
        /// <param name="returnFunc"></param>
        /// <returns></returns>
        public static TDTO CheckObject<TEntity, TDTO>(this TEntity toBeCheckedObject, Func<TEntity, TDTO> returnFunc)
          where TDTO : new()
          where TEntity : class, IBaseEntity<Guid>
       => toBeCheckedObject == null ? default : returnFunc.Invoke(toBeCheckedObject);

        #endregion

        #region Pagination Helpers

        /// <summary>
        /// Prepares pagination dto according to pagination parameters.
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="repository"></param>
        /// <param name="pageIndex"></param>
        /// <param name="requestedItemCount"></param>
        /// <param name="orderByProperty"></param>
        /// <param name="orderByAscending"></param>
        /// <param name="condition"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public static async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> PreparePaginationDTO<TRepository, TEntity, TKey>(this TRepository repository,
                                                                                                                                                      int pageIndex,
                                                                                                                                                      int requestedItemCount,
                                                                                                                                                      string orderByProperty = null,
                                                                                                                                                      bool orderByAscending = false,
                                                                                                                                                      Expression<Func<TEntity, bool>> condition = null,
                                                                                                                                                      Func<IIncludable<TEntity>, IIncludable> includes = null)


            where TRepository : IBaseRepository<TEntity, TKey, EducationAppDbContext>
            where TKey : struct, IEquatable<TKey>
            where TEntity : class, IBaseEntity<TKey>
        {

            return string.IsNullOrWhiteSpace(orderByProperty) ? await repository.GetAsPaginatedAsync(pageIndex,
                                                                                                requestedItemCount,
                                                                                                includes,
                                                                                                condition).ConfigureAwait(false)
                                                              : await repository.GetAsPaginatedAndOrderedAsync(pageIndex,
                                                                                                               requestedItemCount,
                                                                                                               includes,
                                                                                                               orderByProperty,
                                                                                                               orderByAscending,
                                                                                                               condition).ConfigureAwait(false);
        }

        /// <summary>
        /// Prepares pagination dto according to pagination parameters.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="repository"></param>
        /// <param name="pageIndex"></param>
        /// <param name="requestedItemCount"></param>
        /// <param name="orderByProperty"></param>
        /// <param name="orderByAscending"></param>
        /// <param name="condition"></param>
        /// <param name="includes"></param>
        /// <returns></returns>

        public static async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> PreparePaginationDTO<TEntity, TKey>(this IBaseRepository<TEntity, TKey, EducationAppDbContext> repository,
                                                                                                                                                      int pageIndex,
                                                                                                                                                      int requestedItemCount,
                                                                                                                                                      string orderByProperty = null,
                                                                                                                                                      bool orderByAscending = false,
                                                                                                                                                      Expression<Func<TEntity, bool>> condition = null,
                                                                                                                                                      Func<IIncludable<TEntity>, IIncludable> includes = null)


            where TKey : struct, IEquatable<TKey>
            where TEntity : class, IBaseEntity<TKey>
        {

            return string.IsNullOrWhiteSpace(orderByProperty) ? await repository.GetAsPaginatedAsync(pageIndex,
                                                                                                requestedItemCount,
                                                                                                includes,
                                                                                                condition).ConfigureAwait(false)
                                                              : await repository.GetAsPaginatedAndOrderedAsync(pageIndex,
                                                                                                               requestedItemCount,
                                                                                                               includes,
                                                                                                               orderByProperty,
                                                                                                               orderByAscending,
                                                                                                               condition).ConfigureAwait(false);
        }

        /// <summary>
        /// Prepares pagination dto according to pagination parameters.
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="repository"></param>
        /// <param name="pageIndex"></param>
        /// <param name="requestedItemCount"></param>
        /// <param name="orderByKeySelector"></param>
        /// <param name="orderByAscending"></param>
        /// <param name="condition"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public static async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> PreparePaginationDTO<TRepository, TEntity, TKey>(this TRepository repository,
                                                                                                                                                      int pageIndex,
                                                                                                                                                      int requestedItemCount,
                                                                                                                                                      Expression<Func<TEntity, object>> orderByKeySelector = null,
                                                                                                                                                      bool orderByAscending = false,
                                                                                                                                                      Expression<Func<TEntity, bool>> condition = null,
                                                                                                                                                      Func<IIncludable<TEntity>, IIncludable> includes = null)


            where TRepository : IBaseRepository<TEntity, TKey, EducationAppDbContext>
            where TKey : struct, IEquatable<TKey>
            where TEntity : class, IBaseEntity<TKey>
        {

            return orderByKeySelector == null ? await repository.GetAsPaginatedAsync(pageIndex,
                                                                                     requestedItemCount,
                                                                                     includes,
                                                                                     condition).ConfigureAwait(false)
                                                              : await repository.GetAsPaginatedAndOrderedAsync(pageIndex,
                                                                                                               requestedItemCount,
                                                                                                               includes,
                                                                                                               orderByKeySelector,
                                                                                                               orderByAscending,
                                                                                                               condition).ConfigureAwait(false);
        }

        #endregion

        #region Exception Helpers

        /// <summary>
        /// Throwns <see cref="MilvaUserFriendlyException"/> if <paramref name="parameterObject"/> is null.
        /// </summary>
        /// <param name="parameterObject"></param>
        /// <param name="localizerKey"></param>
        public static void ThrowIfParameterIsNull(this object parameterObject, string localizerKey = null)
        {
            if (parameterObject == null)
            {
                if (string.IsNullOrWhiteSpace(localizerKey))
                {
                    throw new MilvaUserFriendlyException(MilvaException.NullParameter);
                }
                else
                {
                    throw new MilvaUserFriendlyException(localizerKey);
                }
            }
        }

        /// <summary>
        /// Throwns <see cref="MilvaUserFriendlyException"/> if <paramref name="list"/> is null or empty.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="localizerKey"></param>
        public static void ThrowIfListIsNullOrEmpty(this List<object> list, string localizerKey = null)
        {
            if (list.IsNullOrEmpty())
            {
                if (string.IsNullOrWhiteSpace(localizerKey))
                {
                    throw new MilvaUserFriendlyException(MilvaException.CannotFindEntity);
                }
                else
                {
                    throw new MilvaUserFriendlyException(localizerKey);
                }
            }
        }

        /// <summary>
        /// Validates file. 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileType"></param>
        public static void ValidateFile(this IFormFile file, FileType fileType)
        {
            int maxFileLength = 14000000;

            var allowedFileExtensions = GlobalConstants.AllowedFileExtensions.Find(i => i.FileType == fileType.ToString()).AllowedExtensions;

            var validationResult = file.ValidateFile(maxFileLength, allowedFileExtensions, fileType);

            switch (validationResult)
            {
                case FileValidationResult.Valid:
                    break;
                case FileValidationResult.FileSizeTooBig:
                    // Get length of file in bytes
                    long fileSizeInBytes = file.Length;
                    // Convert the bytes to Kilobytes (1 KB = 1024 Bytes)
                    double fileSizeInKB = fileSizeInBytes / 1024;
                    // Convert the KB to MegaBytes (1 MB = 1024 KBytes)
                    double fileSizeInMB = fileSizeInKB / 1024;
                    throw new MilvaUserFriendlyException("FileIsTooBigMessage", fileSizeInMB.ToString("0.#"));
                case FileValidationResult.InvalidFileExtension:
                    throw new MilvaUserFriendlyException("UnsupportedFileTypeMessage", string.Join(", ", allowedFileExtensions));
                case FileValidationResult.NullFile:
                    throw new MilvaUserFriendlyException("FileCannotBeEmpty"); ;
            }
        }

        /// <summary>
        /// Throwns <see cref="MilvaUserFriendlyException"/> if <paramref name="list"/> is null or empty.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="localizerKey"></param>
        public static void ThrowIfParameterIsNullOrEmpty<T>(this List<T> list, string localizerKey = null) where T : IEquatable<T>
        {
            if (list.IsNullOrEmpty())
            {
                if (string.IsNullOrWhiteSpace(localizerKey))
                {
                    throw new MilvaUserFriendlyException(MilvaException.NullParameter);
                }
                else
                {
                    throw new MilvaUserFriendlyException(localizerKey);
                }
            }
        }

        /// <summary>
        /// Throwns <see cref="MilvaUserFriendlyException"/> if <paramref name="list"/> is null or empty.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="localizerKey"></param>
        public static void ThrowIfListIsNullOrEmpty(this IEnumerable<object> list, string localizerKey = null)
        {
            if (list.IsNullOrEmpty())
            {
                if (string.IsNullOrWhiteSpace(localizerKey))
                {
                    throw new MilvaUserFriendlyException(MilvaException.CannotFindEntity);
                }
                else
                {
                    throw new MilvaUserFriendlyException(localizerKey);
                }
            }
        }

        /// <summary>
        /// Throwns <see cref="MilvaUserFriendlyException"/> if <paramref name="list"/> is not null or empty.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="message"></param>
        public static void ThrowIfListIsNotNullOrEmpty(this IEnumerable<object> list, string message = null)
        {
            if (!list.IsNullOrEmpty())
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    throw new MilvaUserFriendlyException(MilvaException.NullParameter);
                }
                else
                {
                    throw new MilvaUserFriendlyException(message);
                }
            }
        }

        /// <summary>
        /// Gets identity result as object response.
        /// </summary>
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static async Task<IActionResult> GetActivityResponseAsync(this ConfiguredTaskAwaitable<IdentityResult> asyncTask, string successMessage, string errorMessage)
        {
            ObjectResponse<IdentityResult> response = new()
            {
                Result = await asyncTask
            };

            if (!response.Result.Succeeded)
            {
                response.Message = errorMessage;
                response.StatusCode = MilvaStatusCodes.Status600Exception;
                response.Success = false;
            }
            else
            {
                response.Message = successMessage;
                response.StatusCode = MilvaStatusCodes.Status200OK;
                response.Success = true;
            }
            return new OkObjectResult(response);
        }


        /// <summary>
        /// Throwns <see cref="MilvaUserFriendlyException"/> if <paramref name="entity"/> is null.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="localizerKey"></param>
        public static void ThrowIfNullForGuidObject<TEntity>(this TEntity entity, string localizerKey = null) where TEntity : class, IBaseEntity<Guid>
        {

            if (entity == null)
            {
                if (string.IsNullOrWhiteSpace(localizerKey))
                {
                    throw new MilvaUserFriendlyException(MilvaException.CannotFindEntity);
                }
                else
                {
                    throw new MilvaUserFriendlyException(localizerKey);
                }
            }
        }

        /// <summary>
        /// Throwns <see cref="MilvaUserFriendlyException"/> if <paramref name="entity"/> is null.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="localizerKey"></param>
        public static void ThrowIfNullForIntObject<TEntity>(this TEntity entity, string localizerKey = null) where TEntity : class, IBaseEntity<int>
        {

            if (entity == null)
            {
                if (string.IsNullOrWhiteSpace(localizerKey))
                {
                    throw new MilvaUserFriendlyException(MilvaException.CannotFindEntity);
                }
                else
                {
                    throw new MilvaUserFriendlyException(localizerKey);
                }
            }
        }

        #endregion


    }
}
