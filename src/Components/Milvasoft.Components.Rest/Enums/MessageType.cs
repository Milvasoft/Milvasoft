namespace Milvasoft.Components.Rest.Enums;

/// <summary>
/// Represents the types of response messages returned by the application.
/// </summary>
public enum MessageType : byte
{
    /// <summary>
    /// Indicates an informational response.
    /// </summary>
    Information = 1,

    /// <summary>
    /// Indicates a validation-related response.
    /// </summary>
    Validation = 2,

    /// <summary>
    /// Indicates an unauthorized access response.
    /// </summary>
    Unauthorized = 3,

    /// <summary>
    /// Indicates a forbidden access response.
    /// </summary>
    Forbidden = 4,

    /// <summary>
    /// Indicates a warning response.
    /// </summary>
    Warning = 5,

    /// <summary>
    /// Indicates an error response.
    /// </summary>
    Error = 6,

    /// <summary>
    /// Indicates a fatal error response.
    /// </summary>
    Fatal = 7
}
