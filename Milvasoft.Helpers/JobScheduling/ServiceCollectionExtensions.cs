using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Helpers.Exceptions;
using System;

namespace Milvasoft.Helpers.JobScheduling;

/// <summary>
/// Provides scheduled job operations.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds scheduled cron job to application.
    /// </summary>
    /// <typeparam name="TJob"></typeparam>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IServiceCollection AddMilvaCronJob<TJob>(this IServiceCollection services, Action<IScheduleConfig> options) where TJob : MilvaCronJobService
    {
        if (options == null)
            throw new MilvaDeveloperException("Please provide Schedule Configurations.");

        var config = new ScheduleConfig();

        options.Invoke(config);

        if (string.IsNullOrWhiteSpace(config.CronExpression))
            throw new MilvaDeveloperException("Empty Cron Expression is not allowed.");

        services.AddSingleton<IScheduleConfig>(config);

        services.AddHostedService<TJob>();

        return services;
    }
}
