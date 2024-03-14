using System.Text.RegularExpressions;

namespace Milvasoft.Core;

/// <summary>
/// Helper class for match input string and regex.
/// </summary>
public static class RegexMatcher
{
    /// <summary>
    /// Checks if <paramref name="input"/> matches <paramref name="regex"/>.
    /// </summary>
    /// <param name="regex"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool MatchRegex(this Regex regex, string input) => regex.Match(input).Success;

    /// <summary>
    /// Checks if <paramref name="input"/> matches <paramref name="regexString"/>.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="regexString"></param>
    /// <returns></returns>
    public static bool MatchRegex(this string input, string regexString) => new Regex(regexString).Match(input).Success;

    /// <summary>
    /// Checks if <paramref name="input"/> matches <paramref name="regex"/>.
    /// </summary>
    /// <param name="regex"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public static Match GetMatchResult(this Regex regex, string input) => regex.Match(input);

    /// <summary>
    /// Checks if <paramref name="input"/> matches <paramref name="regexString"/>.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="regexString"></param>
    /// <returns></returns>
    public static Match GetMatchResult(this string input, string regexString) => new Regex(regexString).Match(input);
}
