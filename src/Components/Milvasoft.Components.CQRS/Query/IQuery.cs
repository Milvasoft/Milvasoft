using MediatR;
using Milvasoft.Components.Rest.MilvaResponse;

namespace Milvasoft.Components.CQRS.Query;

/// <summary>
/// Abstraction for <see cref="Response"/> typed requests.
/// </summary>
public interface IQuery : IRequest<Response>
{
}

/// <summary>
/// Abstraction for <see cref="Response{T}"/> typed requests.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IQuery<T> : IRequest<Response<T>>
{
}
