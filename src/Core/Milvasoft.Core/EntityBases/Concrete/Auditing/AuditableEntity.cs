namespace Milvasoft.Core.EntityBases.Concrete.Auditing;

/// <summary>
/// Determines entity's is auditable with modifier and modification date.
/// </summary>
/// <typeparam name="TKey">Type of the user</typeparam>
public abstract class AuditableEntity<TKey> : CreationAuditableEntity<TKey>, IAuditable<TKey> where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Last modification date of entity.
    /// </summary>
    public virtual DateTime? LastModificationDate { get; set; }

    /// <summary>
    /// Modifier of entity.
    /// </summary>
    public virtual string LastModifierUserName { get; set; }
}

/// <summary>
/// Determines entity's is auditable with modifier and modification date.
/// </summary>
/// <typeparam name="TKey">Type of the user</typeparam>
public abstract class AuditableEntityWithoutUser<TKey> : CreationAuditableEntityWithoutUser<TKey>, IAuditableWithoutUser<TKey>
{
    /// <summary>
    /// Last modification date of entity.
    /// </summary>
    public virtual DateTime? LastModificationDate { get; set; }
}
