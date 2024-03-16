namespace Milvasoft.Core.EntityBases.Abstract;

/// <summary>
/// Determines entity is auditable with modifier and modification date.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IAuditable<TKey> : ICreationAuditable<TKey>, IHasModifier where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Last modification date of entity.
    /// </summary>
    DateTime? LastModificationDate { get; set; }
}

/// <summary>
/// Determines entity is auditable without modifier.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IAuditableWithoutUser<TKey> : ICreationAuditableWithoutUser<TKey>
{
    /// <summary>
    /// Last modification date of entity.
    /// </summary>
    DateTime? LastModificationDate { get; set; }
}
