using Milvasoft.Interception.Interceptors.Cache;
using Milvasoft.Interception.Interceptors.Logging;
using Milvasoft.Interception.Interceptors.Response;

namespace Milvasoft.Interception.Builder;

/// <summary>
/// If options are made from the configuration file, the class that allows options that cannot be made from the configuration file.
/// </summary>
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

    /// <summary>
    /// Cache interceptor post configuration options.
    /// </summary>
    public CacheInterceptionPostConfigureOptions Cache { get; set; } = new();
}
