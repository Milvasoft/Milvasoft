using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Milvasoft.Helpers.DataAccess.IncludeLibrary
{
    /// <summary>
    /// Includable extension for Ops!yon API. For reason to use that class: We cannot use default include process which in EntityFramework. Because of mysql and context. 
    /// </summary>
    public static class IncludableExtensions
    {
        /// <summary>
        /// Allows lazy loading.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="includes"></param>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public static IIncludable<TEntity, TProperty> Include<TEntity, TProperty>(this IIncludable<TEntity> includes,
                                                                                  Expression<Func<TEntity, TProperty>> propertySelector) where TEntity : class
        {
            var result = ((Includable<TEntity>)includes).Input.Include(propertySelector);

            return new Includable<TEntity, TProperty>(result);
        }

        /// <summary>
        /// Allows lazy loading.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TOtherProperty"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="includes"></param>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public static IIncludable<TEntity, TOtherProperty> ThenInclude<TEntity, TOtherProperty, TProperty>(this IIncludable<TEntity, TProperty> includes,
                                                                                                           Expression<Func<TProperty, TOtherProperty>> propertySelector) where TEntity : class
        {
            var result = ((Includable<TEntity, TProperty>)includes).IncludableInput.ThenInclude(propertySelector);

            return new Includable<TEntity, TOtherProperty>(result);
        }

        /// <summary>
        /// Allows lazy loading.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TOtherProperty"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="includes"></param>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public static IIncludable<TEntity, TOtherProperty> ThenInclude<TEntity, TOtherProperty, TProperty>(this IIncludable<TEntity, IEnumerable<TProperty>> includes,
                                                                                                          Expression<Func<TProperty, TOtherProperty>> propertySelector) where TEntity : class
        {
            var result = ((Includable<TEntity, IEnumerable<TProperty>>)includes).IncludableInput.ThenInclude(propertySelector);

            return new Includable<TEntity, TOtherProperty>(result);
        }

        /// <summary>
        /// Allows lazy loading.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TOtherProperty"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="includes"></param>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public static IIncludable<TEntity, TOtherProperty> ThenInclude<TEntity, TOtherProperty, TProperty>(this IIncludable<TEntity, ICollection<TProperty>> includes,
                                                                                                           Expression<Func<TProperty, TOtherProperty>> propertySelector) where TEntity : class
        {
            var result = ((Includable<TEntity, ICollection<TProperty>>)includes).IncludableInput.ThenInclude(propertySelector);

            return new Includable<TEntity, TOtherProperty>(result);
        }


    }
}
