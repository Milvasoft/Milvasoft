﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Identity.Abstract;
using Milvasoft.Identity.Concrete;
using Milvasoft.Identity.Concrete.Entity;
using Milvasoft.Identity.Concrete.Options;
using Milvasoft.Identity.TokenProvider;
using Milvasoft.Identity.TokenProvider.AuthToken;

namespace Milvasoft.Identity.Builder;

/// <summary>
/// Implements the standard Identity password hashing.
/// </summary>
public class MilvaIdentityBuilder<TUser, TKey> where TUser : MilvaUser<TKey> where TKey : IEquatable<TKey>
{
    private readonly IServiceCollection _services;
    private readonly IConfigurationManager _configurationManager;
    private bool _optionsConfiguredFromConfigurationManager = false;

    /// <summary>
    /// Configured identity options.
    /// </summary>
    public MilvaIdentityOptions IdentityOptions { get; private set; }

    /// <summary>
    /// Creates new instance of <see cref="MilvaIdentityBuilder{TUser, TKey}"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configurationManager"></param>
    public MilvaIdentityBuilder(IServiceCollection services, IConfigurationManager configurationManager)
    {
        services.AddScoped<IMilvaPasswordHasher, MilvaPasswordHasher>();
        services.AddDataProtection();
        _services = services;
        _configurationManager = configurationManager;
    }

    /// <summary>
    /// Adds and configures the identity system for the specified User and Role types.
    /// </summary>
    /// <returns></returns>
    public MilvaIdentityBuilder<TUser, TKey> WithDefaultUserManager()
    {
        _services.AddScoped<IMilvaUserManager<TUser, TKey>, MilvaUserManager<TUser, TKey>>();

        return this;
    }

    /// <summary>
    /// Adds and configures the identity system for the specified User and Role types.
    /// </summary>
    /// <returns></returns>
    public MilvaIdentityBuilder<TUser, TKey> WithDefaultTokenManager()
    {
        _services.AddScoped<IMilvaTokenManager, MilvaTokenManager>();

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
    public MilvaIdentityBuilder<TUser, TKey> WithTokenManager<TTokenManager>() where TTokenManager : class, IMilvaTokenManager
    {
        _services.AddScoped<IMilvaTokenManager, TTokenManager>();

        return this;
    }

    /// <summary>
    /// Adds and configures the identity system for the specified User and Role types.
    /// </summary>
    /// <param name="identityOptions">An action to configure the <see cref="MilvaIdentityOptions"/>.</param>
    /// <returns></returns>
    public MilvaIdentityBuilder<TUser, TKey> WithOptions(Action<MilvaIdentityOptions> identityOptions)
    {
        if (identityOptions == null)
            throw new MilvaDeveloperException("Please provide identity options.");

        var config = new MilvaIdentityOptions();

        identityOptions.Invoke(config);

        config.Token.TokenValidationParameters.IssuerSigningKey = config.Token.GetSecurityKey();

        _services.AddSingleton(config);

        Rfc6238AuthenticationService.UseUtcForDateTimes = config.Token.UseUtcForDateTimes;

        IdentityOptions = config;

        return this;
    }

    /// <summary>
    /// Adds and configures the identity system for the specified User and Role types.
    /// </summary>
    /// <returns></returns>
    public MilvaIdentityBuilder<TUser, TKey> WithOptions()
    {
        if (_configurationManager == null)
            return WithOptions(identityOptions: null);

        var section = _configurationManager.GetSection(MilvaIdentityOptions.SectionName);

        _services.AddOptions<MilvaIdentityOptions>()
                 .Bind(section)
                 .ValidateDataAnnotations();

        var options = section.Get<MilvaIdentityOptions>();

        options.Token.TokenValidationParameters.IssuerSigningKey = options.Token.GetSecurityKey();

        _services.PostConfigure<MilvaIdentityOptions>(opt =>
        {
            opt.Token.TokenValidationParameters.IssuerSigningKey = options.Token.TokenValidationParameters.IssuerSigningKey;
        });

        WithOptions(identityOptions: (opt) =>
        {
            opt.User = options.User;
            opt.Password = options.Password;
            opt.Lockout = options.Lockout;
            opt.SignIn = options.SignIn;
            opt.Token = options.Token;
        });

        _optionsConfiguredFromConfigurationManager = true;

        return this;
    }

    /// <summary>
    /// Adds and configures the identity system for the specified User and Role types.
    /// </summary>
    /// <returns></returns>
    public MilvaIdentityBuilder<TUser, TKey> PostConfigureIdentityOptions(Action<MilvaIdentityPostConfigureOptions> setupAction)
    {
        if (setupAction == null)
            throw new MilvaDeveloperException("Please provide post configure options.");

        if (!_optionsConfiguredFromConfigurationManager)
            throw new MilvaDeveloperException("Please configure options with WithOptions() builder method before post configuring.");

        var config = new MilvaIdentityPostConfigureOptions();

        setupAction.Invoke(config);

        _services.UpdateSingletonInstance<MilvaIdentityOptions>(opt =>
        {
            opt.Password.Hasher.RandomNumberGenerator = config.RandomNumberGenerator ?? opt.Password.Hasher.RandomNumberGenerator;
        });

        _services.PostConfigure<MilvaIdentityOptions>(opt =>
        {
            opt.Password.Hasher.RandomNumberGenerator = config.RandomNumberGenerator ?? opt.Password.Hasher.RandomNumberGenerator;
        });

        return this;
    }
}
