using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Utils.Models;
using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;

namespace Milvasoft.Core.Extensions;

/// <summary>
/// Generic collection helper extension methods.
/// </summary>
public static class GenericCollection
{
    private static readonly MethodInfo _orderByMethod = typeof(Queryable).GetMethods().Single(method => method.Name == "OrderBy" && method.GetParameters().Length == 2);

    private static readonly MethodInfo _orderByDescendingMethod = typeof(Queryable).GetMethods().Single(method => method.Name == "OrderByDescending" && method.GetParameters().Length == 2);

    /// <summary>
    /// Checks whether or not collection is null or empty. Assumes collection can be safely enumerated multiple times.
    /// </summary>
    public static bool IsNullOrEmpty(this IEnumerable @this) => @this == null || @this.GetEnumerator().MoveNext() == false;

    /// <summary>
    /// Checks whether property exists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"> Source collection for code readiness. </param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static bool PropertyExists<T>(this IQueryable<T> _, string propertyName) => typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase
                                                                                                                                | BindingFlags.Public
                                                                                                                                | BindingFlags.Instance) != null;

    /// <summary>
    /// Order <paramref name="source"/> by <paramref name="propertyName"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static IQueryable<T> OrderByProperty<T>(this IQueryable<T> source, string propertyName)
    {
        if (typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase
                                                | BindingFlags.Public
                                                | BindingFlags.Instance) == null)
        {
            return null;
        }

        var paramterExpression = Expression.Parameter(typeof(T));

        Expression orderByProperty = Expression.Property(paramterExpression, propertyName);

        var lambda = Expression.Lambda(orderByProperty, paramterExpression);

        var genericMethod = _orderByMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);

        var ret = genericMethod.Invoke(null, new object[] { source, lambda });

        return (IQueryable<T>)ret;
    }

    /// <summary>
    /// Order by descending <paramref name="source"/>, by <paramref name="propertyName"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static IQueryable<T> OrderByPropertyDescending<T>(this IQueryable<T> source, string propertyName)
    {
        if (typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase
                                                    | BindingFlags.Public
                                                        | BindingFlags.Instance) == null)
        {
            return null;
        }

        var paramterExpression = Expression.Parameter(typeof(T));

        Expression orderByProperty = Expression.Property(paramterExpression, propertyName);

        var lambda = Expression.Lambda(orderByProperty, paramterExpression);

        var genericMethod = _orderByDescendingMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);

        var ret = genericMethod.Invoke(null, new object[] { source, lambda });

        return (IQueryable<T>)ret;
    }

    #region Max Methods 

    /// <summary>
    /// Returns the maximum value according to <paramref name="propertySelector"/> in <paramref name="source"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="propertySelector"></param>
    /// <returns> A object of type T. </returns>
    public static T MilvaMax<T>(this IEnumerable<T> source, Expression<Func<T, decimal>> propertySelector) where T : class
    {
        if (source.IsNullOrEmpty())
            throw new MilvaDeveloperException("Source cannot be null or empty.");

        decimal value = 0;
        T returnVal = null;
        bool hasValue = false;

        foreach (var item in source)
        {
            var dynamicValue = item.GetPropertyValue(propertySelector);

            if (hasValue)
            {
                if (dynamicValue > value)
                {
                    value = dynamicValue;
                    returnVal = item;
                }
            }
            else
            {
                value = dynamicValue;
                returnVal = item;
                hasValue = true;
            }
        }

        if (hasValue)
            return returnVal;

        throw new MilvaDeveloperException("Sequence contains no elements.");
    }

    /// <summary>
    /// Returns the maximum value according to <paramref name="propertySelector"/> in <paramref name="source"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="propertySelector"></param>
    /// <returns>  A service object of type T or null if there is no such service. </returns>
    public static T MilvaMaxOrDefault<T>(this IEnumerable<T> source, Expression<Func<T, decimal>> propertySelector) where T : class
    {
        if (source.IsNullOrEmpty())
            return default;

        decimal value = 0;
        T returnVal = null;
        bool hasValue = false;

        foreach (var item in source)
        {
            var dynamicValue = item.GetPropertyValue(propertySelector);

            if (hasValue)
            {
                if (dynamicValue > value)
                {
                    value = dynamicValue;
                    returnVal = item;
                }
            }
            else
            {
                value = dynamicValue;
                returnVal = item;
                hasValue = true;
            }
        }

        if (hasValue)
            return returnVal;

        return default;
    }

    /// <summary>
    /// Returns the maximum value according to <paramref name="propertySelector"/> in <paramref name="source"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="propertySelector"></param>
    /// <returns> A object of type T. </returns>
    public static T MilvaMax<T>(this IEnumerable<T> source, Expression<Func<T, int>> propertySelector) where T : class
    {
        if (source.IsNullOrEmpty())
            throw new MilvaDeveloperException("Source cannot be null or empty.");

        int value = 0;
        T returnVal = null;
        bool hasValue = false;

        foreach (var item in source)
        {
            var dynamicValue = item.GetPropertyValue(propertySelector);

            if (hasValue)
            {
                if (dynamicValue > value)
                {
                    value = dynamicValue;
                    returnVal = item;
                }
            }
            else
            {
                value = dynamicValue;
                returnVal = item;
                hasValue = true;
            }
        }

        if (hasValue)
            return returnVal;

        throw new MilvaDeveloperException("Sequence contains no elements.");
    }

    /// <summary>
    /// Returns the maximum value according to <paramref name="propertySelector"/> in <paramref name="source"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="propertySelector"></param>
    /// <returns>  A service object of type T or null if there is no such service. </returns>
    public static T MilvaMaxOrDefault<T>(this IEnumerable<T> source, Expression<Func<T, int>> propertySelector) where T : class
    {
        if (source.IsNullOrEmpty())
            return default;

        int value = 0;
        T returnVal = null;
        bool hasValue = false;

        foreach (var item in source)
        {
            var dynamicValue = item.GetPropertyValue(propertySelector);

            if (hasValue)
            {
                if (dynamicValue > value)
                {
                    value = dynamicValue;
                    returnVal = item;
                }
            }
            else
            {
                value = dynamicValue;
                returnVal = item;
                hasValue = true;
            }
        }

        if (hasValue)
            return returnVal;

        return default;
    }

    #endregion

    #region Min Methods 

    /// <summary>
    /// Returns the maximum value according to <paramref name="propertySelector"/> in <paramref name="source"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="propertySelector"></param>
    /// <returns> A object of type T. </returns>
    public static T MilvaMin<T>(this IEnumerable<T> source, Expression<Func<T, decimal>> propertySelector) where T : class
    {
        if (source.IsNullOrEmpty())
            throw new MilvaDeveloperException("Source cannot be null or empty.");

        decimal value = 0;
        T returnVal = null;
        bool hasValue = false;

        foreach (var item in source)
        {
            var dynamicValue = item.GetPropertyValue(propertySelector);

            if (hasValue)
            {
                if (dynamicValue < value)
                {
                    value = dynamicValue;
                    returnVal = item;
                }
            }
            else
            {
                value = dynamicValue;
                returnVal = item;
                hasValue = true;
            }
        }

        if (hasValue)
            return returnVal;

        throw new MilvaDeveloperException("Sequence contains no elements.");
    }

    /// <summary>
    /// Returns the maximum value according to <paramref name="propertySelector"/> in <paramref name="source"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="propertySelector"></param>
    /// <returns>  A service object of type T or null if there is no such service. </returns>
    public static T MilvaMinOrDefault<T>(this IEnumerable<T> source, Expression<Func<T, decimal>> propertySelector) where T : class
    {
        if (source.IsNullOrEmpty())
            return default;

        decimal value = 0;
        T returnVal = null;
        bool hasValue = false;

        foreach (var item in source)
        {
            var dynamicValue = item.GetPropertyValue(propertySelector);

            if (hasValue)
            {
                if (dynamicValue < value)
                {
                    value = dynamicValue;
                    returnVal = item;
                }
            }
            else
            {
                value = dynamicValue;
                returnVal = item;
                hasValue = true;
            }
        }

        if (hasValue)
            return returnVal;

        return default;
    }

    /// <summary>
    /// Returns the maximum value according to <paramref name="propertySelector"/> in <paramref name="source"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="propertySelector"></param>
    /// <returns> A object of type T. </returns>
    public static T MilvaMin<T>(this IEnumerable<T> source, Expression<Func<T, int>> propertySelector) where T : class
    {
        if (source.IsNullOrEmpty())
            throw new MilvaDeveloperException("Source cannot be null or empty.");

        int value = 0;
        T returnVal = null;
        bool hasValue = false;

        foreach (var item in source)
        {
            var dynamicValue = item.GetPropertyValue(propertySelector);

            if (hasValue)
            {
                if (dynamicValue < value)
                {
                    value = dynamicValue;
                    returnVal = item;
                }
            }
            else
            {
                value = dynamicValue;
                returnVal = item;
                hasValue = true;
            }
        }

        if (hasValue)
            return returnVal;

        throw new MilvaDeveloperException("Sequence contains no elements.");
    }

    /// <summary>
    /// Returns the maximum value according to <paramref name="propertySelector"/> in <paramref name="source"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="propertySelector"></param>
    /// <returns>  A service object of type T or null if there is no such service. </returns>
    public static T MilvaMinOrDefault<T>(this IEnumerable<T> source, Expression<Func<T, int>> propertySelector) where T : class
    {
        if (source.IsNullOrEmpty())
            return default;

        int value = 0;
        T returnVal = null;
        bool hasValue = false;

        foreach (var item in source)
        {
            var dynamicValue = item.GetPropertyValue(propertySelector);

            if (hasValue)
            {
                if (dynamicValue < value)
                {
                    value = dynamicValue;
                    returnVal = item;
                }
            }
            else
            {
                value = dynamicValue;
                returnVal = item;
                hasValue = true;
            }
        }

        if (hasValue)
            return returnVal;

        return default;
    }

    #endregion

    /// <summary>
    /// Gets requested property value.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="property"> property </param>
    /// <returns></returns>
    public static decimal GetPropertyValue<T>(this T obj, Expression<Func<T, decimal>> property)
        => (decimal)obj.GetType().GetProperty(property.GetPropertyName()).GetValue(obj, null);

    /// <summary>
    /// Gets requested property value.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="property"> property </param>
    /// <returns></returns>
    public static int GetPropertyValue<T>(this T obj, Expression<Func<T, int>> property)
        => (int)obj.GetType().GetProperty(property.GetPropertyName()).GetValue(obj, null);

    /// <summary>
    /// Collection mapping.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="values"></param>
    /// <param name="mapExpression"></param>
    /// <returns></returns>
    public static IEnumerable<TOut> Map<TIn, TOut>(this IEnumerable<TIn> values, Expression<Func<TIn, TOut>> mapExpression) where TOut : class, new()
    {
        var mapFunction = mapExpression.Compile();

        foreach (var value in values)
            yield return mapFunction.Invoke(value);
    }

    /// <summary>
    /// Splits list into batches with specified batch size.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="batchSize"></param>
    /// <returns></returns>
    public static IEnumerable<List<T>> SplitList<T>(this List<T> list, int batchSize = 100)
    {
        for (int i = 0; i < list.Count; i += batchSize)
            yield return list.GetRange(i, Math.Min(batchSize, list.Count - i));
    }

    /// <summary>
    /// Updates singleton implementation instance with <paramref name="updateAction"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <param name="updateAction"></param>
    /// <returns></returns>
    public static IServiceCollection UpdateSingletonInstance<T>(this IServiceCollection services, Action<T> updateAction)
    {
        var servicesToReplace = services.Where(service => service.ServiceType == typeof(T)).ToList();

        foreach (var service in servicesToReplace)
        {
            if (service.ImplementationInstance != null && service.Lifetime == ServiceLifetime.Singleton)
                updateAction.Invoke((T)service.ImplementationInstance);
        }

        return services;
    }

    /// <summary>
    /// Updates singleton implementation instance with <paramref name="updateAction"/>.
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    /// <typeparam name="TInstance"></typeparam>
    /// <param name="services"></param>
    /// <param name="updateAction"></param>
    /// <returns></returns>
    public static IServiceCollection UpdateSingletonInstance<TImplementation, TInstance>(this IServiceCollection services, Action<TInstance> updateAction)
    {
        var servicesToReplace = services.Where(service => service.ServiceType == typeof(TImplementation)).ToList();

        foreach (var service in servicesToReplace)
        {
            if (service.ImplementationInstance != null && service.Lifetime == ServiceLifetime.Singleton)
                updateAction.Invoke((TInstance)service.ImplementationInstance);
        }

        return services;
    }
}
