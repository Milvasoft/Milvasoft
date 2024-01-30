using System.Runtime.Serialization;

namespace Milvasoft.Components.Rest.Response;

/// <summary>
/// Represents a validation item containing information about a specific validation error.
/// </summary>
[DataContract]
public class ValidationItem
{
    /// <summary>
    /// Gets or sets the key associated with the validation error.
    /// </summary>
    [DataMember]
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the validation message describing the error.
    /// </summary>
    [DataMember]
    public string ValidationMessage { get; set; }
}
