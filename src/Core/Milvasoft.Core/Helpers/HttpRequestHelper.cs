using System.Text.Json;
using System.Text.RegularExpressions;

namespace Milvasoft.Core;

/// <summary>
/// Helper class for request processes.
/// </summary>
public static partial class HttpRequestHelper
{
    private static readonly string _applicationJson = $"{MimeTypeNames.ApplicationJson}-";

    /// <summary>
    /// Creates an HTTP request message with the specified HTTP method, URL, content, encoding, media type, version, and headers.
    /// </summary>
    /// <param name="httpMethod">The HTTP method of the request.</param>
    /// <param name="url">The URL of the request.</param>
    /// <param name="content">The content of the request.</param>
    /// <param name="encoding">The encoding to be used for the request content. Default is <see cref="Encoding.UTF8"/></param>
    /// <param name="mediaType">The media type of the request content. Default is 'json'</param>
    /// <param name="version">The version of the HTTP protocol to be used. Default is '1.0'</param>
    /// <param name="headers">The headers to be added to the request.</param>
    /// <returns>The created HttpRequestMessage object.</returns>
    public static HttpRequestMessage BuildRequestMessage(this HttpMethod httpMethod,
                                                         string url,
                                                         object content = null,
                                                         List<KeyValuePair<string, string>> headers = null,
                                                         Encoding encoding = null,
                                                         string mediaType = null,
                                                         Version version = null)
    {
        encoding ??= Encoding.UTF8;
        mediaType ??= MimeTypeNames.Json;
        version ??= new Version(1, 0);

        var contentString = content != null ? JsonSerializer.Serialize(content) : string.Empty;

        var requestMessage = new HttpRequestMessage
        {
            Content = new StringContent(contentString, encoding, _applicationJson + httpMethod.ToString() + $"+{mediaType}"),
            RequestUri = new Uri(url),
            Method = httpMethod,
            Version = version
        };

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
        query = query == string.Empty ? query : "?" + query;
        hash = hash == string.Empty ? hash : "#" + hash;
        var requestUrl = $"{protocol}://{hostName}:{port}/{pathName}{query}{hash}";

        // Create new Regex.
        var regex = UriRegex();

        // Call Match on Regex instance.
        var match = regex.Match(requestUrl);

        // Test for Success.
        if (!match.Success)
            throw new MilvaDeveloperException(LocalizerKeys.InvalidUrlErrorMessage);

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
        query = query == string.Empty ? query : "?" + query;
        hash = hash == string.Empty ? hash : "#" + hash;
        var requestUrl = $"{address}/{pathName}{query}{hash}";

        // Create new Regex.
        var regex = UriRegex();

        // Call Match on Regex instance.
        var match = regex.Match(requestUrl);

        // Test for Success.
        if (!match.Success)
            throw new MilvaDeveloperException(LocalizerKeys.InvalidUrlErrorMessage);

        return requestUrl;
    }

    [GeneratedRegex(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\=\,\'\/\\\+&%\$#_]*)?$")]
    private static partial Regex UriRegex();
}
