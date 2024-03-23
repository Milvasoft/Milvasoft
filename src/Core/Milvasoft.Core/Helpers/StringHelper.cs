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
    /// <param name="str">The input string.</param>
    /// <param name="cultureInfo">The culture info to be used for casing rules. If null, the current culture will be used.</param>
    /// <returns>The input string with the first letter uppercased.</returns>
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
    /// <param name="str">The input string.</param>
    /// <returns>The input string with the first letter uppercased using the casing rules of the invariant culture.</returns>
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
    /// <param name="str">The input string.</param>
    /// <param name="cultureInfo">The culture info to be used for casing rules. If null, the current culture will be used.</param>
    /// <returns>The input string with the first letter lowercased.</returns>
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
    /// <param name="str">The input string.</param>
    /// <returns>The input string with the first letter lowercased using the casing rules of the invariant culture.</returns>
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
    /// Gets the byte array representation of the input string.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <returns>The byte array representation of the input string.</returns>
    public static byte[] GetByteArray(this string str) => !string.IsNullOrEmpty(str) ? Encoding.UTF8.GetBytes(str) : null;

    /// <summary>
    /// Gets the string representation of the input byte array.
    /// </summary>
    /// <param name="array">The input byte array.</param>
    /// <returns>The string representation of the input byte array.</returns>
    public static string GetString(this byte[] array) => !array.IsNullOrEmpty() ? Encoding.UTF8.GetString(array) : string.Empty;

    /// <summary>
    /// Hashes the input string using the SHA256 algorithm.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <returns>The hashed string as a byte array.</returns>
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
    /// Normalizes the input string according to the invariant culture.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <returns>The normalized string according to the invariant culture.</returns>
    public static string MilvaNormalize(this string value) => !string.IsNullOrWhiteSpace(value) ? value.ToLower().ToUpperInvariant() : value;
}
