using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Milvasoft.Identity.Abstract;
using Milvasoft.Identity.Concrete.Options;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Milvasoft.Identity.Concrete;

/// <summary>
/// Implements PBKDF2 password hashing.
/// </summary>
/// <remarks>
/// <para>
/// Stored format (base64): <c>0x01 | prf (uint32 BE) | iteration count (uint32 BE) | salt length (uint32 BE) | salt | subkey</c>.
/// The format records the PRF and the iteration count of every hash, so hashes created with older settings
/// (for example HMAC-SHA256 with 10,000 iterations) keep verifying after the settings change.
/// </para>
/// <para>
/// When a stored hash was created with weaker settings than the configured ones, <see cref="VerifyHashedPasswordWithRehashCheck"/>
/// returns <see cref="PasswordVerificationResult.SuccessRehashNeeded"/> so the caller can store a new hash.
/// </para>
/// </remarks>
public class MilvaPasswordHasher : IMilvaPasswordHasher
{
    private const byte _formatMarker = 0x01;
    private const int _headerLength = 13;
    private const int _saltSize = 128 / 8;
    private const int _subkeyLength = 256 / 8;

    private readonly int _iterCount;
    private readonly KeyDerivationPrf _prf;
    private readonly RandomNumberGenerator _rng;

    /// <summary>
    /// Creates a new instance of <see cref="MilvaPasswordHasher"/>.
    /// </summary>
    /// <param name="optionsAccessor"></param>
    public MilvaPasswordHasher(IOptions<MilvaPasswordHasherOptions> optionsAccessor = null) : this(optionsAccessor?.Value)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="MilvaPasswordHasher"/> with <paramref name="options"/>.
    /// </summary>
    /// <param name="options">Hasher options. If null, defaults are used.</param>
    public MilvaPasswordHasher(MilvaPasswordHasherOptions options)
    {
        options ??= new MilvaPasswordHasherOptions();

        _iterCount = options.IterationCount;

        if (_iterCount < 1)
            throw new InvalidOperationException("InvalidPasswordHasherIterationCount");

        _prf = options.Prf;
        _rng = options.RandomNumberGenerator;
    }

    /// <summary>
    /// Returns a hashed representation of the supplied <paramref name="password"/>.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>A hashed representation of the supplied <paramref name="password"/>.</returns>
    public virtual string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = new byte[_saltSize];

        if (_rng != null)
            _rng.GetBytes(salt);
        else
            RandomNumberGenerator.Fill(salt);

        var subkey = KeyDerivation.Pbkdf2(password, salt, _prf, _iterCount, _subkeyLength);

        var outputBytes = new byte[_headerLength + salt.Length + subkey.Length];

        outputBytes[0] = _formatMarker;

        BinaryPrimitives.WriteUInt32BigEndian(outputBytes.AsSpan(1), (uint)_prf);
        BinaryPrimitives.WriteUInt32BigEndian(outputBytes.AsSpan(5), (uint)_iterCount);
        BinaryPrimitives.WriteUInt32BigEndian(outputBytes.AsSpan(9), (uint)salt.Length);

        salt.CopyTo(outputBytes, _headerLength);
        subkey.CopyTo(outputBytes, _headerLength + salt.Length);

        return Convert.ToBase64String(outputBytes);
    }

    /// <summary>
    /// Returns a <see cref="bool"/> indicating the result of a password hash comparison.
    /// </summary>
    /// <param name="hashedPassword">The hash value for a user's stored password.</param>
    /// <param name="providedPassword">The password supplied for comparison.</param>
    /// <returns>A <see cref="bool"/> indicating the result of a password hash comparison.</returns>
    /// <remarks>Implementations of this method should be time consistent.</remarks>
    public virtual bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        => VerifyHashedPasswordWithRehashCheck(hashedPassword, providedPassword) != PasswordVerificationResult.Failed;

    /// <summary>
    /// Verifies <paramref name="providedPassword"/> against <paramref name="hashedPassword"/> and reports whether the stored hash
    /// should be replaced because it was created with weaker settings than the configured ones.
    /// </summary>
    /// <param name="hashedPassword">The hash value for a user's stored password.</param>
    /// <param name="providedPassword">The password supplied for comparison.</param>
    /// <returns>
    /// <see cref="PasswordVerificationResult.Failed"/> if the password does not match,
    /// <see cref="PasswordVerificationResult.SuccessRehashNeeded"/> if it matches but the hash uses a different PRF or fewer iterations than configured,
    /// otherwise <see cref="PasswordVerificationResult.Success"/>.
    /// </returns>
    public virtual PasswordVerificationResult VerifyHashedPasswordWithRehashCheck(string hashedPassword, string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(hashedPassword);
        ArgumentNullException.ThrowIfNull(providedPassword);

        byte[] decodedHashedPassword;

        try
        {
            decodedHashedPassword = Convert.FromBase64String(hashedPassword);
        }
        catch (FormatException)
        {
            return PasswordVerificationResult.Failed;
        }

        if (decodedHashedPassword.Length == 0 || decodedHashedPassword[0] != _formatMarker)
            return PasswordVerificationResult.Failed;

        if (!VerifyHashedPasswordV1(decodedHashedPassword, providedPassword, out var prf, out var iterCount))
            return PasswordVerificationResult.Failed;

        return prf != _prf || iterCount < _iterCount
            ? PasswordVerificationResult.SuccessRehashNeeded
            : PasswordVerificationResult.Success;
    }

    private static bool VerifyHashedPasswordV1(byte[] hashedPassword, string password, out KeyDerivationPrf prf, out int iterCount)
    {
        prf = default;
        iterCount = default;

        if (hashedPassword.Length < _headerLength)
            return false;

        var span = hashedPassword.AsSpan();

        var rawPrf = BinaryPrimitives.ReadUInt32BigEndian(span[1..]);
        var rawIterCount = BinaryPrimitives.ReadUInt32BigEndian(span[5..]);
        var rawSaltLength = BinaryPrimitives.ReadUInt32BigEndian(span[9..]);

        if (!Enum.IsDefined((KeyDerivationPrf)rawPrf) || rawIterCount < 1 || rawIterCount > int.MaxValue)
            return false;

        // Salt and subkey must both be >= 128 bits.
        if (rawSaltLength < 128 / 8 || rawSaltLength > (uint)(hashedPassword.Length - _headerLength))
            return false;

        var saltLength = (int)rawSaltLength;
        var subkeyLength = hashedPassword.Length - _headerLength - saltLength;

        if (subkeyLength < 128 / 8)
            return false;

        prf = (KeyDerivationPrf)rawPrf;
        iterCount = (int)rawIterCount;

        var salt = span.Slice(_headerLength, saltLength).ToArray();
        var expectedSubkey = span.Slice(_headerLength + saltLength, subkeyLength);

        var actualSubkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, subkeyLength);

        return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
    }
}
