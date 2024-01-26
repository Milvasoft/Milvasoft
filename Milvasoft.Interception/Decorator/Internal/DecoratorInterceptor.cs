using Castle.DynamicProxy;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Milvasoft.Interception.Decorator.Internal;

internal class DecoratorInterceptor(ReadOnlyDictionary<MethodInfo, IMilvaInterceptor[]> methodDecoratorMap) : IAsyncInterceptor
{
    public ReadOnlyDictionary<MethodInfo, IMilvaInterceptor[]> MethodDecoratorMap { get; } = methodDecoratorMap;

    public void InterceptSynchronous(IInvocation invocation)
    {
        if (TryGetMethodDecorators(invocation, out var decorators))
        {
            var call = new Call(invocation, decorators);

            try
            {
                GetFirstRunningInterceptor(decorators).OnInvoke(call).Wait();
            }
            catch (AggregateException e) when (e.InnerException != null)
            {
                e.InnerException.Rethrow();
            }

            invocation.ReturnValue = call.ReturnValue;
        }
        else
        {
            invocation.Proceed();
        }
    }

    public void InterceptAsynchronous(IInvocation invocation)
    {
        if (TryGetMethodDecorators(invocation, out var decorators))
        {
            var decoratedTask = WrapInvocationInTask(invocation, decorators.OrderBy(d => d.InterceptionOrder).ToArray());

            if (decoratedTask.IsFaulted)
            {
                (decoratedTask.Exception?.InnerException ?? decoratedTask.Exception).Rethrow();
            }

            if (!decoratedTask.IsCompleted)
            {
                invocation.ReturnValue = decoratedTask;
            }
        }
        else
        {
            invocation.Proceed();
        }
    }

    public void InterceptAsynchronous<TResult>(IInvocation invocation)
    {
        if (TryGetMethodDecorators(invocation, out var decorators))
        {
            invocation.ReturnValue = WrapInvocationInTaskWithResult<TResult>(invocation, decorators);
        }
        else
        {
            invocation.Proceed();
        }
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

    private static async Task WrapInvocationInTask(IInvocation invocation, IMilvaInterceptor[] decorators)
    {
        var call = new Call(invocation, decorators);

        await GetFirstRunningInterceptor(decorators).OnInvoke(call).ConfigureAwait(false);
    }

    private static async Task<TResult> WrapInvocationInTaskWithResult<TResult>(IInvocation invocation, IMilvaInterceptor[] decorators)
    {
        var call = new Call(invocation, decorators);

        try
        {
            await GetFirstRunningInterceptor(decorators).OnInvoke(call).ConfigureAwait(false);
        }
        catch (AggregateException aggreggateException)
        {
            aggreggateException.InnerException?.Rethrow();
            aggreggateException.Rethrow();
        }
        catch (TargetInvocationException targetInvocationException)
        {
            if (targetInvocationException.InnerException is AggregateException aggreggateException)
            {
                aggreggateException.InnerException?.Rethrow();
                aggreggateException.Rethrow();
            }

            targetInvocationException.InnerException?.Rethrow();
            targetInvocationException.Rethrow();
        }

        return (TResult)call.ReturnValue;
    }

    private static IMilvaInterceptor GetFirstRunningInterceptor(IMilvaInterceptor[] decorators) => decorators.MinBy(decorator => decorator.InterceptionOrder);
}
