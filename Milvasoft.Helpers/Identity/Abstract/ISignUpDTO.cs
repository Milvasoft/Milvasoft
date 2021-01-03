namespace Milvasoft.Helpers.Identity.Abstract
{
    /// <summary>
    /// Login and sign up processes are happens with this dto.
    /// </summary>
    public interface ISignUpDTO
    {
        /// <summary>
        /// Username of user.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Email of user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Password of user.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Password of user.
        /// </summary>
        public string PhoneNumber { get; set; }
    }
}
