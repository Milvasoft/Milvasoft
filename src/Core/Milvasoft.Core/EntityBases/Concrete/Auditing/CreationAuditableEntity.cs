using Milvasoft.Core.EntityBases.MultiTenancy;

namespace Milvasoft.Core.EntityBases.Concrete.Auditing;

/// <summary>
/// Determines entity's creation is auditable.
/// </summary>
/// <typeparam name="TKey">Type of the user</typeparam>
public abstract class CreationAuditableEntity<TKey> : BaseEntity<TKey>, ICreationAuditable<TKey>
    where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Creation date of entity.
    /// </summary>
    public virtual DateTime? CreationDate { get; set; }

    /// <summary>
    /// Creator of entity.
    /// </summary>
    public virtual string CreatorUserName { get; set; }
}

/// <summary>
/// Determines entity's creation is auditable without user.
/// </summary>
/// <typeparam name="TKey">Type of the user</typeparam>
public abstract class CreationAuditableEntityWithoutUser<TKey> : EntityBase<TKey>, ICreationAuditableWithoutUser<TKey>
{
    /// <summary>
    /// Creation date of entity.
    /// </summary>
    public virtual DateTime? CreationDate { get; set; }
}

/// <summary>
/// Determines entity's creation is auditable and tenant id.
/// </summary>
/// <typeparam name="TKey">Type of the user</typeparam>
public abstract class CreationAuditableEntityWithTenantId<TKey> : CreationAuditableEntity<TKey>, IHasTenantId
    where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Tenant id of entity.
    /// </summary>
    public virtual TenantId TenantId { get; set; }
}

/// <summary>
/// Determines entity's creation is auditable without user and tenant id.
/// </summary>
/// <typeparam name="TKey">Type of the user</typeparam>
public abstract class CreationAuditableEntityWithTenantIdAndWithoutUser<TKey> : CreationAuditableEntityWithoutUser<TKey>, IHasTenantId
{
    /// <summary>
    /// Tenant id of entity.
    /// </summary>
    public virtual TenantId TenantId { get; set; }
}
