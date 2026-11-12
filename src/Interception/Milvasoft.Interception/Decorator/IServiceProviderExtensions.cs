using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Interception.Decorator;

/// <summary>
/// Service provider extensions.
/// </summary>
public static class IServiceProviderExtensions
{
    /// <summary>
    /// Gets instance of <paramref name="descriptor"/>. Keyed descriptors are supported.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    public static object GetInstance(this IServiceProvider provider, ServiceDescriptor descriptor)
        => provider.GetInstance(descriptor, descriptor.IsKeyedService ? descriptor.ServiceKey : null, out _);

    /// <summary>
    /// Gets instance of <paramref name="descriptor"/>. Keyed descriptors are supported.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="descriptor"></param>
    /// <param name="serviceKey">Requested service key for keyed descriptors (may differ from the descriptor's key when it is <see cref="KeyedService.AnyKey"/>).</param>
    /// <param name="createdByProvider">
    /// True when the instance was created by this call (from an implementation type or factory) and is therefore owned by the container;
    /// false when it is a user supplied instance.
    /// </param>
    /// <returns></returns>
    public static object GetInstance(this IServiceProvider provider, ServiceDescriptor descriptor, object serviceKey, out bool createdByProvider)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        createdByProvider = true;

        if (descriptor.IsKeyedService)
        {
            if (descriptor.KeyedImplementationInstance != null)
            {
                createdByProvider = false;
                return descriptor.KeyedImplementationInstance;
            }

            if (descriptor.KeyedImplementationType != null)
                return ActivatorUtilities.CreateInstance(provider, descriptor.KeyedImplementationType);

            return descriptor.KeyedImplementationFactory(provider, serviceKey);
        }

        if (descriptor.ImplementationInstance != null)
        {
            createdByProvider = false;
            return descriptor.ImplementationInstance;
        }

        if (descriptor.ImplementationType != null)
            return ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType);

        return descriptor.ImplementationFactory(provider);
    }
}
