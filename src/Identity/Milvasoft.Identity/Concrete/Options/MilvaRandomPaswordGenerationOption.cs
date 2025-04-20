using Microsoft.AspNetCore.Identity;

namespace Milvasoft.Identity.Concrete.Options;

/// <summary>
/// Specifies options for password requirements, including length, character types, 
/// and allowed character sets for generating random passwords.
/// </summary>
public class MilvaRandomPaswordGenerationOption
{
    /// <summary>
    /// Default allowed digits
    /// </summary>
    public const string DefaultDigits = "0123456789";

    /// <summary>
    /// Default allowed lower chars
    /// </summary>
    public const string DefaultLowers = "abcdefghijklmnopqrstuvwxyz";

    /// <summary>
    /// Default allowed upper chars
    /// </summary>
    public const string DefaultUppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>
    /// Default allowed special chars
    /// </summary>
    public const string DefaultSymbols = "!@#$%^&*()_+-=[]{}|:;,.<>?";

    /// <summary>
    /// Gets or sets the length of the password. Default is 8.
    /// </summary>
    public int Length { get; set; } = 8;

    /// <summary>
    /// Gets or sets a value indicating whether non-alphanumeric characters are required. Default is true.
    /// </summary>
    public bool NonAlphanumeric { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether digits are required. Default is true.
    /// </summary>
    public bool Digit { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether lowercase letters are required. Default is true.
    /// </summary>
    public bool Lowercase { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether uppercase letters are required. Default is true.
    /// </summary>
    public bool Uppercase { get; set; } = true;

    /// <summary>
    /// Gets or sets the allowed digits for password generation. Default is null, allowing all digits.
    /// </summary>
    public string AllowedDigits { get; set; } = DefaultDigits;

    /// <summary>
    /// Gets or sets the allowed lowercase characters for password generation. Default is null, allowing all lowercase letters.
    /// </summary>
    public string AllowedLowerChars { get; set; } = DefaultLowers;

    /// <summary>
    /// Gets or sets the allowed uppercase characters for password generation. Default is null, allowing all uppercase letters.
    /// </summary>
    public string AllowedUpperChars { get; set; } = DefaultUppers;

    /// <summary>
    /// Gets or sets the allowed symbols for password generation. Default is null, allowing all symbols.
    /// </summary>
    public string AllowedSymbols { get; set; } = DefaultSymbols;

    /// <summary>
    /// Initializes a new instance of the <see cref="MilvaRandomPaswordGenerationOption"/> class.
    /// </summary>
    public MilvaRandomPaswordGenerationOption()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MilvaRandomPaswordGenerationOption"/> class
    /// </summary>
    /// <param name="passwordOptions"></param>
    public MilvaRandomPaswordGenerationOption(PasswordOptions passwordOptions)
    {
        Length = passwordOptions.RequiredLength;
        NonAlphanumeric = passwordOptions.RequireNonAlphanumeric;
        Digit = passwordOptions.RequireDigit;
        Lowercase = passwordOptions.RequireLowercase;
        Uppercase = passwordOptions.RequireUppercase;
    }
}
