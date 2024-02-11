using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvasoft.Notification.Push.Expo.PushNotification;

/// <summary>
/// Expo client for push api requests.
/// </summary>
public class ExpoClient : IDisposable
{
    #region Environemt Configuration

    private const string _expoBackendHost = "https://exp.host";
    private const string _pushSendPath = "/--/api/v2/push/send";
    private const string _pushGetReceiptsPath = "/--/api/v2/push/getReceipts";

    #endregion

    /// <summary>
    /// Make this static to avoid socket saturation and limit concurrent server connections to 6, but only for instances of this class.
    /// </summary>
    private static readonly HttpClientHandler _httpHandler = new() { MaxConnectionsPerServer = 6 };
    private static readonly HttpClient _httpClient = new(_httpHandler);
    private bool disposedValue;

    static ExpoClient()
    {
        _httpClient.BaseAddress = new Uri(_expoBackendHost);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Adds Bearer authorization header value to <see cref="HttpClient.DefaultRequestHeaders"/>.
    /// </summary>
    /// <param name="token"></param>
    public static void AddBearerAuthorizationHeader(string token) => _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    /// <summary>
    /// Sends <paramref name="pushTicketRequest"/> to expo push api and returns <see cref="PushTicketResponse"/>.
    /// </summary>
    /// <param name="pushTicketRequest"></param>
    /// <returns></returns>
    public static async Task<PushTicketResponse> PushSendAsync(PushTicketRequest pushTicketRequest)
    {
        var ticketResponse = await PostAsync<PushTicketRequest, PushTicketResponse>(pushTicketRequest, _pushSendPath);
        return ticketResponse;
    }

    /// <summary>
    /// Sends <paramref name="pushReceiptRequest"/> to expo push api and returns <see cref="PushReceiptResponse"/>.
    /// </summary>
    /// <param name="pushReceiptRequest"></param>
    /// <returns></returns>
    public static async Task<PushReceiptResponse> PushGetReceiptsAsync(PushReceiptRequest pushReceiptRequest)
    {
        var receiptResponse = await PostAsync<PushReceiptRequest, PushReceiptResponse>(pushReceiptRequest, _pushGetReceiptsPath);
        return receiptResponse;
    }

    /// <summary>
    /// Serailizes <paramref name="requestObj"/> and sends post request to <paramref name="path"/> and returns response as <typeparamref name="U"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="requestObj"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private static async Task<U> PostAsync<T, U>(T requestObj, string path)
    {
        var serializedRequestObj = JsonSerializer.Serialize(requestObj, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var requestBody = new StringContent(serializedRequestObj, System.Text.Encoding.UTF8, "application/json");

        var responseBody = default(U);

        using var response = await _httpClient.PostAsync(path, requestBody);

        if (response.IsSuccessStatusCode)
        {
            var rawResponseBody = await response.Content.ReadAsStringAsync();
            responseBody = JsonSerializer.Deserialize<U>(rawResponseBody);
        }

        return responseBody;
    }

    /// <summary>
    /// Disposes this object.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _httpClient.Dispose();
                _httpHandler.Dispose();
            }

            disposedValue = true;
        }
    }

    /// <summary>
    /// Disposes this object.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
