using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.MultiLanguage;
using Milvasoft.Core.MultiLanguage.EntityBases;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.Manager;
using Milvasoft.DataAccess.EfCore.DbContextBase;
using Milvasoft.DataAccess.EfCore.Utils.LookupModels;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.DataAccess.EfCore.Utils;
internal class LookupManager(MilvaDbContext dbContext, IDataAccessConfiguration dataAccessConfiguration)
{
    private readonly MilvaDbContext _dbContext = dbContext;
    private readonly IDataAccessConfiguration _dataAccessConfiguration = dataAccessConfiguration;
    private static readonly MethodInfo _createProjectionExpressionMethod = typeof(MultiLanguageExtensions).GetMethod(nameof(MultiLanguageExtensions.CreateProjectionExpression));
    private const string _taskResultPropertyName = "Result";
    private static readonly ConcurrentDictionary<Type, Func<object>> _typeFactoryCache = new();
    private static readonly ConcurrentDictionary<Type, Func<MilvaDbContext, object, object, object, bool, Task>> _getContentsMethodCache = new();

    internal class LookupContext
    {
        public string EntityName { get; set; }
        public Type EntityType { get; set; }
        public Type TranslationEntityType { get; set; }
        public PropertyNames PropertyNames { get; set; }
        public List<string> PropertyNamesForProjection { get; set; }
        public LookupRequestParameter RequestParameter { get; set; }
    }
    internal class PropertyNames
    {
        public List<string> MainEntityPropertyNames { get; set; }
        public List<string> TranslationEntityPropertyNames { get; set; }
        public List<string> TranslationWithJsonEntityPropertyNames { get; set; }
    }

    public async Task<List<object>> GetLookupsAsync(LookupRequest lookupRequest)
    {
        if (lookupRequest?.Parameters.IsNullOrEmpty() ?? true)
            return [];

        ValidateRequestParameters(lookupRequest);

        var resultList = new List<object>(lookupRequest.Parameters.Count);
        var assemblyTypes = _dataAccessConfiguration.DbContext.DynamicFetch.GetEntityAssembly().GetTypes();
        var multiLanguageManager = _dbContext.ServiceProvider?.GetService<IMultiLanguageManager>();

        foreach (var parameter in lookupRequest.Parameters)
        {
            var context = CreateLookupContext(parameter, assemblyTypes);

            var projectionExpression = CreateProjectionExpression(context);

            parameter.UpdateFilterByForTranslationPropertyNames(context.PropertyNames.TranslationEntityPropertyNames);

            var lookupList = await FetchLookupDataAsync(context, projectionExpression);

            if (lookupList.Count > 0)
            {
                var lookups = ProcessLookupResults(lookupList, context, multiLanguageManager);

                resultList.Add(new LookupResult { EntityName = context.EntityName, Data = lookups });
            }
        }

        return resultList;
    }

    private static LookupContext CreateLookupContext(LookupRequestParameter parameter, Type[] assemblyTypes)
    {
        var entityType = GetEntityType(parameter.EntityName, assemblyTypes);
        var translationEntityType = GetTranslationEntityType(entityType, assemblyTypes);

        var propertyNames = CategorizeRequestedProperties(parameter, entityType, translationEntityType);

        return new LookupContext
        {
            EntityName = parameter.EntityName,
            EntityType = entityType,
            TranslationEntityType = translationEntityType,
            PropertyNames = propertyNames,
            PropertyNamesForProjection = BuildPropertyNameListForProjection(entityType, propertyNames.MainEntityPropertyNames),
            RequestParameter = parameter
        };
    }

    private static Type GetEntityType(string entityName, Type[] assemblyTypes)
        => Array.Find(assemblyTypes, i => i.Name == entityName) ?? throw new MilvaUserFriendlyException(MilvaException.InvalidParameter);

    private static Type GetTranslationEntityType(Type entityType, Type[] assemblyTypes)
        => entityType.CanAssignableTo(typeof(IHasTranslation<>))
            ? Array.Find(assemblyTypes, i => i.IsAssignableTo(typeof(ITranslationEntity<>).MakeGenericType(entityType)))
            : null;

