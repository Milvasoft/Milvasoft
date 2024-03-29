using Milvasoft.Core.MultiLanguage.EntityBases;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Core.MultiLanguage.Manager;

/// <summary>
/// Represents a base class for managing multi-language functionality.
/// </summary>
public abstract class MultiLanguageManager : IMultiLanguageManager
{
    #region Static fields for reflection
    private const string _sourceParameterName = "src";
    private const string _parameterName = "i";
    private static readonly MethodInfo _getTranslationMethodInfo = typeof(MultiLanguageManager).GetMethod(nameof(GetTranslation));
    private static readonly MethodInfo _firstOrDefaultWithPredicateMethodInfo = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Last(mi => mi.Name == nameof(Enumerable.FirstOrDefault) && mi.GetParameters().Length == 2);
    private static readonly MethodInfo _firstOrDefaultMethodInfo = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Last(mi => mi.Name == nameof(Enumerable.FirstOrDefault) && mi.GetParameters().Length == 1);
    #endregion
    protected readonly IServiceProvider _serviceProvider;

    public static ConcurrentBag<ILanguage> Languages { get; protected set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiLanguageManager"/> class.
    /// </summary>
    protected MultiLanguageManager()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiLanguageManager"/> class with the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    protected MultiLanguageManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Updates the list of <see cref="Languages"/> as <paramref name="languages"/>.
    /// </summary>
    /// <param name="languages">The list of languages to update.</param>
    public static void UpdateLanguagesList(List<ILanguage> languages)
    {
        Languages.Clear();

        if (languages != null)
            Languages = [.. languages.OrderBy(l => l.Id)];
    }

    /// <summary>
    /// Gets the ID of the default language.
    /// </summary>
    /// <returns>The ID of the default language.</returns>
    public virtual int GetDefaultLanguageId() => Languages.FirstOrDefault(i => i.IsDefault)?.Id ?? 0;

    /// <summary>
    /// Gets the ID of the current language.
    /// </summary>
    /// <returns>The ID of the current language.</returns>
    public virtual int GetCurrentLanguageId()
    {
        var culture = CultureInfo.CurrentCulture;

        var currentLanguage = Languages.FirstOrDefault(i => i.Code == culture.Name);

        if (currentLanguage != null)
            return currentLanguage.Id;
        else
            return GetDefaultLanguageId();
    }

    /// <summary>
    /// Creates an expression that retrieves the language-specific value of a property for a given entity based on the current language.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <typeparam name="TTranslationEntity">The type of the translation entity.</typeparam>
    /// <param name="propertyExpression">The expression representing the property to retrieve the value from.</param>
    /// <remarks> Steps :
    /// <para> 1- Check if the Translations property is null. If it is, return null. </para>
    /// <para> 2- If it's not null, check if the property of the first record matching the current language ID is null. If it's not null, return the property value of the first record matching the current language ID. </para>
    /// <para> 3- If it's null, check if the property of the first record matching the default language ID is null. If it's not null, return the property value of the first record matching the default language ID. </para>
    /// <para> 4- If it's null, check if the property of the first record is null. If it is, return null. </para>
    /// <para> 5- If it's not null, return the property value of the first record. </para>
    /// </remarks>
    /// <returns>An expression that retrieves the language-specific value of the property for the given entity.</returns>
    public virtual Expression<Func<TEntity, string>> CreateTranslationMapExpression<TEntity, TDto, TTranslationEntity>(Expression<Func<TDto, string>> propertyExpression)
        where TEntity : class, IHasTranslation<TTranslationEntity>
        where TTranslationEntity : class, ITranslationEntity<TEntity>
    {
        if (propertyExpression == null)
            return null;

        // Create a parameter for the source entity => src
        var entityParameter = Expression.Parameter(typeof(TEntity), _sourceParameterName);

        // Get the "Translations" property of the source entity => src.Translations
        var translationsPropertyExpression = Expression.Property(entityParameter, nameof(MultiLanguageEntityPropertyNames.Translations));

        // src.Translations.FirstOrDefault(i => (i.LanguageId == currentLanguageIdConstant))
        var translationsFirstOrDefaultEqualityExpression = CreateTranslationsFirstOrDefaultEqualityExpression();

        var propertyName = propertyExpression.GetPropertyName();

        // src.Translations.FirstOrDefault(i => (i.LanguageId == currentLanguageIdConstant) ||(i.LanguageId == defaultLanguageIdConstant) ||(i.LanguageId == i.LanguageId)).PropertyName
        var propertyOfTranslationsFirstOrDefaultEqualityExpression = Expression.Property(translationsFirstOrDefaultEqualityExpression,
                                                                                         propertyName);

        var resultExpression = MultiLanguageExtensions.CreateTranslationsNullCheckExpression<TTranslationEntity, string>(translationsPropertyExpression,
                                                                                                                         propertyOfTranslationsFirstOrDefaultEqualityExpression);

        // Create a lambda expression that represents the final expression to retrieve the language-specific value of the property
        Expression<Func<TEntity, string>> resultLambdaExpression = Expression.Lambda<Func<TEntity, string>>(resultExpression, entityParameter);

        // Return the lambda expression
        return resultLambdaExpression;

        MethodCallExpression CreateTranslationsFirstOrDefaultEqualityExpression()
        {
            // Create a parameter for the language entity => i
            var translationEntityParameter = Expression.Parameter(typeof(TTranslationEntity), _parameterName);

            // i.LanguageId
            var languageIdProperty = Expression.Property(translationEntityParameter, MultiLanguageEntityPropertyNames.LanguageId);

            var currentLanguageIdConstant = Expression.Constant(GetCurrentLanguageId());

            var defaultLanguageIdConstant = Expression.Constant(GetDefaultLanguageId());

            var currentlanguageIdEqualExpression = Expression.Equal(languageIdProperty, currentLanguageIdConstant);

            var defualtlanguageIdEqualExpression = Expression.Equal(languageIdProperty, defaultLanguageIdConstant);

            var languageIdEqualsLanguageIdExpression = Expression.Equal(languageIdProperty, languageIdProperty);

            // Create an expression to check if the language ID of the language entity is equal to the current language ID or default language ID or get first
            var equalityExpression = Expression.OrElse(Expression.OrElse(currentlanguageIdEqualExpression, defualtlanguageIdEqualExpression), languageIdEqualsLanguageIdExpression);

            // Get the "FirstOrDefault" method of the Enumerable class with the appropriate generic type
            var genericFirstOrDefaultWithPredicateMethod = _firstOrDefaultWithPredicateMethodInfo.MakeGenericMethod(typeof(TTranslationEntity));

            // i => i.LanguageId == currentLanguageId || i.LanguageId == defaultLanguageId || i.LanguageId == i.LanguageId
            var languageIdEqualityLambdaExpression = Expression.Lambda<Func<TTranslationEntity, bool>>(equalityExpression, translationEntityParameter);

            // src.Translations.FirstOrDefault(i => i.LanguageId == currentLanguageId || i.LanguageId == defaultLanguageId || i.LanguageId == i.LanguageId)
            var translationsFirstOrDefaultEqualityExpression = Expression.Call(genericFirstOrDefaultWithPredicateMethod,
                                                                               translationsPropertyExpression,
                                                                               languageIdEqualityLambdaExpression);

            return translationsFirstOrDefaultEqualityExpression;
        }
    }

    /// <summary>
    /// Gets the value of the requested translation property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the translation entity.</typeparam>
    /// <param name="translations">The list of translations.</param>
    /// <param name="propertyName">The name of the requested translation property.</param>
    /// <returns>The value of the requested translation property.</returns>
    public virtual string GetTranslationValue<TEntity>(IEnumerable<TEntity> translations, Expression<Func<TEntity, string>> propertyName)
        => GetTranslation(translations, propertyName.GetPropertyName());

    /// <summary>
    /// Get the value of the requested translation property.
    /// </summary>
    /// <param name="obj">The object to get the translation property value from.</param>
    /// <param name="propertyName">The name of the requested translation property.</param>
    /// <returns>The value of the requested translation property.</returns>
    public virtual string GetTranslationPropertyValue(object obj, string propertyName)
    {
        if (obj == null || !obj.GetType().CanAssignableTo(typeof(IHasTranslation<>)))
            return string.Empty;

        var translations = obj.GetType().GetProperty(MultiLanguageEntityPropertyNames.Translations)?.GetValue(obj, null) as IList;

        if (translations.IsNullOrEmpty())
            return string.Empty;

        var translationEntityType = translations?[0].GetType();

        var getTranslationMethod = _getTranslationMethodInfo.MakeGenericMethod(translationEntityType);

        return (string)getTranslationMethod.Invoke(this, [translations, propertyName]);
    }

    /// <summary>
    /// Gets the value of the requested translation property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the translation entity.</typeparam>
    /// <param name="translations">The list of translations.</param>
    /// <param name="propertyName">The name of the requested translation property.</param>
    /// <returns>The value of the requested translation property.</returns>
    public virtual string GetTranslation<TEntity>(IEnumerable<TEntity> translations, string propertyName)
    {
        if (translations.IsNullOrEmpty() || string.IsNullOrWhiteSpace(propertyName) || !typeof(TEntity).CanAssignableTo(typeof(ITranslationEntity<>)))
            return string.Empty;

        TEntity requestedLang;

        var currentLanguageId = GetCurrentLanguageId();
        var defaultLanguageId = GetDefaultLanguageId();

        if (currentLanguageId != defaultLanguageId)
            requestedLang = GetLanguageValue(currentLanguageId) ?? GetLanguageValue(defaultLanguageId);
        else
            requestedLang = GetLanguageValue(defaultLanguageId);

        requestedLang ??= translations.FirstOrDefault();

        return requestedLang.GetType().GetPublicPropertyIgnoreCase(propertyName).GetValue(requestedLang, null)?.ToString();

        TEntity GetLanguageValue(object languageId) => translations.FirstOrDefault(translation => translation.GetType()
                                                                                                             .GetProperty(MultiLanguageEntityPropertyNames.LanguageId)
                                                                                                             .GetValue(translation) == languageId);
    }
}

