using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Core.Helpers;

/// <summary>
/// Generic collection helper extension methods.
/// </summary>
public static partial class CommonHelper
{
    private static readonly MethodInfo _orderByMethod = typeof(Queryable).GetMethods().Single(method => method.Name == "OrderBy" && method.GetParameters().Length == 2);

    private static readonly MethodInfo _orderByDescendingMethod = typeof(Queryable).GetMethods().Single(method => method.Name == "OrderByDescending" && method.GetParameters().Length == 2);

    /// <summary>
    /// Checks if the collection is null or empty.
    /// </summary>
    /// <param name="this">The collection to check.</param>
    /// <returns>True if the collection is null or empty, otherwise false.</returns>
    public static bool IsNullOrEmpty(this IEnumerable @this) => @this == null || !@this.GetEnumerator().MoveNext();

    /// <summary>
    /// Orders the source collection by the specified property name.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source collection.</typeparam>
    /// <param name="source">The source collection to order.</param>
    /// <param name="propertyName">The name of the property to order by.</param>
    /// <returns>The ordered source collection.</returns>
    public static IQueryable<T> OrderByProperty<T>(this IQueryable<T> source, string propertyName)
    {
        if (source.IsNullOrEmpty() || !source.PropertyExists(propertyName))
            return source;

        var parameter = Expression.Parameter(typeof(T));

        MemberExpression orderByProperty = Expression.Property(parameter, propertyName);

        var lambda = Expression.Lambda(orderByProperty, parameter);

        var genericMethod = _orderByMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);

        var ret = genericMethod.Invoke(null, [source, lambda]);

        return (IQueryable<T>)ret;
    }

    /// <summary>
    /// Orders the source collection in descending order by the specified property name.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source collection.</typeparam>
    /// <param name="source">The source collection to order.</param>
    /// <param name="propertyName">The name of the property to order by.</param>
    /// <returns>The ordered source collection in descending order.</returns>
    public static IQueryable<T> OrderByPropertyDescending<T>(this IQueryable<T> source, string propertyName)
    {
        if (source.IsNullOrEmpty() || !source.PropertyExists(propertyName))
            return source;

        var parameterExpression = Expression.Parameter(typeof(T));

        MemberExpression orderByProperty = Expression.Property(parameterExpression, propertyName);

        var lambda = Expression.Lambda(orderByProperty, parameterExpression);

        var genericMethod = _orderByDescendingMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);

        var ret = genericMethod.Invoke(null, [source, lambda]);

        return (IQueryable<T>)ret;
    }

    /// <summary>
    /// Splits the list into batches with the specified batch size.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to split into batches.</param>
    /// <param name="batchSize">The size of each batch.</param>
    /// <returns>An enumerable of batches.</returns>
    public static IEnumerable<List<T>> ToBatches<T>(this List<T> list, int batchSize = 100)
    {
        if (list.IsNullOrEmpty())
            yield break;

        for (int i = 0; i < list.Count; i += batchSize)
            yield return list.GetRange(i, Math.Min(batchSize, list.Count - i));
    }

    /// <summary>
    /// Updates the singleton implementation instance with the specified update action.
    /// </summary>
    /// <typeparam name="TInstance">The type of the singleton instance.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="updateAction">The update action to apply to the singleton instance.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection UpdateSingletonInstance<TInstance>(this IServiceCollection services, Action<TInstance> updateAction) where TInstance : class
        => services.UpdateSingletonInstance<TInstance, TInstance>(updateAction);

    /// <summary>
    /// Updates the singleton implementation instance with the specified update action.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <typeparam name="TInstance">The type of the singleton instance.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="updateAction">The update action to apply to the singleton instance.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection UpdateSingletonInstance<TImplementation, TInstance>(this IServiceCollection services, Action<TInstance> updateAction) where TInstance : class
    {
        if (services.IsNullOrEmpty() || updateAction == null)
            return services;

        var servicesToReplace = services.Where(service => service.ServiceType == typeof(TImplementation)).ToList();

        foreach (var service in servicesToReplace)
        {
            if (service.ImplementationInstance != null && service.Lifetime == ServiceLifetime.Singleton)
                updateAction.Invoke((TInstance)service.ImplementationInstance);
        }

        return services;
    }

    /// <summary>
    /// Filters the content list by the specified start and end dates.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the content list.</typeparam>
    /// <param name="contentList">The content list to filter.</param>
    /// <param name="dateProperty">The expression representing the date property.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>The filtered content list.</returns>
    /// 
    /// <remarks>
    /// If a selection has been made between two dates, it will return those between the two dates.
    /// If only the <paramref name="endDate"/> value exists, it returns those larger than the <paramref name="startDate"/> value.
    /// If only the <paramref name="startDate"/> value exists, it returns those smaller than the <paramref name="endDate"/> value.
    /// </remarks>
    public static IEnumerable<T> ApplyDateSearch<T>(this IEnumerable<T> contentList, Expression<Func<T, DateTime?>> dateProperty, DateTime? startDate, DateTime? endDate)
    {
        if (contentList.IsNullOrEmpty() || dateProperty == null || (!startDate.HasValue && !endDate.HasValue))
            return contentList;

        var propertyName = dateProperty.GetPropertyName();

        // If a selection has been made between two dates, it will return those between the two dates.
        if (startDate.HasValue && endDate.HasValue)
            contentList = contentList.Where(i => (DateTime)i.GetType().GetProperty(propertyName).GetValue(i, null) >= startDate.Value && (DateTime)i.GetType().GetProperty(propertyName).GetValue(i, null) <= endDate.Value);
        // If only the start date value exists, it returns those larger than the start date value.
        else if (startDate.HasValue && !endDate.HasValue)
        {
            contentList = contentList.Where(i => (DateTime)i.GetType().GetProperty(propertyName).GetValue(i, null) >= startDate.Value);
        }
        // If only the end date value exists, it returns those smaller than the end date value.
        else if (!startDate.HasValue && endDate.HasValue)
            contentList = contentList.Where(i => (DateTime)i.GetType().GetProperty(propertyName).GetValue(i, null) <= endDate.Value);

        return contentList;
    }
}
