using System.Globalization;
using System.Security.Cryptography;

namespace Milvasoft.Core.Helpers;

/// <summary>
/// String extensions.
/// </summary>
public static partial class CommonHelper
{
    /// <summary>
    /// Uppercases the first letter of the word.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public static string ToUpperFirst(this string str, CultureInfo cultureInfo = null)
    {
        cultureInfo ??= CultureInfo.CurrentCulture;

        if (string.IsNullOrWhiteSpace(str) || str.Length == 0)
            return str;
        else if (str.Length == 1)
            return char.ToUpper(str[0], cultureInfo).ToString();
        else
            return char.ToUpper(str[0], cultureInfo) + str[1..];
    }

    /// <summary>
    /// Uppercases the first letter of the word using the casing rules of the invariant culture.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToUpperInvariantFirst(this string str)
    {
        if (string.IsNullOrWhiteSpace(str) || str.Length == 0)
            return str;
        else if (str.Length == 1)
            return char.ToUpperInvariant(str[0]).ToString();
        else
            return char.ToUpperInvariant(str[0]) + str[1..];
    }

    /// <summary>
    /// Lowercases the first letter of the word.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public static string ToLowerFirst(this string str, CultureInfo cultureInfo = null)
    {
        cultureInfo ??= CultureInfo.CurrentCulture;

        if (string.IsNullOrWhiteSpace(str) || str.Length == 0)
            return str;
        else if (str.Length == 1)
            return char.ToLower(str[0], cultureInfo).ToString();
        else
            return char.ToLower(str[0], cultureInfo) + str[1..];
    }

    /// <summary>
    /// Lowercases the first letter of the word using the casing rules of the invariant culture.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToLowerInvariantFirst(this string str)
    {
        if (string.IsNullOrWhiteSpace(str) || str.Length == 0)
            return str;
        else if (str.Length == 1)
            return char.ToLowerInvariant(str[0]).ToString();
        else
            return char.ToLowerInvariant(str[0]) + str[1..];
    }

    /// <summary>
    /// Gets <paramref name="str"/>'s bytes.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] GetByteArray(this string str) => !string.IsNullOrEmpty(str) ? Encoding.UTF8.GetBytes(str) : null;

    /// <summary>
    /// Gets string from <paramref name="array"/>.
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static string GetString(this byte[] array) => !array.IsNullOrEmpty() ? Encoding.UTF8.GetString(array) : string.Empty;

    /// <summary>
    /// Hashes <paramref name="str"/> with <see cref="SHA256"/>
    /// </summary>
    /// <param name="str"></param>
    /// <returns> Hashed <paramref name="str"/> as byte content. </returns>
    public static byte[] HashToByteArray(this string str)
    {
        var byteArray = str.GetByteArray();

        return !byteArray.IsNullOrEmpty() ? SHA256.HashData(byteArray) : null;
    }

    /// <summary>
    /// Computes the SHA256 hash of the given string.
    /// </summary>
    /// <param name="str">The string to be hashed.</param>
    /// <returns>The hashed string as a hexadecimal representation.</returns>
    public static string Hash(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return string.Empty;

        var rawDataBytes = str.GetByteArray();

        // ComputeHash - returns byte array
        byte[] bytes = SHA256.HashData(rawDataBytes);

        // Convert byte array to a string
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < bytes.Length; i++)
            builder.Append(bytes[i].ToString("x2"));

        return builder.ToString();
    }

    /// <summary>
    /// Normalize string according to invariant culture.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string MilvaNormalize(this string value) => !string.IsNullOrWhiteSpace(value) ? value.ToLower().ToUpperInvariant() : value;
}
