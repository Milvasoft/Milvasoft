using System.Linq;
using System.Security.Cryptography;
using System.Text;

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

        /// <summary>
        /// Gets <paramref name="str"/>'s bytes.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] GetByteArray(this string str) => Encoding.ASCII.GetBytes(str);

        /// <summary>
        /// Gets string from <paramref name="array"/>.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string GetString(this byte[] array) => Encoding.ASCII.GetString(array);

        /// <summary>
        /// Hashes <paramref name="str"/> with <see cref="SHA256"/>
        /// </summary>
        /// <param name="str"></param>
        /// <returns> Hashed <paramref name="str"/> as byte content. </returns>
        public static byte[] HashToByteArray(this string str) => SHA256.HashData(str.GetByteArray());

        /// <summary>
        /// Hashes <paramref name="str"/> with <see cref="SHA256"/>
        /// </summary>
        /// <param name="str"></param>
        /// <returns> Hashed <paramref name="str"/> as string. </returns>
        public static string HashToString(this string str) => SHA256.HashData(str.GetByteArray()).GetString();

    }
}
