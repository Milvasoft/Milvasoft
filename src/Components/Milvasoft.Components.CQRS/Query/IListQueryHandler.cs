using MediatR;
using Milvasoft.Components.Rest.MilvaResponse;

namespace Milvasoft.Components.CQRS.Query;

/// <summary>
/// Abstraction for <see cref="Response{T}"/> typed query handler.
/// </summary>
/// <typeparam name="TQuery">The command type.</typeparam>
/// <typeparam name="T">The command response type.</typeparam>
public interface IListQueryHandler<in TQuery, T> : IRequestHandler<TQuery, ListResponse<T>> where TQuery : IListRequestQuery<T>
{
}
