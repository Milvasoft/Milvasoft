using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvasoft.Notification.Push.Expo.PushNotification;

/// <summary>
/// Expo client for push api requests.
/// </summary>
/// <remarks>
/// All instances share one <see cref="HttpClient"/> for the lifetime of the process (connections are recycled every
/// 5 minutes so DNS changes are picked up). Disposing an instance does not dispose the shared client.
/// </remarks>
public class ExpoClient : IDisposable
{
    #region Environemt Configuration

    private const string _expoBackendHost = "https://exp.host";
    private const string _pushSendPath = "/--/api/v2/push/send";
    private const string _pushGetReceiptsPath = "/--/api/v2/push/getReceipts";

    #endregion

    private const string _applicationJsonMimeType = "application/json";
    private const string _tokenName = "Bearer";

    /// <summary>
    /// Shared to avoid socket exhaustion; concurrent connections to Expo are limited to 6.
    /// </summary>
    private static readonly HttpClient _httpClient = new(new SocketsHttpHandler
    {
        MaxConnectionsPerServer = 6,
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    })
    {
        BaseAddress = new Uri(_expoBackendHost),
        DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue(_applicationJsonMimeType) } },
    };

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Access token sent with every request. Set per request instead of mutating the shared client's default headers,
    /// which is not thread-safe while requests are in flight.
    /// </summary>
    private static volatile string _accessToken;

    /// <summary>
    /// Sets the Bearer access token sent with every Expo request. Pass null to stop sending it.
    /// </summary>
    /// <param name="token"></param>
    public static void AddBearerAuthorizationHeader(string token) => _accessToken = token;

    /// <summary>
    /// Sends <paramref name="pushTicketRequest"/> to expo push api and returns <see cref="PushTicketResponse"/>.
    /// </summary>
    /// <param name="pushTicketRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The response, or null if Expo returned a non-success status code.</returns>
    public static Task<PushTicketResponse> PushSendAsync(PushTicketRequest pushTicketRequest, CancellationToken cancellationToken = default)
        => PostAsync<PushTicketRequest, PushTicketResponse>(pushTicketRequest, _pushSendPath, cancellationToken);

    /// <summary>
    /// Sends <paramref name="pushReceiptRequest"/> to expo push api and returns <see cref="PushReceiptResponse"/>.
    /// </summary>
    /// <param name="pushReceiptRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The response, or null if Expo returned a non-success status code.</returns>
    public static Task<PushReceiptResponse> PushGetReceiptsAsync(PushReceiptRequest pushReceiptRequest, CancellationToken cancellationToken = default)
        => PostAsync<PushReceiptRequest, PushReceiptResponse>(pushReceiptRequest, _pushGetReceiptsPath, cancellationToken);

    /// <summary>
    /// Serializes <paramref name="requestObj"/>, sends a post request to <paramref name="path"/> and returns the response as <typeparamref name="U"/>.
    /// </summary>
    private static async Task<U> PostAsync<T, U>(T requestObj, string path, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent(requestObj)
        };

        var accessToken = _accessToken;

        if (!string.IsNullOrEmpty(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue(_tokenName, accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return default;

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await JsonSerializer.DeserializeAsync<U>(responseStream, cancellationToken: cancellationToken);
    }

    private static StringContent JsonContent<T>(T requestObj)
        => new(JsonSerializer.Serialize(requestObj, _jsonSerializerOptions), System.Text.Encoding.UTF8, _applicationJsonMimeType);

    /// <summary>
    /// Disposes this object. The shared <see cref="HttpClient"/> is not disposed because other instances keep using it.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
    }

    /// <summary>
    /// Disposes this object.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
