﻿using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using System.Linq.Expressions;

namespace Milvasoft.Core.MultiLanguage.Manager;

/// <summary>
/// Represents a base class for managing multi-language functionality.
/// </summary>
public interface IMultiLanguageManager
{
    /// <summary>
    /// Gets languages list.
    /// </summary>
    /// <returns>Static languages list.</returns>
    public List<ILanguage> GetLanguages();

    /// <summary>
    /// Gets the ID of the default language.
    /// </summary>
    /// <returns>The ID of the default language.</returns>
    public int GetDefaultLanguageId();

    /// <summary>
    /// Gets the ID of the current language.
    /// </summary>
    /// <returns>The ID of the current language.</returns>
    public int GetCurrentLanguageId();

    /// <summary>
    /// Creates an expression that retrieves the language-specific value of a property for a given entity based on the current language. You can use it on lists or navigation properties.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <typeparam name="TTranslationEntity">The type of the translation entity.</typeparam>
    /// <param name="propertyExpression">The expression representing the property to retrieve the value from.</param>
    /// <returns>An expression that retrieves the language-specific value of the property for the given entity.</returns>
    public Expression<Func<TEntity, string>> CreateTranslationMapExpression<TEntity, TDto, TTranslationEntity>(Expression<Func<TDto, string>> propertyExpression)
        where TEntity : class, IHasTranslation<TTranslationEntity>
        where TTranslationEntity : class, ITranslationEntity<TEntity>;

    /// <summary>
    /// Gets the value of the requested translation property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the translation entity.</typeparam>
    /// <param name="translations">The list of translations.</param>
    /// <param name="propertyName">The name of the requested translation property.</param>
    /// <returns>The value of the requested translation property.</returns>
    public string GetTranslation<TEntity>(IEnumerable<TEntity> translations, Expression<Func<TEntity, string>> propertyName);

    /// <summary>
    /// Gets the value of the requested translation property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the translation entity.</typeparam>
    /// <param name="translations">The list of translations.</param>
    /// <param name="propertyName">The name of the requested translation property.</param>
    /// <returns>The value of the requested translation property.</returns>
    public string GetTranslation<TEntity>(IEnumerable<TEntity> translations, string propertyName);

    /// <summary>
    /// Get the value of the requested translation property.
    /// </summary>
    /// <param name="obj">The object to get the translation property value from.</param>
    /// <param name="propertyName">The name of the requested translation property.</param>
    /// <returns>The value of the requested translation property.</returns>
    public string GetTranslation(object obj, string propertyName);
}
