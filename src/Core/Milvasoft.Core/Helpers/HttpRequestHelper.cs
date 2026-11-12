using System.Net;
using System.Text.Json;

namespace Milvasoft.Core;

/// <summary>
/// Helper class for request processes.
/// </summary>
public static class HttpRequestHelper
{
    /// <summary>
    /// Creates an HTTP request message with the specified HTTP method, URL, content, encoding, media type, version, and headers.
    /// </summary>
    /// <param name="httpMethod">The HTTP method of the request.</param>
    /// <param name="url">The URL of the request.</param>
    /// <param name="content">The content of the request. It is serialized to JSON. If null, the request has no body.</param>
    /// <param name="encoding">The encoding to be used for the request content. Default is <see cref="Encoding.UTF8"/></param>
    /// <param name="mediaType">The media type of the request content. Default is <see cref="MimeTypeNames.ApplicationJson"/>.</param>
    /// <param name="version">The version of the HTTP protocol to be used. Default is HTTP/1.1.</param>
    /// <param name="headers">The headers to be added to the request.</param>
    /// <param name="jsonSerializerOptions">The options used to serialize <paramref name="content"/>. Default is <see cref="JsonSerializerOptions.Default"/>.</param>
    /// <returns>The created HttpRequestMessage object.</returns>
    public static HttpRequestMessage BuildRequestMessage(this HttpMethod httpMethod,
                                                         string url,
                                                         object content = null,
                                                         List<KeyValuePair<string, string>> headers = null,
                                                         Encoding encoding = null,
                                                         string mediaType = null,
                                                         Version version = null,
                                                         JsonSerializerOptions jsonSerializerOptions = null)
    {
        var requestMessage = new HttpRequestMessage(httpMethod, new Uri(url))
        {
            Version = version ?? HttpVersion.Version11
        };

        if (content != null)
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(content, jsonSerializerOptions ?? JsonSerializerOptions.Default),
                                                       encoding ?? Encoding.UTF8,
                                                       mediaType ?? MimeTypeNames.ApplicationJson);

        if (!headers.IsNullOrEmpty())
            for (var i = 0; i < headers.Count; i++)
                requestMessage.Headers.Add(headers[i].Key, headers[i].Value);

        return requestMessage;
    }

    /// <summary>
    /// <b>Makes reques url from requested params.</b> <para> For more detailed information of URL Sections, please refer to the remarks section where the method is described.</para>
    /// </summary>
    /// 
    /// Remarks:
    /// 
    ///   https://www.milvasoft.com:444/api/v1/products/product?id=1
    ///   _____   ________________  ___ _______________________  ____  
    ///     |             |          |              |              |   
    ///   protocol        |          |           pathName        query 
    ///               hostName       |                                 
    ///                             port                               
    /// 
    /// 
    /// 
    ///  !!! Hostname can be IP Address. (e.g. 127.20.10.1) 
    /// 
    /// <param name="protocol"></param>
    /// <param name="hostName"></param>
    /// <param name="port"></param>
    /// <param name="pathName"></param>
    /// <param name="query"></param>
    /// <param name="hash"></param>
    /// <returns></returns>
    public static string BuildRequestUrl(string protocol,
                                         string hostName,
                                         string port,
                                         string pathName,
                                         string query = "",
                                         string hash = "")
    {
        query = string.IsNullOrEmpty(query) ? query : string.Concat("?", query);
        hash = string.IsNullOrEmpty(hash) ? hash : string.Concat("#", hash);
        var requestUrl = $"{protocol}://{hostName}:{port}/{pathName}{query}{hash}";

        ThrowIfNotValidUrl(requestUrl);

        return requestUrl;
    }

    /// <summary>
    /// <b>Makes reques url from requested params.</b>  <para> For more detailed information of URL Sections, please refer to the remarks section where the method is described. </para>
    /// </summary>
    /// 
    /// Remarks:
    /// 
    ///   https://www.milvasoft.com:444/api/v1/products/product?id=1
    ///   _________________________  ___ _______________________  ____  
    ///                |              |              |              |   
    ///             address           |           pathName        query 
    ///                               |                                 
    ///                              port                               
    /// 
    /// 
    /// 
    ///  !!! Hostname can be IP Address. (e.g. 127.20.10.1) 
    /// 
    /// <param name="address"></param>
    /// <param name="pathName"></param>
    /// <param name="query"></param>
    /// <param name="hash"></param>
    /// <returns></returns>
    public static string BuildRequestUrlFromAddress(string address,
                                                    string pathName,
                                                    string query = "",
                                                    string hash = "")
    {
        query = string.IsNullOrEmpty(query) ? query : string.Concat("?", query);
        hash = string.IsNullOrEmpty(hash) ? hash : string.Concat("#", hash);
        var requestUrl = $"{address}/{pathName}{query}{hash}";

        ThrowIfNotValidUrl(requestUrl);

        return requestUrl;
    }

    /// <summary>
    /// Validates that <paramref name="url"/> is an absolute http, https, ftp or ftps url with a host.
    /// </summary>
    private static void ThrowIfNotValidUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https" or "ftp" or "ftps") || string.IsNullOrEmpty(uri.Host))
            throw new MilvaDeveloperException(LocalizerKeys.InvalidUrlErrorMessage);
    }
}
