﻿using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.EntityBase.Abstract;
using Milvasoft.Core.EntityBase.Abstract.Auditing;
using Milvasoft.Core.EntityBase.Concrete;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Extensions;
using Milvasoft.Core.Utils.Constants;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Milvasoft.Core;

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
    public static PropertyInfo ThrowIfPropertyNotExists<T>(string propertyName)
        => ThrowIfPropertyNotExists(typeof(T), propertyName);

    /// <summary>
    /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b><paramref name="content"/></b>. 
    /// </summary>
    /// <param name="content"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static PropertyInfo ThrowIfPropertyNotExists(this object content, string propertyName)
        => ThrowIfPropertyNotExists(content.GetType(), propertyName);

    /// <summary>
    /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b><paramref name="type"/></b>. 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static PropertyInfo ThrowIfPropertyNotExists(this Type type, string propertyName)
        => GetPublicPropertyIgnoreCase(type, propertyName) ?? throw new MilvaDeveloperException($"Type of {type.Name}'s properties doesn't contain '{propertyName}'.");

    /// <summary>
    /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b>typeof(<typeparamref name="T"/>)</b>. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static bool PropertyExists<T>(string propertyName) => PropertyExists(typeof(T), propertyName);

    /// <summary>
    /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b><paramref name="content"/></b>. 
    /// </summary>
    /// <param name="content"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static bool PropertyExists(this object content, string propertyName) => PropertyExists(content.GetType(), propertyName);

    /// <summary>
    /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b><paramref name="type"/></b>. 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static bool PropertyExists(this Type type, string propertyName) => GetPublicPropertyIgnoreCase(type, propertyName) != null;

    /// <summary>
    /// Gets property with <see cref="BindingFlags.Public"/>, <see cref="BindingFlags.IgnoreCase"/>, <see cref="BindingFlags.Instance"/>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static PropertyInfo GetPublicPropertyIgnoreCase(this Type type, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new MilvaDeveloperException("Please send property name correctly");

        return type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
    }

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

        entityType.ThrowIfPropertyNotExists(orderByPropertyName);

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
    public static Expression<Func<T, TPropertyType>> CreatePropertySelector<T, TPropertyType>(string propertyName)
    {
        var entityType = typeof(T);

        if (!PropertyExists<T>(propertyName))
            return null;

        var parameter = Expression.Parameter(entityType);

        return Expression.Lambda<Func<T, TPropertyType>>(Expression.Convert(Expression.Property(parameter, propertyName), typeof(TPropertyType)), parameter);
    }

    /// <summary>
    /// Create property selector predicate.(e.g. i => i.User).
    /// If <typeparamref name="T"/> doesn't contains <paramref name="propertyName"/> throwns <see cref="MilvaDeveloperException"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TPropertyType"></typeparam>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static Func<T, TPropertyType> CreatePropertySelectorFunction<T, TPropertyType>(string propertyName)
    {
        var entityType = typeof(T);

        if (!PropertyExists<T>(propertyName))
            return null;

        var parameter = Expression.Parameter(entityType);

        return Expression.Lambda<Func<T, TPropertyType>>(Expression.Convert(Expression.Property(parameter, propertyName), typeof(TPropertyType)), parameter).Compile();
    }

    /// <summary>
    /// Create property selector predicate.(e.g. i => i.User).
    /// If <typeparamref name="T"/> doesn't contains <paramref name="propertyName"/> returns null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TPropertyType"></typeparam>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static Expression<Func<T, TPropertyType>> CreateRequiredPropertySelector<T, TPropertyType>(string propertyName)
    {
        var entityType = typeof(T);

        entityType.ThrowIfPropertyNotExists(propertyName);

        var parameter = Expression.Parameter(entityType);

        return Expression.Lambda<Func<T, TPropertyType>>(Expression.Convert(Expression.Property(parameter, propertyName), typeof(TPropertyType)), parameter);
    }

    /// <summary>
    /// Create property selector predicate.(e.g. i => i.User).
    /// If <typeparamref name="T"/> doesn't contains <paramref name="propertyName"/> returns null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TPropertyType"></typeparam>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static Func<T, TPropertyType> CreateRequiredPropertySelectorFuction<T, TPropertyType>(string propertyName)
    {
        var entityType = typeof(T);

        entityType.ThrowIfPropertyNotExists(propertyName);

        var parameter = Expression.Parameter(entityType);

        return Expression.Lambda<Func<T, TPropertyType>>(Expression.Convert(Expression.Property(parameter, propertyName), typeof(TPropertyType)), parameter).Compile();
    }

    /// <summary>
    /// Gets <b>entity => entity.IsDeleted == false</b> expression, if <typeparamref name="TEntity"/> is assignable from <see cref="IFullAuditable{TKey}"/>.
    /// </summary>
    /// <returns></returns>
    public static Expression<Func<TEntity, bool>> CreateIsDeletedFalseExpression<TEntity>()
    {
        var entityType = typeof(TEntity);

        if (typeof(ISoftDeletable).IsAssignableFrom(entityType))
        {
            var parameter = Expression.Parameter(entityType, "entity");

            var filterExpression = Expression.Equal(Expression.Property(parameter, entityType.GetProperty(EntityPropertyNames.IsDeleted)), Expression.Constant(false, typeof(bool)));

            return Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
        }
        else return null;
    }

    /// <summary>
    /// Provides get nested property value. e.g. Product.Stock.Amount
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
    public static void EnsureLegalLengths(int maxLength, int minLength, IMilvaLocalizer milvaLocalizer = null)
    {
        if (maxLength < 0) throw new MilvaValidationException(milvaLocalizer != null
                                                                  ? milvaLocalizer[LocalizerKeys.PreventStringInjectionMaxLengthException]
                                                                  : "Please enter a valid value for the maximum character length.");

        if (minLength < 0) throw new MilvaValidationException(milvaLocalizer != null
                                                                  ? milvaLocalizer[LocalizerKeys.PreventStringInjectionMinLengthException]
                                                                  : "Please enter a valid value for the minimum character length.");

        if (maxLength < minLength) throw new MilvaValidationException(milvaLocalizer != null
                                                                           ? milvaLocalizer[LocalizerKeys.PreventStringInjectionMinLengthBigThanMaxLengthException, minLength, maxLength]
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
    /// Converts <paramref name="value"/> to <see cref="string"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToJson(this object value) => JsonSerializer.Serialize(value);

    /// <summary>
    /// Converts <paramref name="value"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T ToObject<T>(this string value) where T : class
        => string.IsNullOrWhiteSpace(value) ? null : JsonSerializer.Deserialize<T>(value);

    #region DateTime

    /// <summary>
    /// Compares <paramref name="date"/> for whether between <paramref name="startTime"/> and <paramref name="endTime"/>. 
    /// </summary>
    /// 
    /// <remarks>
    /// This is a time comparison not a date comparison.
    /// </remarks>
    /// 
    /// <param name="date"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="convertTimesToUtc"></param>
    /// <returns></returns>
    public static bool IsBetween(this DateTime date, TimeSpan startTime, TimeSpan endTime, bool convertTimesToUtc = true)
    {
        if (convertTimesToUtc)
        {
            startTime = startTime.ConvertToUtc();
            endTime = endTime.ConvertToUtc();
        }

        DateTime startDate = new(date.Year, date.Month, date.Day);
        DateTime endDate = startDate;

        //Check whether the endTime is lesser than startTime
        if (startTime >= endTime)
        {
            //Increase the date if endTime is timespan of the Nextday 
            endDate = endDate.AddDays(1);
        }

        //Assign the startTime and endTime to the Dates
        startDate = startDate.Date + startTime;
        endDate = endDate.Date + endTime;

        return date >= startDate && date <= endDate;
    }

    /// <summary>
    /// Removes <paramref name="date"/>'s hour, second and milisecond then adds <paramref name="compareTime"/> to <paramref name="date"/> 
    /// and compares <paramref name="date"/> for whether between <paramref name="startTime"/> and <paramref name="endTime"/>. 
    /// </summary>
    /// 
    /// <remarks>
    /// This is a time comparison not a date comparison.
    /// </remarks>
    /// 
    /// <param name="date"></param>
    /// <param name="compareTime"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="convertTimesToUtc"></param>
    /// <returns></returns>
    public static bool IsBetween(this DateTime date, TimeSpan compareTime, TimeSpan startTime, TimeSpan endTime, bool convertTimesToUtc = true)
    {
        if (convertTimesToUtc)
        {
            compareTime = compareTime.ConvertToUtc();
            startTime = startTime.ConvertToUtc();
            endTime = endTime.ConvertToUtc();
        }

        date = new DateTime(date.Year, date.Month, date.Day).Date + compareTime;

        return date.IsBetween(startTime, endTime);
    }

    /// <summary>
    /// Compares <paramref name="date"/> for whether between <paramref name="startDate"/> and <paramref name="endDate"/>. 
    /// </summary>
    /// <param name="date"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    public static bool IsBetween(this DateTime date, DateTime startDate, DateTime endDate) => date >= startDate && date <= endDate;

    /// <summary>
    /// Compares <paramref name="date"/> for whether between <paramref name="startDate"/> and <paramref name="endDate"/>. 
    /// </summary>
    /// <param name="date"></param>
    /// <param name="time"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="convertTimeToUtc"></param>
    /// <returns></returns>
    public static bool IsBetween(this DateTime date, TimeSpan time, DateTime startDate, DateTime endDate, bool convertTimeToUtc = true)
    {
        if (convertTimeToUtc)
            time = time.ConvertToUtc();

        date = new DateTime(date.Year, date.Month, date.Day).Date + time;

        return date >= startDate && date <= endDate;
    }

    /// <summary>
    /// Converts timespan to universal time.
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static TimeSpan ConvertToUtc(this TimeSpan timeSpan) => TimeSpan.FromTicks(new DateTime(timeSpan.Ticks).ToUniversalTime().Ticks);

    #endregion

    #region Localizer

    /// <summary>
    /// Returns <see cref="IMilvaLocalizer"/> which registered to <see cref="IServiceCollection"/>.
    /// If <see cref="IMilvaLocalizer"/> not registered, returns null.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IMilvaLocalizer GetMilvaLocalizer(this IServiceProvider serviceProvider)
    {
        var milvaLocalizer = serviceProvider.GetService<IMilvaLocalizer>();

        return milvaLocalizer;
    }

    /// <summary>
    /// Returns <see cref="IMilvaLocalizer"/> which registered to <see cref="IServiceCollection"/>.
    /// If <see cref="IMilvaLocalizer"/> not registered, throws <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IMilvaLocalizer GetRequiredMilvaLocalizer(this IServiceProvider serviceProvider)
    {
        var milvaLocalizer = serviceProvider.GetRequiredService<IMilvaLocalizer>();

        return milvaLocalizer;
    }

    #endregion

    /// <summary>
    /// Normalize string according to invariant culture.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string MilvaNormalize(this string value) => !string.IsNullOrWhiteSpace(value) ? value.ToLower().ToUpperInvariant() : null;
}
