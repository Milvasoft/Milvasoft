using Milvasoft.Interception.Interceptors.Logging;
using System.Linq.Expressions;

namespace Milvasoft.Interception.Interceptors.Runner;

public interface IInterceptorRunner : IInterceptable
{
    Task<TResult> InterceptWithLogAsync<TResult>(Expression<Func<Task<TResult>>> expression);

    Task InterceptWithLogAsync(Expression<Func<Task>> expression);

    TResult InterceptWithLog<TResult>(Expression<Func<TResult>> expression);

    [LogRunner]
    void InterceptWithLog<T>(Expression<Action> expression);
}