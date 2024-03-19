using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Core.Extensions;

public static partial class CommonHelper
{
    /// <summary>
    /// Checks whether property exists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"> Source collection for code readiness. </param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static bool PropertyExists<T>(this IEnumerable<T> _, string propertyName) => !string.IsNullOrWhiteSpace(propertyName)
                                                                                        && typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase
                                                                                                                                | BindingFlags.Public
                                                                                                                                | BindingFlags.Instance) != null;

    /// <summary>
    /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b>typeof(<typeparamref name="T"/>)</b>. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static PropertyInfo ThrowIfPropertyNotExists<T>(string propertyName)
        => typeof(T).ThrowIfPropertyNotExists(propertyName);

    /// <summary>
    /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b><paramref name="content"/></b>. 
    /// </summary>
    /// <param name="content"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static PropertyInfo ThrowIfPropertyNotExists(this object content, string propertyName)
        => content.GetType().ThrowIfPropertyNotExists(propertyName);

    /// <summary>
    /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b><paramref name="type"/></b>. 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static PropertyInfo ThrowIfPropertyNotExists(this Type type, string propertyName)
        => type.GetPublicPropertyIgnoreCase(propertyName) ?? throw new MilvaDeveloperException($"Type of {type.Name}'s properties doesn't contain '{propertyName}'.");

    /// <summary>
    /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b>typeof(<typeparamref name="T"/>)</b>. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static bool PropertyExists<T>(string propertyName) => typeof(T).PropertyExists(propertyName);

    /// <summary>
    /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b><paramref name="content"/></b>. 
    /// </summary>
    /// <param name="content"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static bool PropertyExists(this object content, string propertyName) => content.GetType().PropertyExists(propertyName);

    /// <summary>
    /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b><paramref name="type"/></b>. 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static bool PropertyExists(this Type type, string propertyName) => type.GetPublicPropertyIgnoreCase(propertyName) != null;

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
    /// Creates order by key selector by <paramref name="orderByPropertyName"/>.
    /// </summary>
    /// 
    /// <exception cref="MilvaDeveloperException"> Throwns when type of <typeparamref name="T"/>'s properties doesn't contain '<paramref name="orderByPropertyName"/>'. </exception>
    /// 
    /// <typeparam name="T"></typeparam>
    /// <param name="orderByPropertyName"></param>
    /// <returns></returns>
    public static Expression<Func<T, object>> CreateOrderBySelectorExpression<T>(string orderByPropertyName)
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
    /// Dynamically gets method and invokes <see cref="CommonHelper"/> create property selector methods.
    /// </summary>
    /// <param name="createPropertySelectorMethodName"></param>
    /// <param name="entityType"></param>
    /// <param name="propType"></param>
    /// <param name="propName"></param>
    /// <returns></returns>
    public static object DynamicInvokeCreatePropertySelector(string createPropertySelectorMethodName, Type entityType, Type propType, string propName)
    {
        // Step 1: Get the MethodInfo object for the generic method
        var selectorMethod = typeof(CommonHelper).GetMethod(createPropertySelectorMethodName);

        // Step 2: Construct the method generic with desired type of arguments
        MethodInfo genericSelectorMethod = selectorMethod.MakeGenericMethod(entityType, propType);

        // Step 3: Call the generic method with the specified type arguments
        var propertySelectorResult = genericSelectorMethod.Invoke(null, [propName]);

        return propertySelectorResult;
    }
}
