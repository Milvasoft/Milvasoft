using Microsoft.AspNetCore.Mvc;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models;
using Milvasoft.Helpers.Models.Response;
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
        public static ActionResult<MultipleObjectResponse<T>> ReturnArrayResponseForGetAll<T>(this List<T> contentList, string successMessage, string errorMessage = null)
        {
            MultipleObjectResponse<T> response = new MultipleObjectResponse<T>();
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
        public static ActionResult<SingleObjectResponse<T>> ReturnSingleObjectResponseForGetById<T>(this T content, string successMessage, string errorMessage = null)
        {
            SingleObjectResponse<T> response = new SingleObjectResponse<T> { StatusCode = MilvasoftStatusCodes.Status200OK, Success = true };
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
        public static ActionResult ReturnReportResponse<T>(this PaginationDTO<T> paginationDTO, string successMessage, string errorMessage = null)
        {
            SingleObjectResponse<PaginationDTO<T>> response = new SingleObjectResponse<PaginationDTO<T>> { StatusCode = MilvasoftStatusCodes.Status200OK };
            response.Message = successMessage;
            if (paginationDTO.DTOList.IsNullOrEmpty())
            {
                response.Message = string.IsNullOrEmpty(errorMessage) ? _defaultErrorMessage : errorMessage;
                response.StatusCode = MilvasoftStatusCodes.Status204NoContent;
                response.Success = true;
            }
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
        public static ActionResult ReturnInstantReportResponse<T>(this PaginationDTO<T> paginationDTO, string successMessage, string errorMessage = null)
        {
            SingleObjectResponse<PaginationDTO<T>> response = new SingleObjectResponse<PaginationDTO<T>> { StatusCode = MilvasoftStatusCodes.Status200OK };

            response.Message = successMessage;

            if (paginationDTO.DTOList.IsNullOrEmpty())
            {
                response.Message = string.IsNullOrEmpty(errorMessage) ? _defaultErrorMessage : errorMessage;
                response.StatusCode = MilvasoftStatusCodes.Status204NoContent;
                response.Success = true;
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
        public static async Task<ActionResult<MultipleObjectResponse<T>>> ReturnArrayResponseForDeleteAsync<T, TKey>(this ConfiguredTaskAwaitable asyncTask,
                                                                                                                     IEnumerable<TKey> idList,
                                                                                                                     string successMessage,
                                                                                                                     string errorMessage = null) where TKey : struct
        {
            MultipleObjectResponse<T> response = new MultipleObjectResponse<T>();
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
        /// <para> Run the <paramref name="asyncTask"/> then return <see cref="SingleObjectResponse{T}"/>. </para>
        /// <para> If <paramref name="asyncTask"/> is success sets the <paramref name="successMessage"/> to <see cref="SingleObjectResponse{T}"/>.Message.
        ///        Otherwise you should throw exception and cut the request pipeline in <paramref name="asyncTask"/>. </para>
        /// </summary>
        /// 
        /// <param name="asyncTask"></param>
        /// <param name="successMessage"></param>
        /// <returns>  <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<ActionResult<SingleObjectResponse<T>>> ReturnSingleResponseForDeleteAsync<T>(this ConfiguredTaskAwaitable asyncTask, string successMessage)
        {
            SingleObjectResponse<T> response = new SingleObjectResponse<T>();
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
        /// <returns> <see cref="SingleObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
        public static async Task<ActionResult<SingleObjectResponse<T>>> ReturnSingleObjectResponseForAddOrUpdateAsync<T>(this ConfiguredTaskAwaitable asyncTask, string successMessage)
        {
            SingleObjectResponse<T> response = new SingleObjectResponse<T>();

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
        public static async Task<ActionResult<SingleObjectResponse<T>>> ReturnSingleObjectResponseForSpecifedProcessAsync<T>(this ConfiguredTaskAwaitable asyncTask, string successMessage)
        {
            SingleObjectResponse<T> response = new SingleObjectResponse<T>();

            await asyncTask;

            response.Success = true;
            response.Message = successMessage;
            response.StatusCode = MilvasoftStatusCodes.Status200OK;

            return new OkObjectResult(response);
        }


    }
}
