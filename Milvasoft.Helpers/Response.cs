﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models;
using Milvasoft.Helpers.Models.Response;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Milvasoft.Helpers
{
    /// <summary>
    /// <para>Helper class to generate the same type of response.</para>
    /// <para>All methods return <see cref="BaseResponse"/> type in ActionResult. Base response can be <see cref="SingleObjectResponse{T}" /> or <see cref="MultipleObjectResponse{T}"/></para>
    /// <para>All methods return HTTP status code 200 OK.</para>
    /// </summary>
    public static class Response
    {
        /// <summary>
        /// Default error message : "An error occured while processing"
        /// </summary>
        public static string _defaultErrorMessage = "An error occured while processing";

        /// <summary>
        /// <para> Return <see cref="MultipleObjectResponse{T}"/> in <see cref="ActionResult"/>. </para>
        /// <para> If <paramref name="contentList"/> isn't null or empty, sets the <see cref="MultipleObjectResponse{T}"/>.Message to <paramref name="successMessage"/>. </para>
        /// <para> Otherwise if <paramref name="errorMessage"/> is null sets the <see cref="MultipleObjectResponse{T}"/>.Message to <b><see cref="_defaultErrorMessage"/></b>. </para>
        /// <para> Otherwise sets the <see cref="MultipleObjectResponse{T}"/>.Message to <paramref name="errorMessage"/>. </para>
        /// <para> Also in this conditions sets the <see cref="MultipleObjectResponse{T}"/>.StatusCode to <see cref="MilvasoftStatusCodes.Status204NoContent"/>
        ///         and <see cref="MultipleObjectResponse{T}"/>.Success to true. Reason to be considered the <see cref="MultipleObjectResponse{T}"/>.Success true is request was successful.  </para>
        /// </summary>
        /// 
        /// <param name="contentList"></param>
        /// <param name="successMessage"></param>
        /// <param name="errorMessage"></param>
        /// <returns> <see cref="MultipleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static IActionResult ReturnArrayResponseForGetAll<T>(this List<T> contentList, string successMessage, string errorMessage = null)
        {
            var response = new MultipleObjectResponse<T>();
            response.StatusCode = MilvasoftStatusCodes.Status200OK;
            response.Message = successMessage;
            if (contentList.IsNullOrEmpty())
            {
                response.Message = string.IsNullOrEmpty(errorMessage) ? _defaultErrorMessage : errorMessage;
                response.StatusCode = MilvasoftStatusCodes.Status204NoContent;
                response.Success = true;
            }
            response.Result = contentList;

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Return <see cref="SingleObjectResponse{T}"/> in <see cref="ActionResult"/>. </para>
        /// <para> If <paramref name="content"/> isn't null, sets the <see cref="SingleObjectResponse{T}"/>.Message to <paramref name="successMessage"/>. </para>
        /// <para> Otherwise if <paramref name="errorMessage"/> is null sets the <see cref="SingleObjectResponse{T}"/>.Message to <b><see cref="_defaultErrorMessage"/></b>. </para>
        /// <para> Otherwise sets the <see cref="SingleObjectResponse{T}"/>.Message to <paramref name="errorMessage"/>. </para>
        /// <para> Also in this conditions sets the <see cref="SingleObjectResponse{T}"/>.StatusCode to <see cref="MilvasoftStatusCodes.Status204NoContent"/>
        ///         and <see cref="SingleObjectResponse{T}"/>.Success to true. Reason to be considered the <see cref="SingleObjectResponse{T}"/>.Success true is request was successful.  </para>
        /// </summary>
        /// 
        /// <param name="content"></param>
        /// <param name="successMessage"></param>
        /// <param name="errorMessage"></param>
        /// <returns> <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static IActionResult ReturnSingleObjectResponseForGetById<T>(this T content, string successMessage, string errorMessage = null)
        {
            var response = new SingleObjectResponse<T> { StatusCode = MilvasoftStatusCodes.Status200OK, Success = true };
            response.Message = successMessage;
            if (content == null)
            {
                response.Message = string.IsNullOrEmpty(errorMessage) ? _defaultErrorMessage : errorMessage;
                response.StatusCode = MilvasoftStatusCodes.Status204NoContent;
            }
            response.Result = content;
            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Return <paramref name="paginationDTO"/> in <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="paginationDTO"/>.DTOList isn't null or empty, sets the <see cref="SingleObjectResponse{T}"/>.Message to <paramref name="successMessage"/>. </para>
        /// <para> Otherwise if <paramref name="errorMessage"/> is null sets the <see cref="SingleObjectResponse{T}"/>.Message to <b><see cref="_defaultErrorMessage"/></b>. </para>
        /// <para> Otherwise sets the <see cref="SingleObjectResponse{T}"/>.Message to <paramref name="errorMessage"/>. </para>
        /// <para> Also in this conditions sets the <see cref="SingleObjectResponse{T}"/>.StatusCode to <see cref="MilvasoftStatusCodes.Status204NoContent"/>
        ///         and <see cref="SingleObjectResponse{T}"/>.Success to true. Reason to be considered the <see cref="SingleObjectResponse{T}"/>.Success true is request was successful.  </para>
        /// </summary>
        /// 
        /// <param name="paginationDTO"></param>
        /// <param name="successMessage"></param>
        /// <param name="errorMessage"></param>
        /// <returns>  <see cref="SingleObjectResponse{PaginationDTO}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static IActionResult ReturnReportResponse<T>(this PaginationDTO<T> paginationDTO, string successMessage, string errorMessage = null)
        {
            var response = new SingleObjectResponse<PaginationDTO<T>> { StatusCode = MilvasoftStatusCodes.Status200OK };
            response.Message = successMessage;
            if (paginationDTO.DTOList.IsNullOrEmpty())
            {
                response.Message = string.IsNullOrEmpty(errorMessage) ? _defaultErrorMessage : errorMessage;
                response.StatusCode = MilvasoftStatusCodes.Status204NoContent;
                response.Success = true;
            }
            response.Result = paginationDTO;
            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Return <paramref name="paginationDTO"/> in <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="paginationDTO"/>.DTOList isn't null or empty, sets the <see cref="SingleObjectResponse{T}"/>.Message to <paramref name="successMessage"/>. </para>
        /// <para> Otherwise if <paramref name="errorMessage"/> is null sets the <see cref="SingleObjectResponse{T}"/>.Message to <b><see cref="_defaultErrorMessage"/></b>. </para>
        /// <para> Otherwise sets the <see cref="SingleObjectResponse{T}"/>.Message to <paramref name="errorMessage"/>. </para>
        /// <para> Also in this conditions sets the <see cref="SingleObjectResponse{T}"/>.StatusCode to <see cref="MilvasoftStatusCodes.Status204NoContent"/>
        ///         and <see cref="SingleObjectResponse{T}"/>.Success to true. Reason to be considered the <see cref="SingleObjectResponse{T}"/>.Success true is request was successful.  </para>
        /// </summary>
        /// 
        /// <param name="paginationDTO"></param>
        /// <param name="successMessage"></param>
        /// <param name="errorMessage"></param>
        /// <returns> <see cref="SingleObjectResponse{PaginationDTO}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static IActionResult ReturnInstantReportResponse<T>(this PaginationDTO<T> paginationDTO, string successMessage, string errorMessage = null)
        {
            var response = new SingleObjectResponse<PaginationDTO<T>> { StatusCode = MilvasoftStatusCodes.Status200OK };

            response.Message = successMessage;

            if (paginationDTO.DTOList.IsNullOrEmpty())
            {
                response.Message = string.IsNullOrEmpty(errorMessage) ? _defaultErrorMessage : errorMessage;
                response.StatusCode = MilvasoftStatusCodes.Status204NoContent;
                response.Success = true;
            }
            response.Result = paginationDTO;
            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="MultipleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="MultipleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="idList"></param>
        /// <param name="successMessage"></param>
        /// <param name="errorMessage"></param>
        /// <returns>  <see cref="MultipleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> ReturnArrayResponseForDeleteAsync<T, TKey>(this ConfiguredTaskAwaitable asyncTask,
                                                                                                                     IEnumerable<TKey> idList,
                                                                                                                     string successMessage,
                                                                                                                     string errorMessage = null) where TKey : struct
        {
            var response = new MultipleObjectResponse<T>();
            if (idList.IsNullOrEmpty())
            {
                response.Message = string.IsNullOrEmpty(errorMessage) ? _defaultErrorMessage : errorMessage;
                response.StatusCode = MilvasoftStatusCodes.Status500InternalServerError;
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
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="MultipleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="MultipleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="idList"></param>
        /// <param name="successMessage"></param>
        /// <param name="errorMessage"></param>
        /// <returns>  <see cref="MultipleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> ReturnArrayResponseForDeleteAsync<T, TKey>(this ConfiguredTaskAwaitable<IEnumerable<T>> asyncTask,
                                                                                                                     IEnumerable<TKey> idList,
                                                                                                                     string successMessage,
                                                                                                                     string errorMessage = null) where TKey : struct
        {
            var response = new MultipleObjectResponse<T>();
            if (idList.IsNullOrEmpty())
            {
                response.Message = string.IsNullOrEmpty(errorMessage) ? _defaultErrorMessage : errorMessage;
                response.StatusCode = MilvasoftStatusCodes.Status500InternalServerError;
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
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="SingleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns>  <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> ReturnSingleResponseForDeleteAsync<T>(this ConfiguredTaskAwaitable asyncTask, string successMessage)
        {
            var response = new SingleObjectResponse<T>();
            {
                await asyncTask;

                response.Success = true;
                response.Message = successMessage;
                response.StatusCode = MilvasoftStatusCodes.Status200OK;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="SingleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns>  <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> ReturnSingleResponseForDeleteAsync<T>(this ConfiguredTaskAwaitable<T> asyncTask, string successMessage)
        {
            var response = new SingleObjectResponse<T>();
            {
                var result = await asyncTask;

                response.Result = result;
                response.Success = true;
                response.Message = successMessage;
                response.StatusCode = MilvasoftStatusCodes.Status200OK;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="SingleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns>  <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> ReturnSingleResponseForDeleteAsync<T>(this ConfiguredTaskAwaitable<object> asyncTask, string successMessage)
        {
            var response = new SingleObjectResponse<object>();
            {
                var result = await asyncTask;

                response.Result = result;
                response.Success = true;
                response.Message = successMessage;
                response.StatusCode = MilvasoftStatusCodes.Status200OK;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="SingleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns> <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> ReturnSingleObjectResponseForAddOrUpdateAsync<T>(this ConfiguredTaskAwaitable asyncTask, string successMessage)
        {
            var response = new SingleObjectResponse<T>();

            await asyncTask;

            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvasoftStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="SingleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns> <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> ReturnSingleObjectResponseForAddOrUpdateAsync<T>(this ConfiguredTaskAwaitable<object> asyncTask, string successMessage)
        {
            var response = new SingleObjectResponse<object>();

            var result = await asyncTask;

            response.Result = result;
            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvasoftStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="SingleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns> <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> ReturnSingleObjectResponseForAddOrUpdateAsync<T>(this ConfiguredTaskAwaitable<T> asyncTask, string successMessage)
        {
            var response = new SingleObjectResponse<T>();

            var result = await asyncTask;

            response.Result = result;
            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvasoftStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="SingleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns> <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> ReturnSingleObjectResponseForAddOrUpdateAsync<T>(this ConfiguredTaskAwaitable<Guid> asyncTask, string successMessage)
        {
            var response = new SingleObjectResponse<Guid>();

            var result = await asyncTask;

            response.Result = result;
            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvasoftStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }


        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="SingleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns> <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> ReturnSingleObjectResponseForAddOrUpdateAsync<T>(this ConfiguredTaskAwaitable<int> asyncTask, string successMessage)
        {
            var response = new SingleObjectResponse<int>();

            var result = await asyncTask;

            response.Result = result;
            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvasoftStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="SingleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns> <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> ReturnSingleObjectResponseForAddOrUpdateAsync<T>(this ConfiguredTaskAwaitable<sbyte> asyncTask, string successMessage)
        {
            var response = new SingleObjectResponse<sbyte>();

            var result = await asyncTask;

            response.Result = result;
            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvasoftStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }


        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="SingleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns>  <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> ReturnSingleObjectResponseForSpecifedProcessAsync<T>(this ConfiguredTaskAwaitable asyncTask, string successMessage)
        {
            var response = new SingleObjectResponse<T>();

            await asyncTask;

            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvasoftStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }


        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="SingleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns>  <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> ReturnSingleObjectResponseForSpecifedProcessAsync<T>(this ConfiguredTaskAwaitable<T> asyncTask, string successMessage)
        {
            var response = new SingleObjectResponse<T>();

            var result = await asyncTask;

            response.Result = result;
            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvasoftStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="SingleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns>  <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<IActionResult> ReturnSingleObjectResponseForSpecifedProcessAsync<T>(this ConfiguredTaskAwaitable<object> asyncTask, string successMessage)
        {
            var response = new SingleObjectResponse<object>();

            var result = await asyncTask;

            response.Result = result;
            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvasoftStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }


        #region Response Message Helpers

        /// <summary>
        /// Gets error messages for get all operation.
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="keyContent"></param>
        /// <returns></returns>
        public static string GetErrorMessageForGetAll(this IStringLocalizer localizer, string keyContent)
        {
            var localizedEntityName = localizer[$"LocalizedEntityName{keyContent}"].ToString().ToLowerInvariant();
            return localizer["NoEntityWasFound", localizedEntityName];
        }

        /// <summary>
        /// Gets error messages for get all operation.
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="keyContent"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public static string GetSuccessMessageForGetAll(this IStringLocalizer localizer, string keyContent, int recordCount)
        {
            var localizedEntityName = localizer[$"LocalizedEntityName{keyContent}"].ToString();
            return localizer["GetAllSuccessMessage", localizedEntityName, recordCount];
        }

        /// <summary>
        /// Gets error messages for get all and filtering operation.
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="keyContent"></param>
        /// <returns></returns>
        public static string GetErrorMessageForFiltering(this IStringLocalizer localizer, string keyContent)
        {
            var localizedEntityName = localizer[$"LocalizedEntityName{keyContent}"].ToString().ToLowerInvariant();
            return localizer["FilteredWasNotFound", localizedEntityName];
        }

        /// <summary>
        /// Gets error messages for get by id operation.
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="keyContent"></param>
        /// <returns></returns>
        public static string GetErrorMessageForGetById(this IStringLocalizer localizer, string keyContent)
        {
            var localizedEntityName = localizer[$"LocalizedEntityName{keyContent}"].ToString().ToUpperInVariantFirst();
            return localizer["ControllersSingleObjectWasNotFound", localizedEntityName];
        }

        /// <summary>
        /// Gets success messages for adding operation.
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="keyContent"></param>
        /// <returns></returns>
        public static string GetSuccessMessageForAdd(this IStringLocalizer localizer, string keyContent)
        {
            var localizedEntityName = localizer[$"LocalizedEntityName{keyContent}"].ToString().ToUpperInVariantFirst();
            return localizer["SuccessfullyAdded", localizedEntityName];
        }

        /// <summary>
        /// Gets success messages for updating operation.
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="keyContent"></param>
        /// <returns></returns>
        public static string GetSuccessMessageForUpdate(this IStringLocalizer localizer, string keyContent)
        {
            var localizedEntityName = localizer[$"LocalizedEntityName{keyContent}"].ToString().ToUpperInVariantFirst();
            return localizer["SuccessfullyUpdated", localizedEntityName];
        }

        /// <summary>
        /// Gets error messages for deleting operation.
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="keyContent"></param>
        /// <returns></returns>
        public static string GetErrorMessageForDelete(this IStringLocalizer localizer, string keyContent)
        {
            var localizedEntityName = localizer[$"LocalizedEntityName{keyContent}"].ToString().ToLowerInVariantFirst();
            return localizer["NoEntityToBeDeleted", localizedEntityName];
        }

        /// <summary>
        /// Gets success messages for deleting operation.
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="keyContent"></param>
        /// <returns></returns>
        public static string GetSucccesMessageForDelete(this IStringLocalizer localizer, string keyContent)
        {
            var localizedEntityName = localizer[$"LocalizedEntityName{keyContent}"].ToString().ToUpperInVariantFirst();
            return localizer["SuccessfullyDeleted", localizedEntityName];
        }

        /// <summary>
        /// Gets success messages for get all operation.
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="keyContent"></param>
        /// <returns></returns>
        public static string GetMessageForSpecifiedProcess(this IStringLocalizer localizer, string keyContent) => localizer[keyContent].ToString();

        #endregion


    }
}
