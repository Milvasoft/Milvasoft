using Milvasoft.Components.Rest.Enums;
using Milvasoft.Core.Utils.Constants;
using System.Runtime.Serialization;

namespace Milvasoft.Components.Rest.Response;

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

    public ResponseMessage()
    {

    }

    public ResponseMessage(string message) : this(message, MessageType.Information)
    {

    }

    public ResponseMessage(string key, string message) : this(key, message, MessageType.Information)
    {

    }

    public ResponseMessage(string message, MessageType messageType) : this(string.Empty, message, messageType)
    {

    }

    public ResponseMessage(string key, string message, MessageType messageType)
    {
        Key = key;
        Message = message;
        Type = messageType;
    }
}
