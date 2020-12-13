using System.Linq;

namespace Milvasoft.Helpers.Extensions
{
    public static class Strings
    {
        public static string ToUpperFirst(this string str)
        {
            if (str.Length == 0) return str;
            else if (str.Length == 1) return str.First().ToString().ToUpper();
            else return str.First().ToString().ToUpper() + str.Substring(1);
        }

        public static string ToUpperInVariantFirst(this string str)
        {
            if (str.Length == 0) return str;
            else if (str.Length == 1) return str.First().ToString().ToUpperInvariant();
            else return str.First().ToString().ToUpperInvariant() + str.Substring(1);
        }

        public static string ToLowerFirst(this string str)
        {
            if (str.Length == 0) return str;
            else if (str.Length == 1) return str.First().ToString().ToLower();
            else return str.First().ToString().ToLower() + str.Substring(1);
        }

        public static string ToLowerInVariantFirst(this string str)
        {
            if (str.Length == 0) return str;
            else if (str.Length == 1) return str.First().ToString().ToLowerInvariant();
            else return str.First().ToString().ToLowerInvariant() + str.Substring(1);
        }
    }
}
