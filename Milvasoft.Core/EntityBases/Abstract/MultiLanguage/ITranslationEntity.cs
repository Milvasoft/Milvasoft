using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.EntityBases.Abstract.MultiLanguage;

/// <summary>
/// Interface for language entities.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface ITranslationEntity<TEntity> : ITranslationEntity<TEntity, int, int> where TEntity : class
{
}

/// <summary>
/// Interface for language entities.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TLanguageKey"></typeparam>
public interface ITranslationEntity<TEntity, TKey, TLanguageKey> where TEntity : class
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
