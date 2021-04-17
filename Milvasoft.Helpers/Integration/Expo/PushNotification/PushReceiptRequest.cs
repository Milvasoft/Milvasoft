using Newtonsoft.Json;
using System.Collections.Generic;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Milvasoft.Helpers.Integration.Expo.PushNotification
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PushReceiptRequest
    {

        [JsonProperty(PropertyName = "ids")]
        public List<string> PushTicketIds { get; set; }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
