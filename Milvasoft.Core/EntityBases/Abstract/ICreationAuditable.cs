using Milvasoft.Core.EntityBases.Abstract.Auditing;

namespace Milvasoft.Core.EntityBases.Abstract;

/// <summary>
/// Determines entity's creation is auditable with user information.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface ICreationAuditable<TKey> : IBaseEntity<TKey>, IHasCreator where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Creation date of entity.
    /// </summary>
    public DateTime CreationDate { get; set; }
}

/// <summary>
/// Determines entity's creation is auditable.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface ICreationAuditableWithoutUser<TKey> : IEntityBase<TKey>
{
    /// <summary>
    /// Creation date of entity.
    /// </summary>
    public DateTime CreationDate { get; set; }
}
