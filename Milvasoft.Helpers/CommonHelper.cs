﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public static bool PropertyExists<T>(string propertyName) => typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase
                                                                                                       | BindingFlags.Public
                                                                                                       | BindingFlags.Instance) != null;

        /// <summary>
        /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b><paramref name="content"/></b>. 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool PropertyExists(object content, string propertyName) => content.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase
                                                                                                                              | BindingFlags.Public
                                                                                                                              | BindingFlags.Instance) != null;

        /// <summary>
        /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b><paramref name="type"/></b>. 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool PropertyExists(Type type, string propertyName) => type.GetProperty(propertyName, BindingFlags.IgnoreCase
                                                                                                            | BindingFlags.Public
                                                                                                            | BindingFlags.Instance) != null;

        /// <summary>
        /// Creates order by key selector by <paramref name="orderByPropertyName"/>.
        /// </summary>
        /// 
        /// <exception cref="MilvaDeveloperException"> Throwns when type of <typeparamref name="T"/>'s properties doesn't contain '<paramref name="orderByPropertyName"/>'. </exception>
        /// 
        /// <typeparam name="T"></typeparam>
        /// <param name="orderByPropertyName"></param>
        /// <returns></returns>
        public static Expression<Func<T, object>> CreateOrderByKeySelector<T>(string orderByPropertyName)
        {
            var entityType = typeof(T);

            if (!PropertyExists<T>(orderByPropertyName))
                throw new MilvaDeveloperException($"Type of {entityType.Name}'s properties doesn't contain '{orderByPropertyName}'.");

            var parameterExpression = Expression.Parameter(entityType, "i");

            Expression orderByProperty = Expression.Property(parameterExpression, orderByPropertyName);

            return Expression.Lambda<Func<T, object>>(Expression.Convert(orderByProperty, typeof(object)), parameterExpression);
        }

        /// <summary>
        /// Create property selector predicate.(e.g. i => i.User).
        /// If <typeparamref name="T"/> doesn't contains <paramref name="propertyName"/> throwns <see cref="MilvaDeveloperException"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static Expression<Func<T, TPropertyType>> CreatePropertySelector<T, TPropertyType>(string propertyName)
        {
            var entityType = typeof(T);

            if (!PropertyExists<T>(propertyName))
                throw new MilvaDeveloperException($"Type of {entityType.Name}'s properties doesn't contain '{propertyName}'.");

            var parameter = Expression.Parameter(entityType);

            return Expression.Lambda<Func<T, TPropertyType>>(Expression.Property(parameter, propertyName), parameter);
        }

        /// <summary>
        /// Create property selector predicate.(e.g. i => i.User).
        /// If <typeparamref name="T"/> doesn't contains <paramref name="propertyName"/> returns null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static Expression<Func<T, TPropertyType>> CreateRequiredPropertySelector<T, TPropertyType>(string propertyName)
        {
            var entityType = typeof(T);

            if (!PropertyExists<T>(propertyName))
                return null;

            var parameter = Expression.Parameter(entityType);

            return Expression.Lambda<Func<T, TPropertyType>>(Expression.Property(parameter, propertyName), parameter);
        }

        /// <summary>
        /// Provides get nested property value.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetPropertyValue(object obj, string propertyName)
        {
            foreach (var prop in propertyName.Split('.').Select(propName => obj.GetType().GetProperty(propName)))
                obj = prop.GetValue(obj, null);
            return obj;
        }

        /// <summary>
        /// Checks that MinimumLength and MaximumLength have legal values. Throws <see cref="InvalidOperationException"/>  if not.
        /// </summary>
        public static void EnsureLegalLengths(int maxLength, int minLength, IStringLocalizer stringLocalizer = null)
        {
            if (maxLength < 0) throw new MilvaValidationException(stringLocalizer != null
                                                                      ? stringLocalizer[LocalizerKeys.PreventStringInjectionMaxLengthException]
                                                                      : "Please enter a valid value for the maximum character length.");

            if (minLength < 0) throw new MilvaValidationException(stringLocalizer != null
                                                                      ? stringLocalizer[LocalizerKeys.PreventStringInjectionMinLengthException]
                                                                      : "Please enter a valid value for the minimum character length.");

            if (maxLength <= minLength) throw new MilvaValidationException(stringLocalizer != null
                                                                               ? stringLocalizer[LocalizerKeys.PreventStringInjectionMinLengthBigThanMaxLengthException, minLength, maxLength]
                                                                               : $"The minimum value ({minLength}) you entered is greater than the maximum value ({maxLength}). Please enter a valid range of values.");
        }

        /// <summary>
        /// Gets enum description.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetEnumDesciption<T>(this T enumValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                return null;

            var desription = enumValue.ToString();

            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);

                if (!attrs.IsNullOrEmpty())
                    desription = ((DescriptionAttribute)attrs.First()).Description;
            }

            return desription;
        }

        /// <summary>
        /// This method return int value to guid value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Guid ToGuid(this int value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        /// <summary>
        /// Creates localizer instance if IStringLocalizerFactory registered to service collection.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IStringLocalizer GetRequiredLocalizerInstance<TSource>(this IServiceProvider serviceProvider)
        {
            var localizerFactory = serviceProvider.GetRequiredService<IStringLocalizerFactory>();

            var resourceType = typeof(TSource);

            var assemblyName = new AssemblyName(resourceType.GetTypeInfo().Assembly.FullName);

            return localizerFactory.Create(resourceType.Name, assemblyName.Name);
        }

        /// <summary>
        /// Creates localizer instance if IStringLocalizerFactory registered to service collection.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IStringLocalizer GetLocalizerInstance<TSource>(this IServiceProvider serviceProvider)
        {
            var localizerFactory = serviceProvider.GetService<IStringLocalizerFactory>();

            var resourceType = typeof(TSource);

            var assemblyName = new AssemblyName(resourceType.GetTypeInfo().Assembly.FullName);

            return localizerFactory.Create(resourceType.Name, assemblyName.Name);
        }

        /// <summary>
        /// Creates localizer instance if IStringLocalizerFactory registered to service collection.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IStringLocalizer GetRequiredLocalizerInstance<TSource>(this HttpContext context)
        {
            var localizerFactory = context.RequestServices.GetRequiredService<IStringLocalizerFactory>();

            var resourceType = typeof(TSource);

            var assemblyName = new AssemblyName(resourceType.GetTypeInfo().Assembly.FullName);

            return localizerFactory.Create(resourceType.Name, assemblyName.Name);
        }

        /// <summary>
        /// Creates localizer instance if IStringLocalizerFactory registered to service collection.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IStringLocalizer GetLocalizerInstance<TSource>(this HttpContext context)
        {
            var localizerFactory = context.RequestServices.GetService<IStringLocalizerFactory>();

            var resourceType = typeof(TSource);

            var assemblyName = new AssemblyName(resourceType.GetTypeInfo().Assembly.FullName);

            return localizerFactory.Create(resourceType.Name, assemblyName.Name);
        }

        /// <summary>
        /// Creates localizer instance if IStringLocalizerFactory registered to service collection.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        public static IStringLocalizer GetRequiredLocalizerInstance(this IServiceProvider serviceProvider, Type resourceType)
        {
            var localizerFactory = serviceProvider.GetRequiredService<IStringLocalizerFactory>();

            var assemblyName = new AssemblyName(resourceType.GetTypeInfo().Assembly.FullName);

            return localizerFactory.Create(resourceType.Name, assemblyName.Name);
        }

        /// <summary>
        /// Creates localizer instance if IStringLocalizerFactory registered to service collection.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        public static IStringLocalizer GetLocalizerInstance(this IServiceProvider serviceProvider, Type resourceType)
        {
            var localizerFactory = serviceProvider.GetService<IStringLocalizerFactory>();

            var assemblyName = new AssemblyName(resourceType.GetTypeInfo().Assembly.FullName);

            return localizerFactory.Create(resourceType.Name, assemblyName.Name);
        }

        /// <summary>
        /// Converts <paramref name="value"/> to <see cref="string"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToJson(this object value) => JsonConvert.SerializeObject(value);

        /// <summary>
        /// Converts <paramref name="value"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ToObject<T>(this string value) where T : class
            => string.IsNullOrEmpty(value) ? null : JsonConvert.DeserializeObject<T>(value);

        /// <summary>
        /// Prepares custom validation model for response.
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        public static ObjectResult CustomErrorResponse(ActionContext actionContext)
        {
            var errorMessageList = actionContext.ModelState.Where(modelError => modelError.Value.Errors.Count > 0)
                                                           .SelectMany(modelError => modelError.Value.Errors.Select(i => i.ErrorMessage)).ToList();

            var validationResponse = new ExceptionResponse
            {
                Success = false,
                Message = string.Join('~', errorMessageList),
                StatusCode = MilvaStatusCodes.Status600Exception,
                Result = new object(),
                ErrorCodes = new List<int>()
            };

            actionContext.HttpContext.Items.Add(new KeyValuePair<object, object>("StatusCode", MilvaStatusCodes.Status600Exception));

            return new OkObjectResult(validationResponse);
        }
    }
}
