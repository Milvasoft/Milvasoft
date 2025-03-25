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
    private static readonly Lock _statObjLocker = new();

    #region Static fields for reflection
    private const string _sourceParameterName = "src";
    private const string _translationParameterName = "t";
    private static readonly MethodInfo _getTranslationMethodInfo = typeof(MultiLanguageManager).GetGenericMethod(nameof(GetTranslation), 1, typeof(IEnumerable<>), typeof(string));
    private static readonly MethodInfo _firstOrDefaultWithPredicateMethodInfo = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                                                  .Last(mi => mi.Name == nameof(Enumerable.FirstOrDefault)
                                                                                                              && mi.GetParameters().Length == 2);
    private static readonly MethodInfo _firstOrDefaultMethodInfo = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                                     .Last(mi => mi.Name == nameof(Enumerable.FirstOrDefault) && mi.GetParameters().Length == 1);
    private static readonly MethodInfo _enumerableCastMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast));
    #endregion

    /// <summary>
    /// Service provider instance.
    /// </summary>
    protected readonly IServiceProvider _serviceProvider;

    /// <inheritdoc/>
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
        lock (_statObjLocker)
        {
            Languages.Clear();

            if (languages != null)
            {
                Languages = [.. languages.OrderBy(l => l.Id)];
            }
        }
    }

    /// <summary>
    /// Gets the list of languages.
    /// </summary>
    /// <returns></returns>
    public virtual List<ILanguage> GetLanguages() => [.. Languages];

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

        // src.Translations.Select(i => new ProductTranslation() {LanguageId = i.LanguageId, EntityId = i.EntityId, Name = i.Name})
        var projectionExpression = BuildSelectExpression();

        if (currentLanguageId != defaultLanguageId)
        {
            // src.Translations.Any(i => (i.LanguageId == currentLanguageIdConstant)) == false
            var translationsAnyWithCurrentLanguageIdEqualityIsFalse = Expression.Equal(MultiLanguageExtensions.CreateTranslationsAnyExpression<TTranslationEntity>(translationsPropertyExpression, currentLanguageId),
                                                                                       Expression.Constant(false));

            // src.Translations.Select(i => new ProductTranslation() {LanguageId = i.LanguageId, EntityId = i.EntityId, Name = i.Name}).FirstOrDefault(i => (i.LanguageId == currentLanguageIdConstant))
            var translationsFirstOrDefaultWithCurrentLanguageIdEqualityExpression = CreateTranslationsFirstOrDefaultWithLanguageEqualityExpression(currentLanguageId);

            // src.Translations.Select(i => new ProductTranslation() {LanguageId = i.LanguageId, EntityId = i.EntityId, Name = i.Name}).FirstOrDefault(i => (i.LanguageId == currentLanguageIdConstant)).PropertyName
            var propertyOfTranslationsFirstOrDefaultWithCurrentLanguageIdEqualityExpression = Expression.Property(translationsFirstOrDefaultWithCurrentLanguageIdEqualityExpression, propertyName);

            // Create a conditional expression to determine the final language-specific value of the property
            var translationsFirstOrDefaultWithCurrentLanguageIdNullCheckExpression = Expression.Condition(translationsAnyWithCurrentLanguageIdEqualityIsFalse,
                                                                                                          CreateTranslationsFirstOrDefaultWithDefaultLanguageWithNullCheckExpression(),
                                                                                                          propertyOfTranslationsFirstOrDefaultWithCurrentLanguageIdEqualityExpression);

            var resultExpression = MultiLanguageExtensions.CreateTranslationsAnyCheckExpression<TTranslationEntity, string>(translationsPropertyExpression, translationsFirstOrDefaultWithCurrentLanguageIdNullCheckExpression);

            // Create a lambda expression that represents the final expression to retrieve the language-specific value of the property
            resultLambdaExpression = Expression.Lambda<Func<TEntity, string>>(resultExpression, entityParameter);
        }
        else
        {
            var translationsFirstOrDefaultWithDefaultLanguageWithNullCheckExpression = CreateTranslationsFirstOrDefaultWithDefaultLanguageWithNullCheckExpression();

            var resultExpression = MultiLanguageExtensions.CreateTranslationsAnyCheckExpression<TTranslationEntity, string>(translationsPropertyExpression, translationsFirstOrDefaultWithDefaultLanguageWithNullCheckExpression);

            // Create a lambda expression that represents the final expression to retrieve the language-specific value of the property
            resultLambdaExpression = Expression.Lambda<Func<TEntity, string>>(resultExpression, entityParameter);
        }

        // Return the lambda expression
        return resultLambdaExpression;

        MethodCallExpression BuildSelectExpression()
        {
            var translationEntityPropertyNames = new List<string>(capacity: 4) { MultiLanguageEntityPropertyNames.LanguageId, MultiLanguageEntityPropertyNames.EntityId, propertyName };

            if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TTranslationEntity)))
                translationEntityPropertyNames.Add(EntityPropertyNames.IsDeleted);

            var translationEntityParameter = Expression.Parameter(typeof(TTranslationEntity), _translationParameterName);

            var translationBindings = translationEntityPropertyNames.Select(propName => Expression.Bind(typeof(TTranslationEntity).GetProperty(propName),
                                                                                               Expression.Property(translationEntityParameter, propName)));

            var translationBody = Expression.MemberInit(Expression.New(typeof(TTranslationEntity)), translationBindings);

            var translationExpression = Expression.Lambda(translationBody, translationEntityParameter);

            // src.Translations.Select(i => new ProductTranslation() {LanguageId = i.LanguageId, EntityId = i.EntityId, Name = i.Name})
            var selectExpression = Expression.Call(typeof(Enumerable),
                                                   nameof(Enumerable.Select),
                                                   [typeof(TTranslationEntity), typeof(TTranslationEntity)],
                                                   translationsPropertyExpression,
                                                   translationExpression);

            return selectExpression;
        }

        //src.Translations.Select(i => new ProductTranslation() {LanguageId = i.LanguageId, EntityId = i.EntityId, Name = i.Name}).FirstOrDefault(i => (i.LanguageId == languageId)) != null
        ConditionalExpression CreateTranslationsFirstOrDefaultWithDefaultLanguageWithNullCheckExpression()
        {
            // src.Translations.Select(i => new ProductTranslation() {LanguageId = i.LanguageId, EntityId = i.EntityId, Name = i.Name}).FirstOrDefault(i => (i.LanguageId == defaultLanguageIdConstant))
            var translationsFirstOrDefaultWithDefaultLanguageIdEqualityExpression = CreateTranslationsFirstOrDefaultWithLanguageEqualityExpression(defaultLanguageId);

            // src.Translations.Select(i => new ProductTranslation() {LanguageId = i.LanguageId, EntityId = i.EntityId, Name = i.Name}).FirstOrDefault(i => (i.LanguageId == defaultLanguageIdConstant)).PropertyName
            var propertyOfTranslationsFirstOrDefaultWithDefaultLanguageIdEqualityExpression = Expression.Property(translationsFirstOrDefaultWithDefaultLanguageIdEqualityExpression, propertyName);

            // src.Translations.Any(i => i.LanguageId == defaultLanguageIdConstant) == true
            var translationsAnyWithDefaultLanguageIdEqualityIsTrue = Expression.Equal(MultiLanguageExtensions.CreateTranslationsAnyExpression<TTranslationEntity>(translationsPropertyExpression,
                                                                                                                                                                  defaultLanguageId),
                                                                                      Expression.Constant(true));

            // src.Translations.Any() == true
            var translationsAnyEqualityIsTrue = Expression.Equal(MultiLanguageExtensions.CreateTranslationsAnyExpression<TTranslationEntity>(translationsPropertyExpression),
                                                                 Expression.Constant(true));

            // src.Translations.Select(i => new ProductTranslation() {LanguageId = i.LanguageId, EntityId = i.EntityId, Name = i.Name}).FirstOrDefault()
            var translationsFirstOrDefaultExpression = CreateFirstOrDefaultWithIsDeletedExpression(projectionExpression);

            // src.Translations.Select(i => new ProductTranslation() {LanguageId = i.LanguageId, EntityId = i.EntityId, Name = i.Name}).FirstOrDefault().PropertyName
            var propertyOfTranslationsFirstOrDefaultExpression = Expression.Property(translationsFirstOrDefaultExpression, propertyName);

            var translationsFirstOrDefaultWithDefaultLanguageWithNullCheckExpression = Expression.Condition(translationsAnyWithDefaultLanguageIdEqualityIsTrue,
                                                                                                            propertyOfTranslationsFirstOrDefaultWithDefaultLanguageIdEqualityExpression,
                                                                                                            Expression.Condition(translationsAnyEqualityIsTrue,
                                                                                                                                 propertyOfTranslationsFirstOrDefaultExpression,
                                                                                                                                 Expression.Constant(null, typeof(string))));

            return translationsFirstOrDefaultWithDefaultLanguageWithNullCheckExpression;
        }

        // .FirstOrDefault(i=>i.IsDeleted == false)
        MethodCallExpression CreateFirstOrDefaultWithIsDeletedExpression(MethodCallExpression projectionExpression)
        {
            // Create a parameter for the language entity => i
            var translationEntityParameter = Expression.Parameter(typeof(TTranslationEntity), _translationParameterName);

            Expression predicateExpression = null;

            // If TTranslations is soft deletable, add a check for IsDeleted == false
            if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TTranslationEntity)))
            {
                var isDeletedProperty = Expression.Property(translationEntityParameter, nameof(ISoftDeletable.IsDeleted));
                var isDeletedFalseExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));

                // (i.LanguageId == languageId) && (i.IsDeleted == false)
                predicateExpression = isDeletedFalseExpression;

                // i => (i.LanguageId == languageId) && (i.IsDeleted == false)
                var predicateLambdaExpression = Expression.Lambda<Func<TTranslationEntity, bool>>(predicateExpression, translationEntityParameter);

                // Get the "FirstOrDefault" method of the Enumerable class with the appropriate generic type
                var genericFirstOrDefaultWithPredicateMethod = _firstOrDefaultWithPredicateMethodInfo.MakeGenericMethod(typeof(TTranslationEntity));

                // src.Translations.Select(i => new ProductTranslation() {LanguageId = i.LanguageId, EntityId = i.EntityId, Name = i.Name})
                //    .FirstOrDefault(i => (i.LanguageId == languageId) && (i.IsDeleted == false))
                var firstOrDefaultWithIsDeletedFalseEqualityExpression = Expression.Call(genericFirstOrDefaultWithPredicateMethod,
                                                                                                 projectionExpression,
                                                                                                 predicateLambdaExpression);

                return firstOrDefaultWithIsDeletedFalseEqualityExpression;
            }
            else
                return Expression.Call(_firstOrDefaultMethodInfo.MakeGenericMethod(typeof(TTranslationEntity)), projectionExpression);
        }

        // src.Translations.Select(i => new ProductTranslation() {LanguageId = i.LanguageId, EntityId = i.EntityId, Name = i.Name}).FirstOrDefault(i => (i.LanguageId == languageId))
        MethodCallExpression CreateTranslationsFirstOrDefaultWithLanguageEqualityExpression(int languageId)
        {
            // Create constants for the current language ID
            var languageIdConstant = Expression.Constant(languageId);

            // Create a parameter for the language entity => i
            var translationEntityParameter = Expression.Parameter(typeof(TTranslationEntity), _translationParameterName);

            // i.LanguageId
            var languageIdProperty = Expression.Property(translationEntityParameter, MultiLanguageEntityPropertyNames.LanguageId);

            // i.LanguageId == languageId
            var languageIdEqualityExpression = Expression.Equal(languageIdProperty, languageIdConstant);

            Expression predicateExpression = languageIdEqualityExpression;

            // If TTranslations is soft deletable, add a check for IsDeleted == false
            if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TTranslationEntity)))
            {
                var isDeletedProperty = Expression.Property(translationEntityParameter, nameof(ISoftDeletable.IsDeleted));
                var isDeletedFalseExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));

                // (i.LanguageId == languageId) && (i.IsDeleted == false)
                predicateExpression = Expression.AndAlso(predicateExpression, isDeletedFalseExpression);
            }

            // i => (i.LanguageId == languageId) && (i.IsDeleted == false)
            var predicateLambdaExpression = Expression.Lambda<Func<TTranslationEntity, bool>>(predicateExpression, translationEntityParameter);

            // Get the "FirstOrDefault" method of the Enumerable class with the appropriate generic type
            var genericFirstOrDefaultWithPredicateMethod = _firstOrDefaultWithPredicateMethodInfo.MakeGenericMethod(typeof(TTranslationEntity));

            // src.Translations.Select(i => new ProductTranslation() {LanguageId = i.LanguageId, EntityId = i.EntityId, Name = i.Name})
            //    .FirstOrDefault(i => (i.LanguageId == languageId) && (i.IsDeleted == false))
            var translationsFirstOrDefaultWithLanguageIdEqualityExpression = Expression.Call(genericFirstOrDefaultWithPredicateMethod,
                                                                                             projectionExpression,
                                                                                             predicateLambdaExpression);

            return translationsFirstOrDefaultWithLanguageIdEqualityExpression;
        }
    }

    /// <summary>
    /// Gets the value of the requested translation property.
    /// </summary>
    /// <typeparam name="TTranslationEntity">The type of the translation entity.</typeparam>
    /// <param name="translations">The list of translations.</param>
    /// <param name="propertyName">The name of the requested translation property.</param>
    /// <returns>The value of the requested translation property.</returns>
    public virtual string GetTranslation<TTranslationEntity>(IEnumerable<TTranslationEntity> translations, string propertyName)
    {
        if (translations.IsNullOrEmpty() || string.IsNullOrWhiteSpace(propertyName))
            return string.Empty;

        translations = translations.Where(t => t != null);

        TTranslationEntity requestedLang;

        var currentLanguageId = GetCurrentLanguageId();
        var defaultLanguageId = GetDefaultLanguageId();

        if (currentLanguageId != defaultLanguageId)
            requestedLang = GetLanguageValue(currentLanguageId) ?? GetLanguageValue(defaultLanguageId);
        else
            requestedLang = GetLanguageValue(defaultLanguageId);

        requestedLang ??= translations.FirstOrDefault();

        return requestedLang?.GetType().GetPublicPropertyIgnoreCase(propertyName)?.GetValue(requestedLang, null)?.ToString();

        TTranslationEntity GetLanguageValue(int languageId) => translations.FirstOrDefault(t => (int)(t.GetType()
                                                                                                       .GetProperty(MultiLanguageEntityPropertyNames.LanguageId)?
                                                                                                       .GetValue(t) ?? 0) == languageId);
    }

    /// <summary>
    /// Gets the value of the requested translation property.
    /// </summary>
    /// <typeparam name="TTranslationEntity">The type of the translation entity.</typeparam>
    /// <param name="translations">The list of translations.</param>
    /// <param name="propertyName">The name of the requested translation property.</param>
    /// <returns>The value of the requested translation property.</returns>
    public virtual string GetTranslation<TTranslationEntity>(IEnumerable<TTranslationEntity> translations, Expression<Func<TTranslationEntity, string>> propertyName)
        => GetTranslation(translations, propertyName.GetPropertyName());

    /// <summary>
    /// Get the value of the requested translation property.
    /// </summary>
    /// <param name="obj">The object which implements <see cref="IHasTranslation{TTranslationEntity}"/> to get the translation property value from.</param>
    /// <param name="propertyName">The name of the requested translation property.</param>
    /// <returns>The value of the requested translation property.</returns>
    public virtual string GetTranslation(object obj, string propertyName)
    {
        if (obj is null || !obj.GetType().CanAssignableTo(typeof(IHasTranslation<>)))
            return string.Empty;

        var translationsProperty = obj.GetType().GetProperty(MultiLanguageEntityPropertyNames.Translations);
        var translations = translationsProperty?.GetValue(obj) as IList;

        if (translations.IsNullOrEmpty())
            return string.Empty;

        // For null item remove. OfTypes remove null items automatically.
        var translationList = translations.OfType<object>();

        if (translations.IsNullOrEmpty())
            return string.Empty;

        var translationEntityType = translationList.First().GetType();

        // Cast to IEnumerable<translationEntityType>
        var castMethod = _enumerableCastMethod.MakeGenericMethod(translationEntityType);

        var typedEnumerable = castMethod.Invoke(null, [translationList]);

        var getTranslationMethod = _getTranslationMethodInfo.MakeGenericMethod(translationEntityType);

        return (string)getTranslationMethod.Invoke(this, [typedEnumerable, propertyName]);
    }
}

