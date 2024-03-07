using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.EntityBases.Abstract.MultiLanguage;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.EntityBases.Concrete.MultiLanguage;

/// <summary>
/// Base entity for all of entities.
/// </summary>
public abstract class TranslationEntity<TEntity> : TranslationEntity<int, TEntity, int, int>, ITranslationEntity<TEntity>
    where TEntity : class
{
}

/// <summary>
/// Base entity for all of entities.
/// </summary>
public abstract class TranslationEntity<TKey, TEntity, TEntityKey, TLanguageKey> : EntityBase, ITranslationEntity<TEntity, TEntityKey, TLanguageKey>, IBaseEntity<TKey>
    where TKey : struct, IEquatable<TKey>
    where TEntity : class
{
    /// <summary>
    /// Property name for reflection.
    /// </summary>
    public static string LanguageIdPropertyName { get; } = nameof(LanguageId);

    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual TKey Id { get; set; }

    /// <summary> 
    /// Language of menu id which is related.
    /// </summary>
    public TEntityKey EntityId { get; set; }

    /// <summary> 
    /// Language of menu which is related.
    /// </summary>
    public TLanguageKey LanguageId { get; set; }

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
