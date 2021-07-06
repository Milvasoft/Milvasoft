using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.Test.Integration.TestStartup.Abstract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Test.Integration.Utils
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
                RequestUri = new Uri(MilvaTestClient<MilvaTestStartup>.TestApiBaseUrl + url),
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
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="httpRequestMessage"></param>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static async Task<TResponse> GetHttpResponseAsync<TResponse>(HttpRequestMessage httpRequestMessage, HttpClient httpClient)
        {
            var response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            string responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var responseObject = JsonConvert.DeserializeObject<TResponse>(responseString);

            return responseObject;
        }

        /// <summary>
        /// Returns http response in pagination dto.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpRequestMessage"></param>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static async Task<ObjectResponse<PaginationDTO<TResponse>>> GetHttpPaginateResponseAsync<TResponse>(HttpRequestMessage httpRequestMessage, HttpClient httpClient)
        {
            var response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            string responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var responseObject = JsonConvert.DeserializeObject<ObjectResponse<PaginationDTO<TResponse>>>(responseString);

            return responseObject;
        }

        /// <summary>
        /// Returns http response.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="httpRequestMessage"></param>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static async Task<ObjectResponse<TResponse>> GetHttpObjectResponseAsync<TResponse>(HttpRequestMessage httpRequestMessage, HttpClient httpClient)
        {
            var response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            string responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var responseObject = JsonConvert.DeserializeObject<ObjectResponse<TResponse>>(responseString);

            return responseObject;
        }

        /// <summary>
        /// Returns logged user's token.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="loginDTO"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        public static async Task<TLoginResultDTO> LoginForTestAsync<TLoginResultDTO>(HttpClient httpClient, object loginDTO, string httpMethod = "POST")
        {
            var request = HttpRequestMessage(new HttpMethod(httpMethod), MilvaTestClient<MilvaTestStartup>.LoginUrl, obj: loginDTO);

            return await GetHttpResponseAsync<TLoginResultDTO>(request, httpClient).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a token with roles in the <paramref name="roles"/> list.
        /// </summary>
        /// <param name="roles"></param>
        /// <returns></returns>
        public static async Task<string> GetTokenByRoles(params string[] roles)
        {
            var httpClient = MilvaTestClient<MilvaTestStartup>.HttpClient;

            for (int i = 0; i < roles.Length; i++)
                roles[i] = roles[i].Trim();

            var userManagerMethods = MilvaTestClient<MilvaTestStartup>.UserManager.GetType().GetMethods();

            var user = ((IQueryable<object>)MilvaTestClient<MilvaTestStartup>.UserManager.GetType().GetProperty("Users").GetValue(userManagerMethods, null))
                        .ToList().FirstOrDefault(p => p.GetType().GetProperty("UserName").GetValue(p, null).ToString() == MilvaTestClient<MilvaTestStartup>.LoginDtoAndUserName.Item2);

            if (!roles.IsNullOrEmpty())
            {
                var getRolesMethod = userManagerMethods.First(p => p.Name == "GetRolesAsync");
                var removeFromRolesMethod = userManagerMethods.First(p => p.Name == "RemoveFromRolesAsync");
                var addToRolesMethod = userManagerMethods.First(p => p.Name == "AddToRolesAsync");

                var userRoles = await ((Task<IList<string>>)getRolesMethod.Invoke(MilvaTestClient<MilvaTestStartup>.UserManager, new object[] { user })).ConfigureAwait(false);
                _ = await ((Task<IdentityResult>)removeFromRolesMethod.Invoke(MilvaTestClient<MilvaTestStartup>.UserManager, new object[] { user, userRoles })).ConfigureAwait(false);
                _ = await ((Task<IdentityResult>)addToRolesMethod.Invoke(MilvaTestClient<MilvaTestStartup>.UserManager, new object[] { user, roles })).ConfigureAwait(false);
            }

            return await MilvaTestClient<MilvaTestStartup>.GetTokenAsync.Invoke().ConfigureAwait(false);
        }
    }
}
