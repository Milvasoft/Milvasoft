﻿using MediatR;
using Milvasoft.Components.Rest.MilvaResponse;

namespace Milvasoft.Components.CQRS.Command;

/// <summary>
/// Abstraction for <see cref="Response"/> typed requests. 
/// </summary>
public interface ICommand : IRequest<Response>
{
}

/// <summary>
/// Abstraction for <see cref="Response{T}"/> typed requests.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICommand<T> : IRequest<Response<T>>
{
}
