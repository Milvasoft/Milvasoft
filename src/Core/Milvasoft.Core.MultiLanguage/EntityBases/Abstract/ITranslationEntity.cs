using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.MultiLanguage.EntityBases.Abstract;

/// <summary>
/// Interface for language entities.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface ITranslationEntityWithIntKey<TEntity> : ITranslationEntity<TEntity, int> where TEntity : class
{
}

/// <summary>
/// Interface for language entities.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface ITranslationEntity<TEntity, TKey> : ITranslationEntity<TEntity> where TEntity : class
{
    /// <summary> 
    /// Language of menu id which is related.
    /// </summary>
    public TKey EntityId { get; set; }

    /// <summary> 
    /// Language of menu which is related.
    /// </summary>
    public int LanguageId { get; set; }
}

/// <summary>
/// Interface for language entities.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface ITranslationEntity<TEntity> where TEntity : class
{
    /// <summary> 
    /// Language of menu which is related.
    /// </summary>
    [NotMapped]
    public TEntity Entity { get; set; }
}
