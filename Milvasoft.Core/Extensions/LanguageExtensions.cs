using Milvasoft.Core.EntityBases.Abstract.MultiLanguage;
using Milvasoft.Core.EntityBases.Concrete.MultiLanguage;
using Milvasoft.Core.Utils.Constants;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Core.Extensions;

/// <summary>
/// Multi language extensions.
/// </summary>
public static class LanguageExtensions
{
    private const string _sourceParameterName = "src";
    private const string _parameterName = "i";
    private static readonly MethodInfo _firstOrDefaultMethodInfo = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                                     .Where(mi => mi.Name == nameof(Enumerable.FirstOrDefault) && mi.GetParameters().Length == 2)
                                                                                     .Last();

    /// <summary>
    /// Creates an expression that retrieves the language-specific value of a property for a given entity based on the current language.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <typeparam name="TTranslationEntity">The type of the translation entity.</typeparam>
    /// <typeparam name="TEntityKey">The type of the entity key.</typeparam>
    /// <typeparam name="TLanguageKey">The type of the language key.</typeparam>
    /// <param name="defaultLanguageId">The default language ID.</param>
    /// <param name="currentLanguageId">The current language ID.</param>
    /// <param name="propertyExpression">The expression representing the property to retrieve the value from.</param>
    /// <returns>An expression that retrieves the language-specific value of the property for the given entity.</returns>
    public static Expression<Func<TEntity, string>> CreateTranslationMapExpression<TEntity, TDto, TTranslationEntity, TEntityKey, TLanguageKey>(TLanguageKey defaultLanguageId,
                                                                                                                                                TLanguageKey currentLanguageId,
                                                                                                                                                Expression<Func<TDto, string>> propertyExpression)
        where TEntity : class, IHasTranslation<TTranslationEntity>
        where TTranslationEntity : class, ITranslationEntity<TEntity, TEntityKey, TLanguageKey>
    {
        // Create constants for the current language ID and the default language ID
        var languageIdConstant = Expression.Constant(currentLanguageId);
        var defaultLanguageIdConstant = Expression.Constant(defaultLanguageId);

        // Create a parameter for the source entity
        var srcParameter = Expression.Parameter(typeof(TEntity), _sourceParameterName);

        // Create an expression to check if the current language ID is not equal to the default language ID
        var languageIdNotEqualExpression = Expression.NotEqual(languageIdConstant, defaultLanguageIdConstant);

        // Get the "Translations" property of the source entity
        var translationsProperty = Expression.Property(srcParameter, nameof(HasTranslationEntity<int, object>.Translations));

        var translationEntityType = typeof(TTranslationEntity);

        // Create a parameter for the language entity
        var parameter = Expression.Parameter(translationEntityType, _parameterName);

        // Get the "LanguageId" property of the language entity
        var languageIdProperty = Expression.Property(parameter, EntityPropertyNames.LanguageId);

        // Get the "FirstOrDefault" method of the Enumerable class with the appropriate generic type
        var genericFirstOrDefaultMethod = _firstOrDefaultMethodInfo.MakeGenericMethod(translationEntityType);

        // Create an expression to check if the language ID of the language entity is equal to the current language ID
        var languageIdEqualExpression = Expression.Equal(languageIdProperty, languageIdConstant);
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
    /// Creates an expression that retrieves the language-specific value of a property for a given entity based on the current language.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <typeparam name="TTranslationEntity">The type of the translation entity.</typeparam>
    /// <param name="defaultLanguageId">The default language ID.</param>
    /// <param name="currentLanguageId">The current language ID.</param>
    /// <param name="propertyExpression">The expression representing the property to retrieve the value from.</param>
    /// <returns>An expression that retrieves the language-specific value of the property for the given entity.</returns>
    public static Expression<Func<TEntity, string>> CreateTranslationMapExpression<TEntity, TDto, TTranslationEntity>(int defaultLanguageId,
                                                                                                                      int currentLanguageId,
                                                                                                                      Expression<Func<TDto, string>> propertyExpression)
            where TEntity : class, IHasTranslation<TTranslationEntity>
            where TTranslationEntity : class, ITranslationEntity<TEntity>
        => CreateTranslationMapExpression<TEntity, TDto, TTranslationEntity, int, int>(defaultLanguageId, currentLanguageId, propertyExpression);

    /// <summary>
    /// Gets requested translation property value.
    /// </summary>
    /// <param name="translations"></param>
    /// <param name="propertyName"></param>
    /// <param name="defaultLanguageId"></param>
    /// <param name="currentLanguageId"></param>
    /// <returns></returns>
    public static string GetTranslation<TEntity>(this IEnumerable<TEntity> translations, Expression<Func<TEntity, string>> propertyName, int defaultLanguageId, int currentLanguageId)
    {
        if (translations.IsNullOrEmpty())
            return string.Empty;

        var propName = propertyName.GetPropertyName();

        TEntity requestedLang;

        if (currentLanguageId != defaultLanguageId)
            requestedLang = GetLanguageValue(currentLanguageId) ?? GetLanguageValue(defaultLanguageId);
        else
            requestedLang = GetLanguageValue(defaultLanguageId);

        requestedLang ??= translations.FirstOrDefault();

        return requestedLang.GetType().GetProperty(propName).GetValue(requestedLang, null)?.ToString();

        TEntity GetLanguageValue(int languageId) => translations.FirstOrDefault(lang => (int)lang.GetType()
                                                                                          .GetProperty(EntityPropertyNames.LanguageId)
                                                                                          .GetValue(lang) == languageId);
    }

    /// <summary>
    /// Ready mapping is done. For example, it is used to map the data in the Poco class to the PocoDto class..
    /// </summary>
    /// <param name="translations"></param>
    /// <returns></returns>
    public static IEnumerable<TDTO> GetTranslations<TEntity, TDTO>(this IEnumerable<TEntity> translations) where TDTO : new()
    {
        if (translations.IsNullOrEmpty())
            yield break;

        foreach (var lang in translations)
        {
            TDTO dto = new();

            foreach (var entityProp in lang.GetType().GetProperties())
            {
                var dtoProp = dto.GetType().GetProperty(entityProp.Name);

                if (dtoProp != null)
                {
                    var entityPropValue = entityProp.GetValue(lang, null);

                    if (entityProp.Name == EntityPropertyNames.LanguageId)
                        dtoProp.SetValue(dto, entityPropValue, null);
                    else if (entityProp.PropertyType == typeof(string))
                        dtoProp.SetValue(dto, entityPropValue, null);
                }
            }

            yield return dto;
        }
    }
}
