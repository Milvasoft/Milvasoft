using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers;
using Milvasoft.Helpers.Caching;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.Helpers.Utils;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs.AccountDTOs;
using Milvasoft.SampleAPI.Localization;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Utils.Attributes.ActionFilters;
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
    public class AccountController : ControllerBase
    {
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;
        private readonly IAccountService _accountService;

        /// <summary>
        /// Constructor of <c>AccountController</c>
        /// </summary>
        /// <param name="sharedLocalizer"></param>
        /// <param name="accountService"></param>
        public AccountController(IStringLocalizer<SharedResource> sharedLocalizer, IAccountService accountService)
        {
            _sharedLocalizer = sharedLocalizer;
            _accountService = accountService;

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
        public async Task<ActionResult> UsersSignIn([FromBody] LoginDTO loginDTO)
        {
            ObjectResponse<LoginResultDTO> response = new();

            response.Result = await _accountService.SignInAsync(loginDTO, true).ConfigureAwait(false);
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
            await _accountService.SignOutAsync().ConfigureAwait(false);
            return Ok("Success");
        }

        /// <summary>
        /// <para>Change personnel user password.</para>
        /// </summary>
        /// <param name="personnelUpdateDTO"></param>
        /// <returns></returns>
        [HttpPut("Personnel/UpdatePassword")]
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


    }
}
