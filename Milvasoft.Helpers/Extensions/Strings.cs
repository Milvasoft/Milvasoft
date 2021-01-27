using System.Linq;

namespace Milvasoft.Helpers.Extensions
{
    /// <summary>
    /// String extensions.
    /// </summary>
    public static class Strings
    {
        /// <summary>
        /// Uppercases the first letter of the word.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToUpperFirst(this string str)
        {
            if (str.Length == 0) return str;
            else if (str.Length == 1) return str.First().ToString().ToUpper();
            else return str.First().ToString().ToUpper() + str.Substring(1);
        }

        /// <summary>
        /// Uppercases the first letter of the word using the casing rules of the invariant culture.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToUpperInVariantFirst(this string str)
        {
            if (str.Length == 0) return str;
            else if (str.Length == 1) return str.First().ToString().ToUpperInvariant();
            else return str.First().ToString().ToUpperInvariant() + str.Substring(1);
        }

        /// <summary>
        /// Lowercases the first letter of the word.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToLowerFirst(this string str)
        {
            if (str.Length == 0) return str;
            else if (str.Length == 1) return str.First().ToString().ToLower();
            else return str.First().ToString().ToLower() + str.Substring(1);
        }

        /// <summary>
        /// Lowercases the first letter of the word using the casing rules of the invariant culture.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToLowerInVariantFirst(this string str)
        {
            if (str.Length == 0) return str;
            else if (str.Length == 1) return str.First().ToString().ToLowerInvariant();
            else return str.First().ToString().ToLowerInvariant() + str.Substring(1);
        }
    }
}
