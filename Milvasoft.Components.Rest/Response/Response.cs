﻿using Milvasoft.Components.Rest.Enums;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Milvasoft.Components.Rest.Response;

[DataContract]
public class Response : IResponse
{
    [DataMember]
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Gets or sets a status code indicating whether the response is successful.
    /// </summary>
    public int StatusCode { get; set; } = 200;

    [DataMember]
    public List<ResponseMessage> Messages { get; set; }

    public Response()
    {
        Messages = [];
    }

    public Response(string message) : this()
    {
        AddMessage(message);
    }

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
    public void AddMessage(string message, MessageType messageType) => Messages.Add(new ResponseMessage(message, messageType));


    /// <summary>
    /// Adds message to <paramref name="message"/> with key.
    /// </summary>
    /// <param name="message"></param>
    public void AddMessage(string key, string message, MessageType messageType) => Messages.Add(new ResponseMessage(key, message, messageType));

    /// <summary>
    /// Throws exception if <see cref="IsSuccess"/> is false.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void ThrowExceptionIfFail()
    {
        if (IsSuccess == false)
            throw new Exception(ToString());
    }

    public string MessagesToString(string delimiter = ";") => string.Join(delimiter, Messages.Select(m => $"{m.Key}: {m.Message}"));

    public override string ToString() => MessagesToString();

    #region Success

    public static Response Success() => new()
    {
        IsSuccess = true,
        StatusCode = 200,
    };

    public static Response Success(string message)
    {
        var response = new Response(message)
        {
            IsSuccess = true,
            StatusCode = 200,
        };

        return response;
    }

    public static Response Success(string message, MessageType messageType)
    {
        var response = new Response(message)
        {
            IsSuccess = true,
            StatusCode = 200,
        };

        response.AddMessage(message, messageType);

        return response;
    }

    public static Response Success(ResponseMessage responseMessage)
    {
        var response = new Response(responseMessage)
        {
            IsSuccess = true,
            StatusCode = 200,
        };

        return response;
    }

    #endregion


    #region Error

    public static Response Error() => new()
    {
        IsSuccess = false,
        StatusCode = 500,
    };

    public static Response Error(string message)
    {
        var response = new Response(message)
        {
            IsSuccess = false,
            StatusCode = 500,
        };

        return response;
    }

    public static Response Error(string message, MessageType messageType)
    {
        var response = new Response(message)
        {
            IsSuccess = false,
            StatusCode = 500,
        };

        response.AddMessage(message, messageType);

        return response;
    }

    public static Response Error(ResponseMessage responseMessage)
    {
        var response = new Response(responseMessage)
        {
            IsSuccess = false,
            StatusCode = 500,
        };

        return response;
    }

    #endregion

}

public class Response<T> : Response
{
    public T Data { get; set; }

    [DataMember]
    public List<ResponseDataMetadata> ColumnTypes { get; set; }

    public (object, Type) GetResponseData()
    {
        return (Data, typeof(T));
    }

    public Response() : base()
    {

    }

    public Response(T data) : this()
    {
        Data = data;
    }

    public Response(T data, string message) : base(message)
    {
        Data = data;
    }

    public Response(T data, ResponseMessage responseMessage) : base(responseMessage)
    {
        Data = data;
    }

    #region Success

    public static Response<T> Success(T data) => new Response<T>(data)
    {
        IsSuccess = true,
        StatusCode = 200,
    };

    public static Response<T> Success(T data, string message)
    {
        var response = new Response<T>(data, message)
        {
            IsSuccess = true,
            StatusCode = 200,
        };

        return response;
    }

    public static Response<T> Success(T data, string message, MessageType messageType)
    {
        var response = new Response<T>(data)
        {
            IsSuccess = true,
            StatusCode = 200,
        };

        response.AddMessage(message, messageType);

        return response;
    }

    public static Response<T> Success(T data, ResponseMessage responseMessage)
    {
        var response = new Response<T>(data, responseMessage)
        {
            IsSuccess = true,
            StatusCode = 200,
        };

        return response;
    }

    #endregion


    #region Error

    public static Response<T> Error(T data) => new Response<T>(data)
    {
        IsSuccess = false,
        StatusCode = 500,
    };

    public static Response<T> Error(T data, string message)
    {
        var response = new Response<T>(data, message)
        {
            IsSuccess = false,
            StatusCode = 500,
        };

        return response;
    }

    public static Response<T> Error(T data, string message, MessageType messageType)
    {
        var response = new Response<T>(data)
        {
            IsSuccess = false,
            StatusCode = 500,
        };

        response.AddMessage(message, messageType);

        return response;
    }

    public static Response<T> Error(T data, ResponseMessage responseMessage)
    {
        var response = new Response<T>(data, responseMessage)
        {
            IsSuccess = false,
            StatusCode = 500,
        };

        return response;
    }

    #endregion
}