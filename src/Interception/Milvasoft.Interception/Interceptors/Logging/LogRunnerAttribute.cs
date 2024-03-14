using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Logging;

public class LogRunnerAttribute : DecorateAttribute
{
    public LogRunnerAttribute() : base(typeof(LogInterceptor))
    {

    }
}