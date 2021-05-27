using Milvasoft.Helpers.Attributes.Validation;
using Milvasoft.SampleAPI.Localization;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;

namespace Milvasoft.SampleAPI.DTOs.AccountDTOs
{
    /// <summary>
    /// DTO to be user in creation proccess.
    /// </summary>
    public class AppUserDTO
    {
        /// <summary>
        /// UserName of user.
        /// </summary>
        [OValidateString(3, 20)]
        public string UserName { get; set; }

        /// <summary>
        /// Email of user.
        /// </summary>
        [OValidateString(7, 75)]
        [MilvaRegex(typeof(SharedResource), IsRequired = false)]
        public string Email { get; set; }

        /// <summary>
        /// PhoneNumber of user.
        /// </summary>   
        [OValidateString(0, 16)]
        [MilvaRegex(typeof(SharedResource), IsRequired = false)]
        public string PhoneNumber { get; set; }
    }
}
