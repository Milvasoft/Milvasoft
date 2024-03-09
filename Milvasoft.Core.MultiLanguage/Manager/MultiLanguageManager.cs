using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Extensions;
using Milvasoft.Core.MultiLanguage.EntityBases;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;
using Milvasoft.Core.Utils.Constants;
using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Core.MultiLanguage.Manager;

public abstract class MultiLanguageManager : IMultiLanguageManager
{
    private readonly IServiceProvider _serviceProvider;
    private const string _sourceParameterName = "src";
    private const string _parameterName = "i";
    private static readonly MethodInfo _getTranslationMethodInfo = typeof(MultiLanguageManager).GetMethod(nameof(GetTranslation));
    private static readonly MethodInfo _firstOrDefaultMethodInfo = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                                     .Where(mi => mi.Name == nameof(Enumerable.FirstOrDefault) && mi.GetParameters().Length == 2)
                                                                                     .Last();
    public static ConcurrentBag<ILanguage> Languages { get; set; } = [];

    public MultiLanguageManager()
    {

    }

    public MultiLanguageManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void UpdateLanguagesList(List<ILanguage> languages)
    {
        Languages.Clear();
        Languages = [.. languages];
    }

    public virtual int GetCurrentLanguageId()
    {
        var culture = CultureInfo.CurrentCulture;

        var currentLanguage = Languages.FirstOrDefault(i => i.Code == culture.Name);

        if (currentLanguage != null)
            return currentLanguage.Id;
        else
            return GetDefaultLanguageId();
    }

    public virtual int GetDefaultLanguageId() => Languages.FirstOrDefault(i => i.IsDefault)?.Id ?? 0;

    /// <summary>
    /// Creates an expression that retrieves the language-specific value of a property for a given entity based on the current language.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <typeparam name="TTranslationEntity">The type of the translation entity.</typeparam>
    /// <param name="propertyExpression">The expression representing the property to retrieve the value from.</param>
    /// <returns>An expression that retrieves the language-specific value of the property for the given entity.</returns>
    public Expression<Func<TEntity, string>> CreateTranslationMapExpression<TEntity, TDto, TTranslationEntity>(Expression<Func<TDto, string>> propertyExpression)
        where TEntity : class, IHasTranslation<TTranslationEntity>
        where TTranslationEntity : class, ITranslationEntity<TEntity>
    {
        // Create constants for the current language ID and the default language ID
        var currentLanguageIdConstant = Expression.Constant(GetCurrentLanguageId());
        var defaultLanguageIdConstant = Expression.Constant(GetDefaultLanguageId());

        // Create a parameter for the source entity
        var srcParameter = Expression.Parameter(typeof(TEntity), _sourceParameterName);

        // Create an expression to check if the current language ID is not equal to the default language ID
        var languageIdNotEqualExpression = Expression.NotEqual(currentLanguageIdConstant, defaultLanguageIdConstant);

        // Get the "Translations" property of the source entity
        var translationsProperty = Expression.Property(srcParameter, nameof(MultiLanguageEntityPropertyNames.Translations));

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

        // Create a conditional expression to determine the final language-specific value of the property
        var languageNameExpression = Expression.Condition(languageIdNotEqualExpression,
                                                          Expression.Condition(languageIdNullExpression,
                                                                               languageNameDefaultExpression,
                                                                               languageNameProperty),
                                                          languageNameDefaultExpression);

        // Create a lambda expression that represents the final expression to retrieve the language-specific value of the property
        var lambdaExpression = Expression.Lambda<Func<TEntity, string>>(languageNameExpression, srcParameter);

        // Return the lambda expression
        return lambdaExpression;
    }

    /// <summary>
    /// Get langs property in runtime.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="requestedPropName"></param>
    /// <returns></returns>
    public dynamic GetTranslationPropertyValue(object obj, string requestedPropName)
    {
        var translations = obj.GetType().GetProperty(MultiLanguageEntityPropertyNames.Translations)?.GetValue(obj, null) as IList
                               ?? throw new MilvaUserFriendlyException(MilvaException.InvalidParameter);

        if (translations.IsNullOrEmpty())
            return null;

        var translationEntityType = translations[0].GetType();

        var getTranslationMethod = _getTranslationMethodInfo.MakeGenericMethod(translationEntityType);

        return getTranslationMethod.Invoke(this, new object[] { translations, requestedPropName });
    }
    
    /// <summary>
    /// Gets requested translation property value.
    /// </summary>
    /// <param name="translations"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public string GetTranslationValue<TEntity>(IEnumerable<TEntity> translations, Expression<Func<TEntity, string>> propertyName)
        => GetTranslation(translations, propertyName.GetPropertyName());

    /// <summary>
    /// Gets requested translation property value.
    /// </summary>
    /// <param name="translations"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public string GetTranslation<TEntity>(IEnumerable<TEntity> translations, string propertyName)
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

