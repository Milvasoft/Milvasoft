using System.Text.Json.Serialization;

namespace Milvasoft.Notification.Push.Expo.PushNotification;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class PushReceiptResponse
{
    [JsonPropertyName("data")]
    public Dictionary<string, PushTicketDeliveryStatus> PushTicketReceipts { get; set; }

    [JsonPropertyName("errors")]
    public List<PushReceiptErrorInformation> ErrorInformations { get; set; }
}

public class PushTicketDeliveryStatus
{
    [JsonPropertyName("status")]
    public string DeliveryStatus { get; set; }

    [JsonPropertyName("message")]
    public string DeliveryMessage { get; set; }

    [JsonPropertyName("details")]
    public object DeliveryDetails { get; set; }
}

public class PushReceiptErrorInformation
{
    [JsonPropertyName("code")]
    public string ErrorCode { get; set; }

    [JsonPropertyName("message")]
    public string ErrorMessage { get; set; }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member