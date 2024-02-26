using Milvasoft.Core.Abstractions;
using System.Linq.Expressions;

namespace Milvasoft.Interception.Interceptors.Runner;

public interface IInterceptorRunner : IInterceptable
{
    Task<TResult> InterceptWithLogAsync<T, TResult>(Expression<Func<Task<TResult>>> function);
}