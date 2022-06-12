using System.ComponentModel;

namespace Milvasoft.Core.Utils.Enums;

/// <summary>
/// Mail subjects.
/// </summary>
public enum MailSubject
{
    /// <summary>
    /// The enum that should be sent for error messages.
    /// </summary>
    [Description("Error Warning!")]
    Error,
    /// <summary>
    /// The enum that should be sent for hack warning.
    /// </summary>
    [Description("Hack attemption warning!")]
    Hack,
    /// <summary>
    /// The enum that should be sent for hack warning.
    /// </summary>
    [Description("Application shutdown warning!")]
    ShutDown,
    /// <summary>
    /// The enum that should be sent for information.
    /// </summary>
    [Description("Information")]
    Information
}
