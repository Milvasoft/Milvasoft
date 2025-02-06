using Milvasoft.Core.MultiLanguage.EntityBases;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Core.MultiLanguage;

/// <summary>
/// Multilingual helper extensions.
/// </summary>
public static class MultiLanguageExtensions
{
    private const string _sourceParameterName = "c";
    private const string _translationParameterName = "t";
    private static readonly MethodInfo _anyWithPredicateMethodInfo = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                                                  .Last(mi => mi.Name == nameof(Enumerable.Any)
                                                                                                              && mi.GetParameters().Length == 2);
    private static readonly MethodInfo _anyMethodInfo = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                                     .Last(mi => mi.Name == nameof(Enumerable.Any) && mi.GetParameters().Length == 1);

    /// <summary>
    /// Creates projection expression with requested properties for <see cref="IHasTranslation{TTranslationEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TTranslationEntity"></typeparam>
    /// <param name="mainEntityPropertyNames"></param>
    /// <param name="translationEntityPropertyNames"></param>
    /// <param name="hasJsonTranslations"></param>
    /// <returns>Sample; e => new HasTranslationEntity { Id = e.Id, Translations = e.Translations.Select(t=> new TranslationEntity { Name = t.Name } ).ToList() } </returns>
    public static Expression<Func<TEntity, TEntity>> CreateProjectionExpression<TEntity, TTranslationEntity>(IEnumerable<string> mainEntityPropertyNames,
                                                                                                             IEnumerable<string> translationEntityPropertyNames,
                                                                                                             bool hasJsonTranslations)
    {
        var sourceType = typeof(TEntity);
        var parameter = Expression.Parameter(sourceType, _sourceParameterName);
        LambdaExpression translationExpression = null;
        ConditionalExpression expressionForTranslations = null;

        var mainEntityPropertyNameTempList = mainEntityPropertyNames?.ToList() ?? [];

        if (!translationEntityPropertyNames.IsNullOrEmpty())
        {
            var translationEntityType = typeof(TTranslationEntity);
            translationEntityPropertyNames = translationEntityPropertyNames.Append(MultiLanguageEntityPropertyNames.LanguageId);

            var translationParameter = Expression.Parameter(translationEntityType, _translationParameterName);

            var translationBindings = translationEntityPropertyNames.Select(propName => Expression.Bind(translationEntityType.GetProperty(propName),
                                                                                                        Expression.Property(translationParameter, propName)));

            var translationBody = Expression.MemberInit(Expression.New(translationEntityType), translationBindings);

            translationExpression = Expression.Lambda(translationBody, translationParameter);

            var translationsPropertyExpression = Expression.PropertyOrField(parameter, MultiLanguageEntityPropertyNames.Translations);

            var selectExpressionForTranslations = Expression.Call(typeof(Enumerable),
                                                                  nameof(Enumerable.Select),
                                                                  [translationEntityType, translationEntityType],
                                                                  Expression.PropertyOrField(parameter, MultiLanguageEntityPropertyNames.Translations),
                                                                  translationExpression);

            selectExpressionForTranslations = Expression.Call(typeof(Enumerable),
                                                              nameof(Enumerable.ToList),
                                                              [translationEntityType],
                                                              selectExpressionForTranslations);

            expressionForTranslations = CreateTranslationsAnyCheckExpression<TTranslationEntity, List<TTranslationEntity>>(translationsPropertyExpression,
                                                                                                                            selectExpressionForTranslations);

            if (!mainEntityPropertyNameTempList.Exists(i => i == MultiLanguageEntityPropertyNames.Translations))
                mainEntityPropertyNameTempList.Add(MultiLanguageEntityPropertyNames.Translations);
        }

        if (mainEntityPropertyNameTempList.IsNullOrEmpty())
            return i => i;

        var bindings = new List<MemberAssignment>();

        foreach (var item in mainEntityPropertyNameTempList)
        {
            var mapExpression = (Expression)((item == MultiLanguageEntityPropertyNames.Translations && !hasJsonTranslations)
                                                    ? expressionForTranslations
                                                    : Expression.PropertyOrField(parameter, item));

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
                var dtoProp = dtoType.GetProperty(entityProp.Name, entityProp.PropertyType);

                if (dtoProp != null)
                {
                    var entityPropValue = entityProp.GetValue(translation, null);

                    dtoProp.SetValue(dto, entityPropValue, null);
                }
            }

            yield return dto;
        }
    }

    /// <summary>
    /// Creates a conditional expression to check if the translations property is null.
    /// </summary>
    /// <typeparam name="TTranslationEntity">The type of the translation entity.</typeparam>
    /// <typeparam name="TReturn">The return type of the expression.</typeparam>
    /// <param name="translationsPropertyExpression">The member expression representing the translations property.</param>
    /// <param name="expression">The expression to be returned if the translations property is not null.</param>
    /// <returns>The conditional expression to check if the translations property is null. Sample result; x => x.Translations == null ? null : <paramref name="expression"/></returns>
    internal static ConditionalExpression CreateTranslationsAnyCheckExpression<TTranslationEntity, TReturn>(MemberExpression translationsPropertyExpression, Expression expression)
    {
        // src.Translations.Any(conditions)
        var anyExpression = CreateTranslationsAnyExpression<TTranslationEntity>(translationsPropertyExpression, null);

        // src.Translations.Any() == false
        var translationsPropertyAnyExpression = Expression.Equal(anyExpression, Expression.Constant(false));

        var translationNullCheckExpression = Expression.Condition(translationsPropertyAnyExpression,
                                                                  Expression.Constant(null, typeof(TReturn)),
                                                                  expression);

        return translationNullCheckExpression;
    }

    //src.Translations.Any(i => (i.LanguageId == languageId))
    internal static MethodCallExpression CreateTranslationsAnyExpression<TTranslationEntity>(MemberExpression translationsPropertyExpression, int? languageId = null)
    {
        // Create a parameter for the translation entity => i
        var translationEntityParameter = Expression.Parameter(typeof(TTranslationEntity), _translationParameterName);

        Expression predicateExpression = null;

        if (languageId.HasValue)
        {
            // i.LanguageId
            var languageIdProperty = Expression.Property(translationEntityParameter, MultiLanguageEntityPropertyNames.LanguageId);

            // languageId sabiti
            var languageIdConstant = Expression.Constant(languageId.Value);

            // i.LanguageId == languageIdConstant
            predicateExpression = Expression.Equal(languageIdProperty, languageIdConstant);
        }

        // If TTranslations is soft deletable, add a check for IsDeleted == false
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TTranslationEntity)))
        {
            var isDeletedProperty = Expression.Property(translationEntityParameter, nameof(ISoftDeletable.IsDeleted));
            var isDeletedFalseExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));

            // Combine with and also
            predicateExpression = predicateExpression != null
                ? Expression.AndAlso(predicateExpression, isDeletedFalseExpression)
                : isDeletedFalseExpression;
        }

        if (predicateExpression != null)
        {
            // i => i.(conditions)
            var lambda = Expression.Lambda<Func<TTranslationEntity, bool>>(predicateExpression, translationEntityParameter);

            // src.Translations.Any(i => i.(conditions))
            var genericAnyWithPredicateMethod = _anyWithPredicateMethodInfo.MakeGenericMethod(typeof(TTranslationEntity));
            return Expression.Call(genericAnyWithPredicateMethod, translationsPropertyExpression, lambda);
        }
        else
        {
            // src.Translations.Any()
            var genericAnyMethod = _anyMethodInfo.MakeGenericMethod(typeof(TTranslationEntity));
            return Expression.Call(genericAnyMethod, translationsPropertyExpression);
        }
    }
}
