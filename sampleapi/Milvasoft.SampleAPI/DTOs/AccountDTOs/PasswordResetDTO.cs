using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;

namespace Milvasoft.SampleAPI.DTOs.AccountDTOs
{
    /// <summary>
    /// DTO to be used to reset password.
    /// </summary>
    public record PasswordResetDTO
    {
        /// <summary>
        /// The user who wants to change email.
        /// </summary>
        [OValidateString(3, 20)]
        public string UserName { get; init; }

        /// <summary>
        /// New password.
        /// </summary>
        [OValidateString(5, 75, MemberNameLocalizerKey = "LocalizedPassword")]
        public string NewPassword { get; init; }

        /// <summary>
        /// Password reset token.
        /// </summary>
        [OValidateString(20, 1000, MemberNameLocalizerKey = "InvalidVerificationToken")]
        public string TokenString { get; init; }
    }
}
