using System.Text.RegularExpressions;

namespace Milvasoft.Core.Helpers;

/// <summary>
/// Helper class for matching input strings with regular expressions.
/// </summary>
public static class RegexHelper
{
    /// <summary>
    /// Checks if the <paramref name="input"/> matches the specified <paramref name="regex"/>.
    /// </summary>
    /// <param name="regex">The regular expression pattern to match.</param>
    /// <param name="input">The input string to match against the regular expression.</param>
    /// <returns><c>true</c> if the input matches the regex; otherwise, <c>false</c>.</returns>
    public static bool MatchRegex(this string input, Regex regex) => !string.IsNullOrWhiteSpace(input) && (regex?.Match(input)?.Success ?? false);

    /// <summary>
    /// Checks if the <paramref name="input"/> matches the specified <paramref name="regexString"/>.
    /// </summary>
    /// <param name="input">The input string to match against the regular expression.</param>
    /// <param name="regexString">The regular expression pattern to match.</param>
    /// <returns><c>true</c> if the input matches the regex; otherwise, <c>false</c>.</returns>
    public static bool MatchRegex(this string input, string regexString) => !string.IsNullOrWhiteSpace(input)
                                                                            && regexString != null
                                                                            && new Regex(regexString, RegexOptions.NonBacktracking, TimeSpan.FromMilliseconds(100)).IsMatch(input);

}