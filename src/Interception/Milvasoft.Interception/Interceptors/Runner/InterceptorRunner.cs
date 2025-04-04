﻿using Milvasoft.Interception.Interceptors.Logging;
using System.Linq.Expressions;

namespace Milvasoft.Interception.Interceptors.Runner;

/// <summary>
/// When using methods contained in external dlls, it is not possible to mark these methods with attribute.
/// This abstraction allows milva interceptors to intercept these methods.
/// You can write your own runner with implement <see cref="IInterceptorRunner"/> interface.
/// </summary>
public class InterceptorRunner : IInterceptorRunner
{
    /// <inheritdoc/>
    [LogRunner]
    public virtual async Task<TResult> InterceptWithLogAsync<TResult>(Expression<Func<Task<TResult>>> expression)
    {
        var result = await expression.Compile().Invoke().ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc/>
    [LogRunner]
    public virtual Task InterceptWithLogAsync(Expression<Func<Task>> expression)
        => expression.Compile().Invoke();

    /// <inheritdoc/>
    [LogRunner]
    public virtual TResult InterceptWithLog<TResult>(Expression<Func<TResult>> expression)
    {
        var result = expression.Compile().Invoke();

        return result;
    }

    /// <inheritdoc/>
    [LogRunner]
    public virtual void InterceptWithLog<T>(Expression<Action> expression)
        => expression.Compile().Invoke();
}