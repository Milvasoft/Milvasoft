namespace Milvasoft.Interception.Interceptors.Logging;

public class LogInterceptionPostConfigureOptions
{
    /// <summary>
    /// It allows the values to be logged to be sent to the library, other than the values that the Interceptor logs by default.
    /// </summary>
    public Func<IServiceProvider, object> ExtraLoggingPropertiesSelector { get; set; }
}