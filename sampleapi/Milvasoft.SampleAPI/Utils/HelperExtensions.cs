using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.FileOperations.Concrete;
using Milvasoft.Helpers.FileOperations.Enums;
using Milvasoft.SampleAPI.AppStartup;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Localization;
using Milvasoft.SampleAPI.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
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
        /// <param name="stringLocalizer"></param>
        /// <returns></returns>
        public static async Task<string> SaveImageToServerAsync<TEntity, TKey>(this IFormFile file, TEntity entity, IStringLocalizer stringLocalizer) where TEntity : AuditableEntity<AppUser, Guid, TKey>
                                                                                                                                                      where TKey : struct, IEquatable<TKey>
        {
            string basePath = GlobalConstants.ImageLibraryPath;

            FormFileOperations.FilesFolderNameCreator imagesFolderNameCreator = CreateImageFolderNameFromDTO;

            string propertyName = "Id";

            int maxFileLength = 14000000;

            var allowedFileExtensions = GlobalConstants.AllowedFileExtensions.Find(i => i.FileType == FileType.Image.ToString()).AllowedExtensions;

            var validationResult = file.ValidateFile(FileType.Image, maxFileLength, allowedFileExtensions);

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
                    throw new MilvaValidationException(stringLocalizer["FileIsTooBigMessage", fileSizeInMB.ToString("0.#")]);
                case FileValidationResult.InvalidFileExtension:
                    var stringBuilder = new StringBuilder();
                    throw new MilvaValidationException(stringLocalizer["UnsupportedFileTypeMessage", stringBuilder.AppendJoin(", ", allowedFileExtensions)]);
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
        /// <param name="stringLocalizer"></param>
        /// <returns></returns>
        public static async Task<string> SaveVideoToServerAsync<TEntity, TKey>(this IFormFile file, TEntity entity, IStringLocalizer stringLocalizer)
        {
            string basePath = GlobalConstants.VideoLibraryPath;

            FormFileOperations.FilesFolderNameCreator imagesFolderNameCreator = CreateVideoFolderNameFromDTO;

            string propertyName = "Id";

            int maxFileLength = 140000000;

            var allowedFileExtensions = GlobalConstants.AllowedFileExtensions.Find(i => i.FileType == FileType.Video.ToString()).AllowedExtensions;

            var validationResult = file.ValidateFile(FileType.Video, maxFileLength, allowedFileExtensions);

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
                    throw new MilvaUserFriendlyException(stringLocalizer["FileIsTooBigMessage", fileSizeInMB.ToString("0.#")]);
                case FileValidationResult.InvalidFileExtension:
                    var stringBuilder = new StringBuilder();
                    throw new MilvaUserFriendlyException(stringLocalizer["UnsupportedFileTypeMessage", stringBuilder.AppendJoin(", ", allowedFileExtensions)]);
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

        #region Language Helpers

        private const string SystemLanguageIdString = "SystemLanguageId";

        /// <summary>
        /// <para><b>EN: </b>Ready mapping is done. For example, it is used to map the data in the Product class to the ProductDTO class.</para>
        /// <para><b>TR: </b>Hazır mapleme işlemi yapılır. Örnegin Product sınıfındaki verileri ProductDTO sınıfına maplemeye yarar.</para>
        /// </summary>
        /// <param name="langs"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string GetLang<TEntity>(this IEnumerable<TEntity> langs, Expression<Func<TEntity, string>> propertyName)
        {
            var requestedLangId = GetLanguageId();

            if (langs.IsNullOrEmpty()) return "";

            var propName = propertyName.GetPropertyName();

            TEntity requestedLang;

            if (requestedLangId != GlobalConstants.DefaultLanguageId) requestedLang = langs.FirstOrDefault(lang => (int)lang.GetType().GetProperty(SystemLanguageIdString).GetValue(lang) == requestedLangId)
                                                                                        ?? langs.FirstOrDefault(lang => (int)lang.GetType().GetProperty(SystemLanguageIdString).GetValue(lang) == GlobalConstants.DefaultLanguageId);

            else requestedLang = langs.FirstOrDefault(lang => (int)lang.GetType().GetProperty(SystemLanguageIdString).GetValue(lang) == GlobalConstants.DefaultLanguageId);

            requestedLang ??= langs.FirstOrDefault();

            return requestedLang.GetType().GetProperty(propName).GetValue(requestedLang, null)?.ToString();
        }

        /// <summary>
        /// <para><b>EN: </b>Ready mapping is done. For example, it is used to map the data in the Product class to the ProductDTO class.</para>
        /// <para><b>TR: </b>Hazır mapleme işlemi yapılır. Örnegin Product sınıfındaki verileri ProductDTO sınıfına maplemeye yarar.</para>
        /// </summary>
        /// <param name="langs"></param>
        /// <returns></returns>
        public static IEnumerable<TDTO> GetLangs<TEntity, TDTO>(this IEnumerable<TEntity> langs) where TDTO : new()
        {
            if (langs.IsNullOrEmpty()) yield break;

            foreach (var lang in langs)
            {
                TDTO dto = new TDTO();
                foreach (var entityProp in lang.GetType().GetProperties())
                {
                    var dtoProp = dto.GetType().GetProperty(entityProp.Name);

                    var entityPropValue = entityProp.GetValue(lang, null);

                    if (entityProp.Name == SystemLanguageIdString) dtoProp.SetValue(dto, entityPropValue, null);

                    else if (entityProp.PropertyType == typeof(string)) dtoProp.SetValue(dto, entityPropValue, null);
                }
                yield return dto;
            }
        }


        /// <summary>
        /// Stores language id and iso code.
        /// </summary>
        public static Dictionary<string, int> LanguageIdIsoPairs = new Dictionary<string, int>();

        /// <summary>
        /// <para><b>EN: </b>Gets language id from CultureInfo.CurrentCulture.</para>
        /// <para><b>TR: </b>Dilin id'sini isteğin CultureInfo.CurrentCulture'den alır.</para>
        /// </summary>
        public static int GetLanguageId()
        {
            var culture = Thread.CurrentThread.CurrentCulture;
            if (LanguageIdIsoPairs.ContainsKey(culture.Name))
                return LanguageIdIsoPairs[culture.Name];
            else
                return GlobalConstants.DefaultLanguageId;
        }

        #endregion

        #region HttpContextAccessor Helpers

        /// <summary>
        /// <para><b>EN: </b>Gets institution id from request's header. Then returns that id variable.</para>
        /// <para><b>TR: </b> İşletme idsini request header'dan alır. Daha sonra bu kimliği geriye döndürür.</para>
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="localizer"></param>
        public static (int pageIndex, int itemCount) GetPaginationVariablesFromHeader(this IHttpContextAccessor httpContextAccessor, IStringLocalizer<SharedResource> localizer)
        {
            if (httpContextAccessor.HttpContext.Request.Headers.Keys.Contains("pageindex") && httpContextAccessor.HttpContext.Request.Headers.Keys.Contains("itemcount"))
            {
                int pageIndex;
                httpContextAccessor.HttpContext.Request.Headers.TryGetValue("pageindex", out var pageIndexValue);
                if (!pageIndexValue.IsNullOrEmpty())
                {
                    pageIndex = Convert.ToInt32(pageIndexValue[0]);
                    if (pageIndex <= GlobalConstants.Zero) throw new MilvaUserFriendlyException(localizer["InvalidPageIndexException"]);
                }
                else
                    throw new MilvaUserFriendlyException(localizer["MissingHeaderException", "PageIndex"]);

                int itemCount;
                httpContextAccessor.HttpContext.Request.Headers.TryGetValue("itemcount", out var itemCountValue);
                if (!itemCountValue.IsNullOrEmpty())
                {
                    itemCount = Convert.ToInt32(itemCountValue[0]);
                    if (itemCount <= GlobalConstants.Zero) throw new MilvaUserFriendlyException(localizer["InvalidItemRangeException"]);
                }
                else
                    throw new MilvaUserFriendlyException(localizer["MissingHeaderException", "ItemCount"]);

                return (pageIndex: pageIndex, itemCount: itemCount);
            }
            else throw new MilvaUserFriendlyException(localizer["MissingHeaderException", "PageIndex,ItemCount"]);
        }

        #endregion

        #region Default Record Check Helpers

        /// <summary>
        /// Checks <paramref name="id"/> is default record id or not.
        /// </summary>
        /// 
        /// <exception cref="MilvaUserFriendlyException"> Throwns when <paramref name="id"/> is defult record id. </exception>
        /// 
        /// <param name="id"></param>
        /// <param name="stringLocalizer"></param>
        public static void CheckContentIsDefaultRecord(this int id, IStringLocalizer stringLocalizer)
        {
            if (id > GlobalConstants.Zero && id < 50) throw new MilvaUserFriendlyException(stringLocalizer, MilvaExceptionCode.CannotUpdateOrDeleteDefaultRecordException);
        }

        /// <summary>
        /// Checks <paramref name="idList"/> contains default record or not.
        /// </summary>
        /// 
        /// <exception cref="MilvaUserFriendlyException"> Throwns when contents contains defult record id. </exception>
        /// 
        /// <param name="idList"></param>
        /// <param name="stringLocalizer"></param>
        public static void CheckContentIsDefaultRecord(this List<int> idList, IStringLocalizer stringLocalizer)
        {
            if (idList.Any(i => i > GlobalConstants.Zero && i < 50)) throw new MilvaUserFriendlyException(stringLocalizer, MilvaExceptionCode.CannotUpdateOrDeleteDefaultRecordException);
        }

        #endregion

        #region Reflection Helpers

        /// <summary>
        /// Get langs property in runtime.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="langPropName"></param>
        /// <param name="requestedPropName"></param>
        /// <param name="stringLocalizer"></param>
        /// <returns></returns>
        public static dynamic GetLangPropValue(this object obj, string langPropName, string requestedPropName, IStringLocalizer stringLocalizer)
        {
            var langValues = obj.GetType().GetProperty(langPropName)?.GetValue(obj, null) ?? throw new MilvaUserFriendlyException(stringLocalizer, MilvaExceptionCode.InvalidParameterException);

            var enumerator = langValues.GetType().GetMethod("GetEnumerator").Invoke(langValues, null);
            enumerator.GetType().GetMethod("MoveNext").Invoke(enumerator, null);
            var entityType = enumerator.GetType().GetProperty("Current").GetValue(enumerator, null).GetType();

            MethodInfo langMethod = typeof(HelperExtensions).GetMethod("GetLang", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(entityType);

            return langMethod.Invoke(langValues, new object[] { langValues, requestedPropName });
        }

        /// <summary>
        /// <para><b>EN: </b>Ready mapping is done. For example, it is used to map the data in the Product class to the ProductDTO class.</para>
        /// <para><b>TR: </b>Hazır mapleme işlemi yapılır. Örnegin Product sınıfındaki verileri ProductDTO sınıfına maplemeye yarar.</para>
        /// </summary>
        /// <param name="langs"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        private static string GetLang<TEntity>(this HashSet<TEntity> langs, string propName)
        {
            var requestedLangId = GetLanguageId();

            if (langs.IsNullOrEmpty()) return "";

            TEntity requestedLang;

            if (requestedLangId != GlobalConstants.DefaultLanguageId) requestedLang = langs.FirstOrDefault(lang => (int)lang.GetType().GetProperty(SystemLanguageIdString).GetValue(lang) == requestedLangId)
                                                                                        ?? langs.FirstOrDefault(lang => (int)lang.GetType().GetProperty(SystemLanguageIdString).GetValue(lang) == GlobalConstants.DefaultLanguageId);

            else requestedLang = langs.FirstOrDefault(lang => (int)lang.GetType().GetProperty(SystemLanguageIdString).GetValue(lang) == GlobalConstants.DefaultLanguageId);

            requestedLang ??= langs.FirstOrDefault();

            return requestedLang.GetType().GetProperty(propName)?.GetValue(requestedLang, null)?.ToString();
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

            return string.IsNullOrEmpty(orderByProperty) ? await repository.GetAsPaginatedAsync(pageIndex,
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
        /// <param name="message"></param>
        public static void ThrowIfParameterIsNull(this object parameterObject, string message = null)
        {
            if (parameterObject == null)
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new MilvaUserFriendlyException(Startup.SharedStringLocalizer, MilvaExceptionCode.NullParameterException);
                }
                else
                {
                    throw new MilvaUserFriendlyException(message);
                }
            }
        }

        /// <summary>
        /// Throwns <see cref="MilvaUserFriendlyException"/> if <paramref name="list"/> is null or empty.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="message"></param>
        public static void ThrowIfListIsNullOrEmpty(this List<object> list, string message = null)
        {
            if (list.IsNullOrEmpty())
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new MilvaUserFriendlyException(Startup.SharedStringLocalizer, MilvaExceptionCode.CannotFindEntityException);
                }
                else
                {
                    throw new MilvaUserFriendlyException(message);
                }
            }
        }

        /// <summary>
        /// Throwns <see cref="MilvaUserFriendlyException"/> if <paramref name="list"/> is null or empty.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="message"></param>
        public static void ThrowIfParameterIsNullOrEmpty<T>(this List<T> list, string message = null) where T : IEquatable<T>
        {
            if (list.IsNullOrEmpty())
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new MilvaUserFriendlyException(Startup.SharedStringLocalizer, MilvaExceptionCode.NullParameterException);
                }
                else
                {
                    throw new MilvaUserFriendlyException(message);
                }
            }
        }

        /// <summary>
        /// Throwns <see cref="MilvaUserFriendlyException"/> if <paramref name="list"/> is null or empty.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="message"></param>
        public static void ThrowIfListIsNullOrEmpty(this IEnumerable<object> list, string message = null)
        {
            if (list.IsNullOrEmpty())
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new MilvaUserFriendlyException(Startup.SharedStringLocalizer, MilvaExceptionCode.CannotFindEntityException);
                }
                else
                {
                    throw new MilvaUserFriendlyException(message);
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
                if (string.IsNullOrEmpty(message))
                {
                    throw new MilvaUserFriendlyException(Startup.SharedStringLocalizer, MilvaExceptionCode.NullParameterException);
                }
                else
                {
                    throw new MilvaUserFriendlyException(message);
                }
            }
        }

        /// <summary>
        /// Throwns <see cref="MilvaUserFriendlyException"/> if <paramref name="entity"/> is null.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="message"></param>
        public static void ThrowIfNullForGuidObject<TEntity>(this TEntity entity, string message = null) where TEntity : class, IBaseEntity<Guid>
        {

            if (entity == null)
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new MilvaUserFriendlyException(Startup.SharedStringLocalizer, MilvaExceptionCode.CannotFindEntityException);
                }
                else
                {
                    throw new MilvaUserFriendlyException(message);
                }
            }
        }

        /// <summary>
        /// Throwns <see cref="MilvaUserFriendlyException"/> if <paramref name="entity"/> is null.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="message"></param>
        public static void ThrowIfNullForIntObject<TEntity>(this TEntity entity, string message = null) where TEntity : class, IBaseEntity<int>
        {

            if (entity == null)
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new MilvaUserFriendlyException(Startup.SharedStringLocalizer, MilvaExceptionCode.CannotFindEntityException);
                }
                else
                {
                    throw new MilvaUserFriendlyException(message);
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets requested property value. Method faydalı bi iş için yazmıştım fakat unuttum.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"> e.g : ProductLangs.Name </param>
        /// <returns></returns>
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            var propNames = propertyName.Split('.').ToList();

            if (propNames.Count > 2) throw new MilvaUserFriendlyException(Startup.SharedStringLocalizer, MilvaExceptionCode.InvalidParameterException);

            foreach (string propName in propNames)
            {
                if (typeof(IEnumerable).IsAssignableFrom(obj.GetType()))
                {
                    var count = (int)obj.GetType().GetProperty("Count").GetValue(obj, null);
                    var enumerator = obj.GetType().GetMethod("GetEnumerator").Invoke(obj, null);
                    List<object> listProp = new List<object>();
                    for (int i = 0; i < count; i++)
                    {
                        if (i == GlobalConstants.Zero) enumerator.GetType().GetMethod("MoveNext").Invoke(enumerator, null);

                        var currentValue = enumerator.GetType().GetProperty("Current").GetValue(enumerator, null);

                        var isLangPropExist = currentValue.GetType().GetProperties().Any(i => i.Name == "SystemLanguageId");
                        if (isLangPropExist)
                        {
                            var langId = (int)currentValue.GetType().GetProperty("SystemLanguageId").GetValue(currentValue, null);

                            if (langId == HelperExtensions.GetLanguageId())
                            {
                                obj = currentValue.GetType().GetProperty(propName).GetValue(currentValue, null);
                                break;
                            }
                        }
                        else
                        {
                            listProp.Add(currentValue.GetType().GetProperty(propName).GetValue(currentValue, null));
                        }

                        enumerator.GetType().GetMethod("MoveNext").Invoke(enumerator, null);
                    }
                    return listProp;

                }
                else obj = obj.GetType().GetProperty(propName).GetValue(obj, null);
            }

            return obj;
        }
    }
}
