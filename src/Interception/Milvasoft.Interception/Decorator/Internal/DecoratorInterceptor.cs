using Castle.DynamicProxy;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Milvasoft.Interception.Decorator.Internal;

/// <summary>
/// Castle interceptor that runs the <see cref="IMilvaInterceptor"/> pipeline of decorated methods.
/// Synchronous, <see cref="Task"/>, <see cref="Task{TResult}"/>, <see cref="ValueTask"/> and <see cref="ValueTask{TResult}"/> returning methods are supported;
/// for awaitable methods the proxy returns a task that completes when the whole pipeline (including the target's work) completes.
/// </summary>
/// <param name="methodDecoratorMap">Interceptors (ordered by <see cref="IMilvaInterceptor.InterceptionOrder"/>) per implementation method.</param>
internal sealed class DecoratorInterceptor(ReadOnlyDictionary<MethodInfo, IMilvaInterceptor[]> methodDecoratorMap) : IInterceptor
{
    public ReadOnlyDictionary<MethodInfo, IMilvaInterceptor[]> MethodDecoratorMap { get; } = methodDecoratorMap;

    public void Intercept(IInvocation invocation)
    {
        if (!TryGetMethodDecorators(invocation, out var decorators))
        {
            invocation.Proceed();
            return;
        }

        var call = new Call(invocation, decorators, MethodReturnInfo.For(invocation.Method));

        invocation.ReturnValue = call.Run();
    }

    /// <summary>
    /// Gets the method decorators.
    /// </summary>
    /// <param name="invocation"></param>
    /// <param name="decorators"></param>
    /// <returns></returns>
    private bool TryGetMethodDecorators(IInvocation invocation, out IMilvaInterceptor[] decorators)
    {
        var targetMethod = invocation.MethodInvocationTarget.IsGenericMethod
                                ? invocation.MethodInvocationTarget.GetGenericMethodDefinition()
                                : invocation.MethodInvocationTarget;

        return MethodDecoratorMap.TryGetValue(targetMethod, out decorators) && decorators != null && decorators.Length != 0;
    }
}
