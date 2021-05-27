using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs.AccountDTOs
{
    /// <summary>
    /// DTO to be used to verify email.
    /// </summary>
    public record EmailVerificationDTO
    {
        /// <summary>
        /// The user who wants to verify email.
        /// </summary>
        [OValidateString(3, 20)]
        public string UserName { get; init; }

        /// <summary>
        /// Verification token.
        /// </summary>
        [OValidateString(20, 1000, MemberNameLocalizerKey = "InvalidVerificationToken")]
        public string TokenString { get; init; }
    }
}
