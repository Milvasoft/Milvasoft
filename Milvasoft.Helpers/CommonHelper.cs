using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Extensions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Helpers
{
    /// <summary>
    /// Common Helper class.
    /// </summary>
    public static class CommonHelper
    {
        /// <summary>
        /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b>typeof(<typeparamref name="T"/>)</b>. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool PropertyExists<T>(string propertyName) => typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) != null;

        /// <summary>
        /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b><paramref name="content"/></b>. 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool PropertyExists(object content, string propertyName) => content.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) != null;

        /// <summary>
        /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b><paramref name="type"/></b>. 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool PropertyExists(Type type, string propertyName) => type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) != null;

        /// <summary>
        /// Creates order by key selector by <paramref name="orderByPropertyName"/>.
        /// </summary>
        /// 
        /// <exception cref="ArgumentException"> Throwns when type of <typeparamref name="T"/>'s properties doesn't contain '<paramref name="orderByPropertyName"/>'. </exception>
        /// 
        /// <typeparam name="T"></typeparam>
        /// <param name="orderByPropertyName"></param>
        /// <returns></returns>
        public static Expression<Func<T, object>> CreateOrderByKeySelector<T>(string orderByPropertyName)
        {
            var entityType = typeof(T);

            if (!PropertyExists<T>(orderByPropertyName))
                throw new ArgumentException($"Type of {entityType.Name}'s properties doesn't contain '{orderByPropertyName}'.");

            var parameterExpression = Expression.Parameter(entityType, "i");
            Expression orderByProperty = Expression.Property(parameterExpression, orderByPropertyName);

            return Expression.Lambda<Func<T, object>>(Expression.Convert(orderByProperty, typeof(object)), parameterExpression);
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            foreach (var prop in propertyName.Split('.').Select(propName => obj.GetType().GetProperty(propName)))
                obj = prop.GetValue(obj, null);
            return obj;
        }

        /// <summary>
        /// Checks that MinimumLength and MaximumLength have legal values.  Throws InvalidOperationException if not.
        /// </summary>
        public static void EnsureLegalLengths(int maxLength, int minLength , IStringLocalizer stringLocalizer = null)
        {
            if (maxLength < 0) throw new MilvasoftValidationException(stringLocalizer != null 
                                                                      ? stringLocalizer["PreventStringInjectionMaxLengthException"] 
                                                                      : "Please enter a valid value for the maximum character length.");
            if (minLength < 0) throw new MilvasoftValidationException(stringLocalizer != null 
                                                                      ? stringLocalizer["PreventStringInjectionMinLengthException"] 
                                                                      : "Please enter a valid value for the minimum character length.");
            if (maxLength <= minLength) throw new MilvasoftValidationException(stringLocalizer != null
                                                                               ? stringLocalizer["PreventStringInjectionMinLengthBigThanMaxLengthException", minLength, maxLength]
                                                                               : $"The minimum value ({minLength}) you entered is greater than the maximum value ({maxLength}). Please enter a valid range of values.");
        }

        /// <summary>
        /// Gets enum description.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetDesciption<T>(this T enumValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                return null;

            var desription = enumValue.ToString();
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);

                if (!attrs.IsNullOrEmpty())
                {
                    desription = ((DescriptionAttribute)attrs.First()).Description;
                }
            }

            return desription;
        }

    }
}
