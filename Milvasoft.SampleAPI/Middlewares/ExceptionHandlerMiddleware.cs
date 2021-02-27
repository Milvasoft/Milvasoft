using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Middlewares
{
    public class ExceptionHandlerMiddleware
    {

        public RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next.Invoke(httpContext);
            }
            catch (Exception)
            {

            }

            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

    }
}
