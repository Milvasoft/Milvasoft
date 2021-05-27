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
        /// User's name.
        /// </summary>
        [OValidateString(250)]
        public string Name { get; set; }

        /// <summary>
        /// User's surname.
        /// </summary>
        [OValidateString(250)]
        public string Surname { get; set; }

        /// <summary>
        /// User's phone number.
        /// </summary>
        [OValidateString(15, 16)]
        [MilvaRegex(typeof(SharedResource), IsRequired = false)]
        public string PhoneNumber { get; set; }

    }
}
