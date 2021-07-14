using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Identity.Abstract;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs.AccountDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Localization;
using System;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// The class in which user transactions are entered and exited.
    /// </summary>
    public interface IAccountService : IIdentityOperations<UserManager<AppUser>, EducationAppDbContext, IStringLocalizer<SharedResource>, AppUser, AppRole, Guid, LoginResultDTO>
    {
        /// <summary>
        /// Login for incoming user. Returns a token if login informations are valid or the user is not lockedout. Otherwise returns the error list.
        /// </summary>
        /// <returns></returns>
        Task<LoginResultDTO> LoginAsync(LoginDTO loginDTO, bool isMentor);

        /// <summary>
        /// Gets a specific personnel data from repository by token value if exsist.
        /// </summary>
        /// <returns> Logged-in user data. </returns>
        Task<AppUserDTO> GetLoggedInInUserInformationAsync();

        /// <summary>
        /// Change user password.
        /// </summary>
        /// <param name="changeDTO"></param>
        /// <returns></returns>
        Task<IdentityResult> ChangePasswordAsync(ChangePassDTO changeDTO);

        /// <summary>
        /// Updates logged-in user's personal information.
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        Task UpdateAccountAsync(AppUserUpdateDTO userDTO);

        /// <summary>
        /// Sign up process for application user.
        /// If signup process is succesful,then sign in.
        /// </summary>
        /// <param name="userSignUpDTO"></param>
        /// <returns></returns>
        Task<LoginResultDTO> RegisterAsync(RegisterDTO userSignUpDTO);

        /// <summary>
        /// Deletes logged-in user's account. This operation is irreversible.
        /// </summary>
        /// <returns></returns>
        Task DeleteAccountAsync();

        #region Account Activities 

        /// <summary>
        /// Sends email verification mail to logged-in user.
        /// </summary>
        /// <returns></returns>
        Task SendEmailVerificationMailAsync();

        /// <summary>
        /// Sends phone number change mail to logged-in user.
        /// </summary>
        /// <returns></returns>
        Task SendChangePhoneNumberMailAsync(string newPhoneNumber);

        /// <summary>
        /// Sends email chage mail to logged-in user.
        /// </summary>
        /// <returns></returns>
        Task SendChangeEmailMailAsync(string newEmail);

        /// <summary>
        /// Sends password reset mail to logged-in user.
        /// </summary>
        /// <returns></returns>
        Task SendResetPasswordMailAsync();

        /// <summary>
        /// Sends password reset mail to <paramref name="email"/>.
        /// </summary>
        /// <returns></returns>
        Task SendForgotPasswordMailAsync(string email);

        /// <summary>
        /// Sends verification code to logged-in user's phone number.
        /// <para><b> IMPORTANT INFORMATION : The message sending service has not yet been integrated. 
        ///                                   So this method will not send message to the user's gsm number.
        ///                                   Instead of returns verification code for testing. </b></para>
        /// </summary>
        /// <returns></returns>
        Task<string> SendPhoneNumberVerificationMessageAsync();

        /// <summary>
        /// Verifies email, if <paramref name="verificationCode"/> is correct.
        /// </summary>
        /// <param name="verificationCode"></param>
        /// <returns></returns>
        Task<IdentityResult> VerifyPhoneNumberAsync(string verificationCode);

        /// <summary>
        /// Verifies <paramref name="emailVerificationDTO"/>.UserName's email, if <paramref name="emailVerificationDTO"/>.TokenString is valid.
        /// </summary>
        /// <param name="emailVerificationDTO"></param>
        /// <returns></returns>
        Task<IdentityResult> VerifyEmailAsync(EmailVerificationDTO emailVerificationDTO);

        /// <summary>
        /// Changes <paramref name="phoneNumberChangeDTO"/>.UserName's email 
        /// with <paramref name="phoneNumberChangeDTO"/>.NewPhoneNumber, if <paramref name="phoneNumberChangeDTO"/>.TokenString is valid.
        /// </summary>
        /// <param name="phoneNumberChangeDTO"></param>
        /// <returns></returns>
        Task<IdentityResult> ChangePhoneNumberAsync(PhoneNumberChangeDTO phoneNumberChangeDTO);

        /// <summary>
        /// Resets <paramref name="passwordResetDTO"/>.UserName's password with <paramref name="passwordResetDTO"/>.NewPassword, if <paramref name="passwordResetDTO"/>.TokenString is valid.
        /// </summary>
        /// <param name="passwordResetDTO"></param>
        /// <returns></returns>
        Task<IdentityResult> ResetPasswordAsync(PasswordResetDTO passwordResetDTO);

        #endregion
    }
}
