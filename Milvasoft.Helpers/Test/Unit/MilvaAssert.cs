using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Test.Unit.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Milvasoft.Helpers.Test
{
    /// <summary>
    /// Includes custom control methods for the Assert class.
    /// </summary>
    public partial class MilvaAssert : Assert
    {
        /// <summary>
        /// If none of the members in the collection comply with the predicate the error is thrown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        public static void CheckAnyItems<T>(IEnumerable<T> collection, Predicate<T> predicate)
        {
            var counter = 0;
            foreach (var entity in collection)
            {
                var result = predicate.Invoke(entity);

                if (result)
                    counter++;
            }

            if (counter == 0)
                throw new MilvaTestException($"None of the members in the collection are compatible with predicate.");
        }

        /// <summary>
        /// If there are at least 1 item in the collection that meets the predicate it throws an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        public static void CheckNotAnyItems<T>(IEnumerable<T> collection, Predicate<T> predicate)
        {
            foreach (var entity in collection)
            {
                var result = predicate.Invoke(entity);

                if (result)
                    throw new MilvaTestException($"Not all items match to predicate. Entity Id : {GetExcpetionMessageDetail(entity)}");
            }
        }

        /// <summary>
        /// If none of the members in the collection comply with the predicate the error is thrown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="predicate"></param>
        public static void CheckAnyItems<T>(T entity, Predicate<T> predicate)
        {
            var result = predicate.Invoke(entity);

            if (!result)
                throw new MilvaTestException($"None of the entity members are compatible with the predicate. Entity Id : {GetExcpetionMessageDetail(entity)}");
        }

        /// <summary>
        /// 
        /// Performs a check for all members in the collection.
        /// 
        /// <para> Here you can send properties that need to be checked. See <paramref name="expressions"/> </para>
        /// 
        /// <para> Here you can send the rule to be made during the check. See <paramref name="customAssertEnum"/> </para>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="customAssertEnum"></param>
        /// <param name="expressions"></param>
        public static void CheckAllItems<T>(IEnumerable<T> collection, CheckPropertyEnum customAssertEnum, List<Expression<Func<T, object>>> expressions = null)
        {

            List<string> checkedProps = new();

            if (expressions != null)
                foreach (var expression in expressions)
                    checkedProps.Add(expression.GetPropertyName());
            else
                foreach (var item in typeof(T).GetProperties())
                    checkedProps.Add(item.Name);

            foreach (var item in collection)
                foreach (var propertyName in checkedProps)
                {
                    var predicate = GetPredicate<T>(propertyName, customAssertEnum);

                    var result = predicate.Compile().Invoke(item);

                    if (!result)
                        throw new MilvaTestException($"Not all items match to predicate. Mismatched property : {propertyName}, Predicate : {customAssertEnum}");
                }
        }

        /// <summary>
        /// 
        /// Performs a check for all members in the entity.
        /// 
        /// <para> Here you can send properties that need to be checked. See <paramref name="expressions"/> </para>
        /// 
        /// <para> Here you can send the rule to be made during the check. See <paramref name="customAssertEnum"/> </para>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="customAssertEnum"></param>
        /// <param name="expressions"></param>
        public static void CheckAllItems<T>(T entity, CheckPropertyEnum customAssertEnum, List<Expression<Func<T, object>>> expressions = null)
        {
            List<string> checkedProps = new();

            if (expressions != null)
                foreach (var expression in expressions)
                    checkedProps.Add(expression.GetPropertyName());
            else
                foreach (var item in typeof(T).GetProperties())
                    checkedProps.Add(item.Name);

            foreach (var propertyName in checkedProps)
            {
                var predicate = GetPredicate<T>(propertyName, customAssertEnum);

                var result = predicate.Compile().Invoke(entity);

                if (!result)
                    throw new MilvaTestException($"Not all items match to predicate. Mismatched property : {propertyName}, Predicate : {customAssertEnum}");
            }
        }

        /// <summary>
        /// 
        /// Checks whether all members of the collection comply with the predicates.
        /// 
        /// <para>If one member is incompatible, it throws an error.</para>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicates"></param>
        /// <param name="entities"></param>
        public static void CheckAllItems<T>(List<Predicate<T>> predicates, params T[] entities)
        {
            foreach (var entity in entities)
            {
                foreach (var predicate in predicates)
                {
                    var result = predicate.Invoke(entity);

                    if (!result)
                        throw new MilvaTestException($"None of the members in the collection are compatible with predicate. {GetExcpetionMessageDetail(predicates, predicate, entity)}");
                }
            }
        }

        /// <summary>
        /// Checks whether all members of the collection comply with the predicate.
        /// 
        /// <para>If one member is incompatible, it throws an error.</para>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        public static void CheckAllItems<T>(IEnumerable<T> collection, Predicate<T> predicate)
        {
            foreach (var entity in collection)
            {
                var result = predicate.Invoke(entity);

                if (!result)
                    throw new MilvaTestException($"Not all items match to predicate. Entity Id : {GetExcpetionMessageDetail(entity)}");
            }
        }

        /// <summary>
        /// This method used <see cref="Assert.ThrowsAsync(Type, Func{Task})"/>.
        /// </summary>
        /// <param name="func"></param>
        /// <param name="expectedExceptionMessage"></param>
        /// <param name="exceptionType"></param>
        /// <returns></returns>
        public static async Task CheckExceptionAsync(Func<Task> func, string expectedExceptionMessage, Type exceptionType)
        {
            var exception = await ThrowsAsync(exceptionType, func).ConfigureAwait(false);

            Equal(expectedExceptionMessage, exception.Message);
        }

        /// <summary>
        /// This method used <see cref="Assert.Throws{T}(Func{object})"/>.
        /// </summary>
        /// <param name="func"></param>
        /// <param name="expectedExceptionMessage"></param>
        /// <returns></returns>
        public static async Task CheckExceptionAsync<TException>(Func<Task> func, string expectedExceptionMessage) where TException : Exception
        {
            var exception = await ThrowsAsync<TException>(func).ConfigureAwait(false);

            Equal(expectedExceptionMessage, exception.Message);
        }

        /// <summary>
        /// Checks whether there is the same element in both lists.
        /// 
        /// <para> If there is at least one same element, it throws an exception. </para>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="firstCollection"></param>
        /// <param name="secondCollection"></param>
        public static void CheckNotEqualAnyItem<T>(IEnumerable<T> firstCollection, IEnumerable<T> secondCollection)
        {
            foreach (var i in firstCollection)
                foreach (var j in secondCollection)
                    if (i.Equals(j))
                        throw new MilvaTestException("One of the members on the two lists is the same.");
        }

        /// <summary>
        /// Checks whether the incoming value is zero.
        /// 
        /// <para> If it's not zero, it throws an exception. </para>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static void Zero(int value)
        {
            if (value != 0)
                throw new MilvaTestException("Value is not zero.");
        }

        /// <summary>
        /// Checks whether the incoming value is zero.
        /// 
        /// <para> If it's not zero, it throws an exception. </para>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static void Zero(decimal value)
        {
            if (value != 0)
                throw new MilvaTestException("Value is not zero.");
        }

        #region Helper Methods

        /// <summary>
        /// creates a predicate according to <paramref name="customAssertEnum"/> and <paramref name="propName"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propName"></param>
        /// <param name="customAssertEnum"></param>
        /// <returns></returns>
        private static Expression<Func<T, bool>> GetPredicate<T>(string propName, CheckPropertyEnum customAssertEnum)
        {
            ParameterExpression argParam = Expression.Parameter(typeof(T), "p");
            Expression nameProperty = Expression.Property(argParam, propName);

            var defaultExpression = Expression.Default(typeof(T).GetProperty(propName).PropertyType);

            BinaryExpression notEqualOrEqual;
            if (customAssertEnum == CheckPropertyEnum.NotDefault)
                notEqualOrEqual = Expression.NotEqual(nameProperty, defaultExpression);
            else
                notEqualOrEqual = Expression.Equal(nameProperty, defaultExpression);

            return Expression.Lambda<Func<T, bool>>(notEqualOrEqual, argParam);
        }

        /// <summary>
        /// Returns exception message for unit tests.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicates"></param>
        /// <param name="predicate"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static string GetExcpetionMessageDetail<T>(List<Predicate<T>> predicates, Predicate<T> predicate, T entity)
            => $"Predicate Index : {predicates.FindIndex(p => p == predicate)}, Entity Id : {entity.GetType().GetProperty("Id").GetValue(entity, null)}";

        /// <summary>
        /// Returns exception message for unit tests.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static string GetExcpetionMessageDetail<T>(T entity)
            => $"Entity Id : {entity.GetType().GetProperty("Id").GetValue(entity, null)}";

        #endregion
    }
}
