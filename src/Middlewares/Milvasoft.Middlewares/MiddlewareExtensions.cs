using Microsoft.AspNetCore.Builder;
using Milvasoft.Middlewares.ResponseTimeCalculator;

namespace Milvasoft.Middlewares;

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
    public static IApplicationBuilder UseMilvaResponseTimeCalculator(this IApplicationBuilder builder) => builder.UseMiddleware<MilvaResponseTimeCalculator>();
}
