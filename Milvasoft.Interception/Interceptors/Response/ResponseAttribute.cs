using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Response;

public class ResponseAttribute : DecorateAttribute
{
    public ResponseAttribute() : base(typeof(ResponseInterceptor))
    {

    }
}