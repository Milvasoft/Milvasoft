using System.Security.Cryptography;

namespace Milvasoft.Identity.Concrete.Options;

/// <summary>
/// Provides post congifure.
/// </summary>
public class MilvaIdentityPostConfigureOptions
{
    /// <summary>
    /// Random number generator.
    /// </summary>
    public RandomNumberGenerator RandomNumberGenerator { get; set; }
}
