using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace Milvasoft.Middlewares.ResponseTimeCalculator;

/// <summary>
/// Calculates response time and adds result to response headers.
/// Custom header is "X-Response-Time".
/// </summary>
public class MilvaResponseTimeCalculator
{
    /// <summary>
    /// Name of the Response Header, Custom Headers starts with "X-"  
    /// </summary>
    private const string _responseHeader = "X-Response-Time-ms";

    /// <summary>
    /// Handle to the next Middleware in the pipeline  
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes new instances of <see cref="MilvaResponseTimeCalculator"/>.
    /// </summary>
    /// <param name="next"></param>
    public MilvaResponseTimeCalculator(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the method or constructor reflected by this MethodInfo instance.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task InvokeAsync(HttpContext context)
    {
        // Start the Timer using Stopwatch  
        var watch = new Stopwatch();

        watch.Start();

        context.Response.OnStarting(() =>
        {

            // Stop the timer information and calculate the time   
            watch.Stop();

            var responseTimeForCompleteRequest = watch.ElapsedMilliseconds;

            // Add the Response time information in the Response headers.   
            context.Response.Headers[_responseHeader] = $"{responseTimeForCompleteRequest}";

            return Task.CompletedTask;
        });

        // Call the next delegate/middleware in the pipeline   
        return _next(context);
    }
}
