using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Utils.Converters;
using System.Text.Json;

namespace Milvasoft.Components.Rest;

/// <summary>
/// Service collection extensions for rest components.
/// </summary>
public static partial class RestExtensions
{
    /// <summary>
    /// Adds response converters to the <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to add the converters to.</param>
    /// <returns>The updated <see cref="JsonSerializerOptions"/> with added response converters.</returns>
    public static JsonSerializerOptions AddResponseConverters(this JsonSerializerOptions options)
    {
        if (options == null)
            return null;

        options.Converters.Add(new InterfaceConverterFactory<Response, IResponse>());

        return options;
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
