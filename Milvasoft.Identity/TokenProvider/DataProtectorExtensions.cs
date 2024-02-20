using Microsoft.AspNetCore.DataProtection;
using System.Text;

namespace Milvasoft.Identity.TokenProvider;

/// <summary>
/// Provides protection and validation of identity tokens.
/// </summary>
public static class DataProtectorExtensions
{
    /// <summary>
    /// Generates a protected token for the specified <paramref name="userId"/> as an asynchronous operation.
    /// </summary>
    /// <param name="protector"></param>
    /// <param name="purpose">The purpose the token will be used for.</param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static string Generate<TKey>(this IDataProtectionProvider protector, string purpose, TKey userId) where TKey : IEquatable<TKey>
    {
        var ms = new MemoryStream();

        using (var writer = ms.CreateWriter())
        {
            writer.Write(DateTimeOffset.UtcNow);
            writer.Write(userId.ToString());
            writer.Write(purpose ?? "");
            string stamp = null;

            writer.Write(stamp ?? "");
        }

        var protectedBytes = protector.CreateProtector(purpose).Protect(ms.ToArray());
        return Convert.ToBase64String(protectedBytes);
    }

    /// <summary>
    /// Validates the protected <paramref name="token"/> for the specified <paramref name="actualUserId"/> and <paramref name="purpose"/> as an asynchronous operation.
    /// </summary>
    /// <param name="protector"></param>
    /// <param name="purpose">The purpose the token was be used for.</param>
    /// <param name="token">The token to validate.</param>
    /// <param name="actualUserId"></param>
    /// <returns></returns>
    public static bool Validate<TKey>(this IDataProtectionProvider protector, string purpose, string token, TKey actualUserId) where TKey : IEquatable<TKey>
    {
        try
        {
            var unprotectedData = protector.CreateProtector(purpose).Unprotect(Convert.FromBase64String(token));

            var ms = new MemoryStream(unprotectedData);

            using (var reader = ms.CreateReader())
            {
                var creationTime = reader.ReadDateTimeOffset();
                var expirationTime = creationTime + TimeSpan.FromDays(1);
                if (expirationTime < DateTimeOffset.UtcNow)
                {
                    return false;
                }

                var userId = reader.ReadString();
                if (userId != actualUserId.ToString())
                {
                    return false;
                }

                var purp = reader.ReadString();
                if (!string.Equals(purp, purpose))
                {
                    return false;
                }

                var stamp = reader.ReadString();
                if (reader.PeekChar() != -1)
                {
                    return false;
                }

                return stamp == "";
            }
        }
        // ReSharper disable once EmptyGeneralCatchClause
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
    internal static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

    public static BinaryReader CreateReader(this Stream stream)
    {
        return new BinaryReader(stream, DefaultEncoding, true);
    }

    public static BinaryWriter CreateWriter(this Stream stream)
    {
        return new BinaryWriter(stream, DefaultEncoding, true);
    }

    public static DateTimeOffset ReadDateTimeOffset(this BinaryReader reader)
    {
        return new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
    }

    public static void Write(this BinaryWriter writer, DateTimeOffset value)
    {
        writer.Write(value.UtcTicks);
    }
}
