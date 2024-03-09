﻿using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.MultiLanguage.Manager;

namespace Milvasoft.Core.MultiLanguage.Builder;

/// <summary>
/// Implements the multi language components.
/// </summary>
/// <remarks>
/// Creates new instance of <see cref="MilvaMultiLanguageBuilder"/>.
/// </remarks>
/// <param name="services"></param>
/// <param name="configurationManager"></param>
public class MilvaMultiLanguageBuilder(IServiceCollection services)
{
    private readonly IServiceCollection _services = services;

    /// <summary>
    /// Adds and configures the identity system for the specified User and Role types.
    /// </summary>
    /// <returns></returns>
    public MilvaMultiLanguageBuilder WithDefaultMultiLanguageManager()
    {
        _services.AddScoped<IMultiLanguageManager, MilvaMultiLanguageManager>();

        return this;
    }

    /// <summary>
    /// Adds and configures the identity system for the specified User and Role types.
    /// </summary>
    /// <returns></returns>
    public MilvaMultiLanguageBuilder WithMultiLanguageManager<TMultiLanguageManager>() where TMultiLanguageManager : class, IMultiLanguageManager
    {
        _services.AddScoped<IMultiLanguageManager, TMultiLanguageManager>();

        return this;
    }
}
