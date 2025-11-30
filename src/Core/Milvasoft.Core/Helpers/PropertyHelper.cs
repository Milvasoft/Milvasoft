using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Core.Helpers;

public static partial class CommonHelper
{
    /// <summary>
    /// Checks whether a property with the specified name exists in the collection of properties of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the source collection.</typeparam>
    /// <param name="_">The source collection (not used).</param>
    /// <param name="propertyName">The name of the property to check.</param>
    /// <returns>True if the property exists, otherwise false.</returns>
    public static bool PropertyExists<T>(this IEnumerable<T> _, string propertyName) => typeof(T).PropertyExists(propertyName);

    /// <summary>
    /// Checks whether a property with the specified name exists in the properties of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <param name="propertyName">The name of the property to check.</param>
    /// <returns>True if the property exists, otherwise false.</returns>
    public static bool PropertyExists<T>(string propertyName) => typeof(T).PropertyExists(propertyName);

    /// <summary>
    /// Checks whether a property with the specified name exists in the properties of the specified object.
    /// </summary>
    /// <param name="content">The object to check.</param>
    /// <param name="propertyName">The name of the property to check.</param>
    /// <returns>True if the property exists, otherwise false.</returns>
    public static bool PropertyExists(this object content, string propertyName) => content.GetType().PropertyExists(propertyName);

    /// <summary>
    /// Checks whether a property with the specified name exists in the properties of the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="propertyName">The name of the property to check.</param>
    /// <returns>True if the property exists, otherwise false.</returns>
    public static bool PropertyExists(this Type type, string propertyName) => type.GetPublicPropertyIgnoreCase(propertyName) != null;

    /// <summary>
    /// Throws an exception if a property with the specified name does not exist in the collection of properties of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the source collection.</typeparam>
    /// <param name="_">The source collection (not used).</param>
    /// <param name="propertyName">The name of the property to check.</param>
    /// <returns>The <see cref="PropertyInfo"/> object representing the property.</returns>
    public static PropertyInfo ThrowIfPropertyNotExists<T>(this IEnumerable<T> _, string propertyName)
        => typeof(T).ThrowIfPropertyNotExists(propertyName);

    /// <summary>
    /// Throws an exception if a property with the specified name does not exist in the properties of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <param name="propertyName">The name of the property to check.</param>
    /// <returns>The <see cref="PropertyInfo"/> object representing the property.</returns>
    public static PropertyInfo ThrowIfPropertyNotExists<T>(string propertyName)
        => typeof(T).ThrowIfPropertyNotExists(propertyName);

    /// <summary>
    /// Throws an exception if a property with the specified name does not exist in the properties of the specified object.
    /// </summary>
    /// <param name="content">The object to check.</param>
    /// <param name="propertyName">The name of the property to check.</param>
    /// <returns>The <see cref="PropertyInfo"/> object representing the property.</returns>
    public static PropertyInfo ThrowIfPropertyNotExists(this object content, string propertyName)
        => content.GetType().ThrowIfPropertyNotExists(propertyName);

    /// <summary>
    /// Throws an exception if a property with the specified name does not exist in the properties of the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="propertyName">The name of the property to check.</param>
    /// <returns>The <see cref="PropertyInfo"/> object representing the property.</returns>
    public static PropertyInfo ThrowIfPropertyNotExists(this Type type, string propertyName)
        => type.GetPublicPropertyIgnoreCase(propertyName) ?? throw new MilvaDeveloperException($"Type of {type.Name}'s properties doesn't contain '{propertyName}'.");

    /// <summary>
    /// Gets the <see cref="PropertyInfo"/> object representing the property with the specified name in the specified type.
    /// The search is case-insensitive and includes public instance properties.
    /// </summary>
    /// <param name="type">The type to search in.</param>
    /// <param name="propertyName">The name of the property to get.</param>
    /// <returns>The <see cref="PropertyInfo"/> object representing the property, or null if the property does not exist.</returns>
    public static PropertyInfo GetPublicPropertyIgnoreCase(this Type type, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return null;

        return type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                ?? type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
    }

    /// <summary>
    /// Creates a property selector expression for the specified property name.
    /// </summary>
    /// <typeparam name="T">The type to create the expression for.</typeparam>
    /// <typeparam name="TPropertyType">The type of the property.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The property selector expression.</returns>
    /// <exception cref="MilvaDeveloperException">Thrown when <typeparamref name="T"/> does not contain <paramref name="propertyName"/>.</exception>
    public static Expression<Func<T, TPropertyType>> CreatePropertySelector<T, TPropertyType>(string propertyName)
    {
        if (!PropertyExists<T>(propertyName))
            return null;

        return CreatePropertySelectorExpression<T, TPropertyType>(propertyName);
    }

