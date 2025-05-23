﻿using Microsoft.AspNetCore.Http;
using Milvasoft.Components.Rest.Enums;
using System.Text.Json.Serialization;

namespace Milvasoft.Components.Rest.MilvaResponse;

/// <summary>
/// Response model. 
/// </summary>
public class Response : IResponse
{
    /// <summary>
    /// Determines whether response is success or not.
    /// </summary>
    [JsonPropertyOrder(1)]
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Gets or sets a status code indicating whether the response is successful.
    /// </summary>
    [JsonPropertyOrder(2)]
    public int StatusCode { get; set; } = 200;

    /// <summary>
    /// Response messages.
    /// </summary>
    [JsonPropertyOrder(3)]
    public List<ResponseMessage> Messages { get; set; }

    /// <summary>
    /// Initializes new instance.
    /// </summary>
    public Response()
    {
        Messages = [];
    }

    /// <summary>
    /// Initializes new instance with <paramref name="message"/>.
    /// </summary>
    /// <param name="message"></param>
    public Response(string message) : this()
    {
        AddMessage(message);
    }

    /// <summary>
    /// Initializes new instance with <paramref name="responseMessage"/>.
    /// </summary>
    /// <param name="responseMessage"></param>
    public Response(ResponseMessage responseMessage) : this()
    {
        Messages.Add(responseMessage);
    }

    /// <summary>
    /// Adds message to <paramref name="message"/>.
    /// </summary>
    /// <param name="message"></param>
    public void AddMessage(string message) => Messages.Add(new ResponseMessage(message, MessageType.Information));

    /// <summary>
    /// Adds message to <paramref name="message"/>.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="messageType"></param>
    public void AddMessage(string message, MessageType messageType) => Messages.Add(new ResponseMessage(message, messageType));

    /// <summary>
    /// Adds message to <paramref name="message"/> with key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="message"></param>
    /// <param name="messageType"></param>
    public void AddMessage(string key, string message, MessageType messageType) => Messages.Add(new ResponseMessage(key, message, messageType));

    /// <summary>
    /// Throws exception if <see cref="IsSuccess"/> is false.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void ThrowExceptionIfFail()
    {
        if (!IsSuccess)
            throw new MilvaDeveloperException(ToString());
    }

    /// <summary>
    /// Merges response messages to string with <paramref name="delimiter"/>.
    /// </summary>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public string MessagesToString(string delimiter = ";") => string.Join(delimiter, Messages.Select(m => $"{m.Key}: {m.Message}"));

    /// <summary>
    /// Merges response messages to string with ';' delimeter.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => MessagesToString();

    #region Success

    /// <summary>
    /// Creates success response with <see cref="LocalizerKeys.Successful"/> message.
    /// </summary>
    /// <returns></returns>
    public static Response Success() => new(LocalizerKeys.Successful)
    {
        IsSuccess = true,
        StatusCode = StatusCodes.Status200OK,
    };

    /// <summary>
    /// Creates success response with <paramref name="message"/>
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Response Success(string message)
    {
        var response = new Response(message)
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status200OK,
        };

        return response;
    }

    /// <summary>
    /// Creates success response with <paramref name="message"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="messageType"></param>
    /// <returns></returns>
    public static Response Success(string message, MessageType messageType)
    {
        var response = new Response
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status200OK,
        };

        response.AddMessage(message, messageType);

        return response;
    }

    /// <summary>
    /// Creates success response with <paramref name="responseMessage"/>
    /// </summary>
    /// <param name="responseMessage"></param>
    /// <returns></returns>
    public static Response Success(ResponseMessage responseMessage)
    {
        var response = new Response(responseMessage)
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status200OK,
        };

        return response;
    }

    #endregion

    #region Error

    /// <summary>
    /// Creates error response with <see cref="LocalizerKeys.Failed"/> message.
    /// </summary>
    /// <returns></returns>
    public static Response Error() => new(new ResponseMessage(LocalizerKeys.Failed, MessageType.Warning))
    {
        IsSuccess = false,
        StatusCode = StatusCodes.Status400BadRequest,
    };

    /// <summary>
    /// Creates success response with <paramref name="message"/>
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Response Error(string message)
    {
        var response = new Response(new ResponseMessage(message, MessageType.Warning))
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
        };

        return response;
    }

    /// <summary>
    /// Creates success response with <paramref name="message"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="messageType"></param>
    /// <returns></returns>
    public static Response Error(string message, MessageType messageType)
    {
        var response = new Response
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
        };

        response.AddMessage(message, messageType);

        return response;
    }

    /// <summary>
    /// Creates success response with <paramref name="responseMessage"/>
    /// </summary>
    /// <param name="responseMessage"></param>
    /// <returns></returns>
    public static Response Error(ResponseMessage responseMessage)
    {
        var response = new Response(responseMessage)
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
        };

        return response;
    }

    #endregion

}

