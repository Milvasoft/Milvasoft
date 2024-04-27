using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Response;

/// <inheritdoc/>
public class ResponseAttribute : DecorateAttribute
{
    /// <inheritdoc/>
    public ResponseAttribute() : base(typeof(ResponseInterceptor))
    {

    }
}