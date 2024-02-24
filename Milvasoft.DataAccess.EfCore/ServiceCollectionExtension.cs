using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.DataAccess.EfCore.Configuration;

namespace Milvasoft.DataAccess.EfCore;

/// <summary>
/// Service collection extension for configuring milva specific context features.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Adds required services to service collection for configuring milva specific context features.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="dataAccessConfiguration"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureMilvaDataAccess(this IServiceCollection services, Action<DataAccessConfiguration> dataAccessConfiguration)
    {
        var config = new DataAccessConfiguration();

        dataAccessConfiguration?.Invoke(config);

        services.AddSingleton<IDataAccessConfiguration>(config);

        return services;
    }

    /// <summary>
    /// Adds required services to service collection for configuring milva specific context features.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configurationManager"></param>
    /// <param name="getUserNameDelegate"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureMilvaDataAccess(this IServiceCollection services, IConfigurationManager configurationManager, Func<string> getUserNameDelegate = null)
    {
        if (configurationManager == null)
            return services.ConfigureMilvaDataAccess(dataAccessConfiguration: null);

        var section = configurationManager.GetSection(DataAccessConfiguration.SectionName);

        services.AddOptions<DataAccessConfiguration>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        services.PostConfigure<DataAccessConfiguration>(opt =>
        {
            opt.DbContext.GetCurrentUserNameDelegate = getUserNameDelegate ?? opt.DbContext.GetCurrentUserNameDelegate;
        });

        var options = section.Get<DataAccessConfiguration>();

        options.DbContext.GetCurrentUserNameDelegate = getUserNameDelegate;

        services.ConfigureMilvaDataAccess(dataAccessConfiguration: (opt) =>
        {
            opt.DbContext = options.DbContext;
            opt.Repository = options.Repository;
            opt.Auditing = options.Auditing;
        });

        return services;
    }
}
