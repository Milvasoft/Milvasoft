using Milvasoft.Core.Abstractions;

namespace Milvasoft.Interception.Interceptors.Logging;

public class ResponseInterceptionOptions : IResponseInterceptionOptions
{
    /// <summary>
    /// It allows the values to be logged to be sent to the library, other than the values that the Interceptor logs by default.
    /// </summary>
    public Func<string, IMilvaLocalizer, Type, string, string> LocalizationMethod { get; set; }
}


public interface IResponseInterceptionOptions
{
    /// <summary>
    /// It determines with which key pattern the data will be received from the ImilvaLocalizer in the Interceptor. 
    /// If it is not sent, localization is tried with the default pattern. The default pattern is <see cref="ResponseInterceptor.ApplyLocalization(string, IMilvaLocalizer, Type, string)"/>
    /// </summary>
    public Func<string, IMilvaLocalizer, Type, string, string> LocalizationMethod { get; set; }
}