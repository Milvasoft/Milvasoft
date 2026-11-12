using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IO;
using Milvasoft.Cryptography.Abstract;
using Milvasoft.Cryptography.Concrete;

namespace Milvasoft.Cryptography.Builder;

/// <summary>
/// Provides registration of crypthography services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds crypthography services to service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configurationManager"></param>
    /// <returns></returns>
    public static MilvaCryptographyBuilder AddMilvaCryptography(this IServiceCollection services, IConfigurationManager configurationManager = null)
        => new(services, configurationManager);

    /// <summary>
    /// Adds crypthography services to service collection. Adds <see cref="IMilvaCryptographyOptions"/> as singleton to services too.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the configured key is not 16, 24 or 32 bytes.</exception>
    public static MilvaCryptographyBuilder WithOptions(this MilvaCryptographyBuilder builder, Action<IMilvaCryptographyOptions> options)
    {
        var config = new MilvaCryptographyOptions();

        options?.Invoke(config);

        // Fail at startup instead of at the first encryption.
        MilvaCryptographyProvider.ValidateKey(config.Key);

        builder.Services.AddSingleton<IMilvaCryptographyOptions>(config);

        builder.Services.TryAddSingleton(new RecyclableMemoryStreamManager());

        builder.Services.Add(ServiceDescriptor.Describe(typeof(IMilvaCryptographyProvider), typeof(MilvaCryptographyProvider), config.Lifetime));

        return builder;
    }

    /// <summary>
    /// Adds crypthography services to service collection. Adds <see cref="IMilvaCryptographyOptions"/> as singleton to services too.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown when the configuration section is missing.</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when the configured key is not 16, 24 or 32 bytes.</exception>
    public static MilvaCryptographyBuilder WithOptions(this MilvaCryptographyBuilder builder)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithOptions(options: null);

        var section = builder.ConfigurationManager.GetSection(MilvaCryptographyOptions.SectionName);

        builder.Services.AddOptions<MilvaCryptographyOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        var options = section.Get<MilvaCryptographyOptions>() ?? throw new InvalidOperationException($"'{MilvaCryptographyOptions.SectionName}' configuration section is missing.");

        builder.WithOptions(opt =>
        {
            opt.Lifetime = options.Lifetime;
            opt.Key = options.Key;
            opt.Cipher = options.Cipher;
            opt.Padding = options.Padding;
        });

        return builder;
    }
}
