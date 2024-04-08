using Castle.DynamicProxy;
using Fody;
using Milvasoft.Interception.Decorator.Internal;
using System.Reflection;

namespace Milvasoft.Interception.Decorator;

/// <summary>
/// Contains intercepted method information.
/// </summary>
[ConfigureAwait(false)]
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
    /// Gets or sets the return value provided by call target or other <see cref="IMilvaInterceptor"/> instances.
    /// </summary>
    public object ReturnValue { get; set; }

    /// <summary>
    /// Gets or sets the return value provided by call target or other <see cref="IMilvaInterceptor"/> instances.
    /// </summary>
    public Type ReturnType { get; set; }

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
    private int _callIndex = 1;

    #endregion

    internal Call(IInvocation invocation, IMilvaInterceptor[] decorators)
    {
        _invocation = invocation;
        _decorators = decorators;
        _proceedInfo = invocation.CaptureProceedInfo();
        ReturnType = invocation.Method.ReturnType;
    }

    /// <summary>
    /// Executes the next <see cref="IMilvaInterceptor.OnInvoke(Call)"/> and eventually the called method itself.
    /// </summary>
    public async Task NextAsync()
    {
        var currentCallIndex = _callIndex;

        try
        {
            if (_decorators != null && _callIndex < _decorators.Length)
            {
                await _decorators[_callIndex++].OnInvoke(this).ConfigureAwait(false);
            }
            else
            {
                if (!ProceedToOriginalInvocation)
                    return;

                _proceedInfo.Invoke();

                if (_invocation.ReturnValue is Task task && MethodImplementation.IsAsync())
                {
                    await SetReturnValueAsAsync(task).ConfigureAwait(false);
                }
                else
                {
                    ReturnValue = _invocation.ReturnValue;
                }
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
    /// Returns the attribute of type <typeparamref name="T"/> used in the method or class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetInterceptorAttribute<T>() where T : DecorateAttribute
    {
        var decorateAttribute = MethodImplementation.GetCustomAttribute<T>();

        decorateAttribute ??= MethodImplementation.DeclaringType.GetCustomAttribute<T>();

        return decorateAttribute;
    }

    private async Task SetReturnValueAsAsync(Task task)
    {
        if (!task.IsCompleted)
        {
            // Async methods are executed within interception.
            // Non-async method returned task is treated as a result 
            // and is not executed within interception.
            await task.ConfigureAwait(false);
        }

        if (task.IsFaulted && task.Exception != null)
        {
            task.Exception.Rethrow();
        }

        // Runtime might return Task<T> derived type here.
        // Discussed in dotnet/runtime#26312 and microsoft/vs-streamjsonrpc#116.
        if (task.GetType().GetTypeInfo().TryGetGenericTaskType(out var genericTaskType))
        {
            if (genericTaskType.IsTaskWithVoidTaskResult())
            {
                return;
            }

            var resultProperty = genericTaskType.GetDeclaredProperty("Result")
                ?? throw new InvalidOperationException($"Object of type '{genericTaskType}' was expected to contain a property 'Result'.");

            ReturnValue = resultProperty.GetValue(task);
        }
    }
}
