using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers;
using Milvasoft.Helpers.Enums;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.Utils;
using Milvasoft.SampleAPI.DTOs.AccountDTOs;
using Milvasoft.SampleAPI.Localization;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Utils;
using Milvasoft.SampleAPI.Utils.Attributes.ActionFilters;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    /// <summary>
    /// The class in which user transactions are entered and exited
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1.0")]
    [Route("sampleapi/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;
        private readonly IAccountService _accountService;
        private readonly string _defaultSucccessMessage;
        /// <summary>
        /// Constructor of <c>AccountController</c>
        /// </summary>
        /// <param name="sharedLocalizer"></param>
        /// <param name="accountService"></param>
        public AccountsController(IStringLocalizer<SharedResource> sharedLocalizer, IAccountService accountService)
        {
            _sharedLocalizer = sharedLocalizer;
            _accountService = accountService;
            _defaultSucccessMessage = _sharedLocalizer["SuccessfullyOperationMessage"];
        }

        /// <summary>
        /// Sign in method for users. This endpoint is accessible for any requests.
        /// </summary>
        /// <returns></returns>
        /// <param name="loginDTO"></param>
        /// <returns></returns>
        [HttpPost("User/SignIn")]
        [AllowAnonymous]
        [OValidationFilter]
        public async Task<ActionResult> UsersLogin([FromBody] LoginDTO loginDTO)
        {
            ObjectResponse<LoginResultDTO> response = new();

            response.Result = await _accountService.LoginAsync(loginDTO, true).ConfigureAwait(false);
            if (!response.Result.ErrorMessages.IsNullOrEmpty())
            {
                response.Message = string.Join('~', response.Result.ErrorMessages.Select(i => i.Description));

                response.StatusCode = MilvaStatusCodes.Status400BadRequest;
                response.Success = false;
            }
            else if (response.Result.Token == null)
            {
                response.Message = _sharedLocalizer["UnknownLoginProblemMessage"];
                response.StatusCode = MilvaStatusCodes.Status400BadRequest;
                response.Success = false;
            }
            else
            {
                response.Message = _sharedLocalizer["SuccessfullyLoginMessage"];
                response.StatusCode = MilvaStatusCodes.Status200OK;
                response.Success = true;
            }
            return Ok(response);
        }

        /// <summary>
        /// Logout method for users.
        /// </summary>
        /// <returns></returns>
        [HttpGet("User/SignOut")]
        [ApiVersion("1.1")]
        public async Task<IActionResult> UsersLogOut()
        {
            await _accountService.LogoutAsync().ConfigureAwait(false);
            return Ok("Success");
        }

       

        /// <summary>
        /// Returns logged-in user's account information.
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// <para> Both users(admin and mobile app user) should use this endpoint. </para>
        /// 
        /// </remarks>
        /// 
        /// <returns></returns>
        [HttpGet("LoggedIn/User/Info")]
        public async Task<IActionResult> GetLoggedInInUserInformationAsync()
        {
            var errorMessage = _sharedLocalizer.GetErrorMessage("User", CrudOperation.GetById);

            var user = await _accountService.GetLoggedInInUserInformationAsync().ConfigureAwait(false);

            return user.GetObjectResponse(_defaultSucccessMessage, errorMessage);
        }

        /// <summary>
        /// Updates logged-in user's personal information.
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateMyAccountAsync(AppUserUpdateDTO userDTO)
        {
            var successMessage = _sharedLocalizer.GetSuccessMessage("Account", CrudOperation.Update);

            return await _accountService.UpdateAccountAsync(userDTO).ConfigureAwait(false).GetObjectResponseAsync<object>(successMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Provides the registration process of mobile application users.
        /// </summary>
        /// <param name="signUpDTO"></param>
        /// <returns></returns>
        [HttpPost("Register")]
        [AllowAnonymous]
        [OValidationFilter]
        public async Task<IActionResult> RegisterAsync([FromBody] SignUpDTO signUpDTO)
        {
            ObjectResponse<LoginResultDTO> response = new()
            {
                Result = await _accountService.RegisterAsync(signUpDTO).ConfigureAwait(false)
            };

            if (!response.Result.ErrorMessages.IsNullOrEmpty())
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.AppendJoin(',', response.Result.ErrorMessages.Select(i => i.Description));

                response.Message = stringBuilder.ToString();

                //response.Message = string.Join("\r\n", response.Result.ErrorMessages.Select(m => m.Description));
                response.StatusCode = MilvaStatusCodes.Status400BadRequest; //status kod sonradan degistirlebilir.
                response.Success = false;
            } //Bu kontroller cogalabilir. orn her hata kodu icin kendine ozel status kod yazilabilir.
            else if (response.Result.Token == null)
            {
                response.Message = _sharedLocalizer["UnknownLoginProblemMessage"];
                response.StatusCode = MilvaStatusCodes.Status400BadRequest;
                response.Success = false;
            }
            else
            {
                response.Message = _sharedLocalizer["SuccessfullyLoginMessage"];
                response.StatusCode = MilvaStatusCodes.Status200OK;
                response.Success = true;
            }
            return Ok(response);
        }

        /// <summary>
        /// Deletes logged-in user's account.
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteAccountAsync()
        {
            var successMessage = _sharedLocalizer.GetSuccessMessage("Account", CrudOperation.Delete);

            return await _accountService.DeleteAccountAsync().ConfigureAwait(false).GetObjectResponseAsync<object>(successMessage).ConfigureAwait(false);
        }
        #region Account Activities / Note : Editors can be use this endpoints too.

        /// <summary>
        /// Sends email verification mail to logged-in user's email address.
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para>Redirect link in the mail : <b> https://sampleappurl.com/verify?userName=sampleusername{AND}token=sampletoken </b> </para>
        /// 
        /// <para> <b>Note :</b> As "{AND}" is a special character, it is not suitable for a summary syntax. That's why it was written that way.</para>
        /// 
        /// </remarks>
        /// 
        /// <returns></returns>
        [HttpGet("Activity/Send/Mail/EmailVerification")]
        public async Task<IActionResult> SendEmailVerificationMailAsync()
        {
            var successMessage = _sharedLocalizer.GetSuccessMessage("EmailVerificationMailSent", CrudOperation.Specific);

            return await _accountService.SendEmailVerificationMailAsync().ConfigureAwait(false).GetObjectResponseAsync<object>(successMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends phone number change mail to logged-in user's email address.
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para>Redirect link in the mail : <b> https://sampleappurl.com/change/phoneNumber?userName=sampleusername{AND}token=sampletoken </b> </para>
        /// 
        /// <para> <b>Note :</b> As "{AND}" is a special character, it is not suitable for a summary syntax. That's why it was written that way.</para>
        /// 
        /// </remarks>
        /// 
        /// <param name="newPhoneNumber"></param>
        /// <returns></returns>
        [HttpGet("Activity/Send/Mail/PhoneNumberChange/{newPhoneNumber}")]
        public async Task<IActionResult> SendChangePhoneNumberMailAsync(string newPhoneNumber)
        {
            var successMessage = _sharedLocalizer.GetSuccessMessage("PhoneNumberChangeMailSent", CrudOperation.Specific);

            return await _accountService.SendChangePhoneNumberMailAsync(newPhoneNumber).ConfigureAwait(false).GetObjectResponseAsync<object>(successMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends email change mail to logged-in user's email address.
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para>Redirect link in the mail : <b> https://sampleappurl.com/change/email?userName=sampleusername{AND}token=sampletoken </b> </para>
        /// 
        /// <para> <b>Note :</b> As "{AND}" is a special character, it is not suitable for a summary syntax. That's why it was written that way.</para>
        /// 
        /// </remarks>
        /// 
        /// <param name="newEmail"></param>
        /// <returns></returns>
        [HttpGet("Activity/Send/Mail/EmailChange/{newEmail}")]
        public async Task<IActionResult> SendChangeEmailMailAsync(string newEmail)
        {
            var successMessage = _sharedLocalizer.GetSuccessMessage("EmailChangeMailSent", CrudOperation.Specific);

            return await _accountService.SendChangeEmailMailAsync(newEmail).ConfigureAwait(false).GetObjectResponseAsync<object>(successMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends password reset mail to logged-in user's email address.
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para>Redirect link in the mail : <b> https://sampleappurl.com/reset/password?userName=sampleusername{AND}token=sampletoken </b> </para>
        /// 
        /// <para> <b>Note :</b> As "{AND}" is a special character, it is not suitable for a summary syntax. That's why it was written that way.</para>
        /// 
        /// </remarks>
        /// 
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpGet("Activity/Send/Mail/PasswordReset/{userName}")]
        [OValidateStringParameter(3, 30)]
        public async Task<IActionResult> SendResetPasswordMailAsync()
        {
            var successMessage = _sharedLocalizer.GetSuccessMessage("PasswordResetMailSent", CrudOperation.Specific);

            return await _accountService.SendResetPasswordMailAsync().ConfigureAwait(false).GetObjectResponseAsync<object>(successMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends password reset mail to email address(<paramref name="email"/>).
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para>Redirect link in the mail : <b> https://sampleappurl.com/reset/password?userName=sampleusername{AND}token=sampletoken </b> </para>
        /// 
        /// <para> <b>Note :</b> As "{AND}" is a special character, it is not suitable for a summary syntax. That's why it was written that way.</para>
        /// 
        /// </remarks>
        /// 
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("Activity/Send/Mail/ForgotPassword/{email}")]
        [OValidateStringParameter(3, 30)]
        public async Task<IActionResult> SendForgotPasswordMailAsync(string email)
        {
            var successMessage = _sharedLocalizer.GetSuccessMessage("PasswordResetMailSent", CrudOperation.Specific);

            return await _accountService.SendForgotPasswordMailAsync(email).ConfigureAwait(false).GetObjectResponseAsync<object>(successMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends verification code to logged-in user's phone number.
        /// </summary>
        /// <remarks>
        /// 
        /// <para><b> IMPORTANT INFORMATION : The message sending service has not yet been integrated. 
        ///                                   So this method will not send message to the user's gsm number.
        ///                                   Instead of returns verification code for testing. </b></para>
        /// 
        /// </remarks>
        /// 
        /// <returns></returns>
        [HttpGet("Activity/Send/Message/PhoneNumberVerification")]
        public async Task<IActionResult> SendPhoneNumberVerificationMessageAsync()
        {
            var successMessage = _sharedLocalizer.GetSuccessMessage("PhoneNumberVerificationMessageSent", CrudOperation.Specific);

            return await _accountService.SendPhoneNumberVerificationMessageAsync().ConfigureAwait(false).GetObjectResponseAsync(successMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies logged-in user's phone number.
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// The user must be logged-in because verification will be done from within the application.  
        /// 
        /// </remarks>
        /// 
        /// <param name="verificationCode"></param>
        /// <returns></returns>
        [HttpGet("Activity/Verify/PhoneNumber/{verificationCode}")]
        public async Task<IActionResult> VerifyPhoneNumberAsync(string verificationCode)
            => await _accountService.VerifyPhoneNumberAsync(verificationCode).ConfigureAwait(false).GetActivityResponseAsync(_sharedLocalizer["PhoneNumberVerificationSuccessfull"],
                                                                                                                             _sharedLocalizer["AccountActivityErrorMessage"]).ConfigureAwait(false);

        /// <summary>
        /// Verifies <paramref name="emailVerificationDTO"/>.UserName's email.
        /// </summary>
        /// <remarks>
        /// 
        /// The reason the user does not need to be logged in to request this endpoint is that the verification will take place on a web page outside of the application.
        /// 
        /// </remarks>
        /// 
        /// <param name="emailVerificationDTO"></param>
        /// <returns></returns>
        [HttpPut("Activity/Verify/Email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmailAsync([FromBody] EmailVerificationDTO emailVerificationDTO)
            => await _accountService.VerifyEmailAsync(emailVerificationDTO).ConfigureAwait(false).GetActivityResponseAsync(_sharedLocalizer["EmailVerificationSuccessfull"],
                                                                                                                           _sharedLocalizer["AccountActivityErrorMessage"]).ConfigureAwait(false);

        /// <summary>
        /// Changes <paramref name="phoneNumberChangeDTO"/>.UserName's phone number.
        /// </summary>
        /// <remarks>
        /// 
        /// The reason the user does not need to be logged in to request this endpoint is that the change process will take place on a web page outside of the application.
        /// 
        /// </remarks>
        /// 
        /// <param name="phoneNumberChangeDTO"></param>
        /// <returns></returns>
        [HttpPut("Activity/Change/PhoneNumber")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePhoneNumberAsync([FromBody] PhoneNumberChangeDTO phoneNumberChangeDTO)
            => await _accountService.ChangePhoneNumberAsync(phoneNumberChangeDTO).ConfigureAwait(false).GetActivityResponseAsync(_sharedLocalizer["PhoneNumberChangeSuccessfull"],
                                                                                                                                 _sharedLocalizer["AccountActivityErrorMessage"]).ConfigureAwait(false);

        /// <summary>
        /// Resets <paramref name="passwordResetDTO"/>.UserName's password.
        /// </summary>
        /// <remarks>
        /// 
        /// The reason the user does not need to be logged in to request this endpoint is that the reset process will take place on a web page outside of the application.
        /// 
        /// </remarks>
        /// 
        /// <param name="passwordResetDTO"></param>
        /// <returns></returns>
        [HttpPut("Activity/Reset/Password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] PasswordResetDTO passwordResetDTO)
            => await _accountService.ResetPasswordAsync(passwordResetDTO).ConfigureAwait(false).GetActivityResponseAsync(_sharedLocalizer["PasswordResetSuccessfull"],
                                                                                                                         _sharedLocalizer["AccountActivityErrorMessage"]).ConfigureAwait(false);

        /// <summary>
        /// <para>Change personnel user password.</para>
        /// </summary>
        /// <param name="personnelUpdateDTO"></param>
        /// <returns></returns>
        [HttpPut("Activity/Change/Password")]
        [OValidationFilter]
        public async Task<IActionResult> ChangeUserPassword(ChangePassDTO personnelUpdateDTO)
        {
            var response = new ObjectResponse<IdentityResult>
            {
                Result = await _accountService.ChangePasswordAsync(personnelUpdateDTO).ConfigureAwait(false)
            };

            if (response.Result.Succeeded)
            {
                response.StatusCode = MilvaStatusCodes.Status200OK;
                response.Success = true;
                response.Message = _sharedLocalizer["PersonnelSuccessfullyChangePassword"];
            }
            else
            {
                response.StatusCode = MilvaStatusCodes.Status503ServiceUnavailable;
                response.Success = false;
            }
            response.Result = null;
            return Ok(response);
        }

        #endregion

    }
}
