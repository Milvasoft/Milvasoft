using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Logging;

/// <summary>
/// Specifies that the method marked with this attribute will be logged by the <see cref="LogInterceptor"/>.
/// </summary>
public class LogAttribute : DecorateAttribute
{
    /// <summary>
    /// Specifies that the method marked with this attribute will be logged by the <see cref="LogInterceptor"/>.
    /// </summary>
    public LogAttribute() : base(typeof(LogInterceptor))
    {

    }
}