    private static PropertyNames CategorizeRequestedProperties(LookupRequestParameter parameter, Type entityType, Type translationEntityType)
    {
        var mainEntityPropertyNames = new List<string>();
        var translationEntityPropNames = new List<string>();
        var translationEntityWithJsonPropNames = new List<string>();

        bool isTranslationsEntityJson = false;
        bool isTranslationsEntitySoftDeletable = false;

        if (translationEntityType != null)
        {
            var translationsProp = entityType.GetProperty(MultiLanguageEntityPropertyNames.Translations);
            var columnAttribute = translationsProp.GetCustomAttribute<ColumnAttribute>();

            isTranslationsEntityJson = columnAttribute != null && columnAttribute.TypeName == "jsonb";

            var type = translationsProp.PropertyType.GetGenericArguments()[0];

            if (!parameter.FetchSoftDeletedEntities && type.CanAssignableTo(typeof(ISoftDeletable)))
                isTranslationsEntitySoftDeletable = true;
        }

        foreach (var requestedPropName in parameter.RequestedPropertyNames)
        {
            var prop = CommonHelper.GetPublicPropertyIgnoreCase(entityType, requestedPropName);

            if (prop != null)
            {
                mainEntityPropertyNames.Add(requestedPropName);
            }
            else if (translationEntityType != null && CommonHelper.PropertyExists(translationEntityType, requestedPropName))
            {
                if (isTranslationsEntityJson)
                    translationEntityWithJsonPropNames.Add(requestedPropName);
                else
                    translationEntityPropNames.Add(requestedPropName);

                if (isTranslationsEntitySoftDeletable)
                    translationEntityPropNames.Add(EntityPropertyNames.IsDeleted);
            }
            else
            {
                throw new MilvaUserFriendlyException(MilvaException.InvalidParameter);
            }
        }

        return new PropertyNames
        {
            MainEntityPropertyNames = mainEntityPropertyNames,
            TranslationEntityPropertyNames = translationEntityPropNames,
            TranslationWithJsonEntityPropertyNames = translationEntityWithJsonPropNames
        };
    }

    private static object CreateProjectionExpression(LookupContext context)
        => _createProjectionExpressionMethod
            .MakeGenericMethod(context.EntityType, context.TranslationEntityType ?? context.EntityType)
            .Invoke(null,
            [
                context.PropertyNamesForProjection,
                context.PropertyNames.TranslationEntityPropertyNames,
                !context.PropertyNames.TranslationWithJsonEntityPropertyNames.IsNullOrEmpty(),
                context.RequestParameter.FetchSoftDeletedEntities
            ]);

    private async Task<IList> FetchLookupDataAsync(LookupContext context, object projectionExpression)
    {
        var taskResult = CallGetContentsAsync(context.EntityType, context.RequestParameter.Filtering, context.RequestParameter.Sorting, projectionExpression, context.RequestParameter.FetchSoftDeletedEntities);

        await taskResult;

        var resultProperty = taskResult.GetType().GetProperty(_taskResultPropertyName);

        return (IList)resultProperty.GetValue(taskResult);
    }

    private static List<object> ProcessLookupResults(IList lookupList, LookupContext context, IMultiLanguageManager multiLanguageManager)
    {
        var lookups = new List<object>(lookupList.Count);

        foreach (var lookup in lookupList)
        {
            var propDic = new Dictionary<string, object>(context.RequestParameter.RequestedPropertyNames.Count + 1);

            foreach (var prop in lookup.GetType().GetProperties())
            {
                ProcessProperty(prop, lookup, propDic, context, multiLanguageManager);
            }

            lookups.Add(propDic);
        }

        return lookups;
    }

