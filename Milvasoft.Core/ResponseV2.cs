﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Milvasoft.Core.Extensions;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Core.Utils.Enums;
using Milvasoft.Core.Utils.Models;
using Milvasoft.Core.Utils.Models.Response;
using System.Runtime.CompilerServices;

namespace Milvasoft.Core;

/// <summary>
/// <para>Helper class to generate the same type of response.</para>
/// <para>All methods return HTTP status code 200 OK.All methods return <see cref="BaseResponse"/> type in ActionResult. Base response can be <see cref="ObjectResponse{T}" /> or <see cref="ObjectResponse{T}"/></para>
/// </summary>
public static class ResponseV2
{
    /// <summary>
    /// Sets error message or success message with nameof(<typeparamref name="T"/>).
    /// If <paramref name="setDefaultSuccessMessageKey"/> is true it sets <see cref="LocalizerKeys.SuccessfullyOperationMessage"/> to success message.
    /// </summary>
    /// <param name="paginationDTO"></param>
    /// <param name="httpContext"></param>
    /// <param name="localizerKey"></param>
    /// <param name="isFiltering"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns>  <see cref="ObjectResponse{PaginationDTO}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static IActionResult GetPaginationResponseByEntities<T>(this PaginationDTO<T> paginationDTO,
                                                                   HttpContext httpContext,
                                                                   string localizerKey,
                                                                   bool isFiltering,
                                                                   bool setDefaultSuccessMessageKey = true)
    {
        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var response = new ObjectResponse<PaginationDTO<T>>();

        if (paginationDTO.DTOList.IsNullOrEmpty())
        {
            response.Message = stringLocalizer.GetErrorMessage(localizerKey, !isFiltering ? CrudOperation.GetAll : CrudOperation.Filtering);
            response.StatusCode = MilvaStatusCodes.Status204NoContent;
        }
        else
        {
            response.Result = paginationDTO;
            response.Message = !setDefaultSuccessMessageKey
                                    ? stringLocalizer.GetSuccessMessage(localizerKey, !isFiltering ? CrudOperation.GetAll : CrudOperation.Filtering)
                                    : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage];
        }

        return new OkObjectResult(response);
    }

    /// <summary>
    /// Sets error message or success message with nameof(<typeparamref name="T"/>).
    /// If <paramref name="setDefaultSuccessMessageKey"/> is true it sets <see cref="LocalizerKeys.SuccessfullyOperationMessage"/> to success message.
    /// </summary>
    /// <param name="contentList"></param>
    /// <param name="httpContext"></param>
    /// <param name="localizerKey"></param>
    /// <param name="isFiltering"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static IActionResult GetObjectResponseByEntities<T>(this List<T> contentList,
                                                               HttpContext httpContext,
                                                               string localizerKey,
                                                               bool isFiltering,
                                                               bool setDefaultSuccessMessageKey = true)
    {
        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var response = new ObjectResponse<List<T>>();

        if (contentList.IsNullOrEmpty())
        {
            response.Message = stringLocalizer.GetErrorMessage(localizerKey, !isFiltering ? CrudOperation.GetAll : CrudOperation.Filtering);
            response.StatusCode = MilvaStatusCodes.Status204NoContent;
        }
        else
        {
            response.Result = contentList;
            response.Message = !setDefaultSuccessMessageKey
                                    ? stringLocalizer.GetSuccessMessage(localizerKey, !isFiltering ? CrudOperation.GetAll : CrudOperation.Filtering)
                                    : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage];
        }

        return new OkObjectResult(response);
    }

    /// <summary>
    /// Sets error message or success message with nameof(<typeparamref name="T"/>).
    /// If <paramref name="setDefaultSuccessMessageKey"/> is true it sets <see cref="LocalizerKeys.SuccessfullyOperationMessage"/> to success message.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="httpContext"></param>
    /// <param name="localizerKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static IActionResult GetObjectResponseByEntity<T>(this T content,
                                                             HttpContext httpContext,
                                                             string localizerKey,
                                                             bool setDefaultSuccessMessageKey = true)
    {
        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var response = new ObjectResponse<T>();

        if (content == null)
        {
            response.Message = stringLocalizer.GetErrorMessage(localizerKey, httpContext.GetCrudOperationByMethod());
            response.StatusCode = MilvaStatusCodes.Status204NoContent;
        }
        else
        {
            response.Result = content;
            response.Message = !setDefaultSuccessMessageKey
                                ? stringLocalizer.GetSuccessMessage(localizerKey, httpContext.GetCrudOperationByMethod())
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage];
        }

        return new OkObjectResult(response);
    }

    /// <summary>
    /// If <paramref name="idList"/> is null or empty sets error message to <see cref="LocalizerKeys.PleaseEnterAValid"/>.
    /// If operation success sets success message with nameof(<typeparamref name="T"/>).
    /// If <paramref name="setDefaultSuccessMessageKey"/> is true it sets <see cref="LocalizerKeys.SuccessfullyOperationMessage"/> to success message.
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="idList"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T, TKey>(this ConfiguredTaskAwaitable asyncTask,
                                                                                    HttpContext httpContext,
                                                                                    IEnumerable<TKey> idList,
                                                                                    string successMessageKey = null,
                                                                                    bool setDefaultSuccessMessageKey = true) where TKey : IEquatable<TKey>
    {
        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var response = new ObjectResponse<T>();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        if (idList.IsNullOrEmpty())
        {
            response.Message = stringLocalizer[LocalizerKeys.PleaseEnterAValid, itemExists ? typeKey : "Item"];
            response.StatusCode = MilvaStatusCodes.Status600Exception;
            response.Success = false;
        }
        else
        {
            await asyncTask;

            response.Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage];
        }

        return new OkObjectResult(response);
    }

    /// <summary>
    /// If <paramref name="idList"/> is null or empty sets error message to <see cref="LocalizerKeys.PleaseEnterAValid"/>.
    /// If operation success sets success message with nameof(<typeparamref name="T"/>).
    /// If <paramref name="setDefaultSuccessMessageKey"/> is true it sets <see cref="LocalizerKeys.SuccessfullyOperationMessage"/> to success message.
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="idList"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>-
    /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T, TKey>(this Task asyncTask,
                                                                                    HttpContext httpContext,
                                                                                    IEnumerable<TKey> idList,
                                                                                    string successMessageKey = null,
                                                                                    bool setDefaultSuccessMessageKey = true) where TKey : IEquatable<TKey>
    {
        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var response = new ObjectResponse<T>();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        if (idList.IsNullOrEmpty())
        {
            response.Message = stringLocalizer[LocalizerKeys.PleaseEnterAValid, itemExists ? typeKey : "Item"];
            response.StatusCode = MilvaStatusCodes.Status600Exception;
            response.Success = false;
        }
        else
        {
            await asyncTask;

            response.Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage];
        }

        return new OkObjectResult(response);
    }

    /// <summary>
    /// If <paramref name="idList"/> is null or empty sets error message to <see cref="LocalizerKeys.PleaseEnterAValid"/>.
    /// If operation success sets success message with nameof(<typeparamref name="T"/>).
    /// If <paramref name="setDefaultSuccessMessageKey"/> is true it sets <see cref="LocalizerKeys.SuccessfullyOperationMessage"/> to success message.
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="idList"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T, TKey>(this ConfiguredTaskAwaitable<IEnumerable<T>> asyncTask,
                                                                                    HttpContext httpContext,
                                                                                    IEnumerable<TKey> idList,
                                                                                    string successMessageKey = null,
                                                                                    bool setDefaultSuccessMessageKey = true) where TKey : struct, IEquatable<TKey>
    {
        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var response = new ObjectResponse<IEnumerable<T>>();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        if (idList.IsNullOrEmpty())
        {
            response.Message = stringLocalizer[LocalizerKeys.PleaseEnterAValid, itemExists ? typeKey : "Item"];
            response.StatusCode = MilvaStatusCodes.Status600Exception;
            response.Success = false;
        }
        else
        {
            var result = await asyncTask;

            response.Result = result;
            response.Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage];
        }

        return new OkObjectResult(response);
    }

    /// <summary>
    /// If <paramref name="idList"/> is null or empty sets error message to <see cref="LocalizerKeys.PleaseEnterAValid"/>.
    /// If operation success sets success message with nameof(<typeparamref name="T"/>).
    /// If <paramref name="setDefaultSuccessMessageKey"/> is true it sets <see cref="LocalizerKeys.SuccessfullyOperationMessage"/> to success message.
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="idList"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T, TKey>(this Task<IEnumerable<T>> asyncTask,
                                                                                    HttpContext httpContext,
                                                                                    IEnumerable<TKey> idList,
                                                                                    string successMessageKey = null,
                                                                                    bool setDefaultSuccessMessageKey = true) where TKey : struct, IEquatable<TKey>
    {
        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var response = new ObjectResponse<IEnumerable<T>>();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        if (idList.IsNullOrEmpty())
        {
            response.Message = stringLocalizer[LocalizerKeys.PleaseEnterAValid, itemExists ? typeKey : "Item"];
            response.StatusCode = MilvaStatusCodes.Status600Exception;
            response.Success = false;
        }
        else
        {
            var result = await asyncTask;

            response.Result = result;
            response.Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage];
        }

        return new OkObjectResult(response);
    }

    /// <summary>
    /// Runs <paramref name="asyncTask"/> and if operation success sets success message with <see cref="Response.GetSuccessMessage(IStringLocalizer, string, CrudOperation, int?)"/> with nameof(<typeparamref name="T"/>).
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T>(this ConfiguredTaskAwaitable asyncTask,
                                                                              HttpContext httpContext,
                                                                              string successMessageKey = null,
                                                                              bool setDefaultSuccessMessageKey = true)
    {
        await asyncTask;

        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        var response = new ObjectResponse<T>
        {
            Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage]
        };

        return new OkObjectResult(response);
    }

    /// <summary>
    /// Runs <paramref name="asyncTask"/> and if operation success sets success message with <see cref="Response.GetSuccessMessage(IStringLocalizer, string, CrudOperation, int?)"/> with nameof(<typeparamref name="T"/>).
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T>(this ConfiguredTaskAwaitable<T> asyncTask,
                                                                              HttpContext httpContext,
                                                                              string successMessageKey = null,
                                                                              bool setDefaultSuccessMessageKey = true)
    {
        var result = await asyncTask;

        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        var response = new ObjectResponse<T>
        {
            Result = result,
            Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage]
        };

        return new OkObjectResult(response);
    }

    /// <summary>
    /// Runs <paramref name="asyncTask"/> and if operation success sets success message with <see cref="Response.GetSuccessMessage(IStringLocalizer, string, CrudOperation, int?)"/> with nameof(<typeparamref name="T"/>).
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T>(this ConfiguredTaskAwaitable<object> asyncTask,
                                                                              HttpContext httpContext,
                                                                              string successMessageKey = null,
                                                                              bool setDefaultSuccessMessageKey = true)
    {
        var result = await asyncTask;

        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        var response = new ObjectResponse<object>
        {
            Result = result,
            Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage]
        };

        return new OkObjectResult(response);
    }

    /// <summary>
    /// Runs <paramref name="asyncTask"/> and if operation success sets success message with <see cref="Response.GetSuccessMessage(IStringLocalizer, string, CrudOperation, int?)"/> with nameof(<typeparamref name="T"/>).
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T>(this ConfiguredTaskAwaitable<Guid> asyncTask,
                                                                              HttpContext httpContext,
                                                                              string successMessageKey = null,
                                                                              bool setDefaultSuccessMessageKey = true)
    {
        var result = await asyncTask;

        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        var response = new ObjectResponse<Guid>
        {
            Result = result,
            Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage]
        };

        return new OkObjectResult(response);
    }

    /// <summary>
    /// Runs <paramref name="asyncTask"/> and if operation success sets success message with <see cref="Response.GetSuccessMessage(IStringLocalizer, string, CrudOperation, int?)"/> with nameof(<typeparamref name="T"/>).
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T>(this ConfiguredTaskAwaitable<int> asyncTask,
                                                                              HttpContext httpContext,
                                                                              string successMessageKey = null,
                                                                              bool setDefaultSuccessMessageKey = true)
    {
        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var result = await asyncTask;

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        var response = new ObjectResponse<int>
        {
            Result = result,
            Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage]
        };

        return new OkObjectResult(response);
    }

    /// <summary>
    /// Runs <paramref name="asyncTask"/> and if operation success sets success message with <see cref="Response.GetSuccessMessage(IStringLocalizer, string, CrudOperation, int?)"/> with nameof(<typeparamref name="T"/>).
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T>(this ConfiguredTaskAwaitable<sbyte> asyncTask,
                                                                              HttpContext httpContext,
                                                                              string successMessageKey = null,
                                                                              bool setDefaultSuccessMessageKey = true)
    {
        var result = await asyncTask;

        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        var response = new ObjectResponse<sbyte>
        {
            Result = result,
            Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage]
        };

        return new OkObjectResult(response);
    }

    /// <summary>
    /// Runs <paramref name="asyncTask"/> and if operation success sets success message with <see cref="Response.GetSuccessMessage(IStringLocalizer, string, CrudOperation, int?)"/> with nameof(<typeparamref name="T"/>).
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T>(this Task asyncTask,
                                                                              HttpContext httpContext,
                                                                              string successMessageKey = null,
                                                                              bool setDefaultSuccessMessageKey = true)
    {
        await asyncTask;

        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        var response = new ObjectResponse<T>
        {
            Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage]
        };

        return new OkObjectResult(response);
    }

    /// <summary>
    /// Runs <paramref name="asyncTask"/> and if operation success sets success message with <see cref="Response.GetSuccessMessage(IStringLocalizer, string, CrudOperation, int?)"/> with nameof(<typeparamref name="T"/>).
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T>(this Task<T> asyncTask,
                                                                              HttpContext httpContext,
                                                                              string successMessageKey = null,
                                                                              bool setDefaultSuccessMessageKey = true)
    {
        var result = await asyncTask;

        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        var response = new ObjectResponse<T>
        {
            Result = result,
            Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage]
        };

        return new OkObjectResult(response);
    }

    /// <summary>
    /// Runs <paramref name="asyncTask"/> and if operation success sets success message with <see cref="Response.GetSuccessMessage(IStringLocalizer, string, CrudOperation, int?)"/> with nameof(<typeparamref name="T"/>).
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns>  <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T>(this Task<object> asyncTask,
                                                                              HttpContext httpContext,
                                                                              string successMessageKey = null,
                                                                              bool setDefaultSuccessMessageKey = true)
    {
        var result = await asyncTask;

        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        var response = new ObjectResponse<object>
        {
            Result = result,
            Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage]
        };

        return new OkObjectResult(response);
    }

    /// <summary>
    /// Runs <paramref name="asyncTask"/> and if operation success sets success message with <see cref="Response.GetSuccessMessage(IStringLocalizer, string, CrudOperation, int?)"/> with nameof(<typeparamref name="T"/>).
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T>(this Task<Guid> asyncTask,
                                                                              HttpContext httpContext,
                                                                              string successMessageKey = null,
                                                                              bool setDefaultSuccessMessageKey = true)
    {
        var result = await asyncTask;

        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        var response = new ObjectResponse<Guid>
        {
            Result = result,
            Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage]
        };

        return new OkObjectResult(response);
    }

    /// <summary>
    /// Runs <paramref name="asyncTask"/> and if operation success sets success message with <see cref="Response.GetSuccessMessage(IStringLocalizer, string, CrudOperation, int?)"/> with nameof(<typeparamref name="T"/>).
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T>(this Task<int> asyncTask,
                                                                              HttpContext httpContext,
                                                                              string successMessageKey = null,
                                                                              bool setDefaultSuccessMessageKey = true)
    {
        var result = await asyncTask;

        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        var response = new ObjectResponse<int>
        {
            Result = result,
            Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage]
        };

        return new OkObjectResult(response);
    }

    /// <summary>
    /// Runs <paramref name="asyncTask"/> and if operation success sets success message with <see cref="Response.GetSuccessMessage(IStringLocalizer, string, CrudOperation, int?)"/> with nameof(<typeparamref name="T"/>).
    /// </summary>
    /// <param name="asyncTask"></param>
    /// <param name="httpContext"></param>
    /// <param name="successMessageKey"></param>
    /// <param name="setDefaultSuccessMessageKey"></param>
    /// <returns> <see cref="ObjectResponse{T}"/> in 200 OK <see cref="ActionResult"/> </returns>
    public static async Task<IActionResult> GetObjectResponseByEntityAsync<T>(this Task<sbyte> asyncTask,
                                                                              HttpContext httpContext,
                                                                              string successMessageKey = null,
                                                                              bool setDefaultSuccessMessageKey = true)
    {
        var result = await asyncTask;

        var stringLocalizer = httpContext.RequestServices.GetRequiredLocalizerInstanceWithMilvaResource();

        var itemExists = httpContext.Items.TryGetValue("ActionContent", out object key);

        string typeKey = itemExists ? key.ToString() : LocalizerKeys.SuccessfullyOperationMessage;

        var response = new ObjectResponse<sbyte>
        {
            Result = result,
            Message = !setDefaultSuccessMessageKey
                                ? string.IsNullOrWhiteSpace(successMessageKey)
                                        ? stringLocalizer.GetSuccessMessage(typeKey, httpContext.GetCrudOperationByMethod())
                                        : stringLocalizer[successMessageKey]
                                : stringLocalizer[LocalizerKeys.SuccessfullyOperationMessage]
        };

        return new OkObjectResult(response);
    }

    private static CrudOperation GetCrudOperationByMethod(this HttpContext httpContext) => httpContext.Request.Method switch
    {
        "POST" => CrudOperation.Add,
        "PUT" => CrudOperation.Update,
        "DELETE" => CrudOperation.Delete,
        "GET" => CrudOperation.GetById,
        "PATCH" => CrudOperation.GetAll,
        _ => CrudOperation.Specific,
    };
}