using Milvasoft.Core.MultiLanguage.EntityBases;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.Manager;
using System.Linq.Expressions;

namespace Milvasoft.Core.MultiLanguage;
public static class MultiLanguageExtensions
{
    private const string _parameterName = "t";

    /// <summary>
    /// Creates projection expression for contents service.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="mainEntityPropertyNames"></param>
    /// <param name="translationEntityPropertyNames"></param>
    /// <param name="translationEntityType"></param>
    /// <returns></returns>
    public static Expression<Func<TEntity, TEntity>> CreateProjectionExpression<TEntity, TTranslationEntity>(IEnumerable<string> mainEntityPropertyNames,
                                                                                                             IEnumerable<string> translationEntityPropertyNames,
                                                                                                             Type translationEntityType = null,
                                                                                                             IMultiLanguageManager multiLanguageManager = null)
         where TEntity : class, IHasTranslation<TTranslationEntity>
         where TTranslationEntity : class, ITranslationEntity<TEntity>
    {
        var sourceType = typeof(TEntity);

        LambdaExpression translationExpression = null;

        var parameter = Expression.Parameter(sourceType, "c");

        MethodCallExpression selectExpressionForTranslations = null;

        if (!translationEntityPropertyNames.IsNullOrEmpty() && translationEntityType != null)
        {
            translationEntityPropertyNames = translationEntityPropertyNames.Append(MultiLanguageEntityPropertyNames.LanguageId);

            var translationParameter = Expression.Parameter(translationEntityType, _parameterName);

            var translationBindings = translationEntityPropertyNames.Select(propName => Expression.Bind(translationEntityType.GetProperty(propName),
                                                                                                        Expression.Property(translationParameter, propName)));

            // Get the "LanguageId" property of the language entity
            var languageIdProperty = Expression.Property(translationParameter, MultiLanguageEntityPropertyNames.LanguageId);

            // Create constants for the current language ID and the default language ID
            var currentLanguageIdConstant = Expression.Constant(multiLanguageManager.GetCurrentLanguageId());
            var defaultLanguageIdConstant = Expression.Constant(multiLanguageManager.GetDefaultLanguageId());

            // Create an expression to check if the language ID of the language entity is equal to the current language ID
            var currentlanguageIdEqualExpression = Expression.Equal(languageIdProperty, currentLanguageIdConstant);
            var defualtlanguageIdEqualExpression = Expression.Equal(languageIdProperty, defaultLanguageIdConstant);

            var wherePredicate = Expression.Lambda<Func<TTranslationEntity, bool>>(Expression.Or(currentlanguageIdEqualExpression, defualtlanguageIdEqualExpression), translationParameter);

            // Call the "Where" method on the "Translations" property with the lambda expression
            var whereExpression = Expression.Call(typeof(Enumerable),
                                                  nameof(Enumerable.Where),
                                                  [translationEntityType],
                                                  Expression.PropertyOrField(parameter, MultiLanguageEntityPropertyNames.Translations),
                                                  wherePredicate);

            var translationBody = Expression.MemberInit(Expression.New(translationEntityType), translationBindings);

            translationExpression = Expression.Lambda(translationBody, translationParameter);

            // Call the "Select" method on the "Where()" chain with the translationExpression
            selectExpressionForTranslations = Expression.Call(typeof(Enumerable),
                                                              nameof(Enumerable.Select),
                                                              [translationEntityType, translationEntityType],
                                                              whereExpression,
                                                              translationExpression);
        }

        var bindings = new List<MemberAssignment>();

        foreach (var item in mainEntityPropertyNames)
        {
            var mapExpression = (Expression)(item == MultiLanguageEntityPropertyNames.Translations
                                                    ? selectExpressionForTranslations
                                                    : Expression.Property(parameter, item));

            var sourceProperty = sourceType.GetProperty(item);

            if (sourceProperty.PropertyType != mapExpression?.Type)
                bindings.Add(Expression.Bind(sourceProperty, Expression.Convert(mapExpression, sourceProperty.PropertyType)));
            else
                bindings.Add(Expression.Bind(sourceProperty, mapExpression));
        }

        var body = Expression.MemberInit(Expression.New(sourceType), bindings);

        return Expression.Lambda<Func<TEntity, TEntity>>(body, parameter);
    }

    /// <summary>
    /// Ready mapping is done. For example, it is used to map the data in the Poco class to the PocoDto class..
    /// </summary>
    /// <param name="translations"></param>
    /// <returns></returns>
    public static IEnumerable<TDTO> GetTranslations<TEntity, TTranslationEntity, TDTO>(this IEnumerable<TTranslationEntity> translations)
        where TEntity : class, IHasTranslation<TTranslationEntity>
        where TTranslationEntity : class, ITranslationEntity<TEntity>
        where TDTO : new()
    {
        if (translations.IsNullOrEmpty())
            yield break;

        var dtoType = typeof(TDTO);

        foreach (var translation in translations)
        {
            TDTO dto = new();

            foreach (var entityProp in translation.GetType().GetProperties())
            {
                var dtoProp = dtoType.GetProperty(entityProp.Name);

                if (dtoProp != null)
                {
                    var entityPropValue = entityProp.GetValue(translation, null);

                    if (entityProp.Name == MultiLanguageEntityPropertyNames.LanguageId)
                        dtoProp.SetValue(dto, entityPropValue, null);
                    else if (entityProp.PropertyType == typeof(string))
                        dtoProp.SetValue(dto, entityPropValue, null);
                }
            }

            yield return dto;
        }
    }
}
