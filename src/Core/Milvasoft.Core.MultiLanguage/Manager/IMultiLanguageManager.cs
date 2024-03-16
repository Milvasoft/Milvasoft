using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Milvasoft.Core.MultiLanguage.Manager;

public interface IMultiLanguageManager
{
    public static ConcurrentBag<ILanguage> Languages { get; set; }

    void UpdateLanguagesList(List<ILanguage> languages);

    int GetCurrentLanguageId();

    int GetDefaultLanguageId();

    /// <summary>
    /// Creates an expression that retrieves the language-specific value of a property for a given entity based on the current language.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <typeparam name="TTranslationEntity">The type of the translation entity.</typeparam>
    /// <param name="propertyExpression">The expression representing the property to retrieve the value from.</param>
    /// <returns>An expression that retrieves the language-specific value of the property for the given entity.</returns>
    Expression<Func<TEntity, string>> CreateTranslationMapExpression<TEntity, TDto, TTranslationEntity>(Expression<Func<TDto, string>> propertyExpression)
        where TEntity : class, IHasTranslation<TTranslationEntity>
        where TTranslationEntity : class, ITranslationEntity<TEntity>;

    /// <summary>
    /// Get langs property in runtime.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="requestedPropName"></param>
    /// <returns></returns>
    dynamic GetTranslationPropertyValue(object obj, string requestedPropName);

    /// <summary>
    /// Gets requested translation property value.
    /// </summary>
    /// <param name="translations"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    string GetTranslationValue<TEntity>(IEnumerable<TEntity> translations, Expression<Func<TEntity, string>> propertyName);

    /// <summary>
    /// Gets requested translation property value.
    /// </summary>
    /// <param name="translations"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public string GetTranslation<TEntity>(IEnumerable<TEntity> translations, string propertyName);
}
