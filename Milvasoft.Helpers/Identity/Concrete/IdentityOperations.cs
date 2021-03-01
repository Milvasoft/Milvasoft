using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Identity.Abstract;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Identity.Concrete
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
    public class IdentityOperations<TUserManager, TDbContext, TLocalizer, TUser, TRole, TKey, TLoginResultDTO> : IIdentityOperations<TUserManager, TDbContext, TLocalizer, TUser, TRole, TKey, TLoginResultDTO>
       where TUser : IdentityUser<TKey>, IFullAuditable<TKey>,new()
       where TRole : IdentityRole<TKey>
       where TKey : struct, IEquatable<TKey>
       where TDbContext : IdentityDbContext<TUser, TRole, TKey>
       where TUserManager : UserManager<TUser>
       where TLoginResultDTO : class, ILoginResultDTO, new()
       where TLocalizer : IStringLocalizer
    {

        #region Fields

        private readonly TUserManager _userManager;
        private readonly SignInManager<TUser> _signInManager;
        private readonly ITokenManagement _tokenManagement;
        private readonly IContextRepository<TDbContext> _contextRepository;
        private readonly TLocalizer _localizer;
        private readonly string _userName;

        /// <summary>
        /// Gets or sets GetSoftDeletedEntities. Default is false.
        /// If value is true, all methods returns all entities.
        /// If value is false, all methods returns entities which only "IsDeleted" property's value is false.
        /// </summary>
        private bool _useWhiteList
        {
            get => IIdentityOperations<TUserManager, TDbContext, TLocalizer, TUser, TRole, TKey, TLoginResultDTO>.UseWhiteList;
            set { IIdentityOperations<TUserManager, TDbContext, TLocalizer, TUser, TRole, TKey, TLoginResultDTO>.UseWhiteList = value; }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The authentication scheme for the provider the token is associated with.
        /// </summary>
        public string LoginProvider { get; set; }

        /// <summary>
        /// The name of token.
        /// </summary>
        public string TokenName { get; set; }

        #endregion


        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="tokenManagement"></param>
        /// <param name="contextRepository"></param>
        /// <param name="localizer"></param>
        /// <param name="httpContextAccessor"></param>
        public IdentityOperations(TUserManager userManager,
                                  SignInManager<TUser> signInManager,
                                  ITokenManagement tokenManagement,
                                  IContextRepository<TDbContext> contextRepository,
                                  TLocalizer localizer,
                                  IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenManagement = tokenManagement;
            _localizer = localizer;
            _contextRepository = contextRepository;
            _userName = httpContextAccessor.HttpContext.User.Identity.Name;
        }


        /// <summary>
        /// Signs in for incoming user. Returns a token if login informations are valid or the user is not lockedout. Otherwise returns the error list.
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <param name="userValidation"></param>
        /// <param name="tokenExpiredDate"></param>
        /// <returns></returns>
        public virtual async Task<TLoginResultDTO> SignInAsync(ILoginDTO loginDTO,
                                                               IIdentityOperations<TUserManager, TDbContext, TLocalizer, TUser, TRole, TKey, TLoginResultDTO>.UserValidation userValidation,
                                                               DateTime tokenExpiredDate)
        {
            TUser user = new TUser();

            var (tUser, loginResult) = await userValidation.Invoke(loginDTO, user).ConfigureAwait(false);

            user = tUser;

            if (loginResult.ErrorMessages.Count > 0)
                return loginResult;

            SignInResult signInResult = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, loginDTO.Persistent, lockoutOnFailure: true).ConfigureAwait(false);

            //Kimlik doğrulama başarılı ise
            if (signInResult.Succeeded)
            {
                //Token username,IsPersonnel ve rollere göre üretilir
                loginResult.Token = await GenerateTokenWithRoleAsync(user: user, tokenExpiredDate).ConfigureAwait(false);

                if (_useWhiteList)
                {
                    //Bu kullanici daha once token almissa o token silinip yerine yeni alinmis token yazilir.
                    if (SignedInUsers.SignedInUserTokens.ContainsKey(user.UserName))
                        SignedInUsers.SignedInUserTokens.Remove(user.UserName);

                    SignedInUsers.SignedInUserTokens.Add(user.UserName, loginResult.Token);
                }


                return loginResult;
            }

            #region Error Handling

            //Eğer ki başarısız bir account girişi söz konusu ise AccessFailedCount kolonundaki değer +1 arttırılacaktır. 
            await _userManager.AccessFailedAsync(user).ConfigureAwait(false);

            if (signInResult.RequiresTwoFactor)
                loginResult.ErrorMessages.Add(new IdentityError { Code = "RequiresTwoFactor", Description = _localizer["RequiresTwoFactor"] });

            if (signInResult.IsNotAllowed)
                loginResult.ErrorMessages.Add(new IdentityError { Code = "NotAllowed", Description = _localizer["NotAllowed"] });

            #endregion

            return loginResult;
        }

        /// <summary>
        /// Signs in for incoming user. Returns a token if login informations are valid or the user is not lockedout. Otherwise returns the error list.
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <param name="isUserType"></param>
        /// <param name="userValidationByUserType"></param>
        /// <param name="tokenExpiredDate"></param>
        /// <returns></returns>
        public virtual async Task<TLoginResultDTO> SignInAsync(ILoginDTO loginDTO,
                                                               bool isUserType,
                                                               IIdentityOperations<TUserManager, TDbContext, TLocalizer, TUser, TRole, TKey, TLoginResultDTO>.UserValidationByUserType userValidationByUserType,
                                                               DateTime tokenExpiredDate)
        {
            TUser user = new TUser();

            var (tUser, loginResult) = await userValidationByUserType.Invoke(loginDTO, user, isUserType).ConfigureAwait(false);

            user = tUser;

            if (loginResult.ErrorMessages.Count > 0)
                return loginResult;

            SignInResult signInResult = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, loginDTO.Persistent, lockoutOnFailure: true).ConfigureAwait(false);

            //Kimlik doğrulama başarılı ise
            if (signInResult.Succeeded)
            {
                //Token username,IsPersonnel ve rollere göre üretilir
                loginResult.Token = await GenerateTokenWithRoleAsync(user: user, tokenExpiredDate).ConfigureAwait(false);

                if (_useWhiteList)
                {
                    //Bu kullanici daha once token almissa o token silinip yerine yeni alinmis token yazilir.
                    if (SignedInUsers.SignedInUserTokens.ContainsKey(user.UserName))
                        SignedInUsers.SignedInUserTokens.Remove(user.UserName);

                    SignedInUsers.SignedInUserTokens.Add(user.UserName, loginResult.Token);
                }

                return loginResult;
            }

            #region Error Handling

            //Eğer ki başarısız bir account girişi söz konusu ise AccessFailedCount kolonundaki değer +1 arttırılacaktır. 
            await _userManager.AccessFailedAsync(user).ConfigureAwait(false);

            if (signInResult.RequiresTwoFactor)
                loginResult.ErrorMessages.Add(new IdentityError { Code = "RequiresTwoFactor", Description = _localizer["RequiresTwoFactor"] });

            if (signInResult.IsNotAllowed)
                loginResult.ErrorMessages.Add(new IdentityError { Code = "NotAllowed", Description = _localizer["NotAllowed"] });

            #endregion

            return loginResult;
        }

        /// <summary>
        /// Signs out from database. Returns null if already signed out.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IdentityResult> SignOutAsync()
        {
            var user = await _userManager.FindByNameAsync(_userName).ConfigureAwait(false) ?? throw new MilvaUserFriendlyException(_localizer, MilvaExceptionCode.CannotFindEntityException);


            if (await _userManager.GetAuthenticationTokenAsync(user, LoginProvider, TokenName) == null)
                return null;

            IdentityResult identityResult = null;

            await _contextRepository.ApplyTransactionAsync(async () =>
            {

                if (_useWhiteList)
                {
                    if (SignedInUsers.SignedInUserTokens.ContainsKey(user.UserName))
                        SignedInUsers.SignedInUserTokens.Remove(user.UserName);
                }

                _contextRepository.InitializeUpdating<TUser, TKey>(user);

                identityResult = await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider, TokenName);

                await _signInManager.SignOutAsync();

            }).ConfigureAwait(false);

            return identityResult;
        }

        /// <summary>
        /// Delete user by <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id.Equals(userId)).ConfigureAwait(false);

            var identityResult = await _userManager.DeleteAsync(user);

            ThrowErrorMessagesIfNotSuccess(identityResult);
        }

        /// <summary>
        /// Resetes current user's password.
        /// </summary>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public async Task ResetPasswordAsync(string newPassword)
        {
            var currentUser = await _userManager.Users.FirstOrDefaultAsync(p => p.UserName == _userName).ConfigureAwait(false) ?? throw new CannotFindEntityException(_localizer["CannotFindUserWithThisToken"]);

            var authenticationToken = await _userManager.GetAuthenticationTokenAsync(currentUser, LoginProvider, TokenName).ConfigureAwait(false);

            var identityResult = await _userManager.ResetPasswordAsync(currentUser, authenticationToken, newPassword).ConfigureAwait(false);

            ThrowErrorMessagesIfNotSuccess(identityResult);
        }

        /// <summary>
        /// Change current user's password.
        /// </summary>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public async Task<IdentityResult> ChangeCurrentUserPasswordAsync(string oldPassword, string newPassword)
        {
            var user = await _userManager.FindByNameAsync(_userName).ConfigureAwait(false) ?? throw new CannotFindEntityException(_localizer["CannotFindUserWithThisToken"]);

            bool exist = await _userManager.CheckPasswordAsync(user, oldPassword).ConfigureAwait(false);

            if (!exist) throw new UpdatingInvalidEntityException(_localizer["IncorrectOldPassword"]);

            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword).ConfigureAwait(false);
        }

        /// <summary>
        /// Changes <paramref name="user"/> password.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="currentPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public virtual async Task<IdentityResult> ChangePasswordAsync(TUser user, string currentPassword, string newPassword)
        {
            bool exist = await _userManager.CheckPasswordAsync(user, currentPassword).ConfigureAwait(false);

            if (!exist) throw new UpdatingInvalidEntityException(_localizer["IncorrectOldPassword"]);

            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword).ConfigureAwait(false);
        }

        /// <summary>
        /// Changes <paramref name="user"/> email.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newEmail"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async Task<IdentityResult> ChangeEmailAsync(TUser user, string newEmail, string token) => await _userManager.ChangeEmailAsync(user, newEmail, token).ConfigureAwait(false);

        /// <summary>
        /// Changes <paramref name="user"/> phone number.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newPhoneNumber"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async Task<IdentityResult> ChangePhoneNumberAsync(TUser user, string newPhoneNumber, string token) => await _userManager.ChangePhoneNumberAsync(user, newPhoneNumber, token).ConfigureAwait(false);


        #region Helper Methods

        /// <summary>
        /// If <paramref name="identityResult"/> is not succeeded throwns <see cref="InvalidParameterException"/>.
        /// </summary>
        /// <param name="identityResult"></param>
        public virtual void ThrowErrorMessagesIfNotSuccess(IdentityResult identityResult)
        {
            if (!identityResult.Succeeded)
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.AppendJoin(',', identityResult.Errors.Select(i => i.Description));
                throw new IdentityResultException(stringBuilder.ToString());
            }
        }

        /// <summary>
        /// Validating user to login.
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual async Task<(TUser tUser, TLoginResultDTO loginResult)> ValidateUser(ILoginDTO loginDTO, TUser user)
        {
            IdentityError GetLockedError(DateTime lockoutEnd)
            {
                var remainingLockoutEnd = lockoutEnd - DateTime.Now;

                var reminingLockoutEndString = remainingLockoutEnd.Hours > 0
                                                ? _localizer["Hours", remainingLockoutEnd.Hours]
                                                : remainingLockoutEnd.Minutes > 0
                                                     ? _localizer["Minutes", remainingLockoutEnd.Minutes]
                                                     : _localizer["Seconds", remainingLockoutEnd.Seconds];

                return new IdentityError { Code = "Locked", Description = _localizer["Locked", reminingLockoutEndString] };
            }

            int accessFailedCountLimit = 5;

            user = new TUser();

            var loginResult = new TLoginResultDTO { ErrorMessages = new List<IdentityError>() };

            if (loginDTO.UserName == null && loginDTO.Email == null)
                throw new NullParameterException(_localizer["PleaseEnterEmailOrUsername"]);

            //Kullanici adi veya email ile kullanici dogrulama
            #region User Validation

            var userNotFound = true;

            if (loginDTO.UserName != null)
            {
                user = _userManager.FindByNameAsync(userName: loginDTO.UserName).Result;

                userNotFound = user == null;

                if (userNotFound)
                {
                    loginResult.ErrorMessages.Add(new IdentityError { Code = "InvalidUserName", Description = _localizer["InvalidUserName"] });
                    return (user, loginResult);
                }
            }

            if (loginDTO.Email != null && userNotFound)
            {
                user = _userManager.FindByEmailAsync(email: loginDTO.Email).Result;

                userNotFound = user == null;

                if (userNotFound)
                {
                    loginResult.ErrorMessages.Add(new IdentityError { Code = "InvalidEmail", Description = _localizer["InvalidEmail"] });
                    return (user, loginResult);
                }
            }

            if (userNotFound)
            {
                loginResult.ErrorMessages.Add(new IdentityError { Code = "InvalidLogin", Description = _localizer["InvalidLogin"] });

                return (user, loginResult);
            }

            var userLocked = _userManager.IsLockedOutAsync(user).Result;

            if (userLocked && DateTime.Now > user.LockoutEnd.Value.DateTime)
            {
                _contextRepository.InitializeUpdating<TUser, TKey>(user);

                //Locklanmış kullanıcının süresini sıfırlıyoruz
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
            //personnel olarak giris yapmaya calisan gercekten personel mi? degilse basarili giris yapmissa da kabul etme ve hassas mesaj vermesini engellemek icin personel degil diye ayarla

            if (!passIsTrue)
            {
                await _userManager.AccessFailedAsync(user).ConfigureAwait(false);

                if (_userManager.IsLockedOutAsync(user).Result)
                {
                    loginResult.ErrorMessages.Add(GetLockedError(user.LockoutEnd.Value.DateTime));
                    return (user, loginResult);
                }

                var lockWarningMessage = _localizer["LockWarning", accessFailedCountLimit - user.AccessFailedCount];

                loginResult.ErrorMessages.Add(new IdentityError { Code = "InvalidLogin", Description = lockWarningMessage });

                return (user, loginResult);
            }

            return (user, loginResult);

            #endregion
        }

        /// <summary>
        /// Roll is added according to user type and token is produced.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="tokenExpiredDate"></param>
        /// <returns></returns>
        public virtual async Task<string> GenerateTokenWithRoleAsync(TUser user, DateTime tokenExpiredDate)
        {
            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

            string newToken = GenerateToken(username: user.UserName, roles: roles, tokenExpiredDate);

            _contextRepository.InitializeUpdating<TUser, TKey>(user);

            await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider, TokenName).ConfigureAwait(false);

            _contextRepository.InitializeUpdating<TUser, TKey>(user);

            IdentityResult identityResult = await _userManager.SetAuthenticationTokenAsync(user: user,
                                                                                           loginProvider: LoginProvider,//Token nerede kullanılcak
                                                                                           tokenName: TokenName,//Token tipi
                                                                                           tokenValue: newToken).ConfigureAwait(false);

            if (!identityResult.Succeeded)
                throw new IdentityResultException(_localizer);

            return newToken;
        }

        /// <summary>
        /// If Authentication is successful, JWT tokens are generated.
        /// </summary>
        public virtual string GenerateToken(string username, IList<string> roles, DateTime tokenExpiredDate)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            //Kullanıcıya ait roller Tokene Claim olarak ekleniyor
            var claimsIdentityList = new ClaimsIdentity(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            //Kullanıcını UserName 'i tokena claim olarak ekleniyor
            claimsIdentityList.AddClaim(new Claim(ClaimTypes.Name, username));

            //Kullanıcını UserName 'i tokena claim olarak ekleniyor
            claimsIdentityList.AddClaim(new Claim(ClaimTypes.Expired, value: tokenExpiredDate.ToString()));

            //Token üretimi için gerekli bilgiler , _tokenManagement => appsettings.json dosyasını okur
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentityList,
                Issuer = _tokenManagement.Issuer,
                Audience = _tokenManagement.Audience,
                Expires = tokenExpiredDate,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_tokenManagement.Secret)), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);//Token Üretimi

            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Decode Process of Token returns the time inside by Decoding the Token
        /// </summary>
        public virtual DateTime? ValidateTokenGetExpiredTime(string token)
        {
            //Tokenin içindeli claimları döndürür
            var principal = GetPrincipalForAccessToken(token);

            var value = principal?.Identity?.IsAuthenticated ?? false ? principal?.FindFirstValue(ClaimTypes.Expired) : null;

            if (string.IsNullOrEmpty(value))
                return null;

            var experiedTime = DateTime.Parse(value);

            return experiedTime;//tokenın içindeki son kullanma tarihini dondurur
        }

        /// <summary>
        /// Returns Claims in token for token decode
        /// </summary>
        public virtual ClaimsPrincipal GetPrincipalForAccessToken(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

                if (jwtToken == null)
                    return null;

                //_tokenManagement => appsettings.json dosyasını okur
                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _tokenManagement.Issuer,
                    ValidAudience = _tokenManagement.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_tokenManagement.Secret))
                };

                SecurityToken securityToken;

                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out securityToken);

                return principal;//Token içindeki gerekli bilgiler döndürülür
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
    }
}
