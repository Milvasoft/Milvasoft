using Newtonsoft.Json;
using System.Collections.Generic;

namespace Milvasoft.Helpers.Integration.Expo.PushNotification;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[JsonObject(MemberSerialization.OptIn)]
public class PushReceiptRequest
{
    [JsonProperty(PropertyName = "ids")]
    public List<string> PushTicketIds { get; set; }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
