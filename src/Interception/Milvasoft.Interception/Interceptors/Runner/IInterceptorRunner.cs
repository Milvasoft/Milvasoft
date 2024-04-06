using Milvasoft.Interception.Interceptors.Logging;
using System.Linq.Expressions;

namespace Milvasoft.Interception.Interceptors.Runner;

public interface IInterceptorRunner : IInterceptable
{
    Task<TResult> InterceptWithLogAsync<T, TResult>(Expression<Func<Task<TResult>>> expression);

    Task InterceptWithLogAsync<T>(Expression<Func<Task>> expression);

    TResult InterceptWithLog<T, TResult>(Expression<Func<TResult>> expression);

    [LogRunner]
    void InterceptWithLog<T>(Expression<Action> expression);
}