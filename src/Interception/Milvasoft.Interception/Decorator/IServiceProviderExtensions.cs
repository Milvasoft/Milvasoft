﻿using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Interception.Decorator;

/// <summary>
/// Service provider extensions.
/// </summary>
public static class IServiceProviderExtensions
{
    /// <summary>
    /// Gets instance of <paramref name="descriptor"/>.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    public static object GetInstance(this IServiceProvider provider, ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationInstance != null)
            return descriptor.ImplementationInstance;

        if (descriptor.ImplementationType != null)
            return ActivatorUtilities.CreateInstance(provider, instanceType: descriptor.ImplementationType);

        return descriptor.ImplementationFactory(provider);
    }
}
