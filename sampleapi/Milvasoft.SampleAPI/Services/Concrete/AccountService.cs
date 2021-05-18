using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Identity.Abstract;
using Milvasoft.Helpers.Identity.Concrete;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs.AccountDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Localization;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Concrete
{
    /// <summary>
    /// Provides sign-in,sign-up and sign-out process for user.
    /// </summary>
    public class AccountService : IdentityOperations<UserManager<AppUser>, EducationAppDbContext, IStringLocalizer<SharedResource>, AppUser, AppRole, Guid, LoginResultDTO>, IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IContextRepository<EducationAppDbContext> _contextRepository;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TokenManagement _tokenManagement;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;
        private readonly IBaseRepository<Student, Guid, EducationAppDbContext> _studentRepository;
        private readonly IBaseRepository<Mentor, Guid, EducationAppDbContext> _mentorRepository;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="tokenManagement"></param>
        /// <param name="httpClient"></param>
        /// <param name="studentRepository"></param>
        /// <param name="contextRepository"></param>
        /// <param name="mentorRepository"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="sharedLocalizer"></param>
        public AccountService(UserManager<AppUser> userManager,
                              SignInManager<AppUser> signInManager,
                              TokenManagement tokenManagement,
                              HttpClient httpClient,
                              IBaseRepository<Student, Guid, EducationAppDbContext> studentRepository,
                              IContextRepository<EducationAppDbContext> contextRepository,
                              IBaseRepository<Mentor, Guid, EducationAppDbContext> mentorRepository,
                              IHttpContextAccessor httpContextAccessor,
                              IStringLocalizer<SharedResource> sharedLocalizer) : base(userManager, signInManager, tokenManagement, contextRepository, sharedLocalizer, httpContextAccessor, true)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpClient = httpClient;
            _studentRepository = studentRepository;
            _contextRepository = contextRepository;
            _mentorRepository = mentorRepository;
            _httpContextAccessor = httpContextAccessor;
            _sharedLocalizer = sharedLocalizer;
            _tokenManagement = tokenManagement;
        }

        /// <summary>
        /// Signs in for incoming user. Returns a token if login informations are valid or the user is not lockedout. Otherwise returns the error list.
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <param name="isMentor"></param>
        /// <returns></returns>
        public async Task<LoginResultDTO> SignInAsync(LoginDTO loginDTO, bool isMentor) => await base.LoginAsync(loginDTO,
                                                                                                                isMentor,
                                                                                                                userValidationByUserType: ValidateUser,
                                                                                                                tokenExpiredDate: isMentor ? DateTime.Now.AddDays(100) : DateTime.Now.AddDays(5)).ConfigureAwait(false);

        /// <summary>
        /// Signs out from database. Returns null if already signed out.
        /// </summary>
        /// <returns></returns>
        public async Task<IdentityResult> SignOutAsync() => await base.LogoutAsync().ConfigureAwait(false);

        /// <summary>
        /// Chamge user password.
        /// </summary>
        /// <param name="passDTO"></param>
        /// <returns></returns>
        public async Task<IdentityResult> ChangePasswordAsync(ChangePassDTO passDTO)
        {
            if (passDTO.OldPassword == passDTO.NewPassword) throw new MilvaUserFriendlyException("Passwords are already the same");
            var user = await _userManager.FindByNameAsync(passDTO.UserName).ConfigureAwait(false);
            return await base.ChangePasswordAsync(user, passDTO.OldPassword, passDTO.NewPassword);
        }


        /// <summary>
        /// Validating user to login.
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <param name="user"></param>
        /// <param name="isUserType"></param>
        /// <returns></returns>
        private async Task<(AppUser educationUser, LoginResultDTO loginResult)> ValidateUser(ILoginDTO loginDTO, AppUser user, bool isUserType)
        {
            IdentityError GetLockedError(DateTime lockoutEnd)
            {
                var remainingLockoutEnd = lockoutEnd - DateTime.Now;

                var reminingLockoutEndString = remainingLockoutEnd.Hours > GlobalConstants.Zero
                                                ? _sharedLocalizer["Hours", remainingLockoutEnd.Hours]
                                                : remainingLockoutEnd.Minutes > GlobalConstants.Zero
                                                     ? _sharedLocalizer["Minutes", remainingLockoutEnd.Minutes]
                                                     : _sharedLocalizer["Seconds", remainingLockoutEnd.Seconds];

                return new IdentityError { Code = "Locked", Description = _sharedLocalizer["Locked", reminingLockoutEndString] };
            }

            int accessFailedCountLimit = 5;

            var loginResult = new LoginResultDTO { ErrorMessages = new List<IdentityError>() };

            if (loginDTO.UserName == null && loginDTO.Email == null)
                throw new MilvaUserFriendlyException("PleaseEnterEmailOrUsername");

            var userIdentifier = "";

            #region User Validation

            var userNotFound = true;

            if (loginDTO.UserName != null)
            {
                user = _userManager.FindByNameAsync(userName: loginDTO.UserName).Result;

                userNotFound = user == null;

                if (userNotFound && isUserType)
                {
                    loginResult.ErrorMessages.Add(new IdentityError { Code = "InvalidUserName", Description = _sharedLocalizer["InvalidUserName"] });
                    return (user, loginResult);
                }

                userIdentifier = user.UserName;
            }

            if (loginDTO.Email != null && userNotFound)
            {
                user = _userManager.FindByEmailAsync(email: loginDTO.Email).Result;

                userNotFound = user == null;

                if (userNotFound && isUserType)
                {
                    loginResult.ErrorMessages.Add(new IdentityError { Code = "InvalidEmail", Description = _sharedLocalizer["InvalidEmail"] });
                    return (user, loginResult);
                }

                userIdentifier = user.Email;
            }

            if (userNotFound)
            {
                loginResult.ErrorMessages.Add(new IdentityError { Code = "InvalidLogin", Description = _sharedLocalizer["InvalidLogin"] });
                return (user, loginResult);
            }

            var userId = user.Id;

            var isMentorInDb = (await _mentorRepository.GetAllAsync(i => i.AppUserId.Equals(userId))).Any();

            var isStudentInDb = (await _studentRepository.GetAllAsync(i => i.AppUserId.Equals(userId))).Any();

            var confirmedUser = isUserType == isMentorInDb || isStudentInDb;

            if (!confirmedUser)
            {
                loginResult.ErrorMessages.Add(new IdentityError { Code = "InvalidLogin", Description = _sharedLocalizer["InvalidLogin"] });
                return (user, loginResult);
            }

            var userLocked = _userManager.IsLockedOutAsync(user).Result;

            if (userLocked && DateTime.Now > user.LockoutEnd.Value.DateTime)
            {
                _contextRepository.InitializeUpdating<AppUser, Guid>(user);

                await _userManager.SetLockoutEndDateAsync(user, null).ConfigureAwait(false);

                await _userManager.ResetAccessFailedCountAsync(user).ConfigureAwait(false);

                userLocked = false;
            }

            if (userLocked)
            {
                loginResult.ErrorMessages.Add(GetLockedError(user.LockoutEnd.Value.DateTime));
                return (user, loginResult);
            }

            var passIsTrue = _userManager.CheckPasswordAsync(user, loginDTO.Password).Result;


            if (!passIsTrue)
            {
                await _userManager.AccessFailedAsync(user).ConfigureAwait(false);

                if (_userManager.IsLockedOutAsync(user).Result)
                {
                    loginResult.ErrorMessages.Add(GetLockedError(user.LockoutEnd.Value.DateTime));
                    return (user, loginResult);
                }

                var senstiveMessage = isUserType ? _sharedLocalizer["InvalidPassword"] : _sharedLocalizer["InvalidLogin"];

                var lockWarningMessage = _sharedLocalizer["LockWarning", accessFailedCountLimit - user.AccessFailedCount];

                loginResult.ErrorMessages.Add(new IdentityError { Code = "InvalidLogin", Description = $"{senstiveMessage} {lockWarningMessage}" });

                return (user, loginResult);
            }

            return (user, loginResult);

            #endregion
        }

    }
}
