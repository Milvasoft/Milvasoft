using Milvasoft.Interception.Interceptors.Logging;
using System.Linq.Expressions;

namespace Milvasoft.Interception.Interceptors.Runner;

public class InterceptorRunner : IInterceptorRunner
{
    [LogRunner]
    public virtual async Task<TResult> InterceptWithLogAsync<T, TResult>(Expression<Func<Task<TResult>>> expression)
    {
        var result = await expression.Compile().Invoke().ConfigureAwait(false);

        return result;
    }
}