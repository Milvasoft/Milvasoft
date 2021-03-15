using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Helpers;
using Milvasoft.Helpers.DependencyInjection;
using Milvasoft.Helpers.Enums;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Constructor of <c>ExceptionMiddleware</c> class.
        /// </summary>
        /// <param name="next"></param>
        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// <para><b>EN: </b>Invokes the method or constructor reflected by this MethodInfo instance.</para>
        /// <para><b>TR: </b>Bu MethodInfo örneği tarafından yansıtılan yöntemi veya yapıcıyı çağırır.</para>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            void SendExceptionMail(Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<IMilvaLogger>();

                using var sr = new StringReader(ex.StackTrace);

                var path = context.Request.Path;

                var stackTraceFirstLine = sr.ReadLine();

                logger.LogFatal($"{path}|{ex.Message}|{stackTraceFirstLine}", MailSubject.Error);
            }

            var sharedLocalizer = context.RequestServices.GetLocalizerInstance<SharedResource>();

            string message = sharedLocalizer["MiddlewareGeneralErrorMessage"];

            List<int> errorCodes = new List<int>();

            try
            {
                context.Request.EnableBuffering();
                await _next.Invoke(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is MilvaUserFriendlyException userFriendlyEx)
                {
                    //var userFriendlyEx = (MilvaUserFriendlyException)ex;
                    message = userFriendlyEx.Message;
                    errorCodes.Add((int)userFriendlyEx.ExceptionCode);

                    if (userFriendlyEx.ExceptionCode == MilvaExceptionCode.WrongPaginationParamsException)
                    {
                        message = sharedLocalizer["WrongPaginationParamsException", userFriendlyEx.ExceptionObject];
                    }
                    else if (userFriendlyEx.ExceptionCode == MilvaExceptionCode.WrongRequestedPageNumberException)
                    {
                        message = sharedLocalizer["InvalidPageIndexMessage"];
                    }
                    else if (userFriendlyEx.ExceptionCode == MilvaExceptionCode.WrongRequestedItemCountException)
                    {
                        message = sharedLocalizer["InvalidRequestedItemCountMessage"];
                    }
                    else if (userFriendlyEx.ExceptionCode == MilvaExceptionCode.CannotGetResponseException)
                    {
                        //SendExceptionMail(baseEx);
                    }
                }
                else
                {

                    if (ex is OverflowException || ex is StackOverflowException) message = "Please enter a valid value!";
                    else message = ex.Message;//Prodda burası kapatılacak.

                    //SendExceptionMail(ex);
                }
                if (!context.Response.HasStarted)
                {
                    var response = new ExceptionResponse();
                    response.Message = message;
                    response.StatusCode = MilvaStatusCodes.Status600Exception;
                    response.Success = false;
                    response.Result = new object();
                    response.ErrorCodes = errorCodes;
                    var json = JsonConvert.SerializeObject(response);
                    context.Response.ContentType = "application/json";
                    context.Items.Remove("ActionContent");
                    context.Response.StatusCode = MilvaStatusCodes.Status200OK;
                    await context.Response.WriteAsync(json);
                }
            }
           
        }
    }
}

