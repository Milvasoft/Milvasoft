namespace Milvasoft.Testing.IntegrationTest.Enums;

/// <summary>
/// A enum that refers to how authorization processing will occur
/// </summary>
public enum AuthorizeTypeEnum : sbyte
{
    /// <summary>
    /// And process.
    /// </summary>
    And,

    /// <summary>
    /// Or process.
    /// </summary>
    Or,

    /// <summary>
    /// If the login process is not required.
    /// </summary>
    None
}
