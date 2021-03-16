using Milvasoft.Helpers.Attributes.Validation;
using Milvasoft.SampleAPI.Localization;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;

namespace Milvasoft.SampleAPI.DTOs.AccountDTOs
{
    /// <summary>
    /// Login and sign up processes are happens with this dto.
    /// </summary>
    public class SignUpDTO : LoginDTO
    {
        /// <summary>
        /// Customer's name.
        /// </summary>
        [OValidateString(250)]
        public string Name { get; set; }

        /// <summary>
        /// Customer's surname.
        /// </summary>
        [OValidateString(250)]
        public string Surname { get; set; }

        /// <summary>
        /// Customer's phone number.
        /// </summary>
        [OValidateString(15, 16)]
        [MilvaRegex(typeof(SharedResource), IsRequired = false)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Ops!yon Customer Global API token with unlimited time.
        /// </summary>
        [OValidateString(2000)]
        public string Token { get; set; }
    }
}
