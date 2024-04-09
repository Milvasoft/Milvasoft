namespace Milvasoft.Core.EntityBases.Abstract;

/// <summary>
/// Determines entity is auditable with modifier and modification date.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IAuditable<TKey> : ICreationAuditable<TKey>, IHasModificationDate, IHasModifier where TKey : struct, IEquatable<TKey>
{
}

/// <summary>
/// Determines entity is auditable without modifier.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IAuditableWithoutUser<TKey> : IHasModificationDate, ICreationAuditableWithoutUser<TKey>
{
}
