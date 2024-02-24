using Milvasoft.Core.EntityBases.Abstract;

namespace Milvasoft.Core.EntityBases.Concrete;

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
    public virtual DateTime CreationDate { get; set; }

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
    public virtual DateTime CreationDate { get; set; }
}
