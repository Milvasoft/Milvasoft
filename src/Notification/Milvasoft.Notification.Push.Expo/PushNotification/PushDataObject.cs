namespace Milvasoft.Notification.Push.Expo.PushNotification;

/// <summary>
/// Expo api accepts only json objects. So that we use this class to serialize to be transferred data into <see cref="PushTicketRequest.PushData"/>.
/// </summary>
/// <remarks>
/// Initializes new instances of <see cref="PushDataObject"/>.
/// </remarks>
/// <param name="data"></param>
/// <param name="notificationId"></param>
public class PushDataObject(object data, sbyte notificationId)
{

    /// <summary>
    /// To be transferred data.
    /// </summary>
    public object Data { get; set; } = data;

    /// <summary>
    /// Notification type id for mobile application.
    /// </summary>
    public sbyte NotificationId { get; set; } = notificationId;
}
