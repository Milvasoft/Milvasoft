using Milvasoft.Core.EntityBases.Abstract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.EntityBases.Concrete;

/// <summary>
/// Base entity for all of entities.
/// </summary>
public abstract class HasMultiLanguageEntity<TKey, TLanguageEntity> : EntityBase, IHasMultiLanguage<TLanguageEntity>, IBaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
        where TLanguageEntity : class
{
    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual TKey Id { get; set; }

    /// <summary>
    /// Multi language data.
    /// </summary>
    [NotMapped]
    public virtual ICollection<TLanguageEntity> Languages { get; set; }

    /// <summary>
    /// Returns this instance of "<see cref="Type"/>.Name <see cref="BaseEntity{TKey}"/>.Id" as string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"[{GetType().Name} {Id}]";
}

/// <summary>
/// Base entity for all of entities.
/// </summary>
public abstract class MultiLanguageEntity<TKey, TEntity, TEntityKey, TLanguageKey> : EntityBase, ILanguageEntity<TEntity, TEntityKey, TLanguageKey>, IBaseEntity<TKey>
    where TKey : struct, IEquatable<TKey>
    where TEntity : class
{
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
