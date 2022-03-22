using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.Identity.Abstract;
using Milvasoft.Helpers.Identity.Concrete;
using System.Collections.Generic;

namespace Milvasoft.SampleAPI.DTOs.AccountDTOs
{
    /// <summary>
    /// DTO returned by login.
    /// </summary>
    public class LoginResultDTO : ILoginResultDTO<MilvaToken>
    {
        /// <summary>
        /// Error caused by login.
        /// </summary>
        public List<IdentityError> ErrorMessages { get; set; }

        /// <summary>
        /// The token issued as a result of the login process.
        /// </summary>
        public MilvaToken Token { get; set; }
    }
}