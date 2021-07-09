using Milvasoft.Helpers.Attributes.Validation;
using System;

namespace Milvasoft.Helpers.Identity.Concrete
{
    /// <summary>
    /// Login result DTO for DI.
    /// </summary>
    public interface IToken
    {
        /// <summary>
        /// If result is success sets the token.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Expiration data of <see cref="AccessToken"/>.
        /// </summary>
        public DateTime Expiration { get; set; }

        /// <summary>
        /// Refresh token.
        /// </summary>
        public string RefreshToken { get; set; }
    }
}