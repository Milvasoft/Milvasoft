using Milvasoft.Core.Extensions;
using Milvasoft.Core.MultiLanguage.EntityBases;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using System.Linq.Expressions;

namespace Milvasoft.Core.MultiLanguage;
public static class MultiLanguageExtensions
{
    /// <summary>
    /// Creates projection expression for contents service. THIS IS A AMAZING METHOD.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="mainEntityPropertyNames"></param>
    /// <param name="translationEntityPropertyNames"></param>
    /// <param name="translationEntityType"></param>
    /// <returns></returns>
    public static Expression<Func<TEntity, TEntity>> CreateProjectionExpression<TEntity>(IEnumerable<string> mainEntityPropertyNames,
                                                                                         IEnumerable<string> translationEntityPropertyNames,
                                                                                         Type translationEntityType = null)
    {
        var sourceType = typeof(TEntity);

        LambdaExpression translationExpression = null;

        if (!translationEntityPropertyNames.IsNullOrEmpty() && translationEntityType != null)
        {
            translationEntityPropertyNames = translationEntityPropertyNames.Append(MultiLanguageEntityPropertyNames.LanguageId);

            var translationParameter = Expression.Parameter(translationEntityType, "t");

            var translationBindings = translationEntityPropertyNames.Select(propName => Expression.Bind(translationEntityType.GetProperty(propName),
                                                                                                        Expression.Property(translationParameter, propName)));

            var translationBody = Expression.MemberInit(Expression.New(translationEntityType), translationBindings);

            translationExpression = Expression.Lambda(translationBody, translationParameter);
        }

        var parameter = Expression.Parameter(sourceType, "c");

        MethodCallExpression selectExpressionForTranslations = null;

        if (translationExpression != null)
        {
            selectExpressionForTranslations = Expression.Call(typeof(Enumerable),
                                                              nameof(Enumerable.Select),
                                                              new Type[] { translationEntityType, translationEntityType },
                                                              Expression.PropertyOrField(parameter, MultiLanguageEntityPropertyNames.Translations),
                                                              translationExpression);
        }

        var bindings = new List<MemberAssignment>();

        foreach (var item in mainEntityPropertyNames)
        {
            var mapExpression = (Expression)(item == MultiLanguageEntityPropertyNames.Translations
                                                    ? selectExpressionForTranslations
                                                    : Expression.Property(parameter, item));

            var sourceProperty = sourceType.GetProperty(item);

            if (sourceProperty.PropertyType != mapExpression.Type)
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
        where TEntity : class, IHasTranslation
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
