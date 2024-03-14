using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Logging;

public class LogAttribute : DecorateAttribute
{
    public LogAttribute() : base(typeof(LogInterceptor))
    {

    }
}