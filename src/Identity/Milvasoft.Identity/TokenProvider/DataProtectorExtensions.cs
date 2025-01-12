using Microsoft.AspNetCore.DataProtection;

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
    /// <param name="useUtcForDateTimes"></param>
    /// <returns></returns>
    public static string Generate<TKey>(this IDataProtectionProvider protector, string purpose, TKey userId, bool useUtcForDateTimes) where TKey : IEquatable<TKey>
    {
        using var ms = new MemoryStream();

        using (var writer = ms.CreateWriter())
        {
            writer.Write(CommonHelper.GetDateTimeOffsetNow(useUtcForDateTimes));
            writer.Write(userId.ToString());
            writer.Write(purpose ?? string.Empty);
            writer.Write(string.Empty);
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
    /// <param name="useUtcForDateTimes"></param>
    /// <returns></returns>
    public static bool Validate<TKey>(this IDataProtectionProvider protector, string purpose, string token, TKey actualUserId, bool useUtcForDateTimes) where TKey : IEquatable<TKey>
    {
        try
        {
            var unprotectedData = protector.CreateProtector(purpose).Unprotect(Convert.FromBase64String(token));

            using var ms = new MemoryStream(unprotectedData);

            using var reader = ms.CreateReader();
            var creationTime = reader.ReadDateTimeOffset();
            var expirationTime = creationTime + TimeSpan.FromDays(1);
            if (expirationTime < CommonHelper.GetDateTimeOffsetNow(useUtcForDateTimes))
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
    internal static readonly Encoding _defaultEncoding = new UTF8Encoding(false, true);

    public static BinaryReader CreateReader(this Stream stream) => new(stream, _defaultEncoding, true);

    public static BinaryWriter CreateWriter(this Stream stream) => new(stream, _defaultEncoding, true);

    public static DateTimeOffset ReadDateTimeOffset(this BinaryReader reader) => new(reader.ReadInt64(), TimeSpan.Zero);

    public static void Write(this BinaryWriter writer, DateTimeOffset value) => writer.Write(value.UtcTicks);
}
