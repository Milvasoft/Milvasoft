using System.Collections.Generic;

namespace Milvasoft.Helpers.Identity.Concrete
{
    /// <summary>
    /// Contains userId and Token pair for authentication.
    /// </summary>
    public static class SignedInUsers
    {
        /// <summary>
        /// UserName and token pairs for each logged in user.
        /// </summary>
        public static Dictionary<string, string> SignedInUserTokens { get; set; } = new();
    }
}
