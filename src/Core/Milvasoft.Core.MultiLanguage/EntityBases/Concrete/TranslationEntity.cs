using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

/// <summary>
/// Base entity for all of entities.
/// </summary>
public abstract class TranslationEntity<TEntity> : TranslationEntity<int, TEntity, int>, ITranslationEntityWithIntKey<TEntity>
    where TEntity : class
{
}

/// <summary>
/// Base entity for all of entities.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "<Pending>")]
public abstract class TranslationEntity<TKey, TEntity, TEntityKey> : EntityBase<TKey>, ITranslationEntity<TEntity, TEntityKey>, IBaseEntity<TKey>
    where TKey : struct, IEquatable<TKey>
    where TEntity : class
{
    /// <summary> 
    /// Language of menu id which is related.
    /// </summary>
    public TEntityKey EntityId { get; set; }

    /// <summary> 
    /// Language of menu which is related.
    /// </summary>
    public int LanguageId { get; set; }

    /// <summary> 
    /// Language of menu which is related.
    /// </summary>
    [NotMapped]
    public virtual TEntity Entity { get; set; }

    /// <summary>
    /// Returns this instance of "<see cref="Type"/>.Name <see cref="BaseEntity{TKey}"/>.Id" as string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"[{GetType().Name} {Id}]";
}
