using MediatR;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;

namespace Milvasoft.Components.CQRS.Query;

/// <summary>
/// Abstraction for <see cref="Response{T}"/> typed requests.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IListRequestQuery<T> : IRequest<ListResponse<T>>, IListRequest
{
}