    private static void ProcessProperty(PropertyInfo prop, object lookup, Dictionary<string, object> propDic, LookupContext context, IMultiLanguageManager multiLanguageManager)
    {
        if (prop.Name != MultiLanguageEntityPropertyNames.Translations)
        {
            if (prop.Name != EntityPropertyNames.Id && (!context.RequestParameter.RequestedPropertyNames?.Contains(prop.Name) ?? true))
                return;

            propDic.Add(prop.Name, prop.GetValue(lookup, null) ?? GetDefaultValue(prop.PropertyType));
        }
        else
        {
            if (multiLanguageManager != null && context.PropertyNames.TranslationEntityPropertyNames != null)
            {
                foreach (var translationPropName in context.PropertyNames.TranslationEntityPropertyNames.Concat(context.PropertyNames.TranslationWithJsonEntityPropertyNames))
                {
                    if ((!context.RequestParameter.RequestedPropertyNames?.Contains(translationPropName) ?? true))
                        continue;

                    propDic.Add(translationPropName, multiLanguageManager.GetTranslation(lookup, translationPropName));
                }
            }
        }
    }

    private static object GetDefaultValue(Type type)
    {
        if (type == typeof(string))
            return string.Empty;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return null;

        if (type.IsValueType)
        {
            return _typeFactoryCache.GetOrAdd(type, t =>
            {
                var ctor = Expression.New(t);
                var converted = Expression.Convert(ctor, typeof(object));
                var lambda = Expression.Lambda<Func<object>>(converted);
                return lambda.Compile();
            })();
        }

        return null;
    }

    private void ValidateRequestParameters(LookupRequest lookupRequest)
    {
        foreach (var parameter in lookupRequest.Parameters)
        {
            if (string.IsNullOrWhiteSpace(parameter.EntityName)
                || parameter.RequestedPropertyNames.IsNullOrEmpty()
                || parameter.RequestedPropertyNames.Count > _dataAccessConfiguration.DbContext.DynamicFetch.MaxAllowedPropertyCountForLookup
                || !_dataAccessConfiguration.DbContext.DynamicFetch.AllowedEntityNamesForLookup.Contains(parameter.EntityName))
                throw new MilvaUserFriendlyException(MilvaException.InvalidParameter);

            if (parameter.RequestedPropertyNames.Distinct().Count() != parameter.RequestedPropertyNames.Count)
                throw new MilvaUserFriendlyException(MilvaException.InvalidParameter);
        }
    }

    private static List<string> BuildPropertyNameListForProjection(Type entityType, List<string> mainEntityPropertyNames)
    {
        var propNamesForProjection = new List<string>();

        if (entityType.CanAssignableTo(typeof(IMilvaEntity)))
            propNamesForProjection.Add(EntityPropertyNames.Id);

        if (entityType.CanAssignableTo(typeof(IHasTranslation<>)))
            propNamesForProjection.Add(MultiLanguageEntityPropertyNames.Translations);

        if (!mainEntityPropertyNames.IsNullOrEmpty())
            propNamesForProjection.AddRange(mainEntityPropertyNames);

        return propNamesForProjection;
    }

    private Task CallGetContentsAsync(Type entityType, object filtering, object sorting, object projection, bool fetchSoftDeletedEntities)
    {
        var func = _getContentsMethodCache.GetOrAdd(entityType, type =>
        {
            // Get method
            var method = typeof(MilvaDbContext).GetMethod(nameof(MilvaDbContext.GetContentsAsync))!.MakeGenericMethod(type);

            // Parameters: context, filtering, sorting, projection
            var contextParam = Expression.Parameter(typeof(MilvaDbContext));
            var filteringParam = Expression.Parameter(typeof(object));
            var sortingParam = Expression.Parameter(typeof(object));
            var projectionParam = Expression.Parameter(typeof(object));
            var fetchSoftDeletedParam = Expression.Parameter(typeof(bool));

            var call = Expression.Call(contextParam,
                                       method,
                                       Expression.Convert(filteringParam, method.GetParameters()[0].ParameterType),
                                       Expression.Convert(sortingParam, method.GetParameters()[1].ParameterType),
                                       Expression.Convert(projectionParam, method.GetParameters()[2].ParameterType),
                                       fetchSoftDeletedParam);

            var lambda = Expression.Lambda<Func<MilvaDbContext, object, object, object, bool, Task>>(call, contextParam, filteringParam, sortingParam, projectionParam, fetchSoftDeletedParam);

            return lambda.Compile();
        });

        return func(_dbContext, filtering, sorting, projection, fetchSoftDeletedEntities);
    }
}
