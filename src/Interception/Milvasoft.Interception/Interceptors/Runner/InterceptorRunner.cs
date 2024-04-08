using Fody;
using Milvasoft.Interception.Interceptors.Logging;
using System.Linq.Expressions;

namespace Milvasoft.Interception.Interceptors.Runner;

/// <summary>
/// When using methods contained in external dlls, it is not possible to mark these methods with attribute.
/// This abstraction allows milva interceptors to intercept these methods.
/// You can write your own runner with implement <see cref="IInterceptorRunner"/> interface.
/// </summary>
[ConfigureAwait(false)]
public class InterceptorRunner : IInterceptorRunner
{
    [LogRunner]
    public virtual async Task<TResult> InterceptWithLogAsync<TResult>(Expression<Func<Task<TResult>>> expression)
    {
        var result = await expression.Compile().Invoke().ConfigureAwait(false);

        return result;
    }

    [LogRunner]
    public virtual async Task InterceptWithLogAsync(Expression<Func<Task>> expression)
        => await expression.Compile().Invoke().ConfigureAwait(false);

    [LogRunner]
    public virtual TResult InterceptWithLog<TResult>(Expression<Func<TResult>> expression)
    {
        var result = expression.Compile().Invoke();

        return result;
    }

    [LogRunner]
    public virtual void InterceptWithLog<T>(Expression<Action> expression)
        => expression.Compile().Invoke();
}