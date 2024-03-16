namespace Milvasoft.Core.EntityBases.Concrete.Auditing.Dto;

/// <summary>
/// Determines entity is fully auditable soft deletable entity.
/// </summary>
/// <typeparam name="TKey">Type of the user</typeparam>
public abstract class FullAuditableDto<TKey> : AuditableDto<TKey>, IFullAuditable<TKey> where TKey : struct, IEquatable<TKey>
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
public abstract class FullAuditableDtoWithoutUser<TKey> : AuditableDtoWithoutUser<TKey>, IFullAuditableWithoutUser<TKey>
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
