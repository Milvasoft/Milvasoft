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
    /// Checks whether or not collection is null or empty. Assumes collection can be safely enumerated multiple times.
    /// </summary>
    public static bool IsNullOrEmpty(this IEnumerable @this) => @this == null || @this.GetEnumerator().MoveNext() == false;

    /// <summary>
    /// Order <paramref name="source"/> by <paramref name="propertyName"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static IQueryable<T> OrderByProperty<T>(this IQueryable<T> source, string propertyName)
    {
        if (source.IsNullOrEmpty() || !source.PropertyExists(propertyName))
            return source;

        var parameter = Expression.Parameter(typeof(T));

        Expression orderByProperty = Expression.Property(parameter, propertyName);

        var lambda = Expression.Lambda(orderByProperty, parameter);

        var genericMethod = _orderByMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);

        var ret = genericMethod.Invoke(null, [source, lambda]);

        return (IQueryable<T>)ret;
    }

    /// <summary>
    /// Order by descending <paramref name="source"/> by <paramref name="propertyName"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static IQueryable<T> OrderByPropertyDescending<T>(this IQueryable<T> source, string propertyName)
    {
        if (source.IsNullOrEmpty() || !source.PropertyExists(propertyName))
            return source;

        var paramterExpression = Expression.Parameter(typeof(T));

        Expression orderByProperty = Expression.Property(paramterExpression, propertyName);

        var lambda = Expression.Lambda(orderByProperty, paramterExpression);

        var genericMethod = _orderByDescendingMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);

        var ret = genericMethod.Invoke(null, [source, lambda]);

        return (IQueryable<T>)ret;
    }

    /// <summary>
    /// Splits list into batches with specified batch size.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="batchSize"></param>
    /// <returns></returns>
    public static IEnumerable<List<T>> ToBatches<T>(this List<T> list, int batchSize = 100)
    {
        if (list.IsNullOrEmpty())
            yield break;

        for (int i = 0; i < list.Count; i += batchSize)
            yield return list.GetRange(i, Math.Min(batchSize, list.Count - i));
    }

    /// <summary>
    /// Updates singleton implementation instance with <paramref name="updateAction"/>.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <param name="services"></param>
    /// <param name="updateAction"></param>
    /// <returns></returns>
    public static IServiceCollection UpdateSingletonInstance<TInstance>(this IServiceCollection services, Action<TInstance> updateAction) where TInstance : class
        => services.UpdateSingletonInstance<TInstance, TInstance>(updateAction);

    /// <summary>
    /// Updates singleton implementation instance with <paramref name="updateAction"/>.
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    /// <typeparam name="TInstance"></typeparam>
    /// <param name="services"></param>
    /// <param name="updateAction"></param>
    /// <returns></returns>
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
    /// <para> Filter <paramref name="contentList"/> by <paramref name="dateTopValue"/> and <paramref name="dateLowerValue"/> values. </para>
    /// </summary>
    /// 
    /// <remarks>
    /// 
    /// <para><b>Remarks: </b></para>
    /// 
    /// <para> If a selection has been made between two dates, it will return those between the two dates. </para>
    /// <para> If only the <paramref name="dateTopValue"/> value exists, it returns those larger than the <paramref name="dateLowerValue"/> value. </para>
    /// <para> If only the <paramref name="dateLowerValue"/> value exists, it returns those smaller than the <paramref name="dateTopValue"/> value. </para>
    /// 
    /// </remarks>
    ///
    /// <typeparam name="T"></typeparam>
    /// <param name="contentList"></param>
    /// <param name="dateTopValue"></param>
    /// <param name="dateLowerValue"></param>
    /// <param name="dateProperty"></param>
    /// <returns> Filtered <paramref name="contentList"/> </returns>
    public static IEnumerable<T> FilterByDate<T>(this IEnumerable<T> contentList, DateTime? dateTopValue, DateTime? dateLowerValue, Expression<Func<T, DateTime?>> dateProperty)
    {
        var propertyName = dateProperty.GetPropertyName();

        //If a selection has been made between two dates, it will return those between the two dates.
        if (dateLowerValue.HasValue && dateTopValue.HasValue)
            contentList = contentList.Where(i => (DateTime)i.GetType().GetProperty(propertyName).GetValue(i, null) >= dateLowerValue.Value && (DateTime)i.GetType().GetProperty(propertyName).GetValue(i, null) <= dateTopValue.Value);

        // If only the DateTopValue value exists, it returns those larger than the DateLowerValue value.
        else if (dateLowerValue.HasValue && !dateTopValue.HasValue)
        {
            contentList = contentList.Where(i => (DateTime)i.GetType().GetProperty(propertyName).GetValue(i, null) >= dateLowerValue.Value);
        }

        //If only the DateLowerValue value exists, it returns those smaller than the DateTopValue value.
        else if (!dateLowerValue.HasValue && dateTopValue.HasValue)
            contentList = contentList.Where(i => (DateTime)i.GetType().GetProperty(propertyName).GetValue(i, null) <= dateTopValue.Value);
        return contentList;
    }
}