    /// <summary>
    /// Creates a required property selector expression for the specified property name.
    /// </summary>
    /// <typeparam name="T">The type to create the expression for.</typeparam>
    /// <typeparam name="TPropertyType">The type of the property.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The required property selector expression.</returns>
    /// <exception cref="MilvaDeveloperException">Thrown when <typeparamref name="T"/> does not contain <paramref name="propertyName"/>.</exception>
    public static Expression<Func<T, TPropertyType>> CreateRequiredPropertySelector<T, TPropertyType>(string propertyName)
    {
        typeof(T).ThrowIfPropertyNotExists(propertyName);

        return CreatePropertySelectorExpression<T, TPropertyType>(propertyName);
    }

    /// <summary>
    /// Creates an order by key selector expression for the specified property name.
    /// </summary>
    /// <typeparam name="T">The type to create the expression for.</typeparam>
    /// <param name="propertyName">The name of the property to order by.</param>
    /// <returns>The order by key selector expression.</returns>
    /// <exception cref="MilvaDeveloperException">Thrown when the type of <typeparamref name="T"/>'s properties does not contain '<paramref name="propertyName"/>'.</exception>
    public static Expression<Func<T, object>> CreateRequiredPropertySelector<T>(string propertyName)
    {
        typeof(T).ThrowIfPropertyNotExists(propertyName);

        return CreatePropertySelectorExpression<T, object>(propertyName);
    }

    /// <summary>
    /// Creates a property selector function for the specified property name.
    /// </summary>
    /// <typeparam name="T">The type to create the function for.</typeparam>
    /// <typeparam name="TPropertyType">The type of the property.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The property selector function.</returns>
    /// <exception cref="MilvaDeveloperException">Thrown when <typeparamref name="T"/> does not contain <paramref name="propertyName"/>.</exception>
    public static Func<T, TPropertyType> CreatePropertySelectorFunction<T, TPropertyType>(string propertyName)
        => CreatePropertySelector<T, TPropertyType>(propertyName)?.Compile();

    /// <summary>
    /// Creates a required property selector function for the specified property name.
    /// </summary>
    /// <typeparam name="T">The type to create the function for.</typeparam>
    /// <typeparam name="TPropertyType">The type of the property.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The required property selector function.</returns>
    /// <exception cref="MilvaDeveloperException">Thrown when <typeparamref name="T"/> does not contain <paramref name="propertyName"/>.</exception>
    public static Func<T, TPropertyType> CreateRequiredPropertySelectorFuction<T, TPropertyType>(string propertyName)
    {
        typeof(T).ThrowIfPropertyNotExists(propertyName);

        return CreateRequiredPropertySelector<T, TPropertyType>(propertyName).Compile();
    }

    /// <summary>
    /// Dynamically gets a method and invokes the specified create property selector method from <see cref="CommonHelper"/>.
    /// </summary>
    /// <param name="createPropertySelectorMethodName">The name of the create property selector method to invoke.</param>
    /// <param name="entityType">The type of the entity.</param>
    /// <param name="propType">The type of the property.</param>
    /// <param name="propName">The name of the property.</param>
    /// <returns>The result of the create property selector method.</returns>
    public static object DynamicInvokeCreatePropertySelector(string createPropertySelectorMethodName, Type entityType, Type propType, string propName)
    {
        if (string.IsNullOrWhiteSpace(createPropertySelectorMethodName)
            || string.IsNullOrWhiteSpace(propName)
            || entityType == null
            || propType == null)
            throw new MilvaDeveloperException("Please send all dynamic invoke parameters valid!");

        // Step 1: Get the MethodInfo object for the generic method
        var selectorMethod = typeof(CommonHelper).GetMethod(createPropertySelectorMethodName, 2, [typeof(string)])
            ?? throw new MilvaDeveloperException($"Method not found with name {createPropertySelectorMethodName}!");

        // Step 2: Construct the method generic with the desired type of arguments
        MethodInfo genericSelectorMethod = selectorMethod.MakeGenericMethod(entityType, propType);

        // Step 3: Call the generic method with the specified type arguments
        var propertySelectorResult = genericSelectorMethod.Invoke(null, [propName]);

        return propertySelectorResult;
    }

    /// <summary>
    /// Creates a property selector expression for the specified property name.
    /// </summary>
    /// <typeparam name="T">The type to create the expression for.</typeparam>
    /// <typeparam name="TPropertyType">The type of the property.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The property selector expression.</returns>
    /// <exception cref="MilvaDeveloperException">Thrown when <typeparamref name="T"/> does not contain <paramref name="propertyName"/>.</exception>
    private static Expression<Func<T, TPropertyType>> CreatePropertySelectorExpression<T, TPropertyType>(string propertyName)
    {
        var entityType = typeof(T);

        var parameter = Expression.Parameter(entityType, "i");

        return Expression.Lambda<Func<T, TPropertyType>>(Expression.Convert(Expression.Property(parameter, propertyName), typeof(TPropertyType)), parameter);
    }
}
