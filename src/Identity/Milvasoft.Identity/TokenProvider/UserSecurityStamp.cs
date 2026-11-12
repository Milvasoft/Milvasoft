using Milvasoft.Identity.Concrete.Entity;
using System.Security.Cryptography;

namespace Milvasoft.Identity.TokenProvider;

/// <summary>
/// Computes a per-user security stamp from the security-relevant state of a user.
/// </summary>
/// <remarks>
/// <see cref="MilvaUser{TKey}"/> has no persisted security stamp column, so the stamp is derived from the values whose change must
/// invalidate outstanding user tokens: id, password hash, email, email confirmation, phone number and phone confirmation.
/// As a result a token becomes invalid as soon as the operation it authorizes changes one of these values
/// (password reset changes the password hash, email confirmation flips <see cref="MilvaUser{TKey}.EmailConfirmed"/>,
/// email change changes <see cref="MilvaUser{TKey}.Email"/>), which makes tokens single-use.
/// </remarks>
public static class UserSecurityStamp
{
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
