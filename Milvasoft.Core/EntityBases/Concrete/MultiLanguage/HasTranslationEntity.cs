using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.EntityBases.Abstract.MultiLanguage;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.EntityBases.Concrete.MultiLanguage;

/// <summary>
/// Base entity for all of entities.
/// </summary>
public abstract class HasTranslationEntity<TTranslationEntity> : HasTranslationEntity<int, TTranslationEntity>
        where TTranslationEntity : class
{
}

/// <summary>
/// Base entity for all of entities.
/// </summary>
public abstract class HasTranslationEntity<TKey, TTranslationEntity> : EntityBase, IHasTranslation<TTranslationEntity>, IBaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
        where TTranslationEntity : class
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
    public virtual ICollection<TTranslationEntity> Translations { get; set; }

    /// <summary>
    /// Returns this instance of "<see cref="Type"/>.Name <see cref="BaseEntity{TKey}"/>.Id" as string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"[{GetType().Name} {Id}]";
}