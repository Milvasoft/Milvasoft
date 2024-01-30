﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Core.Abstractions;

/// <summary>
/// Milva localization options.
/// </summary>
public interface ILocalizationOptions
{
    /// <summary>
    /// Localization manager service lifetime.
    /// </summary>
    public ServiceLifetime ManagerLifetime { get; set; }

    /// <summary>
    /// Key format
    /// </summary>
    public string KeyFormat { get; set; }

    /// <summary>
    /// Fortmatted key creator delegate.
    /// </summary>
    public Func<string, string> KeyFormatDelegate { get; set; }

    /// <summary>
    /// LocalizationMemoryCache helps speeding up getting localized values from data stores.
    /// It is helpful to set to false during development mode and to true in production.
    /// Default value: false.
    /// </summary>
    public bool UseInMemoryCache { get; set; }

    /// <summary>
    /// Memory cache entry options.
    /// </summary>
    public MemoryCacheEntryOptions MemoryCacheEntryOptions { get; set; }
}