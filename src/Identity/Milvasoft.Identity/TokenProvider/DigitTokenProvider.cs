using Milvasoft.Identity.Concrete.Entity;
using System.Globalization;
using System.Security.Cryptography;

namespace Milvasoft.Identity.TokenProvider;

/// <summary>
/// Generates and validates 6 digit time based (RFC 6238) codes for a user.
/// </summary>
/// <remarks>
/// <para>
/// The per-user TOTP key is derived with HMAC-SHA256 from a server-side <c>secret</c>, the user's security stamp
/// (see <see cref="UserSecurityStamp"/>), the <see cref="Purpose"/> and an optional <c>value</c> the code is bound to
/// (for example the new email address for <see cref="Purpose.EmailChange"/>). Without the secret a code cannot be computed,
/// and a code stops validating as soon as the user's security stamp changes. <c>MilvaUserManager.VerifyDigitToken</c> rotates the stamp
/// after a successful verification, so codes verified through the user manager are single-use once the user is persisted.
/// </para>
/// <para>
/// Codes are valid for roughly 6 to 15 minutes (3 minute time steps, two past and two future steps are accepted).
/// A 6 digit code can be brute forced without rate limiting; callers must limit validation attempts per user.
/// </para>
/// </remarks>
/// <typeparam name="TUser">The type encapsulating a user.</typeparam>
/// <typeparam name="TKey">The type user's key.</typeparam>
public static class DigitTokenProvider<TUser, TKey> where TUser : MilvaUser<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Minimum length of the server-side secret in bytes.
    /// </summary>
    public const int MinSecretLength = 32;

    private const string _keyDerivationLabel = "Milvasoft.Identity.DigitToken.v2";

    /// <summary>
    /// Generates a code for the specified <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="purpose">The purpose the code will be used for.</param>
    /// <param name="user">The user a code should be generated for.</param>
    /// <param name="secret">Server-side secret, at least <see cref="MinSecretLength"/> bytes. Never send it to clients.</param>
    /// <param name="value">Optional value the code is bound to (for example the new email address in an email change flow).</param>
    /// <returns>6 digit code.</returns>
    public static string Generate(Purpose purpose, TUser user, byte[] secret, string value = null)
    {
        var key = DeriveKey(purpose, user, secret, value);

        return Rfc6238AuthenticationService.GenerateCode(key).ToString("D6", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="token"/> is valid for the given
    /// <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="purpose">The purpose the code was generated for.</param>
    /// <param name="token">The code to validate.</param>
    /// <param name="user">The user the code should be validated for.</param>
    /// <param name="secret">The server-side secret used when generating the code.</param>
    /// <param name="value">The value the code was bound to when it was generated.</param>
    /// <returns>True if the code is valid, otherwise false.</returns>
    public static bool Validate(Purpose purpose, string token, TUser user, byte[] secret, string value = null)
    {
        if (string.IsNullOrEmpty(token) || token.Length != 6 || !int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out int code))
            return false;

        var key = DeriveKey(purpose, user, secret, value);

        return Rfc6238AuthenticationService.ValidateCode(key, code);
    }

    /// <summary>
    /// Derives the TOTP key of <paramref name="user"/> for <paramref name="purpose"/>.
    /// </summary>
    internal static byte[] DeriveKey(Purpose purpose, TUser user, byte[] secret, string value)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(secret);

        if (secret.Length < MinSecretLength)
            throw new MilvaDeveloperException($"Digit token secret must be at least {MinSecretLength} bytes.");

        using var ms = new MemoryStream();

        using (var writer = new BinaryWriter(ms, StreamExtensions._defaultEncoding, leaveOpen: true))
        {
            writer.Write(_keyDerivationLabel);
            writer.Write((int)purpose);
            writer.Write(value ?? string.Empty);
            writer.Write(UserSecurityStamp.Compute(user));
        }

        return HMACSHA256.HashData(secret, ms.ToArray());
    }
}
