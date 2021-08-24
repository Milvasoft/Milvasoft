using Microsoft.AspNetCore.Builder;

namespace Milvasoft.SampleAPI.Middlewares
{
    /// <summary>
    /// Custom middleware extensions.
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds exception handler to middleware pipeline.
        /// </summary>
        /// <param name="applicationBuilder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<ExceptionHandlerMiddleware>();
        }

        /// <summary>
        /// Extension method of <c>HeaderCheckMiddleware</c> class.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMilvaGeneralMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MilvaGeneralMiddleware>();
        }

    }
}
