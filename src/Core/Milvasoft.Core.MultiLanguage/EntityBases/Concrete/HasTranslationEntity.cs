using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

/// <summary>
/// Translation base entity.
/// </summary>
public abstract class HasTranslationEntity<TTranslationEntity> : HasTranslationEntity<int, TTranslationEntity>
        where TTranslationEntity : class
{
}

/// <summary>
/// Translation base entity.
/// </summary>
public abstract class HasTranslationEntity<TKey, TTranslationEntity> : EntityBase<TKey>, IHasTranslation<TTranslationEntity>, IBaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
        where TTranslationEntity : class
{
    /// <summary>
    /// Multi language data.
    /// </summary>
    [NotMapped]
    public virtual List<TTranslationEntity> Translations { get; set; }

    /// <summary>
    /// Returns this instance of "<see cref="Type"/>.Name <see cref="BaseEntity{TKey}"/>.Id" as string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"[{GetType().Name} {Id}]";
}