using Microsoft.AspNetCore.Identity;

namespace Milvasoft.Identity.Abstract;

/// <summary>
/// Implements the standard Identity password hashing
/// </summary>
public interface IMilvaPasswordHasher
{
    /// <summary>
    /// Returns a hashed representation of the supplied <paramref name="password"/>.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>A hashed representation of the supplied <paramref name="password"/>.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Returns a <see cref="bool"/> indicating the result of a password hash comparison.
    /// </summary>
    /// <param name="hashedPassword">The hash value for a user's stored password.</param>
    /// <param name="providedPassword">The password supplied for comparison.</param>
    /// <returns>A <see cref="bool"/> indicating the result of a password hash comparison.</returns>
    /// <remarks>Implementations of this method should be time consistent.</remarks>
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);

    /// <summary>
    /// Verifies <paramref name="providedPassword"/> against <paramref name="hashedPassword"/> and reports whether the stored hash
    /// should be replaced because it was created with weaker settings than the configured ones.
    /// </summary>
    /// <param name="hashedPassword">The hash value for a user's stored password.</param>
    /// <param name="providedPassword">The password supplied for comparison.</param>
    /// <returns>A <see cref="PasswordVerificationResult"/> indicating the result of the comparison.</returns>
    PasswordVerificationResult VerifyHashedPasswordWithRehashCheck(string hashedPassword, string providedPassword);
}
