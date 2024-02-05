using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Logging;

public class ResponseAttribute : DecorateAttribute
{
    public ResponseAttribute() : base(typeof(ResponseInterceptor))
    {

    }
}