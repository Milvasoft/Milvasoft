namespace Milvasoft.Interception.Interceptors.Logging;

public class LogInterceptionOptions : ILogInterceptionOptions
{
    /// <summary>
    /// It determines whether the values that the Milvasoft library logs with async.
    /// </summary>
    public bool AsyncLogging { get; set; } = true;

    /// <summary>
    /// It determines whether the values that the Milvasoft library logs by default will be logged. Default is true;
    /// </summary>
    public bool LogDefaultParameters { get; set; } = true;

    /// <summary>
    /// It allows the values to be logged to be sent to the library, other than the values that the Interceptor logs by default.
    /// </summary>
    public Func<IServiceProvider, object> ExtraLoggingPropertiesSelector { get; set; }
}


public interface ILogInterceptionOptions
{
    /// <summary>
    /// It determines whether the values that the Milvasoft library logs with async.
    /// </summary>
    public bool AsyncLogging { get; set; }

    /// <summary>
    /// It determines whether the values that the Milvasoft library logs by default will be logged. Default is true;
    /// </summary>
    public bool LogDefaultParameters { get; set; }

    /// <summary>
    /// It allows the values to be logged to be sent to the library, other than the values that the Interceptor logs by default.
    /// </summary>
    public Func<IServiceProvider, object> ExtraLoggingPropertiesSelector { get; set; }
}