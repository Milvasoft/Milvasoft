using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Core.Utils.JsonConverters;
using System.Text.Json;

namespace Milvasoft.Components.Rest;
public static class RestExtensions
{
    public static JsonSerializerOptions AddResponseConverters(this JsonSerializerOptions options)
    {
        options.Converters.Add(new InterfaceConverterFactory(typeof(Response<>), typeof(IResponse<>)));
        options.Converters.Add(new InterfaceConverterFactory<Response.Response, IResponse>());

        return options;
    }

    public static bool TryConvertToResponse(object obj, out IResponse response)
    {
        try
        {
            response = (IResponse)obj;
        }
        catch
        {
            response = null;
            return false;
        }

        return true;
    }

    #region Success

    /// <summary>
    /// Returns <see cref="Response{T}"/> with success metadata.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Response<T> ToSuccessResponse<T>(this T data) => Response<T>.Success(data);

    /// <summary>
    /// Returns <see cref="Response{T}"/> with success metadata.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Response<T> ToSuccessResponse<T>(this T data, string message) => Response<T>.Success(data, message);

    /// <summary>
    /// Returns <see cref="Response{T}"/> with success metadata.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <param name="messageType"></param>
    /// <returns></returns>
    public static Response<T> ToSuccessResponse<T>(this T data, string message, MessageType messageType) => Response<T>.Success(data, message, messageType);

    /// <summary>
    /// Returns <see cref="Response{T}"/> with success metadata.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="responseMessage"></param>
    /// <returns></returns>
    public static Response<T> ToSuccessResponse<T>(this T data, ResponseMessage responseMessage) => Response<T>.Success(data, responseMessage);

    #endregion

    #region Error

    /// <summary>
    /// Returns <see cref="Response{T}"/> with error metadata.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Response<T> ToErrorResponse<T>(this T data) => Response<T>.Error(data);

    /// <summary>
    /// Returns <see cref="Response{T}"/> with error metadata.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Response<T> ToErrorResponse<T>(this T data, string message) => Response<T>.Error(data, message);

    /// <summary>
    /// Returns <see cref="Response{T}"/> with error metadata.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <param name="messageType"></param>
    /// <returns></returns>
    public static Response<T> ToErrorResponse<T>(this T data, string message, MessageType messageType) => Response<T>.Error(data, message, messageType);

    /// <summary>
    /// Returns <see cref="Response{T}"/> with error metadata.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="responseMessage"></param>
    /// <returns></returns>
    public static Response<T> ToErrorResponse<T>(this T data, ResponseMessage responseMessage) => Response<T>.Error(data, responseMessage);

    #endregion

}
