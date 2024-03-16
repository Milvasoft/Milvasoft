using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Milvasoft.FileOperations.Abstract;
using Milvasoft.FileOperations.Concrete;

namespace Milvasoft.FileOperations.Builder;

/// <summary>
/// Provides registration of caching services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds json file operations services to service collection. Adds <see cref="IJsonFileOperationOptions"/> as singleton to services too.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configurationManager"></param>
    /// <returns></returns>
    public static FileOperationsBuilder AddFileOperations(this IServiceCollection services,
                                                          IConfigurationManager configurationManager = null)
    => new(services, configurationManager);

    /// <summary>
    /// Adds json file operations services to service collection. Adds <see cref="IJsonFileOperationOptions"/> as singleton to services too.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="jsonOptions"></param>
    /// <returns></returns>
    public static FileOperationsBuilder WithJsonOperations(this FileOperationsBuilder builder, Action<IJsonFileOperationOptions> jsonOptions)
    {
        var config = new JsonFileOperationsOptions();

        jsonOptions?.Invoke(config);

        builder.Services.AddSingleton<IJsonFileOperationOptions>(config);

        builder.Services.Add(ServiceDescriptor.Describe(typeof(IJsonOperations), typeof(JsonOperations), config.Lifetime));

        return builder;
    }

    /// <summary>
    /// Adds json file operations services to service collection. Adds <see cref="IJsonFileOperationOptions"/> as singleton to services too.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static FileOperationsBuilder WithJsonOperations(this FileOperationsBuilder builder)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithJsonOperations(jsonOptions: null);

        var section = builder.ConfigurationManager.GetSection(JsonFileOperationsOptions.SectionName);

        builder.Services.AddOptions<JsonFileOperationsOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        var options = section.Get<JsonFileOperationsOptions>();

        builder.WithJsonOperations(opt =>
        {
            opt.BasePath = options.BasePath;
            opt.Lifetime = options.Lifetime;
            opt.EncryptionKey = options.EncryptionKey;
        });

        return builder;
    }

    /// <summary>
    /// Post configuration.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postConfigureAction"></param>
    /// <returns></returns>
    public static FileOperationsBuilder PostConfigureJsonOperationsOptions(this FileOperationsBuilder builder, Action<JsonFileOperationsPostConfigureOptions> postConfigureAction)
    {
        if (postConfigureAction == null)
            throw new MilvaDeveloperException("Please provide post configure options.");

        if (!builder.Services.Any(s => s.ServiceType == typeof(IConfigureOptions<JsonFileOperationsOptions>)))
            throw new MilvaDeveloperException("Please configure options with WithOptions() builder method before post configuring.");

        var config = new JsonFileOperationsPostConfigureOptions();

        postConfigureAction?.Invoke(config);

        builder.Services.UpdateSingletonInstance<IJsonFileOperationOptions>(opt =>
        {
            opt.BasePath = config.BasePath ?? opt.BasePath;
            opt.CultureInfo = config.CultureInfo ?? opt.CultureInfo;
            opt.Encoding = config.Encoding ?? opt.Encoding;
        });

        builder.Services.PostConfigure<JsonFileOperationsOptions>(opt =>
        {
            opt.BasePath = config.BasePath ?? opt.BasePath;
            opt.CultureInfo = config.CultureInfo ?? opt.CultureInfo;
            opt.Encoding = config.Encoding ?? opt.Encoding;
        });

        return builder;
    }
}
