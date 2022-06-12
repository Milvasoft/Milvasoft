namespace Milvasoft.Identity.TokenProvider;

/// <summary>
/// Identity token generating purpose.
/// </summary>
public enum Purpose : sbyte
{
    /// <summary>
    /// Used when email confirmation.
    /// </summary>
    EmailConfirm,

    /// <summary>
    /// Used when email change.
    /// </summary>
    EmailChange,

    /// <summary>
    /// USed when password reset.
    /// </summary>
    PasswordReset,
}
