namespace Milvasoft.Helpers.Integration.Expo.PushNotification
{
    /// <summary>
    /// Expo api accepts only json objects. So that we use this class to serialize to be transferred data into <see cref="PushTicketRequest.PushData"/>.
    /// </summary>
    public class PushDataObject
    {
        /// <summary>
        /// Initializes new instances of <see cref="PushDataObject"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="notificationId"></param>
        public PushDataObject(object data, sbyte notificationId)
        {
            Data = data;
            NotificationId = notificationId;
        }

        /// <summary>
        /// To be transferred data.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Notification type id for mobile application.
        /// </summary>
        public sbyte NotificationId { get; set; }
    }
}
