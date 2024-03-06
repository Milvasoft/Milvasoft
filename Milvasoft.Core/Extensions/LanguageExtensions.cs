using Milvasoft.Core.EntityBases.Concrete;
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
    private static readonly string _languageIdPropName = nameof(MultiLanguageEntity<int, object, object, object>.LanguageId);
    private static readonly MethodInfo _firstOrDefaultMethodInfo = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                                     .Where(mi => mi.Name == nameof(Enumerable.FirstOrDefault) && mi.GetParameters().Length == 2)
                                                                                     .Last();

    /// <summary>
    /// Creates an expression that retrieves the language-specific value of a property for a given entity based on the current language.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <typeparam name="TLanguageEntity">The type of the language entity.</typeparam>
    /// <param name="defaultLanguageId">The default language ID.</param>
    /// <param name="currentLanguageId">The current language ID.</param>
    /// <param name="propertyExpression">The expression representing the property to retrieve the value from.</param>
    /// <returns>An expression that retrieves the language-specific value of the property for the given entity.</returns>
    public static Expression<Func<TEntity, string>> CreateLanguageExpression<TEntity, TDto, TLanguageEntity>(object defaultLanguageId,
                                                                                                             object currentLanguageId,
                                                                                                             Expression<Func<TDto, string>> propertyExpression)
    {
        // Create constants for the current language ID and the default language ID
        var languageIdConstant = Expression.Constant(currentLanguageId);
        var defaultLanguageIdConstant = Expression.Constant(defaultLanguageId);

        // Create a parameter for the source entity
        var srcParameter = Expression.Parameter(typeof(TEntity), _sourceParameterName);

        // Create an expression to check if the current language ID is not equal to the default language ID
        var languageIdNotEqualExpression = Expression.NotEqual(languageIdConstant, defaultLanguageIdConstant);

        // Get the "Languages" property of the source entity
        var languagesProperty = Expression.Property(srcParameter, nameof(HasMultiLanguageEntity<int, object>.Languages));

        var languageEntityType = typeof(TLanguageEntity);

        // Create a parameter for the language entity
        var parameter = Expression.Parameter(languageEntityType, _parameterName);

        // Get the "LanguageId" property of the language entity
        var languageIdProperty = Expression.Property(parameter, _languageIdPropName);

        // Get the "FirstOrDefault" method of the Enumerable class with the appropriate generic type
        var genericFirstOrDefaultMethod = _firstOrDefaultMethodInfo.MakeGenericMethod(languageEntityType);

        // Create an expression to check if the language ID of the language entity is equal to the current language ID
        var languageIdEqualExpression = Expression.Equal(languageIdProperty, languageIdConstant);
        var lambda = Expression.Lambda<Func<TLanguageEntity, bool>>(languageIdEqualExpression, parameter);

        // Call the "FirstOrDefault" method on the "Languages" property with the lambda expression
        var call = Expression.Call(genericFirstOrDefaultMethod, languagesProperty, lambda);

        // Create an expression to check if the result of the "FirstOrDefault" method call is null
        var languageIdNullExpression = Expression.Equal(Expression.Constant(null), call);

        // Get the name of the property from expression
        var propertyName = propertyExpression.GetPropertyName();

        // Get the property of the language entity with the same name as the property to retrieve the value from
        var languageNameProperty = Expression.Property(call, propertyName);

        // Create an expression to check if the language ID of the language entity is equal to the default language ID
        languageIdEqualExpression = Expression.Equal(languageIdProperty, defaultLanguageIdConstant);
        lambda = Expression.Lambda<Func<TLanguageEntity, bool>>(languageIdEqualExpression, parameter);

        // Call the "FirstOrDefault" method on the "Languages" property with the lambda expression
        call = Expression.Call(genericFirstOrDefaultMethod, languagesProperty, lambda);

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
    /// Gets requested lang property value.
    /// </summary>
    /// <param name="langs"></param>
    /// <param name="propertyName"></param>
    /// <param name="defaultLangId"></param>
    /// <param name="requestedLangId"></param>
    /// <returns></returns>
    public static string GetLang<TEntity>(this IEnumerable<TEntity> langs, Expression<Func<TEntity, string>> propertyName, int defaultLangId, int requestedLangId)
    {
        if (langs.IsNullOrEmpty())
            return string.Empty;

        var propName = propertyName.GetPropertyName();

        TEntity requestedLang;

        if (requestedLangId != defaultLangId)
            requestedLang = GetLanguageValue(requestedLangId) ?? GetLanguageValue(defaultLangId);
        else
            requestedLang = GetLanguageValue(defaultLangId);

        requestedLang ??= langs.FirstOrDefault();

        return requestedLang.GetType().GetProperty(propName).GetValue(requestedLang, null)?.ToString();

        TEntity GetLanguageValue(int languageId) => langs.FirstOrDefault(lang => (int)lang.GetType()
                                                                                          .GetProperty(_languageIdPropName)
                                                                                          .GetValue(lang) == languageId);
    }

    /// <summary>
    /// Ready mapping is done. For example, it is used to map the data in the Product class to the ProductDTO class..
    /// </summary>
    /// <param name="langs"></param>
    /// <returns></returns>
    public static IEnumerable<TDTO> GetLangs<TEntity, TDTO>(this IEnumerable<TEntity> langs) where TDTO : new()
    {
        if (langs.IsNullOrEmpty())
            yield break;

        foreach (var lang in langs)
        {
            TDTO dto = new();

            foreach (var entityProp in lang.GetType().GetProperties())
            {
                var dtoProp = dto.GetType().GetProperty(entityProp.Name);

                if (dtoProp != null)
                {
                    var entityPropValue = entityProp.GetValue(lang, null);

                    if (entityProp.Name == _languageIdPropName)
                        dtoProp.SetValue(dto, entityPropValue, null);
                    else if (entityProp.PropertyType == typeof(string))
                        dtoProp.SetValue(dto, entityPropValue, null);
                }
            }

            yield return dto;
        }
    }
}
