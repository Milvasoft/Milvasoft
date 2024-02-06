using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Core;
using System.Reflection;

namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// Represents criteria used for aggregation data.
/// </summary>
public class AggregationCriteria
{
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

    public virtual async Task<AggregationResult> ApplyAggregationAsync<TEntity>(IQueryable<TEntity> query)
    {
        if (string.IsNullOrEmpty(AggregateBy))
            return new AggregationResult(AggregateBy, Type, null);

        var entityType = typeof(TEntity);

        var prop = entityType.GetPublicPropertyIgnoreCase(AggregateBy);

        var propType = prop.PropertyType;

        var propertySelector = CreatePropertySelector(entityType, propType);

        if (Type == AggregationType.Count)
        {
            return new AggregationResult(prop.Name, Type, await query.CountAsync().ConfigureAwait(false));
        }
        else return new AggregationResult(prop.Name, Type, GenericInvokeAggregationMethodAsync(query, propertySelector, entityType, propType));
    }

    /// <summary>
    /// If <see cref="SortBy"/> property is not null or empty apply sorting with <see cref="SortBy"/> and <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    public virtual AggregationResult ApplyAggregation<TEntity>(IQueryable<TEntity> query)
    {
        if (string.IsNullOrEmpty(AggregateBy))
            return new AggregationResult(AggregateBy, Type, null);

        var entityType = typeof(TEntity);

        var prop = entityType.GetPublicPropertyIgnoreCase(AggregateBy);

        var propType = prop.PropertyType;

        var propertySelector = CreatePropertySelector(entityType, propType);

        if (Type == AggregationType.Count)
        {
            return new AggregationResult(prop.Name, Type, query.Count());
        }
        else return new AggregationResult(prop.Name, Type, GenericInvokeAggregationMethod(query, propertySelector, entityType, propType));
    }

    private object CreatePropertySelector(Type entityType, Type propType)
    {
        // Step 1: Get the MethodInfo object for the generic method
        var selectorMethod = typeof(CommonHelper).GetMethod("CreateRequiredPropertySelector");

        // Step 2: Construct the method generic with desired type of arguments
        MethodInfo genericSelectorMethod = selectorMethod.MakeGenericMethod(entityType, propType);

        // Step 3: Call the generic method with the specified type arguments
        var propertySelectorResult = genericSelectorMethod.Invoke(null, new object[] { AggregateBy });

        return propertySelectorResult;
    }

    private async Task<object> GenericInvokeAggregationMethodAsync<TEntity>(IQueryable<TEntity> query, object propertySelectorResult, Type entityType, Type propType)
    {
        MethodInfo genericAggregationMethod = MakeGenericMethod(entityType, propType);

        var taskResult = (Task)genericAggregationMethod.Invoke(null, new object[] { query, propertySelectorResult });

        await taskResult;

        var resultProperty = taskResult.GetType().GetProperty("Result");

        var result = resultProperty.GetValue(taskResult);

        return result;
    }

    private object GenericInvokeAggregationMethod<TEntity>(IQueryable<TEntity> query, object propertySelectorResult, Type entityType, Type propType)
    {
        MethodInfo genericAggregationMethod = MakeGenericMethod(entityType, propType);

        var result = genericAggregationMethod.Invoke(null, new object[] { query, propertySelectorResult });

        return result;
    }

    private static string GetMethodName(AggregationType type, bool isAsync = false) => type switch
    {
        AggregationType.Avg => isAsync ? "AverageAsync" : "Average",
        AggregationType.Sum => isAsync ? "SumAsync" : "Sum",
        AggregationType.Min => isAsync ? "MinByAsync" : "MinBy",
        AggregationType.Max => isAsync ? "MaxByAsync" : "MaxBy",
        AggregationType.Count => isAsync ? "CountAsync" : "Count",
        _ => string.Empty,
    };

    private MethodInfo MakeGenericMethod(Type entityType, Type propType)
    {
        var aggreagationMethod = typeof(Queryable).GetMethods().FirstOrDefault(mi => mi.Name == GetMethodName(Type, true)
                                                                     && mi.IsGenericMethodDefinition
                                                                     //&& mi.GetParameters().Any(pi => pi.ParameterType == propType)
                                                                     && mi.GetParameters().Length == 2);

        MethodInfo genericAggregationMethod;

        if (aggreagationMethod.GetGenericArguments().Length == 1)
        {
            genericAggregationMethod = aggreagationMethod.MakeGenericMethod(entityType);
        }
        else genericAggregationMethod = aggreagationMethod.MakeGenericMethod(entityType, propType);

        return genericAggregationMethod;
    }
}
