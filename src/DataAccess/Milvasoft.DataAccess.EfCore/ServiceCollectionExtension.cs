using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
    /// <returns></returns>
    public static IServiceCollection ConfigureMilvaDataAccess(this IServiceCollection services, IConfigurationManager configurationManager)
    {
        if (configurationManager == null)
            return services.ConfigureMilvaDataAccess(dataAccessConfiguration: null);

        var section = configurationManager.GetSection(DataAccessConfiguration.SectionName);

        services.AddOptions<DataAccessConfiguration>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        var options = section.Get<DataAccessConfiguration>();

        services.ConfigureMilvaDataAccess(dataAccessConfiguration: (opt) =>
        {
            opt.DbContext = options.DbContext;
            opt.Repository = options.Repository;
            opt.Auditing = options.Auditing;
        });

        return services;
    }

    /// <summary>
    /// Adds required services to service collection for configuring milva specific context features.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="postConfigureAction"></param>
    /// <returns></returns>
    public static IServiceCollection PostConfigureMilvaDataAccess(this IServiceCollection services, Action<DataAccessPostConfigureConfiguration> postConfigureAction)
    {
        if (postConfigureAction == null)
            throw new MilvaDeveloperException("Please provide post configure options.");

        if (!services.Any(s => s.ServiceType == typeof(IConfigureOptions<DataAccessConfiguration>)))
            throw new MilvaDeveloperException("Please configure options with ConfigureMilvaDataAccess() method before post configuring.");

        var config = new DataAccessPostConfigureConfiguration();

        postConfigureAction?.Invoke(config);

        services.UpdateSingletonInstance<IDataAccessConfiguration>(opt =>
        {
            opt.DbContext.GetCurrentUserNameMethod = config.GetCurrentUserNameMethod ?? opt.DbContext.GetCurrentUserNameMethod;
        });

        services.PostConfigure<DataAccessConfiguration>(opt =>
        {
            opt.DbContext.GetCurrentUserNameMethod = config.GetCurrentUserNameMethod ?? opt.DbContext.GetCurrentUserNameMethod;
        });

        return services;
    }
}
