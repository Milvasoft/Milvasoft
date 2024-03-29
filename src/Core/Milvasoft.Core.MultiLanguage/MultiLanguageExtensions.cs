using Milvasoft.Core.MultiLanguage.EntityBases;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.Manager;
using System.Linq.Expressions;

namespace Milvasoft.Core.MultiLanguage;
public static class MultiLanguageExtensions
{
    private const string _sourceParameterName = "c";
    private const string _translationsParameterName = "t";

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
                                                                                                             IMultiLanguageManager multiLanguageManager = null)
         where TEntity : class, IHasTranslation<TTranslationEntity>
         where TTranslationEntity : class, ITranslationEntity<TEntity>
    {
        var sourceType = typeof(TEntity);
        var translationEntityType = typeof(TTranslationEntity);
        var parameter = Expression.Parameter(sourceType, _sourceParameterName);
        LambdaExpression translationExpression = null;
        ConditionalExpression expressionForTranslations = null;

        var mainEntityPropertyNameTempList = mainEntityPropertyNames?.ToList() ?? [];

        if (!translationEntityPropertyNames.IsNullOrEmpty())
        {
            translationEntityPropertyNames = translationEntityPropertyNames.Append(MultiLanguageEntityPropertyNames.LanguageId);

            var translationParameter = Expression.Parameter(translationEntityType, _translationsParameterName);

            var translationBindings = translationEntityPropertyNames.Select(propName => Expression.Bind(translationEntityType.GetProperty(propName),
                                                                                                        Expression.Property(translationParameter, propName)));

            var translationBody = Expression.MemberInit(Expression.New(translationEntityType), translationBindings);

            translationExpression = Expression.Lambda(translationBody, translationParameter);

            var translationsPropertyExpression = Expression.PropertyOrField(parameter, MultiLanguageEntityPropertyNames.Translations);

            if (multiLanguageManager != null)
            {
                // Get the "LanguageId" property of the language entity
                var languageIdProperty = Expression.Property(translationParameter, MultiLanguageEntityPropertyNames.LanguageId);

                // Create constants for the current language ID and the default language ID
                var currentLanguageIdConstant = Expression.Constant(multiLanguageManager.GetCurrentLanguageId());
                var defaultLanguageIdConstant = Expression.Constant(multiLanguageManager.GetDefaultLanguageId());

                // Create an expression to check if the language ID of the language entity is equal to the current language ID
                var currentlanguageIdEqualExpression = Expression.Equal(languageIdProperty, currentLanguageIdConstant);
                var defualtlanguageIdEqualExpression = Expression.Equal(languageIdProperty, defaultLanguageIdConstant);
                var trueExpression = Expression.Equal(languageIdProperty, languageIdProperty);

                var equalityExpression = Expression.OrElse(Expression.OrElse(currentlanguageIdEqualExpression, defualtlanguageIdEqualExpression), trueExpression);

                var wherePredicate = Expression.Lambda<Func<TTranslationEntity, bool>>(equalityExpression, translationParameter);

                // Call the "Where" method on the "Translations" property with the lambda expression
                var whereExpression = Expression.Call(typeof(Enumerable),
                                                      nameof(Enumerable.Where),
                                                      [translationEntityType],
                                                      Expression.PropertyOrField(parameter, MultiLanguageEntityPropertyNames.Translations),
                                                      wherePredicate);

                // Call the "Select" method on the "Where()" chain with the translationExpression
                var whereAndSelectExpressionForTranslations = Expression.Call(typeof(Enumerable),
                                                                              nameof(Enumerable.Select),
                                                                              [translationEntityType, translationEntityType],
                                                                              whereExpression,
                                                                              translationExpression);

                expressionForTranslations = CreateTranslationsNullCheckExpression<TTranslationEntity, IEnumerable<TTranslationEntity>>(translationsPropertyExpression,
                                                                                                                                       whereAndSelectExpressionForTranslations);
            }
            else
            {
                var selectExpressionForTranslations = Expression.Call(typeof(Enumerable),
                                                                              nameof(Enumerable.Select),
                                                                              [translationEntityType, translationEntityType],
                                                                              Expression.PropertyOrField(parameter, MultiLanguageEntityPropertyNames.Translations),
                                                                              translationExpression);

                expressionForTranslations = CreateTranslationsNullCheckExpression<TTranslationEntity, IEnumerable<TTranslationEntity>>(translationsPropertyExpression,
                                                                                                                                       selectExpressionForTranslations);

            }

            if (!mainEntityPropertyNameTempList.Exists(i => i == MultiLanguageEntityPropertyNames.Translations))
                mainEntityPropertyNameTempList.Add(MultiLanguageEntityPropertyNames.Translations);
        }

        if (mainEntityPropertyNameTempList.IsNullOrEmpty())
            return i => i;

        var bindings = new List<MemberAssignment>();

        foreach (var item in mainEntityPropertyNameTempList)
        {
            var mapExpression = (Expression)(item == MultiLanguageEntityPropertyNames.Translations
                                                    ? expressionForTranslations
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
    internal static ConditionalExpression CreateTranslationsNullCheckExpression<TTranslationEntity, TReturn>(MemberExpression translationsPropertyExpression, Expression expression)
    {
        // src.Translations == null
        var translationsPropertyIsNullExpression = Expression.Equal(translationsPropertyExpression, Expression.Constant(null, typeof(IEnumerable<TTranslationEntity>)));

        var translationNullCheckExpression = Expression.Condition(translationsPropertyIsNullExpression,
                                                                  Expression.Constant(null, typeof(TReturn)),
                                                                  expression);

        return translationNullCheckExpression;
    }
}
