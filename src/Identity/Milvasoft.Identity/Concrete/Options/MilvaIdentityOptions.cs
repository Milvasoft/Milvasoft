using Microsoft.AspNetCore.Identity;

namespace Milvasoft.Identity.Concrete.Options;

/// <summary>
/// Provides localized identity error messages. All error's code is nameof method. (e.g. Code = "DefaultError")
/// </summary>
public class MilvaIdentityOptions : IMilvaOptions
{
    /// <inheritdoc/>
    public static string SectionName { get; } = $"{MilvaOptionsExtensions.ParentSectionName}:Identity";

    /// <summary>
    /// Gets or sets the <see cref="UserOptions"/> for the identity system.
    /// </summary>
    /// <value>
    /// The <see cref="UserOptions"/> for the identity system.
    /// </value>
    public MilvaUserOptions User { get; set; } = new();

    /// <summary>
    /// Gets or sets the <see cref="PasswordOptions"/> for the identity system.
    /// </summary>
    /// <value>
    /// The <see cref="PasswordOptions"/> for the identity system.
    /// </value>
    public MilvaPasswordOptions Password { get; set; } = new();

    /// <summary>
    /// Gets or sets the <see cref="LockoutOptions"/> for the identity system.
    /// </summary>
    /// <value>
    /// The <see cref="LockoutOptions"/> for the identity system.
    /// </value>
    public MilvaLockoutOptions Lockout { get; set; } = new();

    /// <summary>
    /// Gets or sets the <see cref="SignInOptions"/> for the identity system.
    /// </summary>
    /// <value>
    /// The <see cref="SignInOptions"/> for the identity system.
    /// </value>
    public MilvaSignInOptions SignIn { get; set; } = new();
}
