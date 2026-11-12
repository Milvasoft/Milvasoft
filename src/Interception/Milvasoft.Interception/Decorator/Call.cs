using Castle.DynamicProxy;
using Milvasoft.Interception.Decorator.Internal;
using System.Reflection;

namespace Milvasoft.Interception.Decorator;

/// <summary>
/// Contains intercepted method information.
/// </summary>
public sealed class Call
{
    #region Props

    /// <summary>
    /// Gets the <see cref="MethodInfo"/> representing the method at call site.
    /// It is different from <see cref="MethodImplementation"/> property only when a call originates from an interface.
    /// </summary>
    public MethodInfo Method => _invocation.Method;

    /// <summary>
    /// Gets the <see cref="MethodInfo"/> representing the called method's implementation.
    /// It is different from <see cref="Method"/> property only when a call originates from an interface.
    /// </summary>
    public MethodInfo MethodImplementation => _invocation.MethodInvocationTarget;

    /// <summary>
    /// Gets the collection of arguments passed to the target method.
    /// Arguments can be modified inside <see cref="IMilvaInterceptor"/>.
    /// </summary>
    public object[] Arguments => _invocation.Arguments;

    /// <summary>
    /// Gets or sets the result provided by call target or other <see cref="IMilvaInterceptor"/> instances.
    /// For awaitable methods this is the awaited result (the <c>T</c> of <see cref="Task{TResult}"/> or <see cref="ValueTask{TResult}"/>, null for <see cref="Task"/> and <see cref="ValueTask"/>),
    /// and it is set only after the target's work has completed.
    /// </summary>
    public object ReturnValue { get; set; }

    /// <summary>
    /// Gets or sets the declared return type of the method (for example <see cref="Task{TResult}"/>).
    /// Use <see cref="ResultType"/> for the type of <see cref="ReturnValue"/>.
    /// </summary>
    public Type ReturnType { get; set; }

    /// <summary>
    /// Gets the type of <see cref="ReturnValue"/>: TResult for <see cref="Task{TResult}"/> and <see cref="ValueTask{TResult}"/>,
    /// <see cref="void"/> for <see cref="Task"/>, <see cref="ValueTask"/> and void methods, otherwise the declared return type.
    /// </summary>
    public Type ResultType => _returnInfo.ResultType;

    /// <summary>
    /// Gets whether the intercepted method is awaitable (returns <see cref="Task"/>, <see cref="Task{TResult}"/>, <see cref="ValueTask"/> or <see cref="ValueTask{TResult}"/>).
    /// When it is false the caller blocks until the interceptors complete, so interceptors should prefer synchronous work for such calls.
    /// </summary>
    public bool IsAsync => _returnInfo.IsAsync;

    /// <summary>
    /// Gets the generic arguments of the method.
    /// Returns empty if method is not generic.
    /// </summary>
    public Type[] GenericArguments => _invocation.GenericArguments ?? [];

    /// <summary>
    /// Gets the object on which call was performed.
    /// </summary>
    public object Object => _invocation.InvocationTarget;

    /// <summary>
    /// If you are working with an interceptor such as cache interceptor and you want the calling method not to run at all, false must be sent.
    /// The original caller is not called, only the interceptors are called.
    /// Default is true.
    /// </summary>
    public bool ProceedToOriginalInvocation { get; set; } = true;

    #endregion

    #region Fields

    private readonly IInvocation _invocation;
    private readonly IInvocationProceedInfo _proceedInfo;
    private readonly IMilvaInterceptor[] _decorators;
    private readonly MethodReturnInfo _returnInfo;
    private int _callIndex;

    #endregion

    internal Call(IInvocation invocation, IMilvaInterceptor[] decorators, MethodReturnInfo returnInfo)
    {
        _invocation = invocation;
        _decorators = decorators ?? [];
        _returnInfo = returnInfo;
        _proceedInfo = invocation.CaptureProceedInfo();
        ReturnType = invocation.Method.ReturnType;
    }

    /// <summary>
    /// Executes the next <see cref="IMilvaInterceptor.OnInvoke(Call)"/> and eventually the called method itself.
    /// The returned task completes when the called method's work (including the awaited task of an async method) has completed,
    /// and it faults with the called method's exception.
    /// </summary>
    public async Task NextAsync()
    {
        var currentCallIndex = _callIndex;

        try
        {
            if (_callIndex < _decorators.Length)
            {
                await _decorators[_callIndex++].OnInvoke(this).ConfigureAwait(false);
            }
            else
            {
                if (!ProceedToOriginalInvocation)
                    return;

                _proceedInfo.Invoke();

                ReturnValue = await _returnInfo.AwaitTargetResultAsync(_invocation.ReturnValue).ConfigureAwait(false);
            }
        }
        catch (Exception)
        {
            // Call index is reset to position before the exception, so that
            // the current decorator could call Next() again if needed.
            _callIndex = currentCallIndex;

            throw;
        }
    }

    /// <summary>
    /// Returns the attribute of type <typeparamref name="T"/> that is applied to the called method, its interface method, the implementing class or the interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>The attribute or null if not found.</returns>
    public T GetInterceptorAttribute<T>() where T : DecorateAttribute
        => InterceptorAttributeLocator.Find<T>(MethodImplementation, Method);

    /// <summary>
    /// Starts the interception pipeline and returns the value the proxy must return to the caller.
    /// </summary>
    internal object Run() => _returnInfo.CreateProxyReturnValue(NextAsync(), this);
}
