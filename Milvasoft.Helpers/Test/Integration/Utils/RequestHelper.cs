using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.Test.Helpers;
using Milvasoft.Helpers.Test.Integration.TestStartup.Abstract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Test.Integration.Utils;

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

        var baseUrl = MilvaTestClient<MilvaTestStartup>.TestApiBaseUrl;

        if (baseUrl.IsNullOrEmpty())
            throw new MilvaTestException("Please enter the api base url information.");

        var requestMessage = new HttpRequestMessage
        {
            RequestUri = new Uri(baseUrl + url),
            Method = httpMethod
        };

        requestMessage.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(acceptLanguage));
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

        if (json != "null")
            requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

        if (!string.IsNullOrWhiteSpace(token))
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
    /// <typeparam name="TResponse"></typeparam>
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
    public static async Task<ObjectResponse<TLoginResultDTO>> LoginForTestAsync<TLoginResultDTO>(HttpClient httpClient, object loginDTO, string httpMethod = "POST")
    {
        var loginUrl = MilvaTestClient<MilvaTestStartup>.LoginUrl;

        if (loginUrl.IsNullOrEmpty())
            throw new MilvaTestException("Please enter api login url information.");

        var request = HttpRequestMessage(new HttpMethod(httpMethod), loginUrl, obj: loginDTO);

        return await GetHttpObjectResponseAsync<TLoginResultDTO>(request, httpClient).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns a token with roles in the <paramref name="roles"/> list.
    /// </summary>
    /// <param name="roles"></param>
    /// <returns></returns>
    public static async Task<string> GetTokenByRoles(params string[] roles)
    {
        var httpClient = MilvaTestClient<MilvaTestStartup>.HttpClient;

        roles.ToList().Trim();

        var userManager = MilvaTestClient<MilvaTestStartup>.UserManager;

        var userName = MilvaTestClient<MilvaTestStartup>.LoginDtoAndUserName.Item2;

        var loginDTO = MilvaTestClient<MilvaTestStartup>.LoginDtoAndUserName.Item1;

        var getTokenFunc = MilvaTestClient<MilvaTestStartup>.GetTokenAsync;

        getTokenFunc.IsNull("Please send the 'GetTokenAsync' method from 'MilvaTestClint'.");

        if (string.IsNullOrWhiteSpace(userName))
            throw new MilvaTestException("Please enter the user information required for the test.");

        loginDTO.IsNull("Please enter the user information required for the test.");

        userManager.IsNull("Please send user manager from MilvaTestClient.");

        var userManagerMethods = userManager.GetType().GetMethods();

        var user = ((IQueryable<object>)userManager.GetType().GetProperty("Users").GetValue(userManager, null))
                    .ToList().FirstOrDefault(p => p.GetType().GetProperty("UserName").GetValue(p, null).ToString() == userName);

        if (!roles.IsNullOrEmpty())
        {
            var getRolesMethod = userManagerMethods.First(p => p.Name == "GetRolesAsync");
            var removeFromRolesMethod = userManagerMethods.First(p => p.Name == "RemoveFromRolesAsync");
            var addToRolesMethod = userManagerMethods.First(p => p.Name == "AddToRolesAsync");

            var userRoles = await ((Task<IList<string>>)getRolesMethod.Invoke(userManager, new object[] { user })).ConfigureAwait(false);
            _ = await ((Task<IdentityResult>)removeFromRolesMethod.Invoke(userManager, new object[] { user, userRoles })).ConfigureAwait(false);
            _ = await ((Task<IdentityResult>)addToRolesMethod.Invoke(userManager, new object[] { user, roles })).ConfigureAwait(false);
        }

        return await getTokenFunc.Invoke(loginDTO).ConfigureAwait(false);
    }
}
