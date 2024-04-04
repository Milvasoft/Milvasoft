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
        StringBuilder password = new();

        Random random = new();

        while (password.Length < length)
        {
            char c = (char)random.Next(32, 126);

            password.Append(c);

            if (char.IsDigit(c))
                digit = false;
            else if (char.IsLower(c))
                lowercase = false;
            else if (char.IsUpper(c))
                uppercase = false;
            else if (!char.IsLetterOrDigit(c))
                nonAlphanumeric = false;
        }

        if (nonAlphanumeric)
            password.Append((char)random.Next(33, 48));
        if (digit)
            password.Append((char)random.Next(48, 58));
        if (lowercase)
            password.Append((char)random.Next(97, 123));
        if (uppercase)
            password.Append((char)random.Next(65, 91));

        return password.ToString();
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
