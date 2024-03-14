using MediatR;
using Milvasoft.Components.Rest.Response;

namespace Milvasoft.Components.CQRS.Query;

/// <summary>
/// Abstraction for <see cref="Response"/> typed query handler.
/// </summary>
/// <typeparam name="TQuery"></typeparam>
public interface IQueryHandler<in TQuery> : IRequestHandler<TQuery, IResponse> where TQuery : IQuery
{
}

/// <summary>
/// Abstraction for <see cref="Response{T}"/> typed query handler.
/// </summary>
/// <typeparam name="TQuery">The command type.</typeparam>
/// <typeparam name="T">The command response type.</typeparam>
public interface IQueryHandler<in TQuery, T> : IRequestHandler<TQuery, IResponse<T>> where TQuery : IQuery<T>
{
}
