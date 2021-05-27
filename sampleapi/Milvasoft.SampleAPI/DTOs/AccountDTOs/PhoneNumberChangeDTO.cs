using Milvasoft.Helpers.Attributes.Validation;
using Milvasoft.SampleAPI.Localization;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.DTOs.AccountDTOs
{
    /// <summary>
    /// DTO to be used to change phone number.
    /// </summary>
    public record PhoneNumberChangeDTO
    {
        /// <summary>
        /// The user who wants to change phone number.
        /// </summary>
        [OValidateString(3, 20)]
        public string UserName { get; init; }

        /// <summary>
        /// New phone number.
        /// </summary>
        [OValidateString(0, 16, MemberNameLocalizerKey = "LocalizedPhoneNumber")]
        [MilvaRegex(typeof(SharedResource), IsRequired = false, MemberNameLocalizerKey = "PhoneNumber", ExampleFormatLocalizerKey = "RegexExamplePhoneNumber")]
        public string NewPhoneNumber { get; init; }

        /// <summary>
        /// Phone number change token.
        /// </summary>
        [OValidateString(5, 6, MemberNameLocalizerKey = "InvalidVerificationToken")]
        public string TokenString { get; init; }
    }
}
