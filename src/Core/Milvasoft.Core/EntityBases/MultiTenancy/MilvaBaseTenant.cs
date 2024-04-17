namespace Milvasoft.Core.EntityBases.MultiTenancy;

/// <summary>
/// Tenant base.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public abstract class MilvaBaseTenant<TKey> : FullAuditableEntity<TKey>, IMilvaBaseTenant<TKey> where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Tenancy name of tenant.
    /// </summary>
    public virtual string TenancyName { get; protected set; }

    /// <summary>
    /// Display name of the Tenant.
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// ENCRYPTED connection string of the tenant database.
    /// Can be null if this tenant is stored in host database.
    /// </summary>
    public virtual string ConnectionString { get; set; }

    /// <summary>
    /// Is this tenant active?
    /// If as tenant is not active, no user of this tenant can use the application.
    /// </summary>
    public virtual bool IsActive { get; set; }
}