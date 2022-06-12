using System.Security.Cryptography;

namespace Milvasoft.Identity.Concrete.Options;

/// <summary>
/// Password hasher options configuration.
/// </summary>
public class MilvaPasswordHasherOptions
{
    private static readonly RandomNumberGenerator _defaultRng = RandomNumberGenerator.Create(); // secure PRNG

    /// <summary>
    /// Gets or sets the number of iterations used when hashing passwords using PBKDF2.
    /// </summary>
    /// <value>
    /// The number of iterations used when hashing passwords using PBKDF2.
    /// </value>
    /// <remarks>
    /// This value is only used when the compatibility mode is set to 'V3'.
    /// The value must be a positive integer. The default value is 10,000.
    /// </remarks>
    public int IterationCount { get; set; } = 10000;

    /// <summary>
    /// Random number generator.
    /// </summary>
    public RandomNumberGenerator Rng { get; set; } = _defaultRng;
}
