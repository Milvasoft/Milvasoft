using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Core.Abstractions;

/// <summary>
/// Dummy class for builder implementations.
/// </summary>
public interface IMilvaBuilder
{
    /// <summary>
    /// Service collection.
    /// </summary>
    public IServiceCollection Services { get; }
}
