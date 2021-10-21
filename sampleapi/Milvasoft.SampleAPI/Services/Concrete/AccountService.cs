using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Milvasoft.Helpers;
using Milvasoft.Helpers.Caching;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DependencyInjection;
using Milvasoft.Helpers.Enums;
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
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Concrete
{
    /// <summary>
    /// Provides sign-in,sign-up and sign-out process for user.
    /// </summary>
    public class AccountService : IdentityOperations<UserManager<AppUser>, EducationAppDbContext, IStringLocalizer<SharedResource>, AppUser, AppRole, Guid, LoginResultDTO>, IAccountService
    {
        private enum AccountActivity
        {
            EmailVerification,
            EmailChange,
            PasswordReset,
            PhoneNumberChange
        }

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IContextRepository<EducationAppDbContext> _contextRepository;
        private readonly HttpClient _httpClient;
        private readonly IMilvaMailSender _milvaMailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenManagement _tokenManagement;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IBaseRepository<AppUser, Guid, EducationAppDbContext> _userRepository;
        private readonly IMilvaLogger _milvaLogger;
        private readonly string _userName;
        private readonly IRedisCacheService _redisCacheService;
        private readonly DateTime _tokenExpireDate = DateTime.Now.AddDays(1);

        /// <summary>
        /// The authentication scheme for the provider the token is associated with.
        /// </summary>
        private readonly string _loginProvider;

        /// <summary>
        /// The name of token.
        /// </summary>
        private readonly string _tokenName;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="tokenManagement"></param>
        /// <param name="httpClient"></param>
        /// <param name="contextRepository"></param>
        /// <param name="userRepository"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="milvaMailSender"></param>
        /// <param name="sharedLocalizer"></param>
        /// <param name="milvaLogger"></param>
        /// <param name="redisCacheService"></param>
        public AccountService(UserManager<AppUser> userManager,
                              SignInManager<AppUser> signInManager,
                              ITokenManagement tokenManagement,
                              HttpClient httpClient,
                              IContextRepository<EducationAppDbContext> contextRepository,
                              IBaseRepository<AppUser, Guid, EducationAppDbContext> userRepository,
                              IHttpContextAccessor httpContextAccessor,
                              IMilvaMailSender milvaMailSender,
                              IStringLocalizer<SharedResource> sharedLocalizer,
                              IMilvaLogger milvaLogger,
                              IRedisCacheService redisCacheService) : base(userManager, signInManager, tokenManagement, contextRepository, sharedLocalizer, httpContextAccessor, true)
        {
            _userName = httpContextAccessor.HttpContext.User.Identity.Name;
            _redisCacheService = redisCacheService;
            _milvaLogger = milvaLogger;
            _milvaMailSender = milvaMailSender;
            _userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _httpClient = httpClient;
            _contextRepository = contextRepository;
            _httpContextAccessor = httpContextAccessor;
            _localizer = sharedLocalizer;
            _tokenManagement = tokenManagement;
            _loginProvider = tokenManagement.LoginProvider;
            _tokenName = tokenManagement.TokenName;
        }

        /// <summary>
        /// Signs in for incoming user. 
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <param name="isMentor"></param>
        /// <returns>Returns a token if the login information is correct.Otherwise returns the error list.</returns>
        public async Task<LoginResultDTO> LoginAsync(LoginDTO loginDTO, bool isMentor) => await base.LoginAsync(loginDTO,
                                                                                                                isMentor,
                                                                                                                userValidationByUserType: ValidateUserAsync,
                                                                                                                tokenExpiredDate: isMentor ? DateTime.Now.AddDays(100) : DateTime.Now.AddDays(5)).ConfigureAwait(false);

        /// <summary>
        /// Change user password.
        /// </summary>
        /// <param name="passDTO"></param>
        /// <returns></returns>
        public async Task<IdentityResult> ChangePasswordAsync(ChangePassDTO passDTO)
        {
            if (passDTO.OldPassword == passDTO.NewPassword) throw new MilvaUserFriendlyException("PasswordSameException");

            var user = await _userManager.FindByNameAsync(passDTO.UserName).ConfigureAwait(false);

            return await base.ChangePasswordAsync(user, passDTO.OldPassword, passDTO.NewPassword);
        }

        /// <summary>
        /// Gets a specific personnel data from repository by token value if exsist.
        /// </summary>
        /// <returns> Logged-in user data. </returns>
        public async Task<AppUserDTO> GetLoggedInInUserInformationAsync()
        {
            CheckLoginStatus();

            var user = await _userRepository.GetFirstOrDefaultAsync(a => a.UserName == _userName).ConfigureAwait(false);

            user.ThrowIfNullForGuidObject("CannotGetSignedInUserInfo");

            return new AppUserDTO
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }

        /// <summary>
        /// Updates logged-in user's personal information.
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        public async Task UpdateAccountAsync(AppUserUpdateDTO userDTO)
        {
            CheckLoginStatus();

            var toBeUpdatedUser = await _userRepository.GetFirstOrDefaultAsync(a => a.UserName == _userName).ConfigureAwait(false);

            bool initializeUpdate = false;

            if (!string.IsNullOrWhiteSpace(userDTO.PhoneNumber))
            {
                toBeUpdatedUser.PhoneNumber = userDTO.PhoneNumber;
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
        /// Sign up process for application user.
        /// If signup process is succesful,then sign in.
        /// </summary>
        /// <param name="registerDTO"></param>
        /// <returns></returns>
        public async Task<LoginResultDTO> RegisterAsync(RegisterDTO registerDTO)
        {
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

                loginResult = await LoginAsync(loginDTO, true).ConfigureAwait(false);
            }
            else
            {
                loginResult.ErrorMessages = createResult.Errors.ToList();
            }

            return loginResult;
        }

        /// <summary>
        /// Deletes logged-in user's account. This operation is irreversible.
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAccountAsync()
        {
            CheckLoginStatus();

            var user = await _userRepository.GetFirstOrDefaultAsync(a => a.UserName == _userName).ConfigureAwait(false)
                            ?? throw new MilvaUserFriendlyException(MilvaException.CannotFindEntity);

            var deleteResult = await _userManager.DeleteAsync(user).ConfigureAwait(false);


            if (!deleteResult.Succeeded)
                ThrowErrorMessagesIfNotSuccess(deleteResult);
        }

        #region Account Activities 

        /// <summary>
        /// Sends email verification mail to logged-in user.
        /// </summary>
        /// <returns></returns>
        public async Task SendEmailVerificationMailAsync()
        {
            CheckLoginStatus();

            var mailBodyKeyContentPair = PrepareMailBodyDictionary(_localizer["VerificationMailTitle"],
                                                                   _localizer["VerificationMailBodyTitle"],
                                                                   _localizer["VerificationMailBodyDescription", GlobalConstants.ApplicationSiteUrl],
                                                                   _localizer["VerificationMailBodyButtonText"],
                                                                   _localizer["VerificationMailBodyResendText", GlobalConstants.DeveloperSiteUrl],
                                                                   _localizer["VerificationMailBodyWelcomeText"]);

            await SendActivityMailAsync(mailBodyKeyContentPair, urlPath: "verify", AccountActivity.EmailVerification);
        }

        /// <summary>
        /// Sends phone number change mail to logged-in user.
        /// </summary>
        /// <returns></returns>
        public async Task SendChangePhoneNumberMailAsync(string newPhoneNumber)
        {
            CheckLoginStatus();

            CheckRegex(newPhoneNumber, "PhoneNumber");

            var mailBodyKeyContentPair = PrepareMailBodyDictionary(_localizer["GSMChangeMailTitle"],
                                                                   _localizer["GSMChangeMailBodyTitle"],
                                                                   _localizer["GSMChangeMailBodyDesciption"],
                                                                   _localizer["GSMChangeMailBodyButtonText"],
                                                                   _localizer["GSMChangeMailBodyResendText", GlobalConstants.DeveloperSiteUrl],
                                                                   _localizer["GSMChangeMailBodyWelcomeText"]);

            await SendActivityMailAsync(mailBodyKeyContentPair, urlPath: "change/phoneNumber", AccountActivity.PhoneNumberChange, newPhoneNumber);
        }

        /// <summary>
        /// Sends email chage mail to logged-in user.
        /// </summary>
        /// <returns></returns>
        public async Task SendChangeEmailMailAsync(string newEmail)
        {
            CheckLoginStatus();

            CheckRegex(newEmail, "Email");

            var user = await _userRepository.GetFirstOrDefaultAsync(a => a.UserName == _userName).ConfigureAwait(false);

            //Is there another user with the same email?
            bool mailExist = user != null;

            if (mailExist)
                throw new MilvaUserFriendlyException("IdentityDuplicateEmail");

            var mailBodyKeyContentPair = PrepareMailBodyDictionary(_localizer["EmailChangeMailTitle"],
                                                                   _localizer["EmailChangeMailBodyTitle"],
                                                                   _localizer["EmailChangeMailBodyDesciption"],
                                                                   _localizer["EmailChangeMailBodyButtonText"],
                                                                   _localizer["EmailChangeMailBodyResendText", GlobalConstants.DeveloperSiteUrl],
                                                                   _localizer["EmailChangeMailBodyWelcomeText"]);

            await SendActivityMailAsync(mailBodyKeyContentPair, urlPath: "change/email", AccountActivity.EmailChange, newEmail);
        }

        /// <summary>
        /// Sends password reset mail to logged-in user.
        /// </summary>
        /// <returns></returns>
        public async Task SendResetPasswordMailAsync()
        {
            var mailBodyKeyContentPair = PrepareMailBodyDictionary(_localizer["PasswordResetMailTitle"],
                                                                   _localizer["PasswordResetMailBodyTitle"],
                                                                   _localizer["PasswordResetMailBodyDesciption"],
                                                                   _localizer["PasswordResetMailBodyButtonText"],
                                                                   _localizer["PasswordResetMailBodyResendText", GlobalConstants.DeveloperSiteUrl],
                                                                   _localizer["PasswordResetMailBodyWelcomeText"]);

            await SendActivityMailAsync(mailBodyKeyContentPair, urlPath: "reset/password", AccountActivity.PasswordReset);
        }

        /// <summary>
        /// Sends password reset mail to<paramref name="email"/>.
        /// </summary>
        /// <returns></returns>
        public async Task SendForgotPasswordMailAsync(string email)
        {
            var mailBodyKeyContentPair = PrepareMailBodyDictionary(_localizer["PasswordResetMailTitle"],
                                                                   _localizer["PasswordResetMailBodyTitle"],
                                                                   _localizer["PasswordResetMailBodyDesciption"],
                                                                   _localizer["PasswordResetMailBodyButtonText"],
                                                                   _localizer["PasswordResetMailBodyResendText", GlobalConstants.DeveloperSiteUrl],
                                                                   _localizer["PasswordResetMailBodyWelcomeText"]);

            var user = await _userRepository.GetFirstOrDefaultAsync(a => a.UserName == _userName).ConfigureAwait(false)
                                            ?? throw new MilvaUserFriendlyException(MilvaException.CannotFindEntity);

            await SendActivityMailAsync(mailBodyKeyContentPair, urlPath: "reset/password", AccountActivity.PasswordReset, username: user.UserName);
        }

        /// <summary>
        /// Sends verification code to logged-in user's phone number.
        /// <para><b> IMPORTANT INFORMATION : The message sending service has not yet been integrated. 
        ///                                   So this method will not send message to the user's gsm number.
        ///                                   Instead of returns verification code for testing. </b></para>
        /// </summary>
        /// <returns></returns>
        public async Task<string> SendPhoneNumberVerificationMessageAsync()
        {
            CheckLoginStatus();

            var user = await _userRepository.GetFirstOrDefaultAsync(a => a.UserName == _userName).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(user?.PhoneNumber))
                throw new MilvaUserFriendlyException("IdentityInvalidPhoneNumber");

            var verificationCode = GenerateVerificationCode();

            if (!_redisCacheService.IsConnected())
            {
                try
                {
                    await _redisCacheService.ConnectAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    _ = _milvaLogger.LogFatalAsync("Redis is not available!!", MailSubject.ShutDown);
                    throw new MilvaUserFriendlyException("CannotSendMessageNow");
                }
            }

            await _redisCacheService.SetAsync($"pvc_{_userName}", verificationCode, TimeSpan.FromMinutes(3)).ConfigureAwait(false);

            //Doğrulama kodunu mesaj olarak gönderme entegrasyonu buraya eklenecek.
            //O yüzden şimdilik geriye dönüyoruz bu kodu.

            return verificationCode;
        }

        /// <summary>
        /// Verifies email, if <paramref name="verificationCode"/> is correct.
        /// </summary>
        /// <param name="verificationCode"></param>
        /// <returns></returns>
        public async Task<IdentityResult> VerifyPhoneNumberAsync(string verificationCode)
        {
            CheckLoginStatus();

            var user = await _userRepository.GetFirstOrDefaultAsync(a => a.UserName == _userName).ConfigureAwait(false);

            user.ThrowIfParameterIsNull("IdentityInvalidUserName");

            await _redisCacheService.ConnectAsync().ConfigureAwait(false);

            var cacheKey = $"pvc_{user.UserName}";

            if (!(await _redisCacheService.KeyExistsAsync(cacheKey)))
                throw new MilvaUserFriendlyException("ThereIsNoSavedVerificationCode");

            var verificationCodeInCache = await _redisCacheService.GetAsync(cacheKey).ConfigureAwait(false);

            if (verificationCode == verificationCodeInCache)
            {
                user.PhoneNumberConfirmed = true;

                return await _userManager.UpdateAsync(user).ConfigureAwait(false);
            }
            else throw new MilvaUserFriendlyException("WrongPhoneNumberVerificationCode");

        }

        /// <summary>
        /// Verifies <paramref name="emailVerificationDTO"/>.UserName's email, if <paramref name="emailVerificationDTO"/>.TokenString is valid.
        /// </summary>
        /// <param name="emailVerificationDTO"></param>
        /// <returns></returns>
        public async Task<IdentityResult> VerifyEmailAsync(EmailVerificationDTO emailVerificationDTO)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(a => a.UserName == _userName).ConfigureAwait(false);

            user.ThrowIfParameterIsNull("InvalidVerificationToken");


            var result = await _userManager.ConfirmEmailAsync(user, emailVerificationDTO.TokenString).ConfigureAwait(false);

            result.ThrowErrorMessagesIfNotSuccess();

            return await _userManager.ConfirmEmailAsync(user, emailVerificationDTO.TokenString).ConfigureAwait(false);
        }

        /// <summary>
        /// Changes <paramref name="phoneNumberChangeDTO"/>.UserName's email 
        /// with <paramref name="phoneNumberChangeDTO"/>.NewPhoneNumber, if <paramref name="phoneNumberChangeDTO"/>.TokenString is valid.
        /// </summary>
        /// <param name="phoneNumberChangeDTO"></param>
        /// <returns></returns>
        public async Task<IdentityResult> ChangePhoneNumberAsync(PhoneNumberChangeDTO phoneNumberChangeDTO)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(a => a.UserName == _userName).ConfigureAwait(false);

            user.ThrowIfParameterIsNull("InvalidVerificationToken");

            return await _userManager.ChangePhoneNumberAsync(user, phoneNumberChangeDTO.NewPhoneNumber, phoneNumberChangeDTO.TokenString).ConfigureAwait(false);
        }

        /// <summary>
        /// Resets <paramref name="passwordResetDTO"/>.UserName's password with <paramref name="passwordResetDTO"/>.NewPassword, if <paramref name="passwordResetDTO"/>.TokenString is valid.
        /// </summary>
        /// <param name="passwordResetDTO"></param>
        /// <returns></returns>
        public async Task<IdentityResult> ResetPasswordAsync(PasswordResetDTO passwordResetDTO)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(a => a.UserName == _userName).ConfigureAwait(false);

            user.ThrowIfParameterIsNull("IdentityInvalidUserName");

            return await _userManager.ResetPasswordAsync(user, passwordResetDTO.TokenString, passwordResetDTO.NewPassword).ConfigureAwait(false);
        }

        #endregion

        #region Private Helpers Methods

        /// <summary>
        /// Roll is added according to user type and token is produced.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isAppUser"></param>
        /// <returns></returns>
        private async Task<string> GenerateTokenWithRoleAsync(AppUser user, bool isAppUser)
        {
            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

            string newToken = GenerateToken(username: user.UserName, roles: roles, isAppUser);

            await _userManager.RemoveAuthenticationTokenAsync(user, _loginProvider, _tokenName).ConfigureAwait(false);

            IdentityResult identityResult = await _userManager.SetAuthenticationTokenAsync(user: user,
                                                                                           loginProvider: _loginProvider,//Token nerede kullanılcak
                                                                                           tokenName: _tokenName,//Token tipi
                                                                                           tokenValue: newToken).ConfigureAwait(false);

            if (!identityResult.Succeeded)
                throw new MilvaUserFriendlyException();

            return newToken;
        }

        /// <summary>
        /// If Authentication is successful, JWT tokens are generated.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="roles"></param>
        /// <param name="isAppUser"></param>
        private string GenerateToken(string username, IList<string> roles, bool isAppUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var claimsIdentityList = new ClaimsIdentity(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            claimsIdentityList.AddClaim(new Claim(ClaimTypes.Name, username));

            if (!isAppUser)
                claimsIdentityList.AddClaim(new Claim(ClaimTypes.Expired, value: _tokenExpireDate.ToString()));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentityList,
                Issuer = _tokenManagement.Issuer,
                Audience = _tokenManagement.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_tokenManagement.Secret)), SecurityAlgorithms.HmacSha256Signature)
            };

            if (!isAppUser)
                tokenDescriptor.Expires = _tokenExpireDate;

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generates 6-digit verification code.
        /// </summary>
        /// <returns></returns>
        private static string GenerateVerificationCode()
        {
            Random rand = new();

            List<int> codeList = new();

            string verificationCode = "";

            for (int index = 0; index < 6; index++)
            {
                codeList.Add(rand.Next(1, 9));

                verificationCode += codeList.ElementAt(index).ToString();
            }
            return verificationCode;
        }

        /// <summary>
        /// <para> Please add items to <paramref name="values"/> with this sorting; </para>
        ///          <para> - Mail Title         </para>
        ///          <para> - Body Title         </para>
        ///          <para> - Body Description   </para>
        ///          <para> - Body Button Text   </para>
        ///          <para> - Body Resend Text   </para>
        ///          <para> - Body Bottom Text   </para>
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private static Dictionary<string, string> PrepareMailBodyDictionary(params string[] values)
        {
            var dic = new Dictionary<string, string>
            {
                { "~MailTitle", "" },
                { "~BodyTitle", "" },
                { "~BodyDescription", "" },
                { "~BodyButtonText", "" },
                { "~BodyResendText", "" },
                { "~BodyBottomText", "" },
            };

            int i = 0;
            foreach (var item in dic)
            {
                dic[item.Key] = values[i];
                i++;
            }

            return dic;
        }

        /// <summary>
        /// Sends email to logged-in user's email.
        /// Please make sure <paramref name="localizedMailBodyContents"/> dictionary parameter taken from <see cref="PrepareMailBodyDictionary(string[])"/>.
        /// </summary>
        /// <param name="localizedMailBodyContents"></param>
        /// <param name="urlPath"></param>
        /// <param name="accountActivity"></param>
        /// <param name="newInfo"> Could be new phone number or new email. </param>
        /// <param name="username"></param>
        /// <returns></returns>
        private async Task SendActivityMailAsync(Dictionary<string, string> localizedMailBodyContents,
                                                 string urlPath,
                                                 AccountActivity accountActivity,
                                                 string newInfo = null,
                                                 string username = null)
        {
            var uName = username ?? _userName;

            var user = await _userRepository.GetFirstOrDefaultAsync(a => a.UserName == _userName).ConfigureAwait(false)
                                             ?? throw new MilvaUserFriendlyException(MilvaException.CannotFindEntity);

            if (string.IsNullOrWhiteSpace(user?.Email))
                throw new MilvaUserFriendlyException("IdentityInvalidEmail");

            string token = "";

            switch (accountActivity)
            {
                case AccountActivity.EmailVerification:
                    token = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
                    break;
                case AccountActivity.EmailChange:
                    token = await _userManager.GenerateChangeEmailTokenAsync(user, newInfo).ConfigureAwait(false);
                    break;
                case AccountActivity.PasswordReset:
                    token = await _userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
                    break;
                case AccountActivity.PhoneNumberChange:
                    token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, newInfo).ConfigureAwait(false);
                    break;
            }

            var confirmationUrl = $"{GlobalConstants.ApplicationSiteUrl}/{urlPath}?userName={username ?? _userName}&token={token}";

            var htmlContent = await File.ReadAllTextAsync(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "HTML", "mail_content.html")).ConfigureAwait(false);

            foreach (var localizedMailBodyContent in localizedMailBodyContents)
                htmlContent = htmlContent.Replace(localizedMailBodyContent.Key, localizedMailBodyContent.Value);

            htmlContent = htmlContent.Replace("~BodyButtonLink", confirmationUrl);

            await _milvaMailSender.MilvaSendMailAsync(user.Email, localizedMailBodyContents["~MailTitle"], htmlContent, true);
        }

        /// <summary>
        /// Regex check for action parameter.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="propName"></param>
        private void CheckRegex(string input, string propName)
        {
            var localizedPattern = _localizer[$"RegexPattern{propName}"];

            if (!RegexMatcher.MatchRegex(input, _localizer[localizedPattern]))
            {
                var exampleFormat = _localizer[$"RegexExample{propName}"];
                throw new MilvaUserFriendlyException("RegexErrorMessage", _localizer[$"Localized{propName}"], exampleFormat);
            }
        }
        private async Task<(AppUser educationUser, LoginResultDTO loginResult)> ValidateUserAsync(ILoginDTO loginDTO, AppUser user, bool isUserType) => await base.ValidateUserAsync(loginDTO, user).ConfigureAwait(false);

        /// <summary>
        /// Cheks <see cref="_userName"/>. If is null or empty throwns <see cref="MilvaUserFriendlyException"/>. Otherwise does nothing.
        /// </summary>
        private void CheckLoginStatus()
        {
            if (string.IsNullOrWhiteSpace(_userName))
                throw new MilvaUserFriendlyException("CannotGetSignedInUserInfo");
        }

        /// <summary>
        /// If <paramref name="identityResult"/> is not succeeded throwns <see cref="MilvaUserFriendlyException"/>.
        /// </summary>
        /// <param name="identityResult"></param>
        private void ThrowErrorMessagesIfNotSuccess(IdentityResult identityResult)
        {
            if (!identityResult.Succeeded)
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.AppendJoin(',', identityResult.Errors.Select(i => i.Description));
                throw new MilvaUserFriendlyException(stringBuilder.ToString());
            }
        }

        #endregion
    }
}
