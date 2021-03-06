﻿using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Helpers.Extensions
{
    /// <summary>
    /// Generic collection helper extension methods.
    /// </summary>
    public static class GenericCollection
    {
        private static readonly MethodInfo OrderByMethod = typeof(Queryable).GetMethods().Single(method => method.Name == "OrderBy" && method.GetParameters().Length == 2);

        private static readonly MethodInfo OrderByDescendingMethod = typeof(Queryable).GetMethods().Single(method => method.Name == "OrderByDescending" && method.GetParameters().Length == 2);

        /// <summary>
        /// Checks whether or not collection is null or empty. Assumes collection can be safely enumerated multiple times.
        /// </summary>
        public static bool IsNullOrEmpty(this IEnumerable @this) => @this == null || @this.GetEnumerator().MoveNext() == false;

        /// <summary>
        /// Checks whether property exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool PropertyExists<T>(this IQueryable<T> source, string propertyName) => typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase
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

            var genericMethod = OrderByMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);

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

            var genericMethod = OrderByDescendingMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);

            var ret = genericMethod.Invoke(null, new object[] { source, lambda });

            return (IQueryable<T>)ret;
        }

    }
}
