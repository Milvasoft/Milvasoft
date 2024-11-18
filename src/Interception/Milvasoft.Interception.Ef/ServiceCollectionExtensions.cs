using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Helpers;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Ef.Transaction;
using Milvasoft.Interception.Ef.WithNoLock;

namespace Milvasoft.Interception.Ef;

/// <summary>
/// Service collection extensions for using ef interceptors.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Transaction

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    public static InterceptionBuilder WithTransactionInterceptor(this InterceptionBuilder builder, Action<ITransactionInterceptionOptions> interceptionOptions)
    {
        var config = new TransactionInterceptionOptions();

        interceptionOptions?.Invoke(config);

        if (!builder.Services.Any(s => s.ServiceType == typeof(TransactionInterceptor)))
            builder.Services.Add(ServiceDescriptor.Describe(typeof(TransactionInterceptor), typeof(TransactionInterceptor), config.InterceptorLifetime));

        builder.Services.AddSingleton<ITransactionInterceptionOptions>(config);

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    public static InterceptionBuilder WithTransactionInterceptor(this InterceptionBuilder builder)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithTransactionInterceptor(interceptionOptions: null);

        var section = builder.ConfigurationManager.GetSection(TransactionInterceptionOptions.SectionName);

        builder.Services.AddOptions<TransactionInterceptionOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        var options = section.Get<TransactionInterceptionOptions>();

        builder.WithTransactionInterceptor(interceptionOptions: (opt) =>
        {
            opt.InterceptorLifetime = options.InterceptorLifetime;
            opt.DbContextAssemblyQualifiedName = options.DbContextAssemblyQualifiedName;
            opt.DbContextType = options.DbContextType;
        });

        return builder;
    }

    /// <summary>
    /// If options are made from the configuration file, configures options that cannot be made from the configuration file.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postConfigureAction"></param>
    /// <returns></returns>
    public static InterceptionBuilder PostConfigureTransactionInterceptionOptions(this InterceptionBuilder builder, Action<TransactionInterceptionPostConfigureOptions> postConfigureAction)
    {
        if (postConfigureAction == null)
            throw new MilvaDeveloperException("Please provide post configure options.");

        if (!builder.Services.Any(s => s.ServiceType == typeof(IConfigureOptions<TransactionInterceptionOptions>)))
            throw new MilvaDeveloperException("Please configure options with WithOptions() builder method before post configuring.");

        var config = new TransactionInterceptionPostConfigureOptions();

        postConfigureAction.Invoke(config);

        builder.Services.UpdateSingletonInstance<ITransactionInterceptionOptions>(opt =>
        {
            opt.DbContextType = config.DbContextType ?? opt.DbContextType;
        });

        builder.Services.PostConfigure<TransactionInterceptionOptions>(opt =>
        {
            opt.DbContextType = config.DbContextType ?? opt.DbContextType;
        });

        return builder;
    }

    #endregion

    #region WithNoLock

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    public static InterceptionBuilder WithNoLockInterceptor(this InterceptionBuilder builder, Action<IWithNoLockInterceptionOptions> interceptionOptions)
    {
        var config = new WithNoLockInterceptionOptions();

        interceptionOptions?.Invoke(config);

        if (!builder.Services.Any(s => s.ServiceType == typeof(WithNoLockInterceptor)))
            builder.Services.Add(ServiceDescriptor.Describe(typeof(WithNoLockInterceptor), typeof(WithNoLockInterceptor), config.InterceptorLifetime));

        builder.Services.AddSingleton<IWithNoLockInterceptionOptions>(config);

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    public static InterceptionBuilder WithNoLockInterceptor(this InterceptionBuilder builder)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithNoLockInterceptor(interceptionOptions: null);

        var section = builder.ConfigurationManager.GetSection(WithNoLockInterceptionOptions.SectionName);

        builder.Services.AddOptions<WithNoLockInterceptionOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        var options = section.Get<WithNoLockInterceptionOptions>();

        builder.WithNoLockInterceptor(interceptionOptions: (opt) =>
        {
            opt.InterceptorLifetime = options.InterceptorLifetime;
            opt.DbContextAssemblyQualifiedName = options.DbContextAssemblyQualifiedName;
            opt.DbContextType = options.DbContextType;
        });

        return builder;
    }

    /// <summary>
    /// If options are made from the configuration file, configures options that cannot be made from the configuration file.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postConfigureAction"></param>
    /// <returns></returns>
    public static InterceptionBuilder PostConfigureNoLockInterceptionOptions(this InterceptionBuilder builder, Action<WithNoLockInterceptionPostConfigureOptions> postConfigureAction)
    {
        if (postConfigureAction == null)
            throw new MilvaDeveloperException("Please provide post configure options.");

        if (!builder.Services.Any(s => s.ServiceType == typeof(IConfigureOptions<WithNoLockInterceptionOptions>)))
            throw new MilvaDeveloperException("Please configure options with WithOptions() builder method before post configuring.");

        var config = new WithNoLockInterceptionPostConfigureOptions();

        postConfigureAction.Invoke(config);

        builder.Services.UpdateSingletonInstance<IWithNoLockInterceptionOptions>(opt =>
        {
            opt.DbContextType = config.DbContextType ?? opt.DbContextType;
        });

        builder.Services.PostConfigure<WithNoLockInterceptionOptions>(opt =>
        {
            opt.DbContextType = config.DbContextType ?? opt.DbContextType;
        });

        return builder;
    }

    #endregion
}
