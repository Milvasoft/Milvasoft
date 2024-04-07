using MediatR;
using Milvasoft.Components.Rest.MilvaResponse;

namespace Milvasoft.Components.CQRS.Command;

/// <summary>
/// Abstraction for <see cref="Response"/> typed request handler.
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Response> where TCommand : ICommand
{
}

/// <summary>
/// Abstraction for <see cref="Response{T}"/> typed request handler.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="T">The command response type.</typeparam>
public interface ICommandHandler<in TCommand, T> : IRequestHandler<TCommand, Response<T>> where TCommand : ICommand<T>
{
}
