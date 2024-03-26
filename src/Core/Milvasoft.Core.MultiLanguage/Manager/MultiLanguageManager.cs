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
    private readonly IServiceProvider _serviceProvider;
    private const string _sourceParameterName = "src";
    private const string _parameterName = "i";
    private static readonly MethodInfo _getTranslationMethodInfo = typeof(MultiLanguageManager).GetMethod(nameof(GetTranslation));
    private static readonly MethodInfo _firstOrDefaultWithPredicateMethodInfo = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                                     .Last(mi => mi.Name == nameof(Enumerable.FirstOrDefault) && mi.GetParameters().Length == 2);
    private static readonly MethodInfo _firstOrDefaultMethodInfo = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                                     .Last(mi => mi.Name == nameof(Enumerable.FirstOrDefault) && mi.GetParameters().Length == 1);
    public static ConcurrentBag<ILanguage> Languages { get; protected set; } = [];


    /// <summary>
    /// Initializes a new instance of the <see cref="MultiLanguageManager"/> class.
    /// </summary>
    public MultiLanguageManager()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiLanguageManager"/> class with the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public MultiLanguageManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Updates the list of languages.
    /// </summary>
    /// <param name="languages">The list of languages to update.</param>
    public static void UpdateLanguagesList(List<ILanguage> languages)
    {
        Languages.Clear();

        if (languages != null)
        {
            Languages = [.. languages.OrderBy(l => l.Id)];
        }
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

        Expression<Func<TEntity, string>> resultLambdaExpression;

        // Create a parameter for the source entity => src
        var entityParameter = Expression.Parameter(typeof(TEntity), _sourceParameterName);

        // Get the "Translations" property of the source entity => src.Translations
        var translationsPropertyExpression = Expression.Property(entityParameter, nameof(MultiLanguageEntityPropertyNames.Translations));
        var currentLanguageId = GetCurrentLanguageId();
        var defaultLanguageId = GetDefaultLanguageId();
        var propertyName = propertyExpression.GetPropertyName();

        // src.Translations.FirstOrDefault(i => (i.LanguageId == currentLanguageIdConstant))
        var translationsFirstOrDefaultWithCurrentLanguageIdEqualityExpression = CreateTranslationsFirstOrDefaultWithLanguageEqualityExpression(currentLanguageId);

        // src.Translations.FirstOrDefault(i => (i.LanguageId == currentLanguageIdConstant)).PropertyName
        var propertyOfTranslationsFirstOrDefaultWithCurrentLanguageIdEqualityExpression = Expression.Property(translationsFirstOrDefaultWithCurrentLanguageIdEqualityExpression, propertyName);

        if (currentLanguageId != defaultLanguageId)
        {
            // src.Translations.FirstOrDefault(i => (i.LanguageId == currentLanguageIdConstant)) == null
            var translationsFirstOrDefaultWithCurrentLanguageIdEqualityIsNullExpression = Expression.Equal(translationsFirstOrDefaultWithCurrentLanguageIdEqualityExpression, Expression.Constant(null));

            // Create a conditional expression to determine the final language-specific value of the property
            var translationsFirstOrDefaultWithCurrentLanguageIdNullCheckExpression = Expression.Condition(translationsFirstOrDefaultWithCurrentLanguageIdEqualityIsNullExpression,
                                                                                                          CreateTranslationsFirstOrDefaultWithDefaultLanguageWithNullCheckExpression(),
                                                                                                          propertyOfTranslationsFirstOrDefaultWithCurrentLanguageIdEqualityExpression);

            var resultExpression = CreateTranslationsNullCheckExpression(translationsFirstOrDefaultWithCurrentLanguageIdNullCheckExpression);

            // Create a lambda expression that represents the final expression to retrieve the language-specific value of the property
            resultLambdaExpression = Expression.Lambda<Func<TEntity, string>>(resultExpression, entityParameter);
        }
        else
        {
            var translationsFirstOrDefaultWithDefaultLanguageWithNullCheckExpression = CreateTranslationsFirstOrDefaultWithDefaultLanguageWithNullCheckExpression();

            var resultExpression = CreateTranslationsNullCheckExpression(translationsFirstOrDefaultWithDefaultLanguageWithNullCheckExpression);

            // Create a lambda expression that represents the final expression to retrieve the language-specific value of the property
            resultLambdaExpression = Expression.Lambda<Func<TEntity, string>>(resultExpression, entityParameter);
        }

        // Return the lambda expression
        return resultLambdaExpression;

        ConditionalExpression CreateTranslationsFirstOrDefaultWithDefaultLanguageWithNullCheckExpression()
        {
            // src.Translations.FirstOrDefault(i => (i.LanguageId == defaultLanguageIdConstant))
            var translationsFirstOrDefaultWithDefaultLanguageIdEqualityExpression = CreateTranslationsFirstOrDefaultWithLanguageEqualityExpression(defaultLanguageId);

            // src.Translations.FirstOrDefault(i => (i.LanguageId == defaultLanguageIdConstant)).PropertyName
            var propertyOfTranslationsFirstOrDefaultWithDefaultLanguageIdEqualityExpression = Expression.Property(translationsFirstOrDefaultWithDefaultLanguageIdEqualityExpression, propertyName);

            // src.Translations.FirstOrDefault(i => i.LanguageId == defaultLanguageIdConstant) != null
            var translationsFirstOrDefaultWithDefaultLanguageIdEqualityIsNotNullExpression = Expression.NotEqual(translationsFirstOrDefaultWithDefaultLanguageIdEqualityExpression, Expression.Constant(null));

            // Get the "FirstOrDefault" method of the Enumerable class with the appropriate generic type
            var genericFirstOrDefaultMethod = _firstOrDefaultMethodInfo.MakeGenericMethod(typeof(TTranslationEntity));

            // src.Translations.FirstOrDefault()
            var translationsFirstOrDefaultExpression = Expression.Call(genericFirstOrDefaultMethod, translationsPropertyExpression); // src.Translations.FirstOrDefault()

            // src.Translations.FirstOrDefault().PropertyName
            var propertyOfTranslationsFirstOrDefaultExpression = Expression.Property(translationsFirstOrDefaultExpression, propertyName); //src.Translations.FirstOrDefault().PropertyName

            var translationsFirstOrDefaultWithDefaultLanguageWithNullCheckExpression = Expression.Condition(translationsFirstOrDefaultWithDefaultLanguageIdEqualityIsNotNullExpression,
                                                                                                            propertyOfTranslationsFirstOrDefaultWithDefaultLanguageIdEqualityExpression,
                                                                                                            Expression.Condition(Expression.NotEqual(propertyOfTranslationsFirstOrDefaultExpression, Expression.Constant(null)),
                                                                                                                                 propertyOfTranslationsFirstOrDefaultExpression,
                                                                                                                                 Expression.Constant(null, typeof(string))));

            return translationsFirstOrDefaultWithDefaultLanguageWithNullCheckExpression;
        }

        ConditionalExpression CreateTranslationsNullCheckExpression(ConditionalExpression conditionalExpression)
        {
            // src.Translations == null
            var translationsPropertyIsNullExpression = Expression.Equal(translationsPropertyExpression, Expression.Constant(null, typeof(ICollection<TTranslationEntity>)));

            var translationNullCheckExpression = Expression.Condition(translationsPropertyIsNullExpression,
                                                                      Expression.Constant(null, typeof(string)),
                                                                      conditionalExpression);

            return translationNullCheckExpression;
        }

        MethodCallExpression CreateTranslationsFirstOrDefaultWithLanguageEqualityExpression(int languageId)
        {
            // Create constants for the current language ID and the default language ID
            var languageIdConstant = Expression.Constant(languageId);

            // Create a parameter for the language entity => i
            var translationEntityParameter = Expression.Parameter(typeof(TTranslationEntity), _parameterName);

            // i.LanguageId
            var languageIdProperty = Expression.Property(translationEntityParameter, MultiLanguageEntityPropertyNames.LanguageId);

            // Get the "FirstOrDefault" method of the Enumerable class with the appropriate generic type
            var genericFirstOrDefaultWithPredicateMethod = _firstOrDefaultWithPredicateMethodInfo.MakeGenericMethod(typeof(TTranslationEntity));

            // i.LanguageId == currentLanguageIdConstant
            var languageIdEqualityExpression = Expression.Equal(languageIdProperty, languageIdConstant);

            // i => i.LanguageId == currentLanguageIdConstant
            var languageIdEqualityLambdaExpression = Expression.Lambda<Func<TTranslationEntity, bool>>(languageIdEqualityExpression, translationEntityParameter);

            // src.Translations.FirstOrDefault(i => (i.LanguageId == currentLanguageIdConstant))
            var translationsFirstOrDefaultWithLanguageIdEqualityExpression = Expression.Call(genericFirstOrDefaultWithPredicateMethod, translationsPropertyExpression, languageIdEqualityLambdaExpression);

            return translationsFirstOrDefaultWithLanguageIdEqualityExpression;
        }
    }

    /// <summary>
    /// Get the value of the requested translation property.
    /// </summary>
    /// <param name="obj">The object to get the translation property value from.</param>
    /// <param name="requestedPropName">The name of the requested translation property.</param>
    /// <returns>The value of the requested translation property.</returns>
    public virtual dynamic GetTranslationPropertyValue(object obj, string requestedPropName)
    {
        var translations = obj.GetType().GetProperty(MultiLanguageEntityPropertyNames.Translations)?.GetValue(obj, null) as IList
                               ?? throw new MilvaUserFriendlyException(MilvaException.InvalidParameter);

        if (translations.IsNullOrEmpty())
            return null;

        var translationEntityType = translations[0].GetType();

        var getTranslationMethod = _getTranslationMethodInfo.MakeGenericMethod(translationEntityType);

        return getTranslationMethod.Invoke(this, [translations, requestedPropName]);
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
    /// Gets the value of the requested translation property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the translation entity.</typeparam>
    /// <param name="translations">The list of translations.</param>
    /// <param name="propertyName">The name of the requested translation property.</param>
    /// <returns>The value of the requested translation property.</returns>
    public virtual string GetTranslation<TEntity>(IEnumerable<TEntity> translations, string propertyName)
    {
        if (translations.IsNullOrEmpty())
            return string.Empty;

        TEntity requestedLang;

        var currentLanguageId = GetCurrentLanguageId();
        var defaultLanguageId = GetDefaultLanguageId();

        if (currentLanguageId != defaultLanguageId)
            requestedLang = GetLanguageValue(currentLanguageId) ?? GetLanguageValue(defaultLanguageId);
        else
            requestedLang = GetLanguageValue(defaultLanguageId);

        requestedLang ??= translations.FirstOrDefault();

        return requestedLang.GetType().GetProperty(propertyName).GetValue(requestedLang, null)?.ToString();

        TEntity GetLanguageValue(object languageId) => translations.FirstOrDefault(translation => translation.GetType()
                                                                                                             .GetProperty(MultiLanguageEntityPropertyNames.LanguageId)
                                                                                                             .GetValue(translation) == languageId);
    }
}

