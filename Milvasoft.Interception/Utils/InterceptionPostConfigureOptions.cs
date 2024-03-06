using Milvasoft.Interception.Interceptors.Logging;
using Milvasoft.Interception.Interceptors.Response;

namespace Milvasoft.Interception.Utils;

public class InterceptionPostConfigureOptions
{
    /// <summary>
    /// Logging interceptor post configuration options.
    /// </summary>
    public LogInterceptionPostConfigureOptions Log { get; set; } = new();

    /// <summary>
    /// Response interceptor post configuration options.
    /// </summary>
    public ResponseInterceptionPostConfigureOptions Response { get; set; } = new();
}
