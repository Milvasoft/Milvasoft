using Milvasoft.Identity.Concrete.Entity;
using System.Buffers.Text;
using System.Security.Cryptography;

namespace Milvasoft.Identity.TokenProvider;

/// <summary>
/// Computes the per-user security stamp that user tokens and 6 digit codes are bound to.
/// </summary>
/// <remarks>
/// <para>
/// The stamp is a SHA-256 hash of the values whose change must invalidate outstanding user tokens: id, password hash, email,
/// email confirmation, phone number, phone confirmation and, when it is set, the persisted <see cref="MilvaUser{TKey}.SecurityStamp"/>.
/// A token stops validating as soon as one of these values changes.
/// </para>
/// <para>
/// <see cref="MilvaUser{TKey}.SecurityStamp"/> is a random value that <c>IMilvaUserManager</c> rotates whenever credentials or identity change and after a
/// successful single-use token verification; <c>RotateSecurityStamp</c> revokes all outstanding tokens. The derived columns stay part of the hash
/// so tokens are also invalidated when an application changes for example <see cref="MilvaUser{TKey}.Email"/> without going through the user manager.
/// </para>
/// <para>
/// When <see cref="MilvaUser{TKey}.SecurityStamp"/> is null or empty (rows created before the column existed) the stamp is computed exactly as in
/// earlier versions, so tokens issued before the upgrade stay valid. The first time a stamp is assigned to such a user, the computed stamp changes
/// and every token issued to that user before (data protection tokens, at most one day old, and 6 digit codes) stops validating.
/// </para>
/// </remarks>
public static class UserSecurityStamp
{
    /// <summary>
    /// Size of a new random stamp in bytes.
    /// </summary>
    public const int NewStampByteLength = 32;

    private const string _persistedStampLabel = "Milvasoft.Identity.SecurityStamp.v1";

    /// <summary>
    /// Creates a new random value for <see cref="MilvaUser{TKey}.SecurityStamp"/>: 32 random bytes, Base64Url encoded (43 characters).
    /// </summary>
    /// <returns>New random stamp.</returns>
    public static string NewStamp() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(NewStampByteLength));

    /// <summary>
    /// Computes the SHA-256 security stamp of <paramref name="user"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the user's key.</typeparam>
    /// <param name="user">The user.</param>
    /// <returns>32 byte stamp.</returns>
    public static byte[] Compute<TKey>(MilvaUser<TKey> user) where TKey : IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(user);

        using var ms = new MemoryStream();

        using (var writer = new BinaryWriter(ms, StreamExtensions._defaultEncoding, leaveOpen: true))
        {
            // BinaryWriter.Write(string) is length-prefixed, so field boundaries are unambiguous.
            writer.Write(user.Id?.ToString() ?? string.Empty);
            writer.Write(user.PasswordHash ?? string.Empty);
            writer.Write(user.Email ?? string.Empty);
            writer.Write(user.EmailConfirmed);
            writer.Write(user.PhoneNumber ?? string.Empty);
            writer.Write(user.PhoneNumberConfirmed);

            // Appended only when present, so stamps of users without a persisted stamp are byte-identical to earlier versions.
            if (!string.IsNullOrEmpty(user.SecurityStamp))
            {
                writer.Write(_persistedStampLabel);
                writer.Write(user.SecurityStamp);
            }
        }

        return SHA256.HashData(ms.ToArray());
    }

    /// <summary>
    /// Computes the security stamp of <paramref name="user"/> as a base64 string.
    /// </summary>
    /// <typeparam name="TKey">The type of the user's key.</typeparam>
    /// <param name="user">The user.</param>
    /// <returns>Base64 encoded stamp.</returns>
    public static string ComputeString<TKey>(MilvaUser<TKey> user) where TKey : IEquatable<TKey> => Convert.ToBase64String(Compute(user));
}
