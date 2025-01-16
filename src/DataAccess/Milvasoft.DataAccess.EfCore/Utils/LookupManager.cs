using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.MultiLanguage;
using Milvasoft.Core.MultiLanguage.EntityBases;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.Manager;
using Milvasoft.DataAccess.EfCore.DbContextBase;
using Milvasoft.DataAccess.EfCore.Utils.LookupModels;
using System.Collections;
using System.Reflection;

namespace Milvasoft.DataAccess.EfCore.Utils;
internal class LookupManager(MilvaDbContext dbContext, IDataAccessConfiguration dataAccessConfiguration)
{
    private readonly MilvaDbContext _dbContext = dbContext;
    private readonly IDataAccessConfiguration _dataAccessConfiguration = dataAccessConfiguration;
    private static readonly MethodInfo _createProjectionExpressionMethod = typeof(MultiLanguageExtensions).GetMethod(nameof(MultiLanguageExtensions.CreateProjectionExpression));
    private const string _taskResultPropertyName = "Result";

    internal class LookupContext
    {
        public string EntityName { get; set; }
        public Type EntityType { get; set; }
        public Type TranslationEntityType { get; set; }
        public List<string> MainEntityPropertyNames { get; set; }
        public List<string> TranslationEntityPropertyNames { get; set; }
        public List<string> PropertyNamesForProjection { get; set; }
        public LookupRequestParameter RequestParameter { get; set; }
    }

    public async Task<List<object>> GetLookupsAsync(LookupRequest lookupRequest)
    {
        if (lookupRequest?.Parameters.IsNullOrEmpty() ?? true)
            return [];

        ValidateRequestParameters(lookupRequest);

        var resultList = new List<object>();
        var assemblyTypes = _dataAccessConfiguration.DbContext.DynamicFetch.GetEntityAssembly().GetTypes();
        var multiLanguageManager = _dbContext.ServiceProvider?.GetService<IMultiLanguageManager>();

        foreach (var parameter in lookupRequest.Parameters)
        {
            var context = CreateLookupContext(parameter, assemblyTypes);

            var projectionExpression = CreateProjectionExpression(context);

            parameter.UpdateFilterByForTranslationPropertyNames(context.TranslationEntityPropertyNames);

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

        var (mainEntityPropertyNames, translationEntityPropNames) = CategorizeRequestedProperties(parameter, entityType, translationEntityType);

        return new LookupContext
        {
            EntityName = parameter.EntityName,
            EntityType = entityType,
            TranslationEntityType = translationEntityType,
            MainEntityPropertyNames = mainEntityPropertyNames,
            TranslationEntityPropertyNames = translationEntityPropNames,
            PropertyNamesForProjection = BuildPropertyNameListForProjection(entityType, mainEntityPropertyNames),
            RequestParameter = parameter
        };
    }

    private static Type GetEntityType(string entityName, Type[] assemblyTypes)
        => Array.Find(assemblyTypes, i => i.Name == entityName) ?? throw new MilvaUserFriendlyException(MilvaException.InvalidParameter);

    private static Type GetTranslationEntityType(Type entityType, Type[] assemblyTypes)
        => entityType.CanAssignableTo(typeof(IHasTranslation<>))
            ? Array.Find(assemblyTypes, i => i.IsAssignableTo(typeof(ITranslationEntity<>).MakeGenericType(entityType)))
            : null;

    private static (List<string> mainEntityPropertyNames, List<string> translationEntityPropNames) CategorizeRequestedProperties(LookupRequestParameter parameter, Type entityType, Type translationEntityType)
    {
        var mainEntityPropertyNames = new List<string>();
        var translationEntityPropNames = new List<string>();

        foreach (var requestedPropName in parameter.RequestedPropertyNames)
        {
            if (CommonHelper.PropertyExists(entityType, requestedPropName))
            {
                mainEntityPropertyNames.Add(requestedPropName);
            }
            else if (translationEntityType != null && CommonHelper.PropertyExists(translationEntityType, requestedPropName))
            {
                translationEntityPropNames.Add(requestedPropName);
            }
            else
            {
                throw new MilvaUserFriendlyException(MilvaException.InvalidParameter);
            }
        }

        return (mainEntityPropertyNames, translationEntityPropNames);
    }

    private static object CreateProjectionExpression(LookupContext context)
        => _createProjectionExpressionMethod
            .MakeGenericMethod(context.EntityType, context.TranslationEntityType ?? context.EntityType)
            .Invoke(null, [context.PropertyNamesForProjection, context.TranslationEntityPropertyNames]);

    private async Task<IList> FetchLookupDataAsync(LookupContext context, object projectionExpression)
    {
        var taskResult = (Task)typeof(MilvaDbContext)
                                   .GetMethod(nameof(MilvaDbContext.GetContentsAsync))
                                   .MakeGenericMethod(context.EntityType)
                                   .Invoke(_dbContext, [context.RequestParameter.Filtering, context.RequestParameter.Sorting, projectionExpression]);

        await taskResult;

        var resultProperty = taskResult.GetType().GetProperty(_taskResultPropertyName);

        return (IList)resultProperty.GetValue(taskResult);
    }

    private static List<object> ProcessLookupResults(IList lookupList, LookupContext context, IMultiLanguageManager multiLanguageManager)
    {
        var lookups = new List<object>();

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
            if (multiLanguageManager != null && context.TranslationEntityPropertyNames != null)
            {
                foreach (var translationPropName in context.TranslationEntityPropertyNames)
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
        if (type.IsValueType)
            return Activator.CreateInstance(type);
        else if (type.Name.Contains(nameof(String)))
            return string.Empty;
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
}
