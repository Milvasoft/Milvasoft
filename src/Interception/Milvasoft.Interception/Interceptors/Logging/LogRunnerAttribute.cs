using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Logging;

/// <summary>
/// When using methods contained in external dlls, it is not possible to mark these methods with <see cref="LogAttribute"/>.
/// This attribute allows <see cref="LogInterceptor"/> to intercept these methods.
/// </summary>
public class LogRunnerAttribute : DecorateAttribute
{
    /// <inheritdoc/>
    public LogRunnerAttribute() : base(typeof(LogInterceptor))
    {
    }
}