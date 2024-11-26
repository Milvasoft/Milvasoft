using Milvasoft.Core.EntityBases.MultiTenancy;

namespace Milvasoft.Core.EntityBases.Abstract;

/// <summary>
/// Determines entity is auditable with modifier and modification date.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IAuditable<TKey> : ICreationAuditable<TKey>, IHasModificationDate, IHasModifier where TKey : struct, IEquatable<TKey>;

/// <summary>
/// Determines entity is auditable without modifier.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IAuditableWithoutUser<TKey> : IHasModificationDate, ICreationAuditableWithoutUser<TKey>;

/// <summary>
/// Determines entity is auditable with modifier and modification date and tenant id.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IAuditableWithTenantId<TKey> : IAuditable<TKey>, IHasTenantId where TKey : struct, IEquatable<TKey>;

/// <summary>
/// Determines tenant entity is auditable without modifier and tenant id.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IAuditableWithTenantIdAndWithoutUser<TKey> : IAuditableWithoutUser<TKey>, IHasTenantId;
