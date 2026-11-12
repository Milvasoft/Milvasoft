using Milvasoft.Core.EntityBases.MultiTenancy;

namespace Milvasoft.MultiTenancy.Context;

/// <summary>
/// Scoped holder of the tenant of the current request or unit of work.
/// It is filled once (by the tenant middleware for HTTP requests, or explicitly for background work) and read by everything else,
/// for example the multi tenancy DbContext factory.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Whether a validated tenant is set.
    /// </summary>
    bool IsResolved { get; }

    /// <summary>
    /// Identifier of the current tenant (boxed), or null when <see cref="IsResolved"/> is false.
    /// </summary>
    object TenantIdentifier { get; }

    /// <summary>
    /// The current tenant, or null when <see cref="IsResolved"/> is false.
    /// </summary>
    object Tenant { get; }
}

/// <summary>
/// Scoped holder of the tenant of the current request or unit of work.
/// </summary>
/// <typeparam name="TTenant"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface ITenantContext<TTenant, TKey> : ITenantContext
    where TTenant : class, IMilvaTenantBase<TKey>
    where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// The current tenant, or null when it is not resolved.
    /// </summary>
    new TTenant Tenant { get; }

    /// <summary>
    /// Identifier of the current tenant, or null when it is not resolved.
    /// </summary>
    new TKey? TenantIdentifier { get; }

    /// <summary>
    /// Sets the tenant of the current scope. Use it for work that does not run through the tenant middleware (background jobs, tests).
    /// The tenant can be set only once per scope; setting a different tenant afterwards throws <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <param name="tenant">A validated tenant (for example from the tenant store).</param>
    void SetTenant(TTenant tenant);
}

/// <summary>
/// Default <see cref="ITenantContext{TTenant, TKey}"/> implementation. Registered as scoped.
/// </summary>
/// <typeparam name="TTenant"></typeparam>
/// <typeparam name="TKey"></typeparam>
public class TenantContext<TTenant, TKey> : ITenantContext<TTenant, TKey>
    where TTenant : class, IMilvaTenantBase<TKey>
    where TKey : struct, IEquatable<TKey>
{
    /// <inheritdoc/>
    public bool IsResolved => Tenant != null;

    /// <inheritdoc/>
    public TTenant Tenant { get; private set; }

    /// <inheritdoc/>
    public TKey? TenantIdentifier => Tenant?.Id;

    object ITenantContext.TenantIdentifier => Tenant?.Id;

    object ITenantContext.Tenant => Tenant;

    /// <inheritdoc/>
    public void SetTenant(TTenant tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        if (TenantIdentifierHelper.IsEmpty(tenant.Id))
            throw new ArgumentException("Tenant id cannot be empty.", nameof(tenant));

        if (Tenant != null && !TenantIdentifierHelper.AreEqual(Tenant.Id, tenant.Id))
            throw new InvalidOperationException("The tenant of the current scope is already set to another tenant.");

        Tenant = tenant;
    }
}

/// <summary>
/// Helpers for tenant identifiers.
/// </summary>
public static class TenantIdentifierHelper
{
    /// <summary>
    /// Whether <paramref name="identifier"/> is empty (default value; <see cref="TenantId"/> without a tenancy name).
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public static bool IsEmpty<TKey>(TKey identifier) where TKey : struct, IEquatable<TKey>
        => identifier is TenantId tenantId ? string.IsNullOrWhiteSpace(tenantId.TenancyName) : EqualityComparer<TKey>.Default.Equals(identifier, default);

    /// <summary>
    /// Whether two identifiers are equal.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool AreEqual<TKey>(TKey left, TKey right) where TKey : struct, IEquatable<TKey>
        => left is TenantId leftTenantId && right is TenantId rightTenantId
            ? leftTenantId == rightTenantId
            : EqualityComparer<TKey>.Default.Equals(left, right);
}
