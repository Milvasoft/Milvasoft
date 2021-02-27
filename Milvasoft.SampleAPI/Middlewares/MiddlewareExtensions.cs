using Microsoft.AspNetCore.Builder;

namespace Milvasoft.SampleAPI.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandler(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<ExceptionHandlerMiddleware>();
        }
    }
}
