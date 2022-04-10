using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Helpers.Identity.Concrete.Entity;
using System;

namespace Milvasoft.Helpers.Identity.Builder;

/// <summary>
/// Implements the standard Milva Identity.
/// </summary>
public static class MilvaIdentityServiceCollectionExtensions
{
    /// <summary>
    /// Adds the default identity system configuration for the specified User and Role types.
    /// </summary>
    /// <typeparam name="TUser">The type representing a User in the system.</typeparam>
    /// <typeparam name="TKey">The type representing a Key in the system.</typeparam>
    /// <param name="services">The services available in the application.</param>
    /// <returns>An <see cref="MilvaIdentityBuilder{TUser, TKey}"/> for creating and configuring the identity system.</returns>
    public static MilvaIdentityBuilder<TUser, TKey> AddMilvaIdentity<TUser, TKey>(this IServiceCollection services)
        where TUser : MilvaUser<TKey>
        where TKey : IEquatable<TKey>
        => new(services);
}
