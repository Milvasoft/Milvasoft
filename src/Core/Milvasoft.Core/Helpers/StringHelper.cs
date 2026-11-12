using Milvasoft.Core.Utils.Enums;
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
    public static byte[] GetByteArray(this string str) => !string.IsNullOrEmpty(str) ? Encoding.UTF8.GetBytes(str) : [];

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

        return !byteArray.IsNullOrEmpty() ? SHA256.HashData(byteArray) : [];
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
    /// Normalizes the input string for case insensitive comparisons (e.g. normalized user names and emails).
    /// The result does not depend on the current culture.
    /// </summary>
    /// <remarks>
    /// The value is lowercased and then uppercased with invariant culture rules, and the Turkish dotted/dotless I letters
    /// ('ı', 'İ') are folded to 'I'. So "I", "i", "ı" and "İ" all normalize to "I" (e.g. "istanbul", "İstanbul", "ISTANBUL" → "ISTANBUL";
    /// "ışık", "IŞIK" → "IŞIK").
    /// <para>
    /// Before this change the first step used the current culture. On a tr-TR server 'I' became 'ı' (so "IŞIK" → "ıŞıK"),
    /// on other cultures 'ı' stayed 'ı' (and on invariant culture 'İ' stayed 'İ'). Values normalized by older versions differ from the new result
    /// only by these characters, so they can be migrated with <c>REPLACE(REPLACE(column, 'ı', 'I'), 'İ', 'I')</c>.
    /// </para>
    /// </remarks>
    /// <param name="value">The input string.</param>
    /// <returns>The normalized string.</returns>
    public static string MilvaNormalize(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        return value.ToLowerInvariant()
                    .ToUpperInvariant()
                    .Replace('ı', 'I')
                    .Replace('İ', 'I');
    }

    /// <summary>
    /// Masks string.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="maskChar"></param>
    /// <param name="percentToApply"></param>
    /// <param name="maskOptions"></param>
    /// <returns></returns>
    public static string Mask(this string input, char maskChar = '*', int percentToApply = 25, MaskOption maskOptions = MaskOption.InTheMiddleOfString)
    {
        if (string.IsNullOrWhiteSpace(input) || percentToApply < 1)
            return input;

        var result = input.Trim();

        if (percentToApply >= 100)
            return new string(maskChar, result.Length);

        var maskLength = Math.Max((int)Math.Round(percentToApply * result.Length / 100m), 1);

        var mask = new string(maskChar, maskLength);

        switch (maskOptions)
        {
            case MaskOption.AtTheBeginingOfString:
                result = string.Concat(mask, result.AsSpan(maskLength));
                break;
            case MaskOption.AtTheEndOfString:
                result = string.Concat(result.AsSpan(0, result.Length - maskLength), mask);
                break;
            case MaskOption.InTheMiddleOfString:
                var maskStart = (result.Length - maskLength) / 2;
                result = string.Concat(result.AsSpan(0, maskStart), mask, result.AsSpan(maskStart + maskLength));
                break;
        }

        return result;
    }
}
