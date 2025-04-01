using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace Milvasoft.Identity.Concrete;

/// <summary>
/// Identity helpers.
/// </summary>
public static class IdentityHelpers
{
    /// <summary>
    /// Append joins <see cref="IdentityResult.Errors"/>.Description with <paramref name="seperator"/>.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="seperator"></param>
    /// <returns></returns>
    public static string DescriptionJoin(this IdentityResult result, char seperator = '~')
        => string.Join(seperator, result.Errors?.Select(i => i.Description));

    /// <summary>
    /// Append joins <see cref="IdentityResult.Errors"/>.Description with <paramref name="seperator"/>.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="seperator"></param>
    /// <returns></returns>
    public static string DescriptionJoin(this IdentityResult result, string seperator)
        => string.Join(seperator, result.Errors?.Select(i => i.Description));

    /// <summary>
    /// Append joins <see cref="IdentityResult.Errors"/>.Description with <paramref name="seperator"/>.
    /// </summary>
    /// <param name="errorList"></param>
    /// <param name="seperator"></param>
    /// <returns></returns>
    public static string DescriptionJoin(this List<IdentityError> errorList, char seperator = '~')
        => string.Join(seperator, errorList?.Select(i => i.Description));

    /// <summary>
    /// Append joins <see cref="IdentityResult.Errors"/>.Description with <paramref name="seperator"/>.
    /// </summary>
    /// <param name="errorList"></param>
    /// <param name="seperator"></param>
    /// <returns></returns>
    public static string DescriptionJoin(this List<IdentityError> errorList, string seperator)
        => string.Join(seperator, errorList?.Select(i => i.Description));

    /// <summary>
    /// If <paramref name="result"/> is not succeeded throwns <see cref="MilvaUserFriendlyException"/>.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="seperator"></param>
    public static void ThrowErrorMessagesIfNotSuccess(this IdentityResult result, char seperator = '~')
    {
        if (!result.Succeeded)
            throw new MilvaUserFriendlyException(result.DescriptionJoin(seperator));
    }

    /// <summary>
    /// Generates random password according to <paramref name="passwordOptions"/>.
    /// </summary>
    /// <param name="passwordOptions"></param>
    /// <returns></returns>
    public static string GenerateRandomPassword(this PasswordOptions passwordOptions) => GenerateRandomPassword(passwordOptions.RequiredLength,
                                                                                                                passwordOptions.RequireNonAlphanumeric,
                                                                                                                passwordOptions.RequireDigit,
                                                                                                                passwordOptions.RequireLowercase,
                                                                                                                passwordOptions.RequireUppercase);

    /// <summary>
    /// Generates random password.
    /// </summary>
    /// <param name="length"></param>
    /// <param name="nonAlphanumeric"></param>
    /// <param name="digit"></param>
    /// <param name="lowercase"></param>
    /// <param name="uppercase"></param>
    /// <returns></returns>
    public static string GenerateRandomPassword(int length = 8, bool nonAlphanumeric = true, bool digit = true, bool lowercase = true, bool uppercase = true)
    {
        const string digits = "0123456789";
        const string lowers = "abcdefghijklmnopqrstuvwxyz";
        const string uppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string symbols = "!@#$%^&*()_+-=[]{}|:;,.<>?";

        List<char> characterPool = [];

        if (digit)
            characterPool.AddRange(digits);

        if (lowercase)
            characterPool.AddRange(lowers);

        if (uppercase)
            characterPool.AddRange(uppers);

        if (nonAlphanumeric)
            characterPool.AddRange(symbols);

        if (characterPool.Count == 0)
            throw new ArgumentException("At least one character type must be enabled.");

        StringBuilder password = new();
        Random random = new();

        // Ensure at least one of each required type
        if (digit)
            password.Append(digits[random.Next(digits.Length)]);

        if (lowercase)
            password.Append(lowers[random.Next(lowers.Length)]);

        if (uppercase)
            password.Append(uppers[random.Next(uppers.Length)]);

        if (nonAlphanumeric)
            password.Append(symbols[random.Next(symbols.Length)]);

        while (password.Length < length)
        {
            password.Append(characterPool[random.Next(characterPool.Count)]);
        }

        return new string([.. password.ToString().OrderBy(_ => random.Next())]);
    }

    /// <summary>
    /// Creates random refresh token.
    /// </summary>
    /// <returns></returns>
    public static string CreateRefreshToken()
    {
        byte[] number = new byte[32];

        using var random = RandomNumberGenerator.Create();

        random.GetBytes(number);

        return Convert.ToBase64String(number);
    }
}
