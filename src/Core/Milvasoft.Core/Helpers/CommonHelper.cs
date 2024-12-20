﻿using Milvasoft.Types.Structs;
using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Milvasoft.Core.Helpers;

/// <summary>
/// Common Helper class.
/// </summary>
public static partial class CommonHelper
{
    private static readonly MethodInfo _deserializeMethod = typeof(JsonSerializer).GetMethod(nameof(JsonSerializer.Deserialize), [typeof(JsonElement), typeof(JsonSerializerOptions)]);

    /// <summary>
    /// Creates an expression that represents the condition where the IsDeleted property of an entity is false.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>An expression representing the condition where the IsDeleted property is false.</returns>
    public static Expression<Func<TEntity, bool>> CreateIsDeletedFalseExpression<TEntity>()
    {
        // Get the type of the entity
        var entityType = typeof(TEntity);

        // Check if the entity implements the ISoftDeletable interface
        if (!typeof(ISoftDeletable).IsAssignableFrom(entityType))
            return null;

        // Create a parameter expression for the entity
        var parameter = Expression.Parameter(entityType, "e");

        // Create an expression that represents the condition where the IsDeleted property is false
        var filterExpression = Expression.Equal(Expression.Property(parameter, entityType.GetProperty(EntityPropertyNames.IsDeleted)), Expression.Constant(false, typeof(bool)));

        // Create a lambda expression with the filter expression and the parameter
        return Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
    }

    /// <summary>
    /// Gets the description attribute value of the specified enum value.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="enumValue">The enum value.</param>
    /// <returns>The description attribute value of the enum value.</returns>
    public static string GetEnumDesciption<T>(this T enumValue) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
            return null;

        var description = enumValue.ToString();

        var fieldInfo = enumValue.GetType().GetField(description);

        var descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>(false);

        if (descriptionAttribute != null)
            description = ((DescriptionAttribute)descriptionAttribute).Description;

