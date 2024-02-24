using Milvasoft.Core.EntityBases.Abstract.Auditing;

namespace Milvasoft.Core.EntityBases.Abstract;

/// <summary>
/// Determines entity is fully auditable with user information.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IFullAuditable<TKey> : IAuditable<TKey>, IHasDeleter, ISoftDeletable where TKey : struct, IEquatable<TKey>
{
}

/// <summary>
/// Determines entity is fully auditable.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IFullAuditableWithoutUser<TKey> : IAuditableWithoutUser<TKey>, ISoftDeletable
{
}
