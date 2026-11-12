using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Milvasoft.Identity.TokenProvider;

/// <summary>
/// RFC 6238 time based one-time codes. Time steps are always computed from UTC.
/// </summary>
internal static class Rfc6238AuthenticationService
{
    private static readonly TimeSpan _timestep = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Number of time steps before the current one that are still accepted.
    /// </summary>
    private const int _allowedPastTimeSteps = 2;

    /// <summary>
    /// Number of time steps after the current one that are still accepted (clock skew between servers).
    /// </summary>
    private const int _allowedFutureTimeSteps = 2;

    internal static int ComputeTotp(byte[] key, ulong timestepNumber)
    {
        // # of 0's = length of pin
        const int Mod = 1000000;

        // See https://tools.ietf.org/html/rfc4226
        Span<byte> timestepAsBytes = stackalloc byte[sizeof(ulong)];

        BinaryPrimitives.WriteUInt64BigEndian(timestepAsBytes, timestepNumber);

        var hash = HMACSHA512.HashData(key, timestepAsBytes);

        // Generate DT string
        var offset = hash[^1] & 0xf;

        var binaryCode = (hash[offset] & 0x7f) << 24
                         | (hash[offset + 1] & 0xff) << 16
                         | (hash[offset + 2] & 0xff) << 8
                         | hash[offset + 3] & 0xff;

        return binaryCode % Mod;
    }

    // More info: https://tools.ietf.org/html/rfc6238#section-4
    internal static ulong GetTimeStepNumber(DateTimeOffset now) => (ulong)((now.UtcTicks - DateTimeOffset.UnixEpoch.UtcTicks) / _timestep.Ticks);

    public static int GenerateCode(byte[] key) => GenerateCode(key, DateTimeOffset.UtcNow);

    internal static int GenerateCode(byte[] key, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(key);

        return ComputeTotp(key, GetTimeStepNumber(now));
    }

    public static bool ValidateCode(byte[] key, int code) => ValidateCode(key, code, DateTimeOffset.UtcNow);

    internal static bool ValidateCode(byte[] key, int code, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(key);

        var currentTimeStep = GetTimeStepNumber(now);

        var matched = false;

        // Check every step (no early exit) so the time taken does not depend on which step matched.
        for (var i = -_allowedPastTimeSteps; i <= _allowedFutureTimeSteps; i++)
            matched |= ComputeTotp(key, (ulong)((long)currentTimeStep + i)) == code;

        return matched;
    }
}
