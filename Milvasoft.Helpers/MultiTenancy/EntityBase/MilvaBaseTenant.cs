using Milvasoft.Helpers.DataAccess.EfCore.Concrete.Entity;
using Milvasoft.Helpers.MultiTenancy.LifetimeManagement;
using System;

namespace Milvasoft.Helpers.MultiTenancy.EntityBase;

/// <summary>
/// Tenant base.
/// </summary>
/// <typeparam name="TKey"> Your key must override ToString() method correctly to block exceptions in <see cref="MultiTenantContainer{TTenant, TKey}"/>. </typeparam>
/// <typeparam name="TUserKey"></typeparam>
public abstract class MilvaBaseTenant<TUserKey, TKey> : FullAuditableEntity<TUserKey, TKey>, IMilvaBaseTenant<TKey>
    where TKey : struct, IEquatable<TKey> 
    where TUserKey : struct, IEquatable<TUserKey>
{
    /// <summary>
    /// Tenancy name of tenant.
    /// </summary>
    public virtual string TenancyName { get; set; }

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