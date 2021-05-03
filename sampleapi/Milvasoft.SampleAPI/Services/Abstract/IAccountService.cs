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
    public interface IAccountService : IIdentityOperations<UserManager<AppUser>,EducationAppDbContext,IStringLocalizer<SharedResource>,AppUser,AppRole,Guid,LoginResultDTO>
    {
        /// <summary>
        /// Login for incoming user. Returns a token if login informations are valid or the user is not lockedout. Otherwise returns the error list.
        /// </summary>
        /// <returns></returns>
        Task<LoginResultDTO> SignInAsync(LoginDTO loginDTO,bool isMentor);

        /// <summary>
        /// Signs out from database.
        /// </summary>
        /// <returns></returns>
        Task<IdentityResult> SignOutAsync();

        /// <summary>
        /// Change user password.
        /// </summary>
        /// <param name="changeDTO"></param>
        /// <returns></returns>
        Task<IdentityResult> ChangePasswordAsync(ChangePassDTO changeDTO);

    }
}
