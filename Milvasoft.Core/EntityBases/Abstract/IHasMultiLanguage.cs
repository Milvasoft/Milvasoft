using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.EntityBases.Abstract;

/// <summary>
/// Dummy interface for type findings.
/// </summary>
public interface IHasMultiLanguage
{
}

/// <summary>
/// Interface for entities with multiple language data.
/// </summary>
/// <typeparam name="TLangEntity"></typeparam>
public interface IHasMultiLanguage<TLangEntity> : IHasMultiLanguage where TLangEntity : class
{
    /// <summary>
    /// Multi language data.
    /// </summary>
    [NotMapped]
    public ICollection<TLangEntity> Languages { get; set; }
}

/// <summary>
/// Interface for language entities.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TLanguageKey"></typeparam>
public interface ILanguageEntity<TEntity, TKey, TLanguageKey> where TEntity : class
{
    /// <summary> 
    /// Language of menu id which is related.
    /// </summary>
    public TKey EntityId { get; set; }

    /// <summary> 
    /// Language of menu which is related.
    /// </summary>
    public TLanguageKey LanguageId { get; set; }

    /// <summary> 
    /// Language of menu which is related.
    /// </summary>
    [NotMapped]
    public TEntity Entity { get; set; }
}
