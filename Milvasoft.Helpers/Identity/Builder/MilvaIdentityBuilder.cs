using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Identity.Abstract;
using Milvasoft.Helpers.Identity.Concrete;
using Milvasoft.Helpers.Identity.Concrete.Entity;
using Milvasoft.Helpers.Identity.Concrete.Options;
using System;

namespace Milvasoft.Helpers.Identity.Builder;

/// <summary>
/// Implements the standard Identity password hashing.
/// </summary>
public class MilvaIdentityBuilder<TUser, TKey> where TUser : MilvaUser<TKey> where TKey : IEquatable<TKey>
{
    private readonly IServiceCollection _services;

    /// <summary>
    /// Creates new instance of <see cref="MilvaIdentityBuilder{TUser, TKey}"/>.
    /// </summary>
    /// <param name="services"></param>
    public MilvaIdentityBuilder(IServiceCollection services)
    {
        services.AddScoped<IMilvaPasswordHasher, MilvaPasswordHasher>();
        services.AddDataProtection();
        _services = services;
    }

    /// <summary>
    /// Adds and configures the identity system for the specified User and Role types.
    /// </summary>
    /// <param name="setupAction">An action to configure the <see cref="MilvaIdentityOptions"/>.</param>
    /// <returns></returns>
    public MilvaIdentityBuilder<TUser, TKey> WithOptions(Action<MilvaIdentityOptions> setupAction)
    {
        if (setupAction == null)
            throw new MilvaDeveloperException("Please provide identity options.");

        var config = new MilvaIdentityOptions();

        setupAction.Invoke(config);

        _services.AddSingleton(config);

        return this;
    }

    /// <summary>
    /// Adds and configures the identity system for the specified User and Role types.
    /// </summary>
    /// <returns></returns>
    public MilvaIdentityBuilder<TUser, TKey> WithUserManager<TUserManager>() where TUserManager : class, IMilvaUserManager<TUser, TKey>
    {
        _services.AddScoped<IMilvaUserManager<TUser, TKey>, TUserManager>();

        return this;
    }

    /// <summary>
    /// Adds and configures the identity system for the specified User and Role types.
    /// </summary>
    /// <returns></returns>
    public MilvaIdentityBuilder<TUser, TKey> WithPasswordHasherOptions(Action<MilvaPasswordHasherOptions> setupAction)
    {
        if (setupAction == null)
            throw new MilvaDeveloperException("Please provide password hasher options.");

        _services.Configure(setupAction);

        return this;
    }
}
