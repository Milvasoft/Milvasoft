using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvasoft.Notification.Push.Expo.PushNotification;

/// <summary>
/// Model for expo push notification api send push notification. Max <see cref="PushTo"/> count is 100.
/// </summary>
/// <remarks>
/// Initializes new instance of <see cref="PushTicketRequest"/>
/// </remarks>
/// <param name="notificationId"></param>
public class PushTicketRequest(sbyte notificationId)
{
    private string _pushData;
    private readonly sbyte _notificationId = notificationId;

    /// <summary>
    /// An Expo push token or an array of Expo push tokens specifying the recipient(s) of this message.
    /// Platform : iOS and Android
    /// </summary>
    [JsonPropertyName("to")]
    public List<string> PushTo { get; set; }

    /// <summary>
    /// A JSON object delivered to your app. It may be up to about 4KiB; the total notification payload sent to Apple and Google must be at most 4KiB or else you will get a "Message Too Big" error.
    /// Platform : iOS and Android
    /// </summary>
    [JsonPropertyName("data")]
    public object PushData
    {
        get => JsonSerializer.Deserialize<PushDataObject>(_pushData);
        set => _pushData = JsonSerializer.Serialize(new PushDataObject(value, _notificationId));
    }

    /// <summary>
    /// The title to display in the notification. Often displayed above the notification body
    /// Platform : iOS and Android
    /// </summary>
    [JsonPropertyName("title")]
    public string PushTitle { get; set; }

    /// <summary>
    /// The message to display in the notification.
    /// Platform : iOS and Android
    /// </summary>
    [JsonPropertyName("body")]
    public string PushBody { get; set; }

    /// <summary>
    /// Time to Live: the number of seconds for which the message may be kept around for redelivery if it hasn't been delivered yet. Defaults to undefined in order to use the respective defaults of each provider (0 for iOS/APNs and 2419200 (4 weeks) for Android/FCM).
    /// Platform : iOS and Android
    /// </summary>
    [JsonPropertyName("ttl")]
    public int? PushTTL { get; set; }

    /// <summary>
    /// Timestamp since the UNIX epoch specifying when the message expires. Same effect as ttl (ttl takes precedence over expiration).
    /// Platform : iOS and Android
    /// </summary>
    [JsonPropertyName("expiration")]
    public int? PushExpiration { get; set; }

    /// <summary>
    /// The delivery priority of the message. Specify "default" or omit this field to use the default priority on each platform ("normal" on Android and "high" on iOS).
    /// Platform : iOS and Android
    /// </summary>
    [JsonPropertyName("priority")]  //'default' | 'normal' | 'high'
    public string PushPriority { get; set; }

    /// <summary>
    /// The subtitle to display in the notification below the title.
    /// Platform : iOS 
    /// </summary>  
    [JsonPropertyName("subtitle")]
    public string PushSubTitle { get; set; }

    /// <summary>
    /// Play a sound when the recipient receives this notification. Specify "default" to play the device's default notification sound, or omit this field to play no sound.
    /// Platform : iOS
    /// </summary>
    [JsonPropertyName("sound")] //'default' | null	
    public string PushSound { get; set; }

    /// <summary>
    /// Number to display in the badge on the app icon. Specify zero to clear the badge.
    /// Platform : iOS 
    /// </summary>
    [JsonPropertyName("badge")]
    public int? PushBadgeCount { get; set; }

    /// <summary>
    /// ID of the Notification Channel through which to display this notification. If an ID is specified but the corresponding channel does not exist on the device (i.e. has not yet been created by your app), the notification will not be displayed to the user.
    /// Platform : Android
    /// </summary>
    [JsonPropertyName("channelId")]
    public string PushChannelId { get; set; }
}
