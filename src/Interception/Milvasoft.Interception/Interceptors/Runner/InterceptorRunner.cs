using Milvasoft.Interception.Interceptors.Logging;
using System.Linq.Expressions;

namespace Milvasoft.Interception.Interceptors.Runner;

public class InterceptorRunner : IInterceptorRunner
{
    [LogRunner]
    public virtual async Task<TResult> InterceptWithLogAsync<T, TResult>(Expression<Func<Task<TResult>>> function)
    {
        var result = await function.Compile().Invoke().ConfigureAwait(false);

        return result;
    }
}