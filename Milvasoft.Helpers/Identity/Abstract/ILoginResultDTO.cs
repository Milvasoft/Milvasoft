using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Milvasoft.Helpers.Identity.Concrete
{
    /// <summary>
    /// Login result DTO for DI.
    /// </summary>
    public interface ILoginResultDTO
    {
        /// <summary>
        /// Error messages of result.
        /// </summary>
        public List<IdentityError> ErrorMessages { get; set; }

        /// <summary>
        /// If result is success sets the token.
        /// </summary>
        public IToken Token { get; set; }
    }
}