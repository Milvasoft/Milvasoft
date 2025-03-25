using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.MultiTenancy.ResolutionStrategy;
using Milvasoft.MultiTenancy.Store;
using System.Collections.Concurrent;

namespace Milvasoft.IntegrationTests.Client.Fixtures;

/// <summary>
/// Resolve the test to a tenant identifier
/// </summary>
/// <remarks>
/// Creates new instance of <see cref="TestTenantIdResolutionStrategy"/>
/// </remarks>
public class TestTenantIdResolutionStrategy : ITenantResolutionStrategy<TenantId>
{

    /// <summary>
    /// Get the tenant identifier from header.
    /// </summary>
    /// <returns></returns>
    public Task<TenantId> GetTenantIdentifierAsync() => Task.FromResult(GetTenantIdentifier());

    /// <summary>
    /// Get the tenant identifier from header.
    /// </summary>
    /// <returns></returns>
    public TenantId GetTenantIdentifier() => new("test_1");
}

public class TestTenantStore<TTenant, TKey> : ITenantStore<TTenant, TKey>
    where TKey : struct, IEquatable<TKey>
    where TTenant : class, IMilvaTenantBase<TKey>, new()
{
    private static readonly ConcurrentBag<TTenant> _tenants = [];

    /// <summary>
    /// Returns a tenant according to <paramref name="identifier"/>.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public Task<TTenant> GetTenantAsync(TKey identifier) => Task.FromResult(_tenants.FirstOrDefault(i => i.Id.Equals(identifier)));

    /// <summary>
    /// Sets a tenant with a given identifier.
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="tenant"></param>
    /// <returns></returns>
    public Task<bool> SetTenantAsync(TTenant tenant)
    {
        _tenants.Add(tenant);

        return Task.FromResult(true);
    }
}
