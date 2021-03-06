﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using Milvasoft.Helpers.Identity.Concrete;
using System;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Identity.Abstract
{
    /// <summary>
    /// Provides Identity operations like sign in, sign out.
    /// </summary>
    /// <typeparam name="TUserManager"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    /// <typeparam name="TLocalizer"></typeparam>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TRole"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TLoginResultDTO"></typeparam>
    public interface IIdentityOperations<TUserManager, TDbContext, TLocalizer, TUser, TRole, TKey, TLoginResultDTO>
       where TKey : struct, IEquatable<TKey>
       where TUser : IdentityUser<TKey>, IBaseEntity<TKey>, new()
       where TRole : IdentityRole<TKey>
       where TDbContext : IdentityDbContext<TUser, TRole, TKey>
       where TUserManager : UserManager<TUser>
       where TLoginResultDTO : class, ILoginResultDTO<MilvaToken>, new()
       where TLocalizer : IStringLocalizer
    {
        #region Properties

        /// <summary>
        /// Delegate for user validation.
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public delegate Task<(TUser tUser, TLoginResultDTO loginResult)> UserValidation(ILoginDTO loginDTO, TUser user);

        /// <summary>
        /// Delegate for user validation.
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <param name="user"></param>
        /// <param name="isUserType"></param>
        /// <returns></returns>
        public delegate Task<(TUser tUser, TLoginResultDTO loginResult)> UserValidationByUserType(ILoginDTO loginDTO, TUser user, bool isUserType);

        #endregion

        /// <summary>
        /// Signs in for incoming user. Returns a token if login informations are valid or the user is not lockedout. Otherwise returns the error list.
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <param name="userValidation"></param>
        /// <param name="tokenExpiredDate"></param>
        /// <returns></returns>
        Task<TLoginResultDTO> LoginAsync(ILoginDTO loginDTO, UserValidation userValidation, DateTime tokenExpiredDate);

        /// <summary>
        /// Signs in for incoming user. Returns a token if login informations are valid or the user is not lockedout. Otherwise returns the error list.
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <param name="isUserType"></param>
        /// <param name="userValidationByUserType"></param>
        /// <param name="tokenExpiredDate"></param>
        /// <returns></returns>
        Task<TLoginResultDTO> LoginAsync(ILoginDTO loginDTO, bool isUserType, UserValidationByUserType userValidationByUserType, DateTime tokenExpiredDate);

        /// <summary>
        /// Signs out from database. Returns null if already signed out.
        /// </summary>
        /// <returns></returns>
        Task<IdentityResult> LogoutAsync();

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
        Task<IdentityResult> ChangePasswordAsync(TUser user, string currentPassword, string newPassword);

        /// <summary>
        /// Changes <paramref name="user"/> email.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newEmail"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IdentityResult> ChangeEmailAsync(TUser user, string newEmail, string token);

        /// <summary>
        /// Changes <paramref name="user"/> phone number.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newPhoneNumber"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IdentityResult> ChangePhoneNumberAsync(TUser user, string newPhoneNumber, string token);
    }
}
