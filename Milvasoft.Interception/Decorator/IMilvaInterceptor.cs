namespace Milvasoft.Interception.Decorator;

/// <summary>
/// Describes a decorator used for intercepting methods according to <see cref="DecorateAttribute"/>.
/// </summary>
public interface IMilvaInterceptor
{
    /// <summary>
    /// It determines the operating order between the interceptors. The lower value runs first.
    /// </summary>
    public int InterceptionOrder { get; set; }

    /// <summary>
    /// <para>Additional logic to be executed when a decorated method is intercepted.</para>
    /// <para>Use 'await call.Next()' to continue execution.</para>
    /// </summary>
    /// <param name="call">Representation of the intercepted method call.</param>
    Task OnInvoke(Call call);
}
