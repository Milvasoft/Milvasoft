using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Identity.Abstract;
using Milvasoft.Helpers.Identity.Concrete;
using Milvasoft.Helpers.Mail;
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
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Concrete
{
    /// <summary>
    /// Provides sign-in,sign-up and sign-out process for user.
    /// </summary>
    public class AccountService : IdentityOperations<UserManager<AppUser>, EducationAppDbContext, IStringLocalizer<SharedResource>, AppUser, AppRole, Guid, LoginResultDTO>, IAccountService
    {
        #region Fields
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;
        private readonly IContextRepository<EducationAppDbContext> _contextRepository;
        private readonly IBaseRepository<Student, Guid, EducationAppDbContext> _studentRepository;
        private readonly IBaseRepository<Mentor, Guid, EducationAppDbContext> _mentorRepository;
        private readonly IBaseRepository<AppUser, Guid, EducationAppDbContext> _userRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenManagement _tokenManagement;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _userName;
        private readonly IMilvaMailSender _milvaMailSender;
        private readonly DateTime _tokenExpireDate = DateTime.Now.AddDays(1);

        /// <summary>
        /// The authentication scheme for the provider the token is associated with.
        /// </summary>
        private readonly string _loginProvider;

        /// <summary>
        /// The name of token.
        /// </summary>
        private readonly string _tokenName;
        #endregion

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="userRepository"></param>
        /// <param name="studentRepository"></param>
        /// <param name="mentorRepository"></param>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="tokenManagement"></param>
        /// <param name="localizer"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="milvaMailSender"></param>
        /// <param name="contextRepository"></param>
        /// <param name="sharedLocalizer"></param>
        public AccountService(IBaseRepository<AppUser, Guid, EducationAppDbContext> userRepository,
                              IBaseRepository<Student,Guid,EducationAppDbContext> studentRepository,
                              IBaseRepository<Mentor,Guid,EducationAppDbContext> mentorRepository,
                              UserManager<AppUser> userManager,
                              SignInManager<AppUser> signInManager,
                              TokenManagement tokenManagement,
                              IStringLocalizer<SharedResource> localizer,
                              IHttpContextAccessor httpContextAccessor,
                              IMilvaMailSender milvaMailSender,
                              IContextRepository<EducationAppDbContext> contextRepository,
                              IStringLocalizer<SharedResource> sharedLocalizer): base(userManager, signInManager, tokenManagement, contextRepository, sharedLocalizer, httpContextAccessor, true)
        {
            _studentRepository = studentRepository;
            _mentorRepository = mentorRepository;
            _contextRepository = contextRepository;
            _sharedLocalizer = sharedLocalizer;
            _userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenManagement = tokenManagement;
            _localizer = localizer;
            _milvaMailSender = milvaMailSender;
            _userName = httpContextAccessor.HttpContext.User.Identity.Name;
            _loginProvider = tokenManagement.LoginProvider;
            _tokenName = tokenManagement.TokenName;
        }

        #region AppUser
        /// <summary>
        /// Signs in for incoming user. Returns a token if login informations are valid or the user is not lockedout. Otherwise returns the error list.
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <param name="isMentor"></param>
        /// <returns></returns>
        public async Task<LoginResultDTO> LoginAsync(LoginDTO loginDTO, bool isMentor) => await base.LoginAsync(loginDTO,
                                                                                                                isMentor,
                                                                                                                userValidationByUserType: ValidateUser,
                                                                                                                tokenExpiredDate: isMentor ? DateTime.Now.AddDays(100) : DateTime.Now.AddDays(5)).ConfigureAwait(false);

        /// <summary>
        /// Signs out from database. Returns null if already signed out.
        /// </summary>
        /// <returns></returns>
        public async Task<IdentityResult> LoggoutAsync() => await base.LogoutAsync().ConfigureAwait(false);

        /// <summary>
        /// Gets a specific personnel data from repository by token value if exsist.
        /// </summary>
        /// <returns> Logged-in user data. </returns>
        public async Task<AppUserDTO> GetLoggedInInUserInformationAsync()
        {
            CheckLoginStatus();

            var user = await _userRepository.GetFirstOrDefaultAsync(a=>a.UserName==_userName).ConfigureAwait(false);

            user.ThrowIfNullForGuidObject("CannotGetSignedInUserInfo");

            return new AppUserDTO
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }

        /// <summary>
        /// Sign up process for application user.
        /// If signup process is succesful,then sign in.
        /// </summary>
        /// <param name="registerDTO"></param>
        /// <returns></returns>
        public async Task<LoginResultDTO> RegisterAsync(SignUpDTO registerDTO)
        {
            if (string.IsNullOrEmpty(registerDTO.Token))
                throw new MilvaDeveloperException("Please enter the notification info.");

            AppUser userToBeSignUp = new()
            {
                UserName = registerDTO.UserName,
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.PhoneNumber
            };

            LoginResultDTO loginResult = new();

            var createResult = await _userManager.CreateAsync(userToBeSignUp, registerDTO.Password);

            if (createResult.Succeeded)
            {
                LoginDTO loginDTO = new();
                loginDTO.UserName = userToBeSignUp.UserName;
                loginDTO.Password = registerDTO.Password;

                loginResult = await LoginAsync(loginDTO,true).ConfigureAwait(false);
            }
            else
            {
                loginResult.ErrorMessages = createResult.Errors.ToList();
            }

            return loginResult;
        }

        /// <summary>
        /// Updates logged-in user's personal information.
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        public async Task UpdateAccountAsync(AppUserUpdateDTO userDTO)
        {
            CheckLoginStatus();

            var toBeUpdatedUser = await _userRepository.GetFirstOrDefaultAsync(a=>a.UserName==_userName).ConfigureAwait(false);

            bool initializeUpdate = false;

            //TODO OGUZHAN Userin mentör veya öğrenci olduğuna bakılarak isim ve soyisim değiştirilebilecek.

            if (!string.IsNullOrEmpty(userDTO.PhoneNumber))
            {
                toBeUpdatedUser.PhoneNumber = userDTO.NewPhoneNumber;
                toBeUpdatedUser.PhoneNumberConfirmed = false;
                initializeUpdate = true;
            }


            if (initializeUpdate)
            {
                toBeUpdatedUser.LastModificationDate = DateTime.Now;

                var updateResult = await _userManager.UpdateAsync(toBeUpdatedUser).ConfigureAwait(false);

                ThrowErrorMessagesIfNotSuccess(updateResult);
            }
        }

        /// <summary>
        /// Deletes logged-in user's account. This operation is irreversible.
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAccountAsync()
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(a => a.UserName == _userName).ConfigureAwait(false);

            user.ThrowIfNullForGuidObject("CannotGetSignedInUserInfo");

            await base.DeleteUserAsync(user.Id).ConfigureAwait(false);//TODO OGUZHAN İlgili userin ilişkilerini veritabanından sil.
        }

        #endregion


        /// <summary>
        /// Validating user to login.
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <param name="user"></param>
        /// <param name="isUserType"></param>
        /// <returns></returns>
        private async Task<(AppUser educationUser, LoginResultDTO loginResult)> ValidateUser(ILoginDTO loginDTO, AppUser user, bool isUserType) => await base.ValidateUserAsync(loginDTO, user).ConfigureAwait(false);

        #region Account Activities

        /// <summary>
        /// Change user password with old password and new password.
        /// </summary>
        /// <param name="changeDTO"></param>
        /// <returns></returns>
        public async Task<IdentityResult> ChangePasswordAsync(ChangePassDTO changeDTO)
        {
            CheckLoginStatus();

            var user = await _userRepository.GetFirstOrDefaultAsync(a => a.UserName == _userName).ConfigureAwait(false);

            user.ThrowIfNullForGuidObject("IdentityInvalidUserName");

            return await base.ChangePasswordAsync(user, changeDTO.OldPassword, changeDTO.NewPassword);
        }

        #endregion

        #region Private Helpers Methods
        /// <summary>
        /// If <paramref name="identityResult"/> is not succeeded throwns <see cref="MilvaUserFriendlyException"/>.
        /// </summary>
        /// <param name="identityResult"></param>
        public void ThrowErrorMessagesIfNotSuccess(IdentityResult identityResult)
        {
            if (!identityResult.Succeeded)
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.AppendJoin(',', identityResult.Errors.Select(i => i.Description));
                throw new MilvaUserFriendlyException(stringBuilder.ToString());
            }
        }

        /// <summary>
        /// Cheks <see cref="_userName"/>. If is null or empty throwns <see cref="MilvaUserFriendlyException"/>. Otherwise does nothing.
        /// </summary>
        private void CheckLoginStatus()
        {
            if (string.IsNullOrEmpty(_userName))
                throw new MilvaUserFriendlyException("CannotGetSignedInUserInfo");
        }

        #endregion
        

    }
}
