using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Identity.Abstract;
using Milvasoft.SampleAPI.DTOs.IdentityDTOs;
using Milvasoft.SampleAPI.Entity.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// <para><b>EN:</b> The class in which user transactions are entered and exited</para>
    /// <para><b>TR:</b> Kullanıcı işlemlerinin giriş-çıkış işlemlerinin yapıldığı sınıf</para>
    /// </summary>
    public interface IAccountService
    {
        Task<LoginResultDTO> SignUpAsync(SignUpDTO userSignUpDTO);

        /// <summary>
        /// Signs in for incoming user. Returns a token if login informations are valid or the user is not lockedout. Otherwise returns the error list.
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <returns></returns>
        Task<LoginResultDTO> SignInAsync(ILoginDTO loginDTO);

        /// <summary>
        /// Signs out from database. Returns null if already signed out.
        /// </summary>
        /// <returns></returns>
        Task<IdentityResult> SignOutAsync();

        /// <summary>
        /// Delete user by <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task DeleteUserAsync(Guid userId);

        /// <summary>
        /// Resetes current user's password.
        /// </summary>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        Task ResetPasswordAsync(string newPassword);

        /// <summary>
        /// Change current user's password.
        /// </summary>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        Task<IdentityResult> ChangeCurrentUserPasswordAsync(string oldPassword, string newPassword);

        /// <summary>
        /// Changes <paramref name="user"/> password.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="currentPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        Task<IdentityResult> ChangePasswordAsync(MilvaUser user, string currentPassword, string newPassword);

        /// <summary>
        /// Changes <paramref name="user"/> email.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newEmail"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IdentityResult> ChangeEmailAsync(MilvaUser user, string newEmail, string token);

        /// <summary>
        /// Changes <paramref name="user"/> phone number.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newPhoneNumber"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IdentityResult> ChangePhoneNumberAsync(MilvaUser user, string newPhoneNumber, string token);


        #region Helper Methods

        /// <summary>
        /// If <paramref name="identityResult"/> is not succeeded throwns <see cref="MilvaUserFriendlyException"/>.
        /// </summary>
        /// <param name="identityResult"></param>
        void ThrowErrorMessagesIfNotSuccess(IdentityResult identityResult);

        /// <summary>
        /// Validating user to login.
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<(MilvaUser tUser, LoginResultDTO loginResult)> ValidateUser(ILoginDTO loginDTO, MilvaUser user);

        /// <summary>
        /// Roll is added according to user type and token is produced.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="tokenExpiredDate"></param>
        /// <returns></returns>
        Task<string> GenerateTokenWithRoleAsync(MilvaUser user, DateTime tokenExpiredDate);

        /// <summary>
        /// If Authentication is successful, JWT tokens are generated.
        /// </summary>
        string GenerateToken(string username, IList<string> roles, DateTime tokenExpiredDate);

        /// <summary>
        /// Decode Process of Token returns the time inside by Decoding the Token
        /// </summary>
        DateTime? ValidateTokenGetExpiredTime(string token);

        /// <summary>
        /// Returns Claims in token for token decode
        /// </summary>
        ClaimsPrincipal GetPrincipalForAccessToken(string token);

        #endregion
    }
}
