using System.Text.Json.Serialization;

namespace Milvasoft.Notification.Push.Expo.PushNotification;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class PushReceiptRequest
{
    [JsonPropertyName("ids")]
    public List<string> PushTicketIds { get; set; }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
