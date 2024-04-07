using Milvasoft.Interception.Interceptors.Logging;
using System.Linq.Expressions;

namespace Milvasoft.Interception.Interceptors.Runner;

/// <summary>
/// When using methods contained in external dlls, it is not possible to mark these methods with attribute.
/// This abstraction allows milva interceptors to intercept these methods.
/// You can write your own runner with implement <see cref="IInterceptorRunner"/> interface.
/// </summary>
public interface IInterceptorRunner : IInterceptable
{
    /// <summary>
    /// It ensures that the action(<paramref name="expression"/>) sent as a parameter is logged by the <see cref="LogInterceptor"/>.
    /// </summary>
    /// <typeparam name="TResult">Result type of <paramref name="expression"/></typeparam>
    /// <param name="expression"></param>
    /// <returns>Result of <paramref name="expression"/></returns>
    Task<TResult> InterceptWithLogAsync<TResult>(Expression<Func<Task<TResult>>> expression);

    /// <summary>
    /// It ensures that the action(<paramref name="expression"/>) sent as a parameter is logged by the <see cref="LogInterceptor"/>.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    Task InterceptWithLogAsync(Expression<Func<Task>> expression);

    /// <summary>
    /// It ensures that the action(<paramref name="expression"/>) sent as a parameter is logged by the <see cref="LogInterceptor"/>.
    /// </summary>
    /// <typeparam name="TResult">Result type of <paramref name="expression"/></typeparam>
    /// <param name="expression"></param>
    /// <returns>Result of <paramref name="expression"/></returns>
    TResult InterceptWithLog<TResult>(Expression<Func<TResult>> expression);

    /// <summary>
    /// It ensures that the action(<paramref name="expression"/>) sent as a parameter is logged by the <see cref="LogInterceptor"/>.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    void InterceptWithLog<T>(Expression<Action> expression);
}
