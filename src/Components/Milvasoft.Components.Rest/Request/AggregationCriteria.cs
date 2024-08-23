using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using System.Reflection;

namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// Represents criteria used for aggregation data.
/// </summary>
public class AggregationCriteria
{
    private static readonly Dictionary<QueryProviderType, List<MethodInfo>> _aggregationMethods = BuildAggregationMethodDictionary();
    private QueryProviderType _queryProviderType;
    private Type _entityType;
    private Type _propType;
    private bool _runAsync;

    /// <summary>
    /// Aggregation column name.
    /// </summary>
    /// <example>{columnName}</example>
    public virtual string AggregateBy { get; set; }

    /// <summary>
    /// Aggregation type.
    /// </summary>
    /// <example>{columnName}</example>
    public virtual AggregationType Type { get; set; }

    /// <summary>
    /// Applies the aggregation criteria to the given query and returns the aggregation result asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity in the query.</typeparam>
    /// <param name="query">The query to apply the aggregation criteria to.</param>
    /// <param name="runAsync">Indicates whether to run the aggregation asynchronously.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The aggregation result.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S6966:Awaitable method should be used", Justification = "<Pending>")]
    public virtual async Task<AggregationResult> ApplyAggregationAsync<TEntity>(IQueryable<TEntity> query, bool runAsync = true, CancellationToken cancellationToken = default)
    {
        // Check if the aggregation column name or the query is null or empty
        if (string.IsNullOrWhiteSpace(AggregateBy) || query == null)
            return new AggregationResult(AggregateBy, Type, null);

        // Initialize the fields and get the property to aggregate by
        var prop = InitializeFields(query, runAsync);

        // Create the property selector based on the query provider type
        var propertySelector = _queryProviderType switch
        {
            QueryProviderType.List => CommonHelper.DynamicInvokeCreatePropertySelector(nameof(CommonHelper.CreateRequiredPropertySelectorFuction), _entityType, _propType, prop.Name),
            QueryProviderType.Enumerable => CommonHelper.DynamicInvokeCreatePropertySelector(nameof(CommonHelper.CreateRequiredPropertySelector), _entityType, _propType, prop.Name),
            QueryProviderType.AsyncQueryable => CommonHelper.DynamicInvokeCreatePropertySelector(nameof(CommonHelper.CreateRequiredPropertySelector), _entityType, _propType, prop.Name),
            _ => CommonHelper.DynamicInvokeCreatePropertySelector(nameof(CommonHelper.CreateRequiredPropertySelector), _entityType, _propType, prop.Name),
        };

        // Apply the aggregation based on the aggregation type
        if (Type == AggregationType.Count)
        {
            var count = runAsync ? await query.CountAsync(cancellationToken).ConfigureAwait(false) : query.Count();
            return new AggregationResult(prop.Name, Type, count);
        }
        else
        {
            var res = await GenericInvokeAggregationMethodAsync(query, propertySelector, cancellationToken).ConfigureAwait(false);
            return new AggregationResult(prop.Name, Type, res);
        }
    }

    /// <summary>
    /// Invokes the generic aggregation method asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity in the query.</typeparam>
    /// <param name="query">The query to apply the aggregation criteria to.</param>
    /// <param name="propertySelectorResult">The result of the property selector.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the aggregation.</returns>
    private async Task<object> GenericInvokeAggregationMethodAsync<TEntity>(IQueryable<TEntity> query, object propertySelectorResult, CancellationToken cancellationToken = default)
    {
        // Get the generic aggregation method
        MethodInfo genericAggregationMethod = MakeGenericMethod();

        // If the generic aggregation method is null, return null
        if (genericAggregationMethod == null)
            return null;

        object result;

        // Invoke the generic aggregation method based on whether it should run asynchronously or not
        if (_runAsync)
        {
            var taskResult = (Task)genericAggregationMethod.Invoke(null, [query, propertySelectorResult, cancellationToken]);
            await taskResult;
            var resultProperty = taskResult.GetType().GetProperty(nameof(Task<TEntity>.Result));
            result = resultProperty.GetValue(taskResult);
        }
        else
        {
            result = genericAggregationMethod.Invoke(null, [query, propertySelectorResult]);
        }

        return result;
    }

    /// <summary>
    /// Makes the generic aggregation method based on the aggregation type and whether it should run asynchronously or not.
    /// </summary>
    /// <returns>The generic aggregation method.</returns>
    private MethodInfo MakeGenericMethod()
    {
        var methodName = GetMethodName(Type, _runAsync);

        // Get the related methods based on the query provider type and the method name
        var relatedMethods = _aggregationMethods[_queryProviderType].Where(mi => mi.Name == methodName);

        // Get the aggregation method that contains the desired type in its parameters
        MethodInfo aggregationMethod = relatedMethods.FirstOrDefault(method => IsMethodParametersContainsDesiredType(method.GetParameters().Select(i => i.ParameterType)))
                                            ?? throw new MilvaDeveloperException("Aggregation by property type not supported with this aggregation method.");

        // If the aggregation method is not a generic method, return it
        if (!aggregationMethod.IsGenericMethod)
            return aggregationMethod;

        // Make the generic aggregation method based on the entity type and the property type
        MethodInfo genericAggregationMethod = aggregationMethod.GetGenericArguments().Length == 1
                                                ? aggregationMethod.MakeGenericMethod(_entityType)
                                                : aggregationMethod.MakeGenericMethod(_entityType, _propType);

        return genericAggregationMethod;

        // Checks if the method parameters contain the desired type
        bool IsMethodParametersContainsDesiredType(IEnumerable<Type> types)
        {
            var genericTypes = types.Where(i => i.ContainsGenericParameters);

            // If all the generic parameters of the method parameters are generic types, return true
            if (genericTypes.Count() > 1 && genericTypes.All(t => Array.TrueForAll(t.GenericTypeArguments, g => g.IsGenericType)))
                return true;

            // If any of the method parameters is the desired type or the property type, return true
            if (types.Any(type => type.IsGenericType && IsMethodParametersContainsDesiredType(type.GetGenericArguments()) || (type == _propType || type == _entityType)))
                return true;

            return false;
        }
    }

