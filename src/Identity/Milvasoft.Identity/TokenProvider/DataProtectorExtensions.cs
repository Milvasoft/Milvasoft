using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;

namespace Milvasoft.Identity.TokenProvider;

/// <summary>
/// Provides protection and validation of identity tokens.
/// </summary>
/// <remarks>
/// Token payload (protected with ASP.NET Core data protection): creation time (UTC ticks) | user id | purpose | security stamp.
/// The security stamp binds a token to the state of the user it was issued for (see <see cref="UserSecurityStamp"/>),
/// so a token stops validating once that state changes.
/// </remarks>
public static class DataProtectorExtensions
{
    /// <summary>
    /// Default token lifespan.
    /// </summary>
    public static readonly TimeSpan DefaultTokenLifespan = TimeSpan.FromDays(1);

    /// <summary>
    /// Generates a protected token for the specified <paramref name="userId"/>.
    /// </summary>
    /// <param name="protector"></param>
    /// <param name="purpose">The purpose the token will be used for.</param>
    /// <param name="userId">The id of the user the token is for.</param>
    /// <param name="securityStamp">The current security stamp of the user. See <see cref="UserSecurityStamp"/>. Must not be empty.</param>
    /// <returns>Base64 encoded protected token.</returns>
    public static string Generate<TKey>(this IDataProtectionProvider protector, string purpose, TKey userId, string securityStamp) where TKey : IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(protector);
        ArgumentException.ThrowIfNullOrEmpty(securityStamp);

        using var ms = new MemoryStream();

        using (var writer = ms.CreateWriter())
        {
            writer.Write(DateTimeOffset.UtcNow);
            writer.Write(userId.ToString());
            writer.Write(purpose ?? string.Empty);
            writer.Write(securityStamp);
        }

        var protectedBytes = protector.CreateProtector(purpose).Protect(ms.ToArray());

        return Convert.ToBase64String(protectedBytes);
    }

    /// <summary>
    /// Validates the protected <paramref name="token"/> for the specified <paramref name="actualUserId"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="protector"></param>
    /// <param name="purpose">The purpose the token was be used for.</param>
    /// <param name="token">The token to validate.</param>
    /// <param name="actualUserId">The id of the user the token is validated for.</param>
    /// <param name="securityStamp">The current security stamp of the user. See <see cref="UserSecurityStamp"/>.</param>
    /// <param name="lifespan">Token lifespan. Default is <see cref="DefaultTokenLifespan"/>.</param>
    /// <returns>True if the token is valid, otherwise false.</returns>
    public static bool Validate<TKey>(this IDataProtectionProvider protector,
                                      string purpose,
                                      string token,
                                      TKey actualUserId,
                                      string securityStamp,
                                      TimeSpan? lifespan = null) where TKey : IEquatable<TKey>
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(securityStamp))
            return false;

        try
        {
            var unprotectedData = protector.CreateProtector(purpose).Unprotect(Convert.FromBase64String(token));

            using var ms = new MemoryStream(unprotectedData);

            using var reader = ms.CreateReader();

            var creationTime = reader.ReadDateTimeOffset();
            var expirationTime = creationTime + (lifespan ?? DefaultTokenLifespan);

            if (expirationTime < DateTimeOffset.UtcNow)
                return false;

            var userId = reader.ReadString();

            if (userId != actualUserId.ToString())
                return false;

            var purp = reader.ReadString();

            if (!string.Equals(purp, purpose, StringComparison.Ordinal))
                return false;

            var stamp = reader.ReadString();

            if (reader.PeekChar() != -1)
                return false;

            return CryptographicOperations.FixedTimeEquals(StreamExtensions._defaultEncoding.GetBytes(stamp),
                                                           StreamExtensions._defaultEncoding.GetBytes(securityStamp));
        }
        catch
        {
            // Do not leak exception
        }

        return false;
    }
}

/// <summary>
/// Utility extensions to streams
/// </summary>
internal static class StreamExtensions
{
    internal static readonly Encoding _defaultEncoding = new UTF8Encoding(false, true);

    public static BinaryReader CreateReader(this Stream stream) => new(stream, _defaultEncoding, true);

    public static BinaryWriter CreateWriter(this Stream stream) => new(stream, _defaultEncoding, true);

    public static DateTimeOffset ReadDateTimeOffset(this BinaryReader reader) => new(reader.ReadInt64(), TimeSpan.Zero);

    public static void Write(this BinaryWriter writer, DateTimeOffset value) => writer.Write(value.UtcTicks);
}
