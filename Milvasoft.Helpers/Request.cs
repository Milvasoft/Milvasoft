using Milvasoft.Helpers.Models.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Milvasoft.Helpers
{
    /// <summary>
    /// Helper class for request processes.
    /// </summary>
    public static class Request
    {
        /// <summary>
        /// Creates HTTP request message.
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="mediaType"> etc "json" </param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <returns> HttpRequestMessage </returns>
        public static HttpRequestMessage CreateRequestMessage(this HttpMethod httpMethod, Encoding encoding, string mediaType, string url, params KeyValuePair<string, string>[] headers)
        {
            var requestMessage = new HttpRequestMessage
            {
                Content = new StringContent("", encoding, "application/json-" + httpMethod.ToString() + $"+{mediaType}"),
                RequestUri = new Uri(url),
                Method = httpMethod
            };

            if (headers != null && headers.Length > 0)
                for (var i = 0; i < headers.Length; i++)
                    requestMessage.Headers.Add(headers[i].Key, headers[i].Value);

            return requestMessage;
        }

        /// <summary>
        /// Creates HTTP request message.
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="mediaType"> etc "json" </param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <param name="headers"></param>
        /// <returns> HttpRequestMessage </returns>
        public static HttpRequestMessage CreateRequestMessage(this HttpMethod httpMethod, Encoding encoding, string mediaType, string url, object content , params KeyValuePair<string, string>[] headers)
        {
            var json = JsonConvert.SerializeObject(content);

            var requestMessage = new HttpRequestMessage
            {
                Content = new StringContent(json, encoding, "application/json-" + httpMethod.ToString() + $"+{mediaType}"),
                RequestUri = new Uri(url),
                Method = httpMethod
            };

            if (headers != null && headers.Length > 0)
                for (var i = 0; i < headers.Length; i++)
                    requestMessage.Headers.Add(headers[i].Key, headers[i].Value);

            return requestMessage;
        }


        /// <summary>
        /// Creates HTTP request message.
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="mediaType"> etc "json" </param>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <param name="version"></param>
        /// <param name="headers"></param>
        /// <returns> HttpRequestMessage </returns>
        public static HttpRequestMessage CreateRequestMessage(this HttpMethod httpMethod, Encoding encoding, string mediaType, string url, object content , Version version , params KeyValuePair<string, string>[] headers)
        {
            var json = JsonConvert.SerializeObject(content);

            var requestMessage = new HttpRequestMessage
            {
                Content = new StringContent(json, encoding, "application/json-" + httpMethod.ToString() + $"+{mediaType}"),
                RequestUri = new Uri(url),
                Method = httpMethod,
                Version = version != null ? version : new Version(1, 0)
            };

            if (headers != null && headers.Length > 0)
                for (var i = 0; i < headers.Length; i++)
                    requestMessage.Headers.Add(headers[i].Key, headers[i].Value);

            return requestMessage;
        }

        /// <summary>
        /// Sends request by <paramref name="httpRequestMessage"/>.
        /// <para><b> If you want to use the "ErrorCodes" property, change the return value to (ExceptionResponse) where necessary.</b></para> 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpRequestMessage"></param>
        /// <param name="httpClient"></param>
        /// <returns> ObjectResponse </returns>
        public static async Task<object> SendRequestDeserializeSingle<T>(this HttpClient httpClient, HttpRequestMessage httpRequestMessage)
        {
            var response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var responseObject = (JObject)JsonConvert.DeserializeObject(contentString);

                if (!responseObject.PropertyExists("errorCodes")) return responseObject.ToObject<SingleObjectResponse<T>>();
                else return responseObject.ToObject<ExceptionResponse>();
            }

            return response;
        }

        /// <summary>
        /// Sends request by <paramref name="httpRequestMessage"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpRequestMessage"></param>
        /// <param name="httpClient"></param>
        /// <returns> ObjectResponse </returns>
        public static async Task<object> SendRequestDeserializeMultiple<T>(this HttpClient httpClient, HttpRequestMessage httpRequestMessage)
        {
            var response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var responseObject = (JObject)JsonConvert.DeserializeObject(contentString);

                if (!responseObject.PropertyExists("errorCodes")) return responseObject.ToObject<MultipleObjectResponse<T>>();
                else return responseObject.ToObject<ExceptionResponse>();
            }
            return response;
        }

        /// <summary>
        /// Sends request by <paramref name="httpRequestMessage"/>.
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <param name="httpClient"></param>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns> <typeparamref name="TReturn"/> </returns>
        public static async Task<TReturn> SendRequestDeserialize<TReturn>(this HttpClient httpClient, HttpRequestMessage httpRequestMessage)
        {
            var response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            var contentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<TReturn>(contentString);
        }


        /// <summary>
        /// Checks <paramref name="propertyName"/> exists in <paramref name="jObject"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="jObject"></param>
        /// <returns></returns>
        public static bool PropertyExists(this JObject jObject, string propertyName) => jObject.Property(propertyName) != null;

        /// <summary>
        /// <b>Makes reques url from requested params.</b> <para> For more detailed information of URL Sections, please refer to the remarks section where the method is described.</para>
        /// </summary>
        /// 
        /// Remarks:
        /// 
        ///   https: //www.milvasoft.com:444/api/v1/products/product?id=1
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
        public static string CreateRequestUrl(string protocol, string hostName, string port, string pathName, string query = "", string hash = "")
        {
            query = query == "" ? query : "?" + query;
            hash = hash == "" ? hash : "#" + hash;
            var requestUrl = $"{protocol}://{hostName}:{port}/{pathName}{query}{hash}";

            // Create new Regex.
            var regex = new Regex(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$");

            // Call Match on Regex instance.
            var match = regex.Match(requestUrl);

            try
            {
                // Test for Success.
                if (!match.Success)
                    throw new Exception("Invalid url");

                return requestUrl;
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid url: " + requestUrl); throw;
            }
        }

        /// <summary>
        /// <b>Makes reques url from requested params.</b>  <para> For more detailed information of URL Sections, please refer to the remarks section where the method is described. </para>
        /// </summary>
        /// 
        /// Remarks:
        /// 
        ///   https: //www.milvasoft.com:444/api/v1/products/product?id=1
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
        public static string CreateRequestUrlFromAddress(string address, string pathName, string query = "", string hash = "")
        {
            query = query == "" ? query : "?" + query;
            hash = hash == "" ? hash : "#" + hash;
            var requestUrl = $"{address}/{pathName}{query}{hash}";

            // Create new Regex.
            var regex = new Regex(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$");

            // Call Match on Regex instance.
            var match = regex.Match(requestUrl);

            try
            {
                // Test for Success.
                if (!match.Success)
                    throw new Exception("Invalid url");

                return requestUrl;
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid url: " + requestUrl); throw;
            }

        }

        /// <summary>
        /// Determines <paramref name="baseResponse"/>'s status code success or fail.
        /// </summary>
        /// <param name="baseResponse"></param>
        /// <returns></returns>
        public static bool IsSuccessStatusCode(this BaseResponse baseResponse)
        {
            return ((int)baseResponse.StatusCode >= 200) && ((int)baseResponse.StatusCode <= 299);
        }

        private static KeyValuePair<string, string> GetProperty(JObject objects, string propertyKey, params string[] propertyNames)
        {
            var token = objects[propertyNames.First()];
            foreach (var name in propertyNames.Skip(1))
                token = token[0][name];
            return new KeyValuePair<string, string>(propertyKey, (string)token);
        }
    }
}
