﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Core;
using Milvasoft.Core.Exceptions;
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
    private string _aggregateBy;
    private AggregationType _type;

    /// <summary>
    /// Aggregation column name.
    /// </summary>
    /// <example>{columnName}</example>
    public virtual string AggregateBy { get => _aggregateBy; set => _aggregateBy = value; }

    /// <summary>
    /// Aggregation type.
    /// </summary>
    /// <example>{columnName}</example>
    public virtual AggregationType Type { get => _type; set => _type = value; }

    public virtual async Task<AggregationResult> ApplyAggregationAsync<TEntity>(IQueryable<TEntity> query, bool runAsync = true)
    {
        if (string.IsNullOrEmpty(_aggregateBy))
            return new AggregationResult(_aggregateBy, _type, null);

        var prop = InitializeFields(query, runAsync);

        var propertySelector = CreatePropertySelector();

        if (_type == AggregationType.Count)
        {
            var count = runAsync ? await query.CountAsync().ConfigureAwait(false) : query.Count();

            return new AggregationResult(prop.Name, _type, count);
        }
        else
        {
            var res = await GenericInvokeAggregationMethodAsync(query, propertySelector);

            return new AggregationResult(prop.Name, _type, res);
        }
    }

    private async Task<object> GenericInvokeAggregationMethodAsync<TEntity>(IQueryable<TEntity> query, object propertySelectorResult)
    {
        MethodInfo genericAggregationMethod = MakeGenericMethod();

        if (genericAggregationMethod == null)
            return null;

        object result;

        if (_runAsync)
        {
            var taskResult = (Task)genericAggregationMethod.Invoke(null, new object[] { query, propertySelectorResult, null });

            await taskResult;

            var resultProperty = taskResult.GetType().GetProperty("Result");

            result = resultProperty.GetValue(taskResult);
        }
        else
        {
            result = genericAggregationMethod?.Invoke(null, new object[] { query, propertySelectorResult });
        }

        return result;
    }

    private MethodInfo MakeGenericMethod()
    {
        MethodInfo aggregationMethod = null;

        var methodName = GetMethodName(_type, _runAsync);

        var relatedMethods = _aggregationMethods[_queryProviderType].Where(mi => mi.Name == methodName);

        foreach (var method in relatedMethods)
        {
            var methodParameters = method.GetParameters().Select(i => i.ParameterType);

            if (IsMethodParametersContainsDesiredType(methodParameters))
            {
                aggregationMethod = method;
                break;
            }
        }

        if (aggregationMethod == null)
            throw new MilvaDeveloperException("Unkown query provider.");

        if (!aggregationMethod.IsGenericMethod)
            return aggregationMethod;

        MethodInfo genericAggregationMethod;

        if (aggregationMethod.GetGenericArguments().Length == 1)
        {
            genericAggregationMethod = aggregationMethod.MakeGenericMethod(_entityType);
        }
        else genericAggregationMethod = aggregationMethod.MakeGenericMethod(_entityType, _propType);

        return genericAggregationMethod;

        bool IsMethodParametersContainsDesiredType(IEnumerable<Type> types)
        {
            var genericTypes = types.Where(i => i.ContainsGenericParameters);

            //If method parameter's all generic parameters is generic type
            if (genericTypes.Count() > 1 && genericTypes.All(t => t.GenericTypeArguments.All(g => g.IsGenericType)))
                return true;

            foreach (var type in types)
            {
                if (type.IsGenericType)
                    if (IsMethodParametersContainsDesiredType(type.GetGenericArguments()))
                        return true;

                if (type == _propType || type == _entityType)
                    return true;
            }

            return false;
        }
    }

    private object CreatePropertySelector()
    {
        string selectorMethodName = _queryProviderType switch
        {
            QueryProviderType.List => nameof(CommonHelper.CreateRequiredPropertySelector),
            QueryProviderType.Enumerable => nameof(CommonHelper.CreateRequiredPropertySelectorFuction),
            QueryProviderType.AsyncQueryable => nameof(CommonHelper.CreateRequiredPropertySelector),
            _ => nameof(CommonHelper.CreateRequiredPropertySelector),
        };

        // Step 1: Get the MethodInfo object for the generic method
        var selectorMethod = typeof(CommonHelper).GetMethod(selectorMethodName);

        // Step 2: Construct the method generic with desired type of arguments
        MethodInfo genericSelectorMethod = selectorMethod.MakeGenericMethod(_entityType, _propType);

        // Step 3: Call the generic method with the specified type arguments
        var propertySelectorResult = genericSelectorMethod.Invoke(null, new object[] { _aggregateBy });

        return propertySelectorResult;
    }

    private static string GetMethodName(AggregationType type, bool runAsync) => type switch
    {
        AggregationType.Avg => runAsync ? nameof(EntityFrameworkQueryableExtensions.AverageAsync) : nameof(Queryable.Average),
        AggregationType.Sum => runAsync ? nameof(EntityFrameworkQueryableExtensions.SumAsync) : nameof(Queryable.Sum),
        AggregationType.Min => runAsync ? nameof(EntityFrameworkQueryableExtensions.MinAsync) : nameof(Queryable.MinBy),
        AggregationType.Max => runAsync ? nameof(EntityFrameworkQueryableExtensions.MaxAsync) : nameof(Queryable.MaxBy),
        AggregationType.Count => runAsync ? nameof(EntityFrameworkQueryableExtensions.CountAsync) : nameof(Queryable.Count),
        _ => string.Empty,
    };

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

            //Add collection methods
            var relatedMethods = typeof(Queryable).GetMethods()
                                                  .Where(mi => mi.Name == syncMethodName
                                                                         && mi.IsGenericMethodDefinition
                                                                         && mi.GetParameters().Length == 2);

            methodInfos[QueryProviderType.List].AddRange(relatedMethods);

            //Add entity framework sync methods
            relatedMethods = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                               .Where(mi => mi.Name == syncMethodName
                                                            && mi.GetParameters().Length == 2);

            methodInfos[QueryProviderType.Enumerable].AddRange(relatedMethods);

            //Add entity framework async methods
            relatedMethods = typeof(EntityFrameworkQueryableExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                       .Where(mi => mi.Name == asyncMethodName
                                                                                    && mi.GetParameters().Length == 3);

            methodInfos[QueryProviderType.AsyncQueryable].AddRange(relatedMethods);

        }

        return methodInfos;
    }

    private PropertyInfo InitializeFields<TEntity>(IQueryable<TEntity> query, bool runAsync)
    {
        _runAsync = runAsync;
        _entityType = typeof(TEntity);

        var prop = _entityType.ThrowIfPropertyNotExists(_aggregateBy);

        _propType = prop.PropertyType;

        if (runAsync)
        {
            _queryProviderType = QueryProviderType.AsyncQueryable;
        }
        else
        {
            if (query.Provider.GetType().IsAssignableTo(typeof(EnumerableQuery)))
                _queryProviderType = QueryProviderType.List;
            else if (query.Provider.GetType().IsAssignableTo(typeof(EntityQueryProvider)))
                _queryProviderType = QueryProviderType.Enumerable;
            else _queryProviderType = QueryProviderType.List;
        }

        return prop;
    }



    private enum QueryProviderType
    {
        List,
        Enumerable,
        AsyncQueryable
    }
}