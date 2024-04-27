using Milvasoft.Components.Rest.Enums;
using System.Runtime.Serialization;

namespace Milvasoft.Components.Rest.MilvaResponse;

/// <summary>
/// Represents a message returned as part of a response.
/// </summary>
[DataContract]
public class ResponseMessage
{
    /// <summary>
    /// Gets or sets the key associated with the message.
    /// </summary>
    [DataMember]
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the message content.
    /// </summary>
    [DataMember]
    public string Message { get; set; } = LocalizerKeys.Successful;

    /// <summary>
    /// Gets or sets the type of message.
    /// </summary>
    [DataMember]
    public MessageType Type { get; set; }

    /// <summary>
    /// Initializes new instance.
    /// </summary>
    public ResponseMessage()
    {

    }

    /// <summary>
    /// Initializes new instance with <paramref name="message"/>
    /// </summary>
    /// <param name="message"></param>
    public ResponseMessage(string message) : this(message, MessageType.Information)
    {

    }

    /// <summary>
    /// Initializes new instance with <paramref name="key"/> and <paramref name="message"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="message"></param>
    public ResponseMessage(string key, string message) : this(key, message, MessageType.Information)
    {

    }

    /// <summary>
    /// Initializes new instance with empty key and <paramref name="message"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="messageType"></param>
    public ResponseMessage(string message, MessageType messageType) : this(string.Empty, message, messageType)
    {

    }

    /// <summary>
    /// Initializes new instance with <paramref name="key"/> and <paramref name="message"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="message"></param>
    /// <param name="messageType"></param>
    public ResponseMessage(string key, string message, MessageType messageType)
    {
        Key = key;
        Message = message;
        Type = messageType;
    }
}