        return description;
    }

    /// <summary>
    /// Determines wheter the <paramref name="type"/> is enumerable or not.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsEnumerableType(this Type type) => type?.GetInterface(nameof(IEnumerable)) != null;

    /// <summary>
    /// Assigns the updated properties of a DTO object to the corresponding properties of an entity object.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity class that implements the <see cref="EntityBase"/> interface.</typeparam>
    /// <typeparam name="TDto">The type of the DTO class that inherits from the <see cref="DtoBase"/> class.</typeparam>
    /// <param name="entity">The entity object to update.</param>
    /// <param name="dto">The DTO object containing the updated property values.</param>
    /// <returns>A list of PropertyInfo objects representing the updated properties.</returns>
    /// <remarks> To find out how this method finds the updated properties, please see <see cref="FindUpdatablePropertiesAndAct{TEntity, TDto}(TDto, Action{PropertyInfo, object})"/>. </remarks>
    public static List<PropertyInfo> AssignUpdatedProperties<TEntity, TDto>(this TEntity entity, TDto dto) where TEntity : IMilvaEntity where TDto : DtoBase
    {
        if (entity == null || dto == null)
            return [];

        List<PropertyInfo> updatedProps = null;

        FindUpdatablePropertiesAndAct<TEntity, TDto>(dto, (matchingEntityProp, dtoPropertyValue) =>
        {
            matchingEntityProp.SetValue(entity, dtoPropertyValue);

            updatedProps ??= [];

            updatedProps.Add(matchingEntityProp);
        });

        return updatedProps;
    }

    /// <summary>
    /// Finds the updatable properties in the provided DTO object and performs the specified action on each property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity class that implements the IMilvaEntity interface.</typeparam>
    /// <typeparam name="TDto">The type of the DTO class that inherits from the DtoBase class.</typeparam>
    /// <param name="dto">The DTO object from which to find the updatable properties.</param>
    /// <param name="action">The action to perform on each updatable property. It takes two parameters: the PropertyInfo of the matching property in the entity class, and the value of the property in the DTO object.</param>
    /// <remarks>
    /// This method is used to update the entity object with the values of the updatable properties in the DTO object.
    /// It iterates over the updatable properties in the DTO object and finds the matching property in the entity class.
    /// If a matching property is found and the property value is an instance of <see cref="IUpdateProperty"/> and IsUpdated property is true,
    /// the specified action is performed on the matching property in the entity object.
    /// </remarks>
    public static void FindUpdatablePropertiesAndAct<TEntity, TDto>(TDto dto, Action<PropertyInfo, object> action) where TEntity : IMilvaEntity where TDto : DtoBase
    {
        if (dto == null || action == null)
            return;

        var updatableProperties = dto.GetUpdatableProperties();

        foreach (var dtoProp in updatableProperties)
        {
            var matchingEntityProp = Array.Find(typeof(TEntity).GetProperties(), i => i.Name == dtoProp.Name);

            if (matchingEntityProp == null)
                continue;

            var dtoValue = dtoProp.GetValue(dto);

            var updateProp = (IUpdateProperty)dtoValue;

            if (updateProp?.IsUpdated ?? false)
            {
                action(matchingEntityProp, updateProp.GetValue());
            }
        }
    }

    /// <summary>
    /// Determines whether the specified type can be assigned to the target type. 
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="targetType">The target type to check against.</param>
    /// <remarks>
    /// The <see cref="Type.IsAssignableTo(Type?)"/> method returns false in the examples like below. But <see cref="CanAssignableTo"/> returns true.
    /// <code> typeof(SomeClass&lt;&gt;).IsAssignableTo(typeof(ISomeInterface&lt;&gt;)) </code>
    /// <code> typeof(SomeClass&lt;int&gt;).IsAssignableTo(typeof(ISomeInterface&lt;&gt;)) </code>
    /// </remarks>
    /// <returns>True if the specified type can be assigned to the target type; otherwise, false.</returns>
    public static bool CanAssignableTo(this Type type, Type targetType)
    {
        if (type == null || targetType == null)
            return false;

        if (!targetType.IsGenericType && !type.IsGenericType)
            return type.IsAssignableTo(targetType);

        if (!IsTypeArgumentsCanBeAssignableToEachOther(type, targetType))
            return false;

        type = GetTypeAccordingToGenericDefinition(type);
        targetType = GetTypeAccordingToGenericDefinition(targetType);

        if (type.IsInterface)
        {
            return type.IsAssignableTo(targetType);
        }
        else if (targetType.IsInterface)
        {
            var interfaceType = Array.Find(type.GetInterfaces(), i => GetTypeAccordingToGenericDefinition(i) == targetType);

            return interfaceType != null;
        }
        else
        {
            Type compareType;

            if (type.BaseType == typeof(object))
                compareType = type;
            else
                compareType = GetTypeAccordingToGenericDefinition(type.BaseType);

            if (compareType.IsAssignableTo(targetType))
                return true;
            else
                return false;
        }

        static Type GetTypeAccordingToGenericDefinition(Type type) => type.IsGenericType ? type.GetGenericTypeDefinition() == typeof(Task<>)
                                                                                             ? type.GenericTypeArguments.FirstOrDefault()
                                                                                             : type.GetGenericTypeDefinition()
                                                                                         : type;

        static bool IsTypeArgumentsCanBeAssignableToEachOther(Type type, Type targetType)
        {
            var typeGenericArguments = type.GenericTypeArguments;
            var targetTypeGenericArguments = targetType.GenericTypeArguments;

            if (typeGenericArguments.Length == 0 || targetTypeGenericArguments.Length == 0)
                return true;

            var loopCount = int.Min(typeGenericArguments.Length, targetTypeGenericArguments.Length);

            for (int i = 0; i < loopCount; i++)
            {
                if (!typeGenericArguments[i].CanAssignableTo(targetTypeGenericArguments[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Gets generic method info.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="methodName"></param>
    /// <param name="genericParameterCount"></param>
    /// <param name="parameterTypes"></param>
    /// <returns></returns>
    public static MethodInfo GetGenericMethod(this Type type, string methodName, int genericParameterCount, params Type[] parameterTypes)
    {
        var methodInfo = Array.Find(type.GetMethods(), m => m.Name == methodName
                                                            && m.IsGenericMethodDefinition
                                                            && m.GetGenericArguments().Length == genericParameterCount
                                                            && ParameterTypesMatch(m, parameterTypes));

        return methodInfo;

        static bool ParameterTypesMatch(MethodInfo method, Type[] types)
        {
            var methodParameters = method.GetParameters().ToList();
            methodParameters.RemoveAll(i => i.ParameterType.IsGenericMethodParameter);

            if (methodParameters.Count != types.Length)
                return false;

            for (int i = 0; i < methodParameters.Count; i++)
            {
                if (!methodParameters[i].ParameterType.IsGenericMethodParameter && !methodParameters[i].ParameterType.CanAssignableTo(types[i]))
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Determines whether the specified type is a non-nullable value type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the specified type is a non-nullable value type; otherwise, false.</returns>

    public static bool IsNonNullableValueType(this Type type) => type != null && type.IsValueType && Nullable.GetUnderlyingType(type) == null;
}
