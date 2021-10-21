using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Enums;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Milvasoft.Helpers
{
    /// <summary>
    /// <para>Helper class to generate the same type of response.</para>
    /// <para>All methods return HTTP status code 200 OK.All methods return <see cref="BaseResponse"/> type in ActionResult. Base response can be <see cref="ObjectResponse{T}" /> or <see cref="ObjectResponse{T}"/></para>
    /// </summary>
    public static class Response
    {
        /// <summary>
        /// <para> Return <paramref name="paginationDTO"/> in <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="paginationDTO"/>.DTOList isn't null or empty, sets the <see cref="ObjectResponse{T}"/>.Message to <paramref name="successMessage"/>. </para>
        /// <para> Otherwise if <paramref name="errorMessage"/> is null sets the <see cref="ObjectResponse{T}"/>.Message to <b><see cref="LocalizerKeys.DefaultErrorMessage"/></b>. </para>
        /// <para> Otherwise sets the <see cref="ObjectResponse{T}"/>.Message to <paramref name="errorMessage"/>. </para>
        /// <para> Also in this conditions sets the <see cref="ObjectResponse{T}"/>.StatusCode to <see cref="MilvaStatusCodes.Status204NoContent"/>
        ///         and <see cref="ObjectResponse{T}"/>.Success to true. Reason to be considered the <see cref="ObjectResponse{T}"/>.Success true is request was successful.  </para>
        /// </summary>
        /// 
        /// <param name="paginationDTO"></param>
        /// <param name="successMessage"></param>
        /// <param name="errorMessage"></param>
        /// <returns>  <see cref="ObjectResponse{PaginationDTO}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static IActionResult GetPaginationResponse<T>(this PaginationDTO<T> paginationDTO,
                                                             string successMessage,
                                                             string errorMessage = null)
        {
            var response = new ObjectResponse<PaginationDTO<T>>
            {
                StatusCode = MilvaStatusCodes.Status200OK,
                Message = successMessage
            };

            if (paginationDTO.DTOList.IsNullOrEmpty())
            {
                response.Message = string.IsNullOrWhiteSpace(errorMessage) ? LocalizerKeys.DefaultErrorMessage : errorMessage;
                response.StatusCode = MilvaStatusCodes.Status204NoContent;
                response.Success = true;
            }

            response.Result = paginationDTO;

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Return <see cref="ObjectResponse{T}"/> in <see cref="ActionResult"/>. </para>
        /// <para> If <paramref name="contentList"/> isn't null or empty, sets the <see cref="ObjectResponse{T}"/>.Message to <paramref name="successMessage"/>. </para>
        /// <para> Otherwise if <paramref name="errorMessage"/> is null sets the <see cref="ObjectResponse{T}"/>.Message to <b><see cref="LocalizerKeys.DefaultErrorMessage"/></b>. </para>
        /// <para> Otherwise sets the <see cref="ObjectResponse{T}"/>.Message to <paramref name="errorMessage"/>. </para>
        /// <para> Also in this conditions sets the <see cref="ObjectResponse{T}"/>.StatusCode to <see cref="MilvaStatusCodes.Status204NoContent"/>
        ///         and <see cref="ObjectResponse{T}"/>.Success to true. Reason to be considered the <see cref="ObjectResponse{T}"/>.Success true is request was successful.  </para>
        /// </summary>
        /// 
        /// <param name="contentList"></param>
        /// <param name="successMessage"></param>
        /// <param name="errorMessage"></param>
        /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static IActionResult GetObjectResponse<T>(this List<T> contentList,
                                                         string successMessage,
                                                         string errorMessage = null)
        {
            var response = new ObjectResponse<List<T>>
            {
                StatusCode = MilvaStatusCodes.Status200OK,
                Message = successMessage
            };

            if (contentList.IsNullOrEmpty())
            {
                response.Message = string.IsNullOrWhiteSpace(errorMessage) ? LocalizerKeys.DefaultErrorMessage : errorMessage;
                response.StatusCode = MilvaStatusCodes.Status204NoContent;
                response.Success = true;
            }
            response.Result = contentList;

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Return <see cref="ObjectResponse{T}"/> in <see cref="ActionResult"/>. </para>
        /// <para> If <paramref name="content"/> isn't null, sets the <see cref="ObjectResponse{T}"/>.Message to <paramref name="successMessage"/>. </para>
        /// <para> Otherwise if <paramref name="errorMessage"/> is null sets the <see cref="ObjectResponse{T}"/>.Message to <b><see cref="LocalizerKeys.DefaultErrorMessage"/></b>. </para>
        /// <para> Otherwise sets the <see cref="ObjectResponse{T}"/>.Message to <paramref name="errorMessage"/>. </para>
        /// <para> Also in this conditions sets the <see cref="ObjectResponse{T}"/>.StatusCode to <see cref="MilvaStatusCodes.Status204NoContent"/>
        ///         and <see cref="ObjectResponse{T}"/>.Success to true. Reason to be considered the <see cref="ObjectResponse{T}"/>.Success true is request was successful.  </para>
        /// </summary>
        /// 
        /// <param name="content"></param>
        /// <param name="successMessage"></param>
        /// <param name="errorMessage"></param>
        /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static IActionResult GetObjectResponse<T>(this T content,
                                                         string successMessage,
                                                         string errorMessage = null)
        {
            var response = new ObjectResponse<T>
            {
                StatusCode = MilvaStatusCodes.Status200OK,
                Success = true
            };

            response.Message = successMessage;
            if (content == null)
            {
                response.Message = string.IsNullOrWhiteSpace(errorMessage) ? LocalizerKeys.DefaultErrorMessage : errorMessage;
                response.StatusCode = MilvaStatusCodes.Status204NoContent;
            }
            response.Result = content;
            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="idList"></param>
        /// <param name="successMessage"></param>
        /// <param name="errorMessage"></param>
        /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T, TKey>(this ConfiguredTaskAwaitable asyncTask,
                                                                                IEnumerable<TKey> idList,
                                                                                string successMessage,
                                                                                string errorMessage = null) where TKey : IEquatable<TKey>
        {
            var response = new ObjectResponse<T>();

            if (idList.IsNullOrEmpty())
            {
                response.Message = string.IsNullOrWhiteSpace(errorMessage) ? LocalizerKeys.DefaultErrorMessage : errorMessage;
                response.StatusCode = MilvaStatusCodes.Status600Exception;
                response.Success = false;
            }
            else
            {
                await asyncTask;

                response.Message = successMessage;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="idList"></param>
        /// <param name="successMessage"></param>
        /// <param name="errorMessage"></param>
        /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T, TKey>(this Task asyncTask,
                                                                                IEnumerable<TKey> idList,
                                                                                string successMessage,
                                                                                string errorMessage = null) where TKey : IEquatable<TKey>
        {
            var response = new ObjectResponse<T>();

            if (idList.IsNullOrEmpty())
            {
                response.Message = string.IsNullOrWhiteSpace(errorMessage) ? LocalizerKeys.DefaultErrorMessage : errorMessage;
                response.StatusCode = MilvaStatusCodes.Status600Exception;
                response.Success = false;
            }
            else
            {
                await asyncTask;

                response.Message = successMessage;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="idList"></param>
        /// <param name="successMessage"></param>
        /// <param name="errorMessage"></param>
        /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T, TKey>(this ConfiguredTaskAwaitable<IEnumerable<T>> asyncTask,
                                                                                IEnumerable<TKey> idList,
                                                                                string successMessage,
                                                                                string errorMessage = null) where TKey : struct, IEquatable<TKey>
        {
            var response = new ObjectResponse<IEnumerable<T>>();

            if (idList.IsNullOrEmpty())
            {
                response.Message = string.IsNullOrWhiteSpace(errorMessage) ? LocalizerKeys.DefaultErrorMessage : errorMessage;
                response.StatusCode = MilvaStatusCodes.Status600Exception;
                response.Success = false;
            }
            else
            {
                var result = await asyncTask;

                response.Result = result;
                response.Message = successMessage;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="idList"></param>
        /// <param name="successMessage"></param>
        /// <param name="errorMessage"></param>
        /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T, TKey>(this Task<IEnumerable<T>> asyncTask,
                                                                                IEnumerable<TKey> idList,
                                                                                string successMessage,
                                                                                string errorMessage = null) where TKey : struct, IEquatable<TKey>
        {
            var response = new ObjectResponse<IEnumerable<T>>();

            if (idList.IsNullOrEmpty())
            {
                response.Message = string.IsNullOrWhiteSpace(errorMessage) ? LocalizerKeys.DefaultErrorMessage : errorMessage;
                response.StatusCode = MilvaStatusCodes.Status600Exception;
                response.Success = false;
            }
            else
            {
                var result = await asyncTask;

                response.Result = result;
                response.Message = successMessage;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T>(this ConfiguredTaskAwaitable asyncTask, string successMessage)
        {
            var response = new ObjectResponse<T>();
            {
                await asyncTask;

                response.Success = true;
                response.Message = successMessage;
                response.StatusCode = MilvaStatusCodes.Status200OK;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T>(this ConfiguredTaskAwaitable<T> asyncTask, string successMessage)
        {
            var response = new ObjectResponse<T>();
            {
                var result = await asyncTask;

                response.Result = result;
                response.Success = true;
                response.Message = successMessage;
                response.StatusCode = MilvaStatusCodes.Status200OK;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T>(this ConfiguredTaskAwaitable<object> asyncTask, string successMessage)
        {
            var response = new ObjectResponse<object>();
            {
                var result = await asyncTask;

                response.Result = result;
                response.Success = true;
                response.Message = successMessage;
                response.StatusCode = MilvaStatusCodes.Status200OK;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T>(this ConfiguredTaskAwaitable<Guid> asyncTask, string successMessage)
        {
            var response = new ObjectResponse<Guid>();

            var result = await asyncTask;

            response.Result = result;
            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvaStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T>(this ConfiguredTaskAwaitable<int> asyncTask, string successMessage)
        {
            var response = new ObjectResponse<int>();

            var result = await asyncTask;

            response.Result = result;
            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvaStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T>(this ConfiguredTaskAwaitable<sbyte> asyncTask, string successMessage)
        {
            var response = new ObjectResponse<sbyte>();

            var result = await asyncTask;

            response.Result = result;
            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvaStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T>(this Task asyncTask, string successMessage)
        {
            var response = new ObjectResponse<T>();
            {
                await asyncTask;

                response.Success = true;
                response.Message = successMessage;
                response.StatusCode = MilvaStatusCodes.Status200OK;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T>(this Task<T> asyncTask, string successMessage)
        {
            var response = new ObjectResponse<T>();
            {
                var result = await asyncTask;

                response.Result = result;
                response.Success = true;
                response.Message = successMessage;
                response.StatusCode = MilvaStatusCodes.Status200OK;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T>(this Task<object> asyncTask, string successMessage)
        {
            var response = new ObjectResponse<object>();
            {
                var result = await asyncTask;

                response.Result = result;
                response.Success = true;
                response.Message = successMessage;
                response.StatusCode = MilvaStatusCodes.Status200OK;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T>(this Task<Guid> asyncTask, string successMessage)
        {
            var response = new ObjectResponse<Guid>();

            var result = await asyncTask;

            response.Result = result;
            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvaStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T>(this Task<int> asyncTask, string successMessage)
        {
            var response = new ObjectResponse<int>();

            var result = await asyncTask;

            response.Result = result;
            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvaStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="ObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="ObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> GetObjectResponseAsync<T>(this Task<sbyte> asyncTask, string successMessage)
        {
            var response = new ObjectResponse<sbyte>();

            var result = await asyncTask;

            response.Result = result;
            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvaStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }


        #region Response Message Helpers

        /// <summary>
        /// Gets error messages according to <paramref name="operation"/>.
        /// <paramref name="keyContent"/> will combine with <see cref="LocalizerKeys.LocalizedEntityName"/> except <paramref name="operation"/> is <see cref="CrudOperation.Specific"/>. 
        /// Return message determined by <paramref name="operation"/>.
        /// For messages see <see cref="LocalizerKeys"/>.
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="keyContent"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static string GetErrorMessage(this IStringLocalizer localizer, string keyContent, CrudOperation operation)
        {
            string localizedEntityName;

            switch (operation)
            {
                case CrudOperation.Add:
                    localizedEntityName = localizer[LocalizerKeys.LocalizedEntityName + keyContent].ToString().ToLowerInVariantFirst();
                    return localizer[LocalizerKeys.AddErrorMessage, localizedEntityName];

                case CrudOperation.Update:
                    localizedEntityName = localizer[LocalizerKeys.LocalizedEntityName + keyContent].ToString().ToLowerInVariantFirst();
                    return localizer[LocalizerKeys.UpdateErrorMessage, localizedEntityName];

                case CrudOperation.Delete:
                    localizedEntityName = localizer[LocalizerKeys.LocalizedEntityName + keyContent].ToString().ToLowerInVariantFirst();
                    return localizer[LocalizerKeys.DeleteErrorMessage, localizedEntityName];

                case CrudOperation.GetById:
                    localizedEntityName = localizer[LocalizerKeys.LocalizedEntityName + keyContent].ToString().ToUpperInVariantFirst();
                    return localizer[LocalizerKeys.GetByIdErrorMessage, localizedEntityName];

                case CrudOperation.GetAll:
                    localizedEntityName = localizer[LocalizerKeys.LocalizedEntityName + keyContent].ToString().ToLowerInvariant();
                    return localizer[LocalizerKeys.GetAllErrorMessage, localizedEntityName];

                case CrudOperation.Filtering:
                    localizedEntityName = localizer[LocalizerKeys.LocalizedEntityName + keyContent].ToString().ToLowerInvariant();
                    return localizer[LocalizerKeys.FilteringErrorMessage, localizedEntityName];

                case CrudOperation.Specific:
                    return localizer[keyContent].ToString();

                default:
                    return "";
            }
        }

        /// <summary>
        /// Gets success messages according to <paramref name="operation"/>.
        /// <paramref name="keyContent"/> will combine with <see cref="LocalizerKeys.LocalizedEntityName"/> except <paramref name="operation"/> is <see cref="CrudOperation.Specific"/>. 
        /// Return message determined by <paramref name="operation"/>.
        /// For messages see <see cref="LocalizerKeys"/>.
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="keyContent"></param>
        /// <param name="recordCount"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static string GetSuccessMessage(this IStringLocalizer localizer,
                                               string keyContent,
                                               CrudOperation operation,
                                               int? recordCount = null)
        {
            string localizedEntityName;

            switch (operation)
            {
                case CrudOperation.Add:
                    localizedEntityName = localizer[LocalizerKeys.LocalizedEntityName + keyContent].ToString().ToUpperInVariantFirst();
                    return localizer[LocalizerKeys.AddSuccessMessage, localizedEntityName];

                case CrudOperation.Update:
                    localizedEntityName = localizer[LocalizerKeys.LocalizedEntityName + keyContent].ToString().ToUpperInVariantFirst();
                    return localizer[LocalizerKeys.UpdateSuccessMessage, localizedEntityName];

                case CrudOperation.Delete:
                    localizedEntityName = localizer[LocalizerKeys.LocalizedEntityName + keyContent].ToString().ToUpperInVariantFirst();
                    return localizer[LocalizerKeys.DeleteSuccessMessage, localizedEntityName];

                case CrudOperation.GetById:
                    localizedEntityName = localizer[LocalizerKeys.LocalizedEntityName + keyContent].ToString();
                    return localizer[LocalizerKeys.GetByIdSuccessMessage, localizedEntityName, recordCount];

                case CrudOperation.GetAll:
                    localizedEntityName = localizer[LocalizerKeys.LocalizedEntityName + keyContent].ToString();
                    return localizer[LocalizerKeys.GetAllSuccessMessage, localizedEntityName, recordCount.GetValueOrDefault()];

                case CrudOperation.Filtering:
                    localizedEntityName = localizer[LocalizerKeys.LocalizedEntityName + keyContent].ToString();
                    return localizer[LocalizerKeys.FilteringSuccessMessage, localizedEntityName, recordCount];

                case CrudOperation.Specific:
                    return localizer[keyContent].ToString();

                default:
                    return "";
            }
        }

        #endregion

    }


}
