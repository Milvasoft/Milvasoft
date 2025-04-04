﻿using Microsoft.AspNetCore.Http;
using Milvasoft.Components.Rest.Enums;
using System.Text.Json.Serialization;

namespace Milvasoft.Components.Rest.MilvaResponse;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Listed or paginated response.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ListResponse<T> : Response<List<T>>
{
    /// <summary>
    /// Current page number.
    /// </summary>
    [JsonPropertyOrder(4)]
    public int? CurrentPageNumber { get; set; }

    /// <summary>
    /// Total page count.
    /// </summary>
    [JsonPropertyOrder(5)]
    public int? TotalPageCount { get; set; }

    /// <summary>
    /// Total data count.
    /// </summary>
    [JsonPropertyOrder(6)]
    public int? TotalDataCount { get; set; }

    /// <summary>
    /// Aggregation results.
    /// </summary>
    [JsonPropertyOrder(8)]
    public List<AggregationResult> AggregationResults { get; set; }

    public ListResponse() : base()
    {
    }

    public ListResponse(string message) : base()
    {
        Messages = [new() { Message = message }];
    }

    public ListResponse(List<T> data, string message) : base(data, message)
    {
    }

    public ListResponse(List<T> data, ResponseMessage responseMessage) : base(data, responseMessage)
    {
    }

    public ListResponse(List<T> data,
                        int? currentPage = null,
                        int? totalPage = null,
                        int? totalData = null) : base(data)
    {
        CurrentPageNumber = currentPage;
        TotalPageCount = totalPage;
        TotalDataCount = totalData;
    }

    public ListResponse(bool isSuccess,
                        string message,
                        List<T> data,
                        int? currentPage = null,
                        int? totalPageCount = null,
                        int? totalDataCount = null) : this(isSuccess,
                                               [new() { Message = message }],
                                               data,
                                               currentPage,
                                               totalPageCount,
                                               totalDataCount)
    {
    }

    public ListResponse(bool isSuccess,
                        List<ResponseMessage> messages,
                        List<T> data,
                        int? currentPage = null,
                        int? totalPageCount = null,
                        int? totalDataCount = null) : base(data)
    {
        IsSuccess = isSuccess;
        Messages = messages;
        CurrentPageNumber = currentPage;
        TotalPageCount = totalPageCount;
        TotalDataCount = totalDataCount;
    }

    public ListResponse<TReturn> Convert<TReturn>(List<TReturn> data)
    {
        var response = new ListResponse<TReturn>
        {
            Data = data,
            IsSuccess = IsSuccess,
            Messages = Messages,
            CurrentPageNumber = CurrentPageNumber,
            TotalPageCount = TotalPageCount,
            TotalDataCount = TotalDataCount,
            AggregationResults = AggregationResults,
            IsCachedData = IsCachedData,
            Metadatas = Metadatas,
            StatusCode = StatusCode
        };

        return response;
    }

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    #region Success

    public static ListResponse<T> Success() => new(LocalizerKeys.Successful)
    {
        IsSuccess = true,
        StatusCode = StatusCodes.Status200OK,
    };

    public static ListResponse<T> Success(List<T> data) => new(data, LocalizerKeys.Successful)
    {
        IsSuccess = true,
        StatusCode = StatusCodes.Status200OK,
    };

    public static ListResponse<T> Success(List<T> data, string message)
    {
        var response = new ListResponse<T>(data, message)
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status200OK,
        };

        return response;
    }

    public static ListResponse<T> Success(List<T> data, int? currentPage, int? totalPageCount, int? totalDataCount) => new(data, currentPage, totalPageCount, totalDataCount)
    {
        IsSuccess = true,
        StatusCode = StatusCodes.Status200OK,
    };

    public static ListResponse<T> Success(List<T> data, string message, int? currentPage, int? totalPageCount, int? totalDataCount)
    {
        var response = new ListResponse<T>(true, message, data, currentPage, totalPageCount, totalDataCount)
        {
            StatusCode = StatusCodes.Status200OK,
        };

        return response;
    }

    public static ListResponse<T> Success(List<T> data, string message, MessageType messageType)
    {
        var response = new ListResponse<T>(data)
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status200OK,
        };

        response.AddMessage(message, messageType);

        return response;
    }

    public static ListResponse<T> Success(List<T> data, ResponseMessage responseMessage)
    {
        var response = new ListResponse<T>(data, responseMessage)
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status200OK,
        };

        return response;
    }

    #endregion

    #region Error

    public static ListResponse<T> Error() => new(LocalizerKeys.Failed)
    {
        IsSuccess = false,
        StatusCode = StatusCodes.Status400BadRequest,
    };

    public static ListResponse<T> Error(List<T> data) => new(data)
    {
        IsSuccess = false,
        StatusCode = StatusCodes.Status400BadRequest,
    };

    public static ListResponse<T> Error(List<T> data, string message)
    {
        var response = new ListResponse<T>(data, message)
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
        };

        return response;
    }

    public static ListResponse<T> Error(List<T> data, int? currentPage, int? totalPageCount, int? totalDataCount) => new(data, currentPage, totalPageCount, totalDataCount)
    {
        IsSuccess = false,
        StatusCode = StatusCodes.Status400BadRequest,
    };

    public static ListResponse<T> Error(List<T> data, string message, int? currentPage, int? totalPageCount, int? totalDataCount)
    {
        var response = new ListResponse<T>(false, message, data, currentPage, totalPageCount, totalDataCount)
        {
            StatusCode = StatusCodes.Status400BadRequest,
        };

        return response;
    }

    public static ListResponse<T> Error(List<T> data, string message, MessageType messageType)
    {
        var response = new ListResponse<T>(data)
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
        };

        response.AddMessage(message, messageType);

        return response;
    }

    public static ListResponse<T> Error(List<T> data, ResponseMessage responseMessage)
    {
        var response = new ListResponse<T>(data, responseMessage)
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
        };

        return response;
    }

    #endregion
}
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