    /// <summary>
    /// Gets the method name based on the aggregation type and whether it should run asynchronously or not.
    /// </summary>
    /// <param name="type">The aggregation type.</param>
    /// <param name="runAsync">Indicates whether to run the aggregation asynchronously.</param>
    /// <returns>The method name.</returns>
    private static string GetMethodName(AggregationType type, bool runAsync) => type switch
    {
        AggregationType.Avg => runAsync ? nameof(EntityFrameworkQueryableExtensions.AverageAsync) : nameof(Queryable.Average),
        AggregationType.Sum => runAsync ? nameof(EntityFrameworkQueryableExtensions.SumAsync) : nameof(Queryable.Sum),
        AggregationType.Min => runAsync ? nameof(EntityFrameworkQueryableExtensions.MinAsync) : nameof(Queryable.Min),
        AggregationType.Max => runAsync ? nameof(EntityFrameworkQueryableExtensions.MaxAsync) : nameof(Queryable.Max),
        AggregationType.Count => runAsync ? nameof(EntityFrameworkQueryableExtensions.CountAsync) : nameof(Queryable.Count),
        _ => string.Empty,
    };

    /// <summary>
    /// Builds the dictionary of aggregation methods based on the query provider type.
    /// </summary>
    /// <returns>The dictionary of aggregation methods.</returns>
    private static Dictionary<QueryProviderType, List<MethodInfo>> BuildAggregationMethodDictionary()
    {
        var aggregationTypes = Enum.GetValues<AggregationType>();

        Dictionary<QueryProviderType, List<MethodInfo>> methodInfos = [];

        methodInfos.Add(QueryProviderType.List, []);
        methodInfos.Add(QueryProviderType.Enumerable, []);
        methodInfos.Add(QueryProviderType.AsyncQueryable, []);

        foreach (var aggregationType in aggregationTypes)
        {
            var syncMethodName = GetMethodName(aggregationType, runAsync: false);
            var asyncMethodName = GetMethodName(aggregationType, runAsync: true);

            // Add collection methods
            var relatedMethods = typeof(Enumerable).GetMethods()
                                                  .Where(mi => mi.Name == syncMethodName
                                                                         && mi.IsGenericMethodDefinition
                                                                         && mi.GetParameters().Length == 2);

            methodInfos[QueryProviderType.List].AddRange(relatedMethods);

            // Add entity framework sync methods
            relatedMethods = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                               .Where(mi => mi.Name == syncMethodName
                                                            && mi.GetParameters().Length == 2);

            methodInfos[QueryProviderType.Enumerable].AddRange(relatedMethods);

            // Add entity framework async methods
            relatedMethods = typeof(EntityFrameworkQueryableExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                       .Where(mi => mi.Name == asyncMethodName
                                                                                    && mi.GetParameters().Length == 3);

            methodInfos[QueryProviderType.AsyncQueryable].AddRange(relatedMethods);
        }

        return methodInfos;
    }

    /// <summary>
    /// Initializes the fields and gets the property to aggregate by.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity in the query.</typeparam>
    /// <param name="query">The query to apply the aggregation criteria to.</param>
    /// <param name="runAsync">Indicates whether to run the aggregation asynchronously.</param>
    /// <returns>The property to aggregate by.</returns>
    private PropertyInfo InitializeFields<TEntity>(IQueryable<TEntity> query, bool runAsync)
    {
        _runAsync = runAsync;
        _entityType = typeof(TEntity);

        var prop = _entityType.ThrowIfPropertyNotExists(AggregateBy);

        _propType = prop.PropertyType.IsNonNullableValueType() ? typeof(Nullable<>).MakeGenericType(prop.PropertyType) : prop.PropertyType;

        // Determine the query provider type based on whether it should run asynchronously or not
#pragma warning disable EF1001 // Internal EF Core API usage.
        if (runAsync)
        {
            if (query.Provider.GetType().IsAssignableTo(typeof(EnumerableQuery)))
                throw new MilvaDeveloperException("Query provider type is 'EnumerableQuery' cannot run asynchronously!");

            _queryProviderType = QueryProviderType.AsyncQueryable;
        }
        else
        {
            if (query.Provider.GetType().IsAssignableTo(typeof(EnumerableQuery)))
                _queryProviderType = QueryProviderType.List;
            else if (query.Provider.GetType().IsAssignableTo(typeof(EntityQueryProvider)))
                _queryProviderType = QueryProviderType.Enumerable;
            else
                _queryProviderType = QueryProviderType.List;
        }
#pragma warning restore EF1001 // Internal EF Core API usage.

        return prop;
    }

    private enum QueryProviderType
    {
        List,
        Enumerable,
        AsyncQueryable
    }
}
