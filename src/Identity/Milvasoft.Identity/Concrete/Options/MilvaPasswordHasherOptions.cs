using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Milvasoft.Identity.Concrete.Options;

/// <summary>
/// Password hasher options configuration.
/// </summary>
public class MilvaPasswordHasherOptions
{
    /// <summary>
    /// Default PBKDF2 iteration count for <see cref="KeyDerivationPrf.HMACSHA512"/> (OWASP recommendation).
    /// </summary>
    public const int DefaultIterationCount = 210_000;

    /// <summary>
    /// Gets or sets the number of iterations used when hashing passwords using PBKDF2.
    /// </summary>
    /// <remarks>
    /// The value must be a positive integer. The default value is <see cref="DefaultIterationCount"/>.
    /// Hashes created with fewer iterations still verify and are reported as needing a rehash.
    /// </remarks>
    public int IterationCount { get; set; } = DefaultIterationCount;

    /// <summary>
    /// Gets or sets the pseudo-random function used by PBKDF2. Default is <see cref="KeyDerivationPrf.HMACSHA512"/>.
    /// </summary>
    /// <remarks>
    /// Hashes created with a different PRF still verify and are reported as needing a rehash.
    /// </remarks>
    public KeyDerivationPrf Prf { get; set; } = KeyDerivationPrf.HMACSHA512;

    /// <summary>
    /// Random number generator used for salts. If null, <see cref="RandomNumberGenerator.Fill(Span{byte})"/> is used.
    /// </summary>
    public RandomNumberGenerator RandomNumberGenerator { get; set; }
}
