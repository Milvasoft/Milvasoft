namespace Milvasoft.Interception.Interceptors.Logging;

/// <summary>
/// If options are made from the configuration file, the class that allows options that cannot be made from the configuration file.
/// </summary>
public class LogInterceptionPostConfigureOptions
{
    /// <summary>
    /// It allows the values to be logged to be sent to the library, other than the values that the Interceptor logs by default.
    /// </summary>
    public Func<IServiceProvider, object> ExtraLoggingPropertiesSelector { get; set; }
}