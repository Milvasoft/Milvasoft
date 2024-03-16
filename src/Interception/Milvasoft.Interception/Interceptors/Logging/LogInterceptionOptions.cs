using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Interception.Interceptors.Logging;

public class LogInterceptionOptions : ILogInterceptionOptions
{
    public static string SectionName { get; } = $"{MilvaOptionsExtensions.ParentSectionName}:Interception:Log";

    public ServiceLifetime InterceptorLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// It determines whether the values that the Milvasoft library logs with async.
    /// </summary>
    public bool AsyncLogging { get; set; } = true;

    /// <summary>
    /// It determines whether the values that the Milvasoft library logs by default will be logged. Default is true;
    /// </summary>
    public bool LogDefaultParameters { get; set; } = true;

    /// <summary>
    /// If you are using the milva <see cref="IResponse"/> infrastructure and logging response objects, send this option to true if you do not want to log the <see cref="IResponse.Metadatas"/> information in the <see cref="IResponse"/> object. 
    /// </summary>
    public bool ExcludeResponseMetadataFromLog { get; set; } = false;

    /// <summary>
    /// It allows the values to be logged to be sent to the library, other than the values that the Interceptor logs by default.
    /// </summary>
    public Func<IServiceProvider, object> ExtraLoggingPropertiesSelector { get; set; }
}

public interface ILogInterceptionOptions : IMilvaOptions
{
    public ServiceLifetime InterceptorLifetime { get; set; }

    /// <summary>
    /// It determines whether the values that the Milvasoft library logs with async.
    /// </summary>
    public bool AsyncLogging { get; set; }

    /// <summary>
    /// It determines whether the values that the Milvasoft library logs by default will be logged. Default is true;
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