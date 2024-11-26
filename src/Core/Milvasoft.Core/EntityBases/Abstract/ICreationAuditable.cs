using Milvasoft.Core.EntityBases.MultiTenancy;

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
    public DateTime? CreationDate { get; set; }
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
    public DateTime? CreationDate { get; set; }
}

/// <summary>
/// Determines entity's creation is auditable with user information and tenant id.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface ICreationAuditableWithTenantId<TKey> : ICreationAuditable<TKey>, IHasTenantId where TKey : struct, IEquatable<TKey>;

/// <summary>
/// Determines entity's creation is auditable and tenant id.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface ICreationAuditableWithTenantIdAndWithoutUser<TKey> : ICreationAuditableWithoutUser<TKey>, IHasTenantId;
