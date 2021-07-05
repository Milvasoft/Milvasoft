using Milvasoft.Helpers.MilvaTest.Integration.TestStartup.Abstract;
using Milvasoft.Helpers.Models;
using Milvasoft.Helpers.Models.Response;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.MilvaTest.Integration.Helpers
{
    /// <summary>
    /// Request helper class.
    /// </summary>
    public static class RequestHelper
    {
        /// <summary>
        /// Cretater http request messsage.
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <param name="acceptLanguage"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static HttpRequestMessage HttpRequestMessage(HttpMethod httpMethod, string url, string token = null, object obj = null, string acceptLanguage = "tr-TR")
        {
            var json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var requestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(MilvaTestStartup._testApiBaseUrl + url),
                Method = httpMethod
            };

            requestMessage.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(acceptLanguage));
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            if (json != "null")
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(token))
                requestMessage.Headers.Add("Authorization", $"Bearer {token}");

            return requestMessage;
        }

        /// <summary>
        /// Returns http response.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpRequestMessage"></param>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static async Task<ObjectResponse<T>> GetHttpResponseAsync<T>(HttpRequestMessage httpRequestMessage, HttpClient httpClient)
        {
            var response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            string responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var responseObject = JsonConvert.DeserializeObject<ObjectResponse<T>>(responseString);

            return responseObject;
        }

        /// <summary>
        /// Returns http response in pagination dto.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpRequestMessage"></param>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static async Task<ObjectResponse<PaginationDTO<T>>> GetHttpPaginateResponseAsync<T>(HttpRequestMessage httpRequestMessage, HttpClient httpClient)
        {
            var response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            string responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var responseObject = JsonConvert.DeserializeObject<ObjectResponse<PaginationDTO<T>>>(responseString);

            return responseObject;
        }

        /// <summary>
        /// Returns logged user's token.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="loginDTO"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        public static async Task<ObjectResponse<TLoginResultDTO>> LoginForTestAsync<TLoginDTO, TLoginResultDTO>(HttpClient httpClient, TLoginDTO loginDTO, string httpMethod = "POST")
        {
            var request = HttpRequestMessage(new HttpMethod(httpMethod), MilvaTestStartup._loginUrl, obj: loginDTO);

            return await GetHttpResponseAsync<TLoginResultDTO>(request, httpClient).ConfigureAwait(false);
        }
    }
}
