using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;

namespace Milvasoft.SampleAPI.DTOs.AccountDTOs
{
    /// <summary>
    /// Token information of the user is kept.
    /// </summary>
    public record VerificationTokenDTO
    {
        /// <summary>
        /// User name.
        /// </summary>
        [OValidateString(2, 100)]
        public string UserName { get; set; }

        /// <summary>
        /// Token information generated specifically for the user.
        /// </summary>
        [OValidateString(20, 1000, MemberNameLocalizerKey = "InvalidVerificationToken")]
        public string TokenString { get; init; }
    }
}
