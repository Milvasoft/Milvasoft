using System.Text.Json.Serialization;

namespace Milvasoft.Notification.Push.Expo.PushNotification;

public class PushReceiptRequest
{
    [JsonPropertyName("ids")]
    public List<string> PushTicketIds { get; set; }
}