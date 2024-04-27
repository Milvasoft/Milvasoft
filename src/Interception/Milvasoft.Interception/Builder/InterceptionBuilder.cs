using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Interception.Builder;

/// <summary>
/// Represents a builder for configuring interception services.
/// </summary>
/// <remarks>
/// This class creates a new instance of the <see cref="InterceptionBuilder"/> and provides methods for configuring interception services.
/// </remarks>
/// <param name="services">The <see cref="IServiceCollection"/> to add the interception services to.</param>
/// <param name="configurationManager">The optional <see cref="IConfigurationManager"/> to be used for configuration.</param>
public sealed class InterceptionBuilder(IServiceCollection services, IConfigurationManager configurationManager = null) : IMilvaBuilder
{
    /// <inheritdoc/>
    public IServiceCollection Services { get; } = services;

    /// <inheritdoc/>
    public IConfigurationManager ConfigurationManager { get; } = configurationManager;
}
