using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.Identity.Concrete;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System.Collections.Generic;

namespace Milvasoft.SampleAPI.DTOs.AccountDTOs
{
    /// <summary>
    /// DTO returned by login.
    /// </summary>
    public class LoginResultDTO : ILoginResultDTO
    {
        /// <summary>
        /// Error caused by login.
        /// </summary>
        public List<IdentityError> ErrorMessages { get; set; }

        /// <summary>
        /// The token issued as a result of the login process.
        /// </summary>
        [OValidateString(5000)]
        public string Token { get; set; }
    }
}