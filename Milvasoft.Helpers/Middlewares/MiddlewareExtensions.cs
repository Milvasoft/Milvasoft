using Microsoft.AspNetCore.Builder;

namespace Milvasoft.Helpers.Middlewares;

/// <summary>
/// Provides registration of milva middlewares.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds response time calculator.
    /// 
    /// <para> Calculates response time and adds result to response headers. </para>
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseMilvaResponseTimeCalculator(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MilvaResponseTimeCalculator>();
    }
}
