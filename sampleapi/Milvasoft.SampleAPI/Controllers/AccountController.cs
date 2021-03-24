using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Caching;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.Helpers.Utils;
using Milvasoft.SampleAPI.DTOs.AccountDTOs;
using Milvasoft.SampleAPI.Localization;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Utils.Attributes.ActionFilters;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1.0")]
    [Route("sampleapi/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;
        private readonly IRedisCacheService _cacheServer;

        public AccountController(IAccountService accountService, IStringLocalizer<SharedResource> sharedLocalizer, IRedisCacheService cacheServer)
        {
            _accountService = accountService;
            _sharedLocalizer = sharedLocalizer;
            _cacheServer = cacheServer;
        }

        [HttpGet("Tenant")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(TenantId tenantId)
        {
            return Ok();
        }

        [HttpGet("CacheDemo")]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            await _cacheServer.ConnectAsync();

            if (_cacheServer.IsConnected())
            {
                await _cacheServer.SetAsync("lemon", "lemonate").ConfigureAwait(false);

                return Ok(await _cacheServer.GetAsync("lemon").ConfigureAwait(false));
            }
            else return Ok("Error when connecting redis server.");
        }

        /// <summary>
        /// Sign up method for users.
        /// </summary>
        /// 
        /// <returns></returns>
        /// <param name="signUpDTO"></param>
        /// <returns></returns>
        [HttpPost("SignUp")]
        [AllowAnonymous]
        [OValidationFilter]
        public async Task<ActionResult> SignUpAsync([FromBody] SignUpDTO signUpDTO)
        {
            ObjectResponse<LoginResultDTO> response = new ObjectResponse<LoginResultDTO>();

            response.Result = await _accountService.SignUpAsync(signUpDTO).ConfigureAwait(false);

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
        /// Sign in method for customers. This endpoint is accessible for only customers.
        /// </summary>
        /// 
        /// <remarks>
        /// <para><b>EN:</b> </para>
        /// <para> Sign in method for customers. This endpoint is accessible for only customers. </para> 
        /// <br></br>
        /// <para><b>TR:</b></para>
        /// <para> Müşteriler için giriş yapma işlemi. Bu endpoint sadece müşteriler için erişilebilirdir. </para>
        /// 
        /// </remarks>
        /// 
        /// <returns></returns>
        /// <param name="loginDTO"></param>
        /// <returns></returns>
        [HttpPost("SignIn")]
        [AllowAnonymous]
        [OValidateIdParameter(EntityName = "Customer")]
        public async Task<ActionResult> SignInAsync(LoginDTO loginDTO)
        {
            ObjectResponse<LoginResultDTO> response = new ObjectResponse<LoginResultDTO>();

            response.Result = await _accountService.SignInAsync(loginDTO).ConfigureAwait(false);

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
        /// Sign out method for customers. This endpoint is accessible for only customers.
        /// </summary>
        /// 
        /// <remarks>
        /// <para><b>EN:</b> </para>
        /// <para> Sign out method for customers. This endpoint is accessible for only customers. </para> 
        /// <br></br>
        /// <para><b>TR:</b></para>
        /// <para> Müşteriler için çıkış işlemi. Bu endpoint sadece müşteriler için erişilebilirdir. </para>
        /// 
        /// </remarks>
        /// <returns></returns>
        [HttpGet("SignOut")]
        public async Task<ActionResult> SignOutCustomerAsync()
        {
            //try to get user from customers
            var response = new ObjectResponse<IdentityResult>();
            response.Result = await _accountService.SignOutAsync();

            if (response.Result == null)
            {
                response.StatusCode = MilvaStatusCodes.Status503ServiceUnavailable;
                response.Message = _sharedLocalizer["AlreadyLoggedOutMessage"];
                response.Success = false;
            }
            else if (response.Result.Succeeded)
            {
                response.StatusCode = MilvaStatusCodes.Status200OK;
                response.Message = _sharedLocalizer["SuccessfullyLoguotMessage"];
                response.Success = true;
            }
            else
            {
                response.StatusCode = MilvaStatusCodes.Status503ServiceUnavailable;
                response.Message = _sharedLocalizer["UnknownLogoutProblemMessage"];
                response.Success = false;
            }
            return Ok(response);
        }

    }
}
