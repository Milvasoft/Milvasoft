using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.FileOperations.Abstract;
using Milvasoft.FileOperations.Concrete;
using System.Text;

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
    public static FileOperationsBuilder WithJsonOperations(this FileOperationsBuilder builder,
                                                          Action<IJsonFileOperationOptions> jsonOptions)
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
    /// <param name="basePath"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static FileOperationsBuilder WithJsonOperations(this FileOperationsBuilder builder, string basePath = null, Encoding encoding = null)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithJsonOperations(jsonOptions: null);

        var section = builder.ConfigurationManager.GetSection(JsonFileOperationsOptions.SectionName);

        builder.Services.AddOptions<JsonFileOperationsOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        builder.Services.PostConfigure<JsonFileOperationsOptions>(opt =>
        {
            opt.Encoding = encoding ?? opt.Encoding;
            opt.BasePath = basePath ?? opt.BasePath;
        });

        var options = section.Get<JsonFileOperationsOptions>();

        builder.WithJsonOperations(opt =>
        {
            opt.Lifetime = options.Lifetime;
            opt.EncryptionKey = options.EncryptionKey;
            opt.CultureCode = options.CultureCode;
            opt.Encoding = encoding ?? opt.Encoding;
            opt.BasePath = basePath ?? opt.BasePath;
        });

        return builder;
    }
}
