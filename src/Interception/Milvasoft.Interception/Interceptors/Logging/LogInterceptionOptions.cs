using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Interception.Interceptors.Logging;

/// <summary>
/// Represents the options for log interception.
/// </summary>
public class LogInterceptionOptions : ILogInterceptionOptions
{
    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Interception:Log";

    public ServiceLifetime InterceptorLifetime { get; set; } = ServiceLifetime.Scoped;

    public bool AsyncLogging { get; set; } = true;

    public bool LogDefaultParameters { get; set; } = true;

    public bool ExcludeResponseMetadataFromLog { get; set; } = false;

    public Func<IServiceProvider, object> ExtraLoggingPropertiesSelector { get; set; }
}

/// <summary>
/// Represents the options for log interception.
/// </summary>
public interface ILogInterceptionOptions : IMilvaOptions
{
    /// <summary>
    /// Log interceptor lifetime.
    /// </summary>
    public ServiceLifetime InterceptorLifetime { get; set; }

    /// <summary>
    /// It determines whether the values that the Milvasoft library logs with async.
    /// </summary>
    public bool AsyncLogging { get; set; }

    /// <summary>
    /// It determines whether the values that the Milvasoft library logs by default will be logged. Default is true; 
    /// You can see default parameters in <see cref="LogEntityBase{TKey}"/>.
    /// </summary>
    public bool LogDefaultParameters { get; set; }

    /// <summary>
    /// If you are using the milva <see cref="IResponse"/> infrastructure and logging response objects, send this option to true if you do not want to log the <see cref="IResponse.Metadatas"/> information in the <see cref="IResponse"/> object. 
    /// </summary>
    public bool ExcludeResponseMetadataFromLog { get; set; }

    /// <summary>
    /// It allows the values to be logged to be sent to the library, other than the values that the Interceptor logs by default.
    /// </summary>
    public Func<IServiceProvider, object> ExtraLoggingPropertiesSelector { get; set; }
}