using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;

namespace Milvasoft.Interception.Builder;

/// <summary>
/// Configure caching services.
/// </summary>
/// <remarks>
/// Creates new instance of <see cref="CacheBuilder"/>.
/// </remarks>
/// <param name="services"></param>
/// <param name="configurationManager"></param>
public sealed class InterceptionBuilder(IServiceCollection services, IConfigurationManager configurationManager = null) : IMilvaBuilder
{
    public IServiceCollection Services { get; } = services;
    public IConfigurationManager ConfigurationManager { get; } = configurationManager;

}