/// <summary>
/// Response model. 
/// </summary>
/// <typeparam name="T"></typeparam>
public class Response<T> : Response, IResponse<T>
{
    /// <summary>
    /// Response data.
    /// </summary>
    [JsonPropertyOrder(7)]
    public T Data { get; set; }

    /// <summary>
    /// Specifies the response data is cached or not.
    /// </summary>
    [JsonIgnore]
    public bool IsCachedData { get; set; }

    /// <summary>
    /// Response data metadatas.
    /// </summary>
    [JsonPropertyOrder(99)]
    public List<ResponseDataMetadata> Metadatas { get; set; }

    /// <summary>
    /// Gets response data and type.
    /// </summary>
    /// <returns></returns>
    public (object, Type) GetResponseDataTypePair() => (Data, typeof(T));

    #region Constructors

    /// <summary>
    /// Initializes new instance.
    /// </summary>
    public Response() : base()
    {

    }

    /// <summary>
    /// Initializes new instance with <paramref name="data"/>.
    /// </summary>
    /// <param name="data"></param>
    public Response(T data) : this()
    {
        Data = data;
    }

    /// <summary>
    /// Initializes new instance <paramref name="data"/> and <paramref name="message"/>.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="message"></param>
    public Response(T data, string message) : base(message)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes new instance <paramref name="data"/> and <paramref name="responseMessage"/>
    /// </summary>
    /// <param name="data"></param>
    /// <param name="responseMessage"></param>
    public Response(T data, ResponseMessage responseMessage) : base(responseMessage)
    {
        Data = data;
    }

    #endregion

    #region Success

    /// <summary>
    /// Creates success response with <paramref name="data"/> and <see cref="LocalizerKeys.Successful"/> message.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Response<T> Success(T data) => new(data, LocalizerKeys.Successful)
    {
        IsSuccess = true,
        StatusCode = StatusCodes.Status200OK,
    };

    /// <summary>
    /// Creates success response with <paramref name="data"/> and <paramref name="message"/> message.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Response<T> Success(T data, string message)
    {
        var response = new Response<T>(data, message)
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status200OK,
        };

        return response;
    }

    /// <summary>
    /// Creates success response with <paramref name="data"/> and <paramref name="message"/> message.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <param name="messageType"></param>
    /// <returns></returns>
    public static Response<T> Success(T data, string message, MessageType messageType)
    {
        var response = new Response<T>(data)
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status200OK,
        };

        response.AddMessage(message, messageType);

        return response;
    }

    /// <summary>
    /// Creates success response with <paramref name="data"/> and <paramref name="responseMessage"/> message.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="responseMessage"></param>
    /// <returns></returns>
    public static Response<T> Success(T data, ResponseMessage responseMessage)
    {
        var response = new Response<T>(data, responseMessage)
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status200OK,
        };

        return response;
    }

    #endregion

    #region Error

    /// <summary>
    /// Creates error response with <paramref name="data"/> and <see cref="LocalizerKeys.Failed"/> message.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Response<T> Error(T data) => new(data, new ResponseMessage(LocalizerKeys.Failed, MessageType.Warning))
    {
        IsSuccess = false,
        StatusCode = StatusCodes.Status400BadRequest,
    };

    /// <summary>
    /// Creates error response with <paramref name="data"/> and <paramref name="message"/> message.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Response<T> Error(T data, string message)
    {
        var response = new Response<T>(data, new ResponseMessage(message, MessageType.Warning))
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
        };

        return response;
    }

    /// <summary>
    /// Creates error response with <paramref name="data"/> and <paramref name="message"/> message.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <param name="messageType"></param>
    /// <returns></returns>
    public static Response<T> Error(T data, string message, MessageType messageType)
    {
        var response = new Response<T>(data)
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
        };

        response.AddMessage(message, messageType);

        return response;
    }

    /// <summary>
    /// Creates error response with <paramref name="data"/> and <paramref name="responseMessage"/> message.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="responseMessage"></param>
    /// <returns></returns>
    public static Response<T> Error(T data, ResponseMessage responseMessage)
    {
        var response = new Response<T>(data, responseMessage)
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
        };

        return response;
    }

    #endregion
}