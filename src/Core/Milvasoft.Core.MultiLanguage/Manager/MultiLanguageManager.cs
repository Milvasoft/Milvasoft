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
    private static readonly MethodInfo _firstOrDefaultMethodInfo = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                                     .Where(mi => mi.Name == nameof(Enumerable.FirstOrDefault) && mi.GetParameters().Length == 2)
                                                                                     .Last();
    private static readonly MethodInfo _firstOrDefaultWithOneParamMethodInfo = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                                                 .Where(mi => mi.Name == nameof(Enumerable.FirstOrDefault) && mi.GetParameters().Length == 1)
                                                                                                 .Last();
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
    /// <returns>An expression that retrieves the language-specific value of the property for the given entity.</returns>
    public virtual Expression<Func<TEntity, string>> CreateTranslationMapExpression<TEntity, TDto, TTranslationEntity>(Expression<Func<TDto, string>> propertyExpression)
        where TEntity : class, IHasTranslation<TTranslationEntity>
        where TTranslationEntity : class, ITranslationEntity<TEntity>
    {
        if (propertyExpression == null)
            return null;

        var currentLanguageId = GetCurrentLanguageId();
        var defaultLanguageId = GetDefaultLanguageId();

        // Create constants for the current language ID and the default language ID
        var currentLanguageIdConstant = Expression.Constant(currentLanguageId);
        var defaultLanguageIdConstant = Expression.Constant(defaultLanguageId);

        Expression<Func<TEntity, string>> lambdaExpression;

        // Create a parameter for the source entity
        var srcParameter = Expression.Parameter(typeof(TEntity), _sourceParameterName);

        // Get the "Translations" property of the source entity
        var translationsProperty = Expression.Property(srcParameter, nameof(MultiLanguageEntityPropertyNames.Translations));

        var translationsPropertyEqualNullExpression = Expression.Equal(translationsProperty, Expression.Constant(null, typeof(ICollection<TTranslationEntity>)));

        var translationEntityType = typeof(TTranslationEntity);

        // Create a parameter for the language entity
        var parameter = Expression.Parameter(translationEntityType, _parameterName);

        // Get the "LanguageId" property of the language entity
        var languageIdProperty = Expression.Property(parameter, MultiLanguageEntityPropertyNames.LanguageId);

        // Get the "FirstOrDefault" method of the Enumerable class with the appropriate generic type
        var genericFirstOrDefaultMethod = _firstOrDefaultMethodInfo.MakeGenericMethod(translationEntityType);

        // Create an expression to check if the language ID of the language entity is equal to the current language ID
        var languageIdEqualExpression = Expression.Equal(languageIdProperty, currentLanguageIdConstant);
        var lambda = Expression.Lambda<Func<TTranslationEntity, bool>>(languageIdEqualExpression, parameter);

        // Call the "FirstOrDefault" method on the "Translations" property with the lambda expression
        var call = Expression.Call(genericFirstOrDefaultMethod, translationsProperty, lambda);

        // Create an expression to check if the result of the "FirstOrDefault" method call is null
        var languageIdNullExpression = Expression.Equal(Expression.Constant(null), call);

        // Get the name of the property from expression
        var propertyName = propertyExpression.GetPropertyName();

        // Get the property of the language entity with the same name as the property to retrieve the value from
        var languageNameProperty = Expression.Property(call, propertyName);

        // Create an expression to check if the language ID of the language entity is equal to the default language ID
        languageIdEqualExpression = Expression.Equal(languageIdProperty, defaultLanguageIdConstant);
        lambda = Expression.Lambda<Func<TTranslationEntity, bool>>(languageIdEqualExpression, parameter);

        // Call the "FirstOrDefault" method on the "Translations" property with the lambda expression
        call = Expression.Call(genericFirstOrDefaultMethod, translationsProperty, lambda);

        // Get the property of the language entity with the same name as the property to retrieve the value from
        var languageNameDefaultExpression = Expression.Property(call, propertyName);

        var nullStringConstant = Expression.Constant(null, typeof(string));

        if (currentLanguageId != defaultLanguageId)
        {
            // Create a conditional expression to determine the final language-specific value of the property
            var languageNameExpression = Expression.Condition(translationsPropertyEqualNullExpression,
                                                              nullStringConstant,
                                                              Expression.Condition(languageIdNullExpression,
                                                              GetDefaultWithNullCheckExpression(),
                                                              languageNameProperty));

            // Create a lambda expression that represents the final expression to retrieve the language-specific value of the property
            lambdaExpression = Expression.Lambda<Func<TEntity, string>>(languageNameExpression, srcParameter);
        }
        else
        {
            var defaultConditionExpression = Expression.Condition(translationsPropertyEqualNullExpression,
                                                                  nullStringConstant,
                                                                  GetDefaultWithNullCheckExpression());

            // Create a lambda expression that represents the final expression to retrieve the language-specific value of the property
            lambdaExpression = Expression.Lambda<Func<TEntity, string>>(defaultConditionExpression, srcParameter);
        }

        // Return the lambda expression
        return lambdaExpression;

        ConditionalExpression GetDefaultWithNullCheckExpression()
        {
            // Create an expression to check if the language ID of the language entity is equal to the current language ID
            var languageIdEqualExpression = Expression.Equal(languageIdProperty, defaultLanguageIdConstant);
            var lambda = Expression.Lambda<Func<TTranslationEntity, bool>>(languageIdEqualExpression, parameter);

            // Call the "FirstOrDefault" method on the "Translations" property with the lambda expression
            var call = Expression.Call(genericFirstOrDefaultMethod, translationsProperty, lambda);

            //src.Translations.FirstOrDefault(i => i.LanguageId == 1) != null
            var languageNameDefaultNotEqualNullExpression = Expression.NotEqual(Expression.Constant(null), call);

            // Get the "FirstOrDefault" method of the Enumerable class with the appropriate generic type
            var genericFirstOrDefaultMethodWithOneParam = _firstOrDefaultWithOneParamMethodInfo.MakeGenericMethod(translationEntityType);

            // Call the "FirstOrDefault" method on the "Translations" property with the lambda expression
            var firstOrDefaultCall = Expression.Call(genericFirstOrDefaultMethodWithOneParam, translationsProperty); // src.Translations.FirstOrDefault()

            var languageNamePropertyFirst = Expression.Property(firstOrDefaultCall, propertyName); //src.Translations.FirstOrDefault().Name

            var languageNameExpression = Expression.Condition(languageNameDefaultNotEqualNullExpression, //src.Translations.FirstOrDefault(i => i.LanguageId == 1) != null
                                                              languageNameDefaultExpression, //src.Translations.FirstOrDefault(i => i.LanguageId == 1).Name
                                                              Expression.Condition(Expression.NotEqual(Expression.Constant(null), languageNamePropertyFirst), // src.Translations.FirstOrDefault().Name != null
                                                                                   languageNamePropertyFirst, //src.Translations.FirstOrDefault().Name
                                                                                   nullStringConstant));

            return languageNameExpression;
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

