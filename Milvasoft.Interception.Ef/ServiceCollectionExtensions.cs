using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Ef.Transaction;
using Milvasoft.Interception.Ef.WithNoLock;

namespace Milvasoft.Interception.Ef;
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
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
    /// <typeparam name="T">Service type to be decorated</typeparam>
    public static InterceptionBuilder WithTransactionInterceptor(this InterceptionBuilder builder)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithTransactionInterceptor(interceptionOptions: null);

        var section = builder.ConfigurationManager.GetSection(TransactionInterceptionOptions.SectionName);

        builder.Services.AddOptions<TransactionInterceptionOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        var options = (ITransactionInterceptionOptions)section.Get<TransactionInterceptionOptions>();

        builder.WithTransactionInterceptor(interceptionOptions: (opt) =>
        {
            opt.InterceptorLifetime = options.InterceptorLifetime;
            opt.DbContextAssemblyQualifiedName = options.DbContextAssemblyQualifiedName;
        });

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
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
    /// <typeparam name="T">Service type to be decorated</typeparam>
    public static InterceptionBuilder WithNoLockInterceptor(this InterceptionBuilder builder)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithNoLockInterceptor(interceptionOptions: null);

        var section = builder.ConfigurationManager.GetSection(WithNoLockInterceptionOptions.SectionName);

        builder.Services.AddOptions<WithNoLockInterceptionOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        var options = (IWithNoLockInterceptionOptions)section.Get<WithNoLockInterceptionOptions>();

        builder.WithNoLockInterceptor(interceptionOptions: (opt) =>
        {
            opt.InterceptorLifetime = options.InterceptorLifetime;
            opt.DbContextAssemblyQualifiedName = options.DbContextAssemblyQualifiedName;
        });

        return builder;
    }
}
