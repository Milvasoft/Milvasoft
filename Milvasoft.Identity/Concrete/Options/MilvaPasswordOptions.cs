﻿namespace Milvasoft.Identity.Concrete.Options;

/// <summary>
/// Specifies options for password requirements.
/// </summary>
public class MilvaPasswordOptions
{
    /// <summary>
    /// Password hasher options.
    /// </summary>
    public MilvaPasswordHasherOptions Hasher { get; set; } = new();

    /// <summary>
    /// Gets or sets the minimum length a password must be.
    /// </summary>
    /// <remarks>
    /// This defaults to 6.
    /// </remarks>
    public int RequiredLength { get; set; } = 6;

    /// <summary>
    /// Gets or sets the minimum number of unique chars a password must comprised of.
    /// </summary>
    /// <remarks>
    /// This defaults to 1.
    /// </remarks>
    public int RequiredUniqueChars { get; set; } = 1;

    /// <summary>
    /// Gets or sets a flag indicating if passwords must contain a non-alphanumeric character.
    /// </summary>
    /// <value>True if passwords must contain a non-alphanumeric character, otherwise false.</value>
    /// <remarks>
    /// This defaults to true.
    /// </remarks>
    public bool RequireNonAlphanumeric { get; set; } = true;

    /// <summary>
    /// Gets or sets a flag indicating if passwords must contain a lower case ASCII character.
    /// </summary>
    /// <value>True if passwords must contain a lower case ASCII character.</value>
    /// <remarks>
    /// This defaults to true.
    /// </remarks>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>
    /// Gets or sets a flag indicating if passwords must contain a upper case ASCII character.
    /// </summary>
    /// <value>True if passwords must contain a upper case ASCII character.</value>
    /// <remarks>
    /// This defaults to true.
    /// </remarks>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>
    /// Gets or sets a flag indicating if passwords must contain a digit.
    /// </summary>
    /// <value>True if passwords must contain a digit.</value>
    /// <remarks>
    /// This defaults to true.
    /// </remarks>
    public bool RequireDigit { get; set; } = true;
}
