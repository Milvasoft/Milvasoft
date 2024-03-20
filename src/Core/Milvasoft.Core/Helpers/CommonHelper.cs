using Milvasoft.Core.Utils;
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
    /// Gets <b>entity => entity.IsDeleted == false</b> expression, if <typeparamref name="TEntity"/> is assignable from <see cref="IFullAuditable{TKey}"/>.
    /// </summary>
    /// <returns></returns>
    public static Expression<Func<TEntity, bool>> CreateIsDeletedFalseExpression<TEntity>()
    {
        var entityType = typeof(TEntity);

        if (!typeof(ISoftDeletable).IsAssignableFrom(entityType))
            return null;

        var parameter = Expression.Parameter(entityType, "e");

        var filterExpression = Expression.Equal(Expression.Property(parameter, entityType.GetProperty(EntityPropertyNames.IsDeleted)), Expression.Constant(false, typeof(bool)));

        return Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
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

        var description = enumValue.ToString();

        var fieldInfo = enumValue.GetType().GetField(description);

        var descriptionAttribute = fieldInfo.GetCustomAttribute(typeof(DescriptionAttribute), false);

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
    /// Updates entity matching properties with <paramref name="dto"/>'s not null properties.
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="dto"></param>
    /// <param name="entity"></param>
    /// <returns>Updated property informations.</returns>
    public static List<PropertyInfo> AssignUpdatedProperties<TEntity, TDto>(this TEntity entity, TDto dto) where TEntity : class, IMilvaEntity where TDto : DtoBase
    {
        if (entity == null || dto == null)
            return null;

        List<PropertyInfo> updatedProps = null;

        FindUpdatablePropertiesAndAct<TEntity, TDto>(dto, (matchingEntityProp, dtoPropertyValue) =>
        {
            matchingEntityProp.SetValue(entity, dtoPropertyValue);

            updatedProps ??= [];

            updatedProps.Add(matchingEntityProp);
        });

        return updatedProps;
    }

    public static void FindUpdatablePropertiesAndAct<TEntity, TDto>(TDto dto, Action<PropertyInfo, object> action) where TEntity : class, IMilvaEntity where TDto : DtoBase
    {
        if (dto == null || action == null)
            return;

        var updatableProperties = dto.GetUpdatableProperties();

        foreach (var dtoProp in updatableProperties)
        {
            var matchingEntityProp = typeof(TEntity).GetProperties().FirstOrDefault(i => i.Name == dtoProp.Name);

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
}
