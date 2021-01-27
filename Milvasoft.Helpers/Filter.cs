using Milvasoft.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Milvasoft.Helpers
{
    /// <summary>
    /// Helper for filtering.
    /// </summary>
    public static class Filter
    {
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
                contentList = contentList.Where(i => (DateTime)i.GetType().GetProperty(propertyName).GetValue(i, null) >= (DateTime)dateLowerValue.Value && (DateTime)i.GetType().GetProperty(propertyName).GetValue(i, null) <= (DateTime)dateTopValue.Value);


            // If only the DateTopValue value exists, it returns those larger than the DateLowerValue value.
            else if (dateLowerValue.HasValue && !dateTopValue.HasValue)
            {
                contentList = contentList.Where(i => (DateTime)i.GetType().GetProperty(propertyName).GetValue(i, null) >= (DateTime)dateLowerValue.Value);
            }

            //If only the DateLowerValue value exists, it returns those smaller than the DateTopValue value.
            else if (!dateLowerValue.HasValue && dateTopValue.HasValue)
                contentList = contentList.Where(i => (DateTime)i.GetType().GetProperty(propertyName).GetValue(i, null) <= (DateTime)dateTopValue.Value);
            return contentList;
        }

        /// <summary>
        /// <para> Creates expression by <paramref name="dateTopValue"/> and <paramref name="dateLowerValue"/> values. </para>
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
        /// <param name="dateTopValue"></param>
        /// <param name="dateLowerValue"></param>
        /// <param name="dateProperty"></param>
        /// <returns> 
        /// 
        /// Example Return when both values has value :
        /// 
        /// <code> i => i.<paramref name="dateProperty"/> <b>biggerThanOrEqual</b> <paramref name="dateLowerValue"/> <b>AND</b> i.<paramref name="dateProperty"/> <b>lessThanOrEqual</b> <paramref name="dateTopValue"/> </code>
        /// 
        /// </returns>
        public static Expression<Func<T, bool>> CreateDateFilterExpression<T>(DateTime? dateTopValue, DateTime? dateLowerValue, Expression<Func<T, DateTime?>> dateProperty)
        {
            Expression<Func<T, bool>> mainExpression = null;

            var entityType = typeof(T);
            var propertyName = dateProperty.GetPropertyName();

            var parameterExpression = Expression.Parameter(entityType, "i");
            Expression orderByProperty = Expression.Property(parameterExpression, propertyName);

            var prop = Expression.Lambda<Func<T, DateTime>>(Expression.Convert(Expression.Property(parameterExpression, propertyName), typeof(DateTime)), parameterExpression);

            Expression<Func<DateTime>> dateLowerExpression = () => dateLowerValue.Value;
            var greaterThanOrEqualBinaryExpression = Expression.GreaterThanOrEqual(prop.Body, dateLowerExpression.Body);
            var greaterThanOrEqualExpression = Expression.Lambda<Func<T, bool>>(Expression.Convert(greaterThanOrEqualBinaryExpression, typeof(bool)), parameterExpression);

            Expression<Func<DateTime>> dateTopExpression = () => dateTopValue.Value;
            var lessThanOrEqualBinaryExpression = Expression.LessThanOrEqual(prop.Body, dateTopExpression.Body);

            var lessThanOrEqualExpression = Expression.Lambda<Func<T, bool>>(Expression.Convert(lessThanOrEqualBinaryExpression, typeof(bool)), parameterExpression);

            var predicate = greaterThanOrEqualExpression.Append(lessThanOrEqualExpression, ExpressionType.AndAlso);

            //If a selection has been made between two dates, it will return those between the two dates.
            if (dateLowerValue.HasValue && dateTopValue.HasValue)
                mainExpression = mainExpression.Append(predicate, ExpressionType.AndAlso);


            // If only the DateTopValue value exists, it returns those larger than the DateLowerValue value.
            else if (dateLowerValue.HasValue && !dateTopValue.HasValue)
            {
                mainExpression = mainExpression.Append(greaterThanOrEqualExpression, ExpressionType.AndAlso);
            }

            //If only the DateLowerValue value exists, it returns those smaller than the DateTopValue value.
            else if (!dateLowerValue.HasValue && dateTopValue.HasValue)
                mainExpression = mainExpression.Append(lessThanOrEqualExpression, ExpressionType.AndAlso);
            return mainExpression;
        }

        /// <summary>
        /// <para> Creates expression by <paramref name="dateTopValue"/> and <paramref name="dateLowerValue"/> values. </para>
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
        /// <param name="dateTopValue"></param>
        /// <param name="dateLowerValue"></param>
        /// <param name="dateProperty"></param>
        /// <returns> 
        /// 
        /// Example Return when both values has value :
        /// 
        /// <code> i => i.<paramref name="dateProperty"/> <b>biggerThanOrEqual</b> <paramref name="dateLowerValue"/> <b>AND</b> i.<paramref name="dateProperty"/> <b>lessThanOrEqual</b> <paramref name="dateTopValue"/> </code>
        /// 
        /// </returns>
        public static Expression<Func<T, bool>> CreateDateFilterExpression<T>(DateTime? dateTopValue, DateTime? dateLowerValue, Expression<Func<T, DateTime>> dateProperty)
        {
            Expression<Func<T, bool>> mainExpression = null;


            var entityType = typeof(T);
            var propertyName = dateProperty.GetPropertyName();

            var parameterExpression = Expression.Parameter(entityType, "i");
            Expression orderByProperty = Expression.Property(parameterExpression, propertyName);

            var prop = Expression.Lambda<Func<T, DateTime>>(Expression.Convert(Expression.Property(parameterExpression, propertyName), typeof(DateTime)), parameterExpression);

            Expression<Func<DateTime>> dateLowerExpression = () => dateLowerValue.Value;
            var greaterThanOrEqualBinaryExpression = Expression.GreaterThanOrEqual(prop.Body, dateLowerExpression.Body);
            var greaterThanOrEqualExpression = Expression.Lambda<Func<T, bool>>(Expression.Convert(greaterThanOrEqualBinaryExpression, typeof(bool)), parameterExpression);

            Expression<Func<DateTime>> dateTopExpression = () => dateTopValue.Value;
            var lessThanOrEqualBinaryExpression = Expression.LessThanOrEqual(prop.Body, dateTopExpression.Body);

            var lessThanOrEqualExpression = Expression.Lambda<Func<T, bool>>(Expression.Convert(lessThanOrEqualBinaryExpression, typeof(bool)), parameterExpression);

            var predicate = greaterThanOrEqualExpression.Append(lessThanOrEqualExpression, ExpressionType.AndAlso);

            //If a selection has been made between two dates, it will return those between the two dates.
            if (dateLowerValue.HasValue && dateTopValue.HasValue)
                mainExpression = mainExpression.Append(predicate, ExpressionType.AndAlso);


            // If only the DateTopValue value exists, it returns those larger than the DateLowerValue value.
            else if (dateLowerValue.HasValue && !dateTopValue.HasValue)
            {
                mainExpression = mainExpression.Append(greaterThanOrEqualExpression, ExpressionType.AndAlso);
            }

            //If only the DateLowerValue value exists, it returns those smaller than the DateTopValue value.
            else if (!dateLowerValue.HasValue && dateTopValue.HasValue)
                mainExpression = mainExpression.Append(lessThanOrEqualExpression, ExpressionType.AndAlso);
            return mainExpression;
        }

        /// <summary>
        /// <para>Gets maximum list values of matching properties of <paramref name="maxValuesObject"/> and <typeparamref name = "T" />.</para>
        /// <para>Returns that max values in <paramref name="maxValuesObject"/>.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TReturnObject"></typeparam>
        /// <param name="entities"></param>
        /// <param name="maxValuesObject"></param>
        /// <param name="requestedProperties"></param>
        /// <returns> Mapped <paramref name="maxValuesObject"/> </returns>
        public static TReturnObject GetSpecMaxValues<T, TReturnObject>(this IEnumerable<T> entities, TReturnObject maxValuesObject, params Expression<Func<T, decimal>>[] requestedProperties)
        {
            var propertyListOfMaxValuesObject = maxValuesObject.GetType().GetProperties().ToList();
            foreach (var expression in requestedProperties)
            {
                var memberName = expression.GetPropertyInfo().Name;
                if (propertyListOfMaxValuesObject.Select(i => i.Name).Contains(memberName))
                {
                    var propertyOfMaxValuesObject = maxValuesObject.GetType().GetProperty(memberName);
                    var maxValueOfProperty = entities.Max(i => i.GetType().GetProperty(memberName).GetValue(i));
                    propertyOfMaxValuesObject.SetValue(maxValuesObject, maxValueOfProperty);
                }
            }
            return maxValuesObject;
        }


    }
}
