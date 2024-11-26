using Milvasoft.Core.EntityBases.MultiTenancy;

namespace Milvasoft.Core.EntityBases.Concrete.Auditing;

/// <summary>
/// Determines entity is fully auditable soft deletable entity.
/// </summary>
/// <typeparam name="TKey">Type of the user</typeparam>
public abstract class FullAuditableEntity<TKey> : AuditableEntity<TKey>, IFullAuditable<TKey> where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Deletion date of entity.
    /// </summary>
    public virtual DateTime? DeletionDate { get; set; }

    /// <summary>
    /// Deleter of entity.
    /// </summary>
    public virtual string DeleterUserName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether IsDeleted.
    /// </summary>
    public virtual bool IsDeleted { get; set; }
}

/// <summary>
/// Determines entity is fully auditable without user.
/// </summary>
/// <typeparam name="TKey">Type of the user</typeparam>
public abstract class FullAuditableEntityWithoutUser<TKey> : AuditableEntityWithoutUser<TKey>, IFullAuditableWithoutUser<TKey>
{
    /// <summary>
    /// Deletion date of entity.
    /// </summary>
    public virtual DateTime? DeletionDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether IsDeleted.
    /// </summary>
    public virtual bool IsDeleted { get; set; }
}

/// <summary>
/// Determines entity is fully auditable soft deletable entity and tenant id.
/// </summary>
/// <typeparam name="TKey">Type of the user</typeparam>
public abstract class FullAuditableEntityWithTenantId<TKey> : FullAuditableEntity<TKey>, IHasTenantId where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Tenant id of entity.
    /// </summary>
    public virtual TenantId TenantId { get; set; }
}

/// <summary>
/// Determines entity is fully auditable without user and tenant id.
/// </summary>
/// <typeparam name="TKey">Type of the user</typeparam>
public abstract class FullAuditableEntityWithTenantIdAndWithoutUser<TKey> : FullAuditableEntityWithoutUser<TKey>, IHasTenantId
{
    /// <summary>
    /// Tenant id of entity.
    /// </summary>
    public virtual TenantId TenantId { get; set; }
}
