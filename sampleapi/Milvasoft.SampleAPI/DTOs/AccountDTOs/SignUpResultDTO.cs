using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;

namespace Milvasoft.SampleAPI.DTOs.AccountDTOs
{
    /// <summary>
    /// Sign up result.
    /// </summary>
    public class SignUpResultDTO
    {
        /// <summary>
        /// Response message.
        /// </summary>
        [OValidateString(2000)]
        public string Message { get; set; }

        /// <summary>
        /// Local API token.
        /// </summary>
        [OValidateString(2000)]
        public string Token { get; set; }

        /// <summary>
        /// Global API token.
        /// </summary>
        [OValidateString(2000)]
        public string GlobalToken { get; set; }

        /// <summary>
        /// Determines Global API error or Local API error.
        /// </summary>
        public bool LocalAPIFailure { get; set; }
    }
}
