using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.Utils;
using Milvasoft.SampleAPI.Localization;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Middlewares
{
    /// <summary>
    /// <para><b>EN: </b>Checks if the title is suitable for the conditions.</para>
    /// <para><b>TR: </b>Başlığın koşullara uygun olup olmadığını kontrol eder.</para>
    /// </summary>
    public class MilvaGeneralMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Constructor of <c>HeaderCheckMiddleware</c> class.
        /// </summary>
        /// <param name="next"></param>
        public MilvaGeneralMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// <para><b>EN: </b>Invokes the method or constructor reflected by this MethodInfo instance.</para>
        /// <para><b>TR: </b>Bu MethodInfo örneği tarafından yansıtılan yöntemi veya yapıcıyı çağırır.</para>
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext httpContext)
        {
            async Task ReturnResponse(string errorMessage, int statusCode)
            {
                ExceptionResponse validationResponse = new ExceptionResponse
                {
                    Message = errorMessage,
                    Success = false,
                    StatusCode = statusCode
                };

                httpContext.Response.StatusCode = 200;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(validationResponse)).ConfigureAwait(false);
            }

            await _next.Invoke(httpContext).ConfigureAwait(false);

            if (httpContext.Response.StatusCode == MilvaStatusCodes.Status401Unauthorized || httpContext.Response.StatusCode == MilvaStatusCodes.Status403Forbidden)
            {
                var localizer = httpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();

                var statusCode = Convert.ToInt32(httpContext.Response.StatusCode);
                var errorMessage = localizer["Unauthorized"];

                if (statusCode == Convert.ToInt32(MilvaStatusCodes.Status403Forbidden))
                    errorMessage = localizer["Forbidden"];

                await ReturnResponse(errorMessage, statusCode).ConfigureAwait(false);
            }

            //if (httpContext.Response.HasStarted)
            //{
            //    var statusCode = httpContext.Items["StatusCode"] != null ? (int)httpContext.Items["StatusCode"] : 200;

            //    var actionContent = httpContext.Items["ActionContent"];

            //    if (actionContent != null && statusCode >= 200 && statusCode <= 299)
            //    {
            //        var requestMethod = httpContext.Request.Method;

            //        if (requestMethod == "PUT" || requestMethod == "POST" || requestMethod == "DELETE")
            //        {

            //            var username = httpContext.User?.Identity?.Name ?? "";
            //            var userActivityLogRepository = (IUserActivityLogRepository)httpContext.RequestServices.GetService(typeof(IUserActivityLogRepository));
            //            await userActivityLogRepository.AddAsync(new UserActivityLog
            //            {
            //                UserName = username,
            //                Activity = $"{actionContent}"
            //            }).ConfigureAwait(false);
            //        }
            //    }
            //}
        }
    }
}
