using Microsoft.AspNetCore.Mvc;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models;
using Milvasoft.Helpers.Models.Response;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Milvasoft.Helpers
{
    public static class Response
    {
        public static string _successMessage;

        /// <summary>
        /// <para> Controller configuration. </para>
        /// </summary>
        /// <param name="contentList"></param>
        /// <returns></returns>
        public static MultipleObjectResponse<T> GetArrayResponseForGetAll<T>(this List<T> contentList, string successMessage, string errorMessage = null)
        {
            MultipleObjectResponse<T> response = new MultipleObjectResponse<T>();
            response.StatusCode = MilvasoftStatusCodes.Status200OK;
            response.Message = successMessage;
            if (contentList.IsNullOrEmpty())
            {
                response.Message = errorMessage;
                response.StatusCode = MilvasoftStatusCodes.Status204NoContent;
                response.Success = true;
            }
            response.Result = contentList;

            return response;
        }

        /// <summary>
        /// <para> Controller configuration. </para>
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static SingleObjectResponse<T> GetSingleObjectResponseForGetById<T>(this T content, string successMessage, string errorMessage = null)
        {
            SingleObjectResponse<T> response = new SingleObjectResponse<T> { StatusCode = MilvasoftStatusCodes.Status200OK, Success = true };
            response.Message = successMessage;
            if (content == null)
            {
                response.Message = errorMessage;
                response.StatusCode = MilvasoftStatusCodes.Status204NoContent;
            }
            response.Result = content;
            return response;
        }

        /// <summary>
        /// <para> Controller configuration. </para>
        /// </summary>
        /// <param name="paginationDTO"></param>
        /// <returns></returns>
        public static IActionResult ReturnReportResponse<T>(this PaginationDTO<T> paginationDTO, string successMessage, string errorMessage = null)
        {
            SingleObjectResponse<PaginationDTO<T>> response = new SingleObjectResponse<PaginationDTO<T>> { StatusCode = MilvasoftStatusCodes.Status200OK };
            response.Message = successMessage;
            if (paginationDTO.DTOList.IsNullOrEmpty())
            {
                response.Message = errorMessage;
                response.StatusCode = MilvasoftStatusCodes.Status204NoContent;
                response.Success = true;
            }
            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Controller configuration </para>
        /// </summary>
        /// <param name="paginationDTO"></param>
        /// <returns></returns>
        public static IActionResult ReturnInstantReportResponse<T>(this PaginationDTO<T> paginationDTO, string successMessage, string errorMessage = null)
        {
            SingleObjectResponse<PaginationDTO<T>> response = new SingleObjectResponse<PaginationDTO<T>> { StatusCode = MilvasoftStatusCodes.Status200OK };
            response.Message = successMessage;
            if (paginationDTO.DTOList.IsNullOrEmpty())
            {
                response.Message = errorMessage;
                response.StatusCode = MilvasoftStatusCodes.Status204NoContent;
                response.Success = true;
            }
            return new OkObjectResult(response);
        }

        /// <summary>
        /// <para> Controller configuration. </para>
        /// </summary>
        /// <param name="asyncTask"></param>
        /// <param name="idList"></param>
        /// <returns></returns>
        public static async Task<MultipleObjectResponse<T>> GetArrayResponseForDeleteAsync<T, TKey>(this ConfiguredTaskAwaitable asyncTask, IEnumerable<TKey> idList, string successMessage, string errorMessage = null) where TKey : struct
        {
            MultipleObjectResponse<T> response = new MultipleObjectResponse<T>();
            if (idList.IsNullOrEmpty())
            {
                response.Message = errorMessage;
                response.StatusCode = MilvasoftStatusCodes.Status204NoContent;
                response.Success = true;
            }
            else
            {
                await asyncTask;
                response.Message = successMessage;
                response.StatusCode = MilvasoftStatusCodes.Status200OK;
                response.Success = true;
            }

            return response;
        }

        /// <summary>
        /// <para> Controller configuration. </para>
        /// </summary>
        /// <param name="asyncTask"></param>
        /// <returns></returns>
        public static async Task<SingleObjectResponse<T>> GetSingleResponseForDeleteAsync<T>(this ConfiguredTaskAwaitable asyncTask, string successMessage)
        {
            SingleObjectResponse<T> response = new SingleObjectResponse<T>();
            {
                await asyncTask;
                response.Message = successMessage;
                response.StatusCode = MilvasoftStatusCodes.Status200OK;
                response.Success = true;
            }

            return response;
        }

        /// <summary>
        /// <para> Controller configuration. </para>
        /// </summary>
        /// <param name="asyncTask"></param>
        /// <returns></returns>
        public static async Task<SingleObjectResponse<T>> GetSingleObjectResponseForAddOrUpdateAsync<T>(this ConfiguredTaskAwaitable asyncTask, string successMessage)
        {
            SingleObjectResponse<T> response = new SingleObjectResponse<T>();

            await asyncTask;
            response.Message = successMessage;
            return response;
        }

        /// <summary>
        /// <para> Controller configuration. </para>
        /// </summary>
        /// <param name="asyncTask"></param>
        /// <returns></returns>
        public static async Task<SingleObjectResponse<T>> GetSingleObjectResponseForSpecifedProcessAsync<T>(this ConfiguredTaskAwaitable asyncTask, string successMessage)
        {
            SingleObjectResponse<T> response = new SingleObjectResponse<T>();

            await asyncTask;
            response.Success = false;
            response.Message = successMessage;
            return response;
        }


    }
}
