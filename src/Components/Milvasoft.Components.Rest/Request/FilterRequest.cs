using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using ExpressionBuilder.Helpers;
using ExpressionBuilder.Interfaces;
using ExpressionBuilder.Operations;
using Milvasoft.Components.Rest.Enums;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// Filter specs.
/// </summary>
public class FilterRequest
{
    private static readonly FilterType[] _membershipFilterTypes = [FilterType.In, FilterType.NotIn];

    private static readonly Dictionary<TypeGroup, HashSet<FilterType>> _supportedFilterTypes = new()
    {
        { TypeGroup.Default, [FilterType.EqualTo , FilterType.NotEqualTo, .. _membershipFilterTypes] },
        { TypeGroup.Text, [FilterType.Contains, FilterType.DoesNotContain, FilterType.EndsWith, FilterType.EqualTo, FilterType.IsEmpty, FilterType.IsNotEmpty, FilterType.IsNotNull, FilterType.IsNotNullNorWhiteSpace, FilterType.IsNull, FilterType.IsNullOrWhiteSpace, FilterType.NotEqualTo, FilterType.StartsWith, .. _membershipFilterTypes] },
        { TypeGroup.Number, [FilterType.Between, FilterType.EqualTo, FilterType.GreaterThan, FilterType.GreaterThanOrEqualTo, FilterType.LessThan,FilterType.LessThanOrEqualTo, FilterType.NotEqualTo, .. _membershipFilterTypes] },
        { TypeGroup.Boolean, new HashSet<FilterType>  { FilterType.EqualTo , FilterType.NotEqualTo } },
        { TypeGroup.Date, new HashSet<FilterType> { FilterType.Between, FilterType.DateEqualTo, FilterType.GreaterThan, FilterType.GreaterThanOrEqualTo, FilterType.LessThan, FilterType.LessThanOrEqualTo } }
    };

    /// <summary>
    /// Specifies the type of merging for the filter criterias.
    /// </summary>
    public virtual Connector MergeType { get; set; } = Connector.And;

    /// <summary>
    /// Filter details.
    /// </summary>
    public virtual List<FilterCriteria> Criterias { get; set; }

    /// <summary>
    /// Builds a filter expression based on the specified filter criteria.
    /// Criteria without <see cref="FilterCriteria.FilterBy"/> are ignored.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to filter.</typeparam>
    /// <returns>The filter expression.</returns>
    /// <exception cref="MilvaUserFriendlyException">
    /// Thrown when a criteria refers to a property that does not exist or cannot be filtered (see <see cref="RequestPropertyPolicy"/>),
    /// when the filter type is not supported for the property type, or when required values are missing.
    /// </exception>
    public virtual Expression<Func<TEntity, bool>> BuildFilterExpression<TEntity>() where TEntity : class
    {
        if (Criterias.IsNullOrEmpty())
            return null;

        Criterias = [.. Criterias.Where(f => !string.IsNullOrWhiteSpace(f.FilterBy)).DistinctBy(f => new { f.FilterBy, f.Type, f.Value })];

        var expression = new Filter<TEntity>();

        foreach (var filter in Criterias)
        {
            var (declaredPropertyType, propertyType) = CheckPropertyAndGetType<TEntity>(filter);

            if (!IsFilterTypeSupported(propertyType, filter))
                throw new MilvaUserFriendlyException($"Filter type '{filter.Type}' is not supported for '{filter.FilterBy}'.", MilvaException.InvalidParameter);

            IOperation operation = GetOperationByFilterType(filter.Type);

            if (operation.NumberOfValues == 0)
            {
                expression.By(filter.FilterBy, operation, MergeType);
            }
            else if (operation.NumberOfValues == 1)
            {
                var value = _membershipFilterTypes.Contains(filter.Type)
                                ? GetListValue(filter, declaredPropertyType)
                                : GetValueWithCorrectType(filter, filter.Value, declaredPropertyType);

                expression.By(filter.FilterBy, operation, value, MergeType);
            }
            else
            {
                var value = GetValueWithCorrectType(filter, filter.Value, declaredPropertyType);
                var otherValue = GetValueWithCorrectType(filter, filter.OtherValue, declaredPropertyType);

                expression.By(filter.FilterBy, operation, value, otherValue, MergeType);
            }
        }

        return expression;
    }

    /// <summary>
    /// Gets the value with the correct type for the given property.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="value"></param>
    /// <param name="propType">Declared type of the filtered property.</param>
    /// <returns></returns>
    private static object GetValueWithCorrectType(FilterCriteria filter, object value, Type propType)
    {
        if (value == null)
        {
            if (propType.IsNonNullableValueType())
                throw new MilvaUserFriendlyException("Please provide filter values!", MilvaException.InvalidParameter);

            if (propType == typeof(string))
                value = string.Empty;
        }

        if (value is JsonElement jsonElementOfValue)
            value = DeserializeFilterValue(filter, jsonElementOfValue, propType);

        return value;
    }

    /// <summary>
    /// Gets the list value of <see cref="FilterType.In"/> and <see cref="FilterType.NotIn"/> filters as a <see cref="List{T}"/> of the property type.
    /// </summary>
    private static object GetListValue(FilterCriteria filter, Type propType)
    {
        var listType = typeof(List<>).MakeGenericType(propType);

        switch (filter.Value)
        {
            case JsonElement { ValueKind: JsonValueKind.Array } jsonElement:
                return DeserializeFilterValue(filter, jsonElement, listType);
            case not null when listType.IsInstanceOfType(filter.Value):
                return filter.Value;
            case IEnumerable values and not string:
                var list = (IList)Activator.CreateInstance(listType);

                foreach (var item in values)
                    list.Add(item is JsonElement element ? DeserializeFilterValue(filter, element, propType) : item);

                return list;
            default:
                throw new MilvaUserFriendlyException($"Filter type '{filter.Type}' requires a list of values for '{filter.FilterBy}'.", MilvaException.InvalidParameter);
        }
    }

    private static object DeserializeFilterValue(FilterCriteria filter, JsonElement value, Type targetType)
    {
        try
        {
            return value.Deserialize(targetType);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException or ArgumentException)
        {
            throw new MilvaUserFriendlyException($"Filter value is not valid for '{filter.FilterBy}'.", MilvaException.InvalidParameter);
        }
    }

    /// <summary>
    /// Checks the property (and the nested property of reference/collection paths) and gets its types for the given filter criteria.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="filter"></param>
    /// <returns>The declared type of the filtered property and its non nullable type.</returns>
    /// <exception cref="MilvaUserFriendlyException">When a property does not exist or cannot be filtered.</exception>
    private static (Type DeclaredType, Type Type) CheckPropertyAndGetType<TEntity>(FilterCriteria filter)
    {
        string propertyName = filter.FilterBy;

        if (filter.FilterByContainsSpecialChars())
            propertyName = filter.GetUntilSpecialCharFromFilterBy();

        var property = GetAllowedProperty(typeof(TEntity), propertyName, filter);

        var declaredType = property.PropertyType;
        var propertyType = Nullable.GetUnderlyingType(declaredType) ?? declaredType;

        if (propertyType != typeof(string) && (propertyType.IsArray || propertyType.IsEnumerableType() || propertyType.IsClass))
        {
            Type ownerType = propertyType.IsArray
                                ? propertyType.GetElementType()
                                : propertyType.IsGenericType ? propertyType.GetGenericArguments()[0] : propertyType;

            if (!filter.FilterByContainsSpecialChars() || ownerType == null)
                throw RequestPropertyPolicy.NotAllowed(filter.FilterBy, RequestPropertyUsage.Filter);

            var childProperty = GetAllowedProperty(ownerType, filter.GetChildrenPropertyNameFromFilterBy(), filter);

            declaredType = childProperty.PropertyType;
            propertyType = Nullable.GetUnderlyingType(declaredType) ?? declaredType;
        }

        return (declaredType, propertyType);
    }

    private static PropertyInfo GetAllowedProperty(Type type, string propertyName, FilterCriteria filter)
    {
        var property = type.GetPublicPropertyIgnoreCase(propertyName);

        if (property == null || !RequestPropertyPolicy.IsAllowed(property, RequestPropertyUsage.Filter))
            throw RequestPropertyPolicy.NotAllowed(filter.FilterBy, RequestPropertyUsage.Filter);

        return property;
    }

    /// <summary>
    /// Checks if the filter type is supported for the given property type.
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    private static bool IsFilterTypeSupported(Type propertyType, FilterCriteria filter)
    {
        var typeGroup = TypeGroup.Default;

        if (OperationHelper.TypeGroups.Any(i => i.Value.Any(v => v.Name == propertyType.Name)))
            typeGroup = OperationHelper.TypeGroups.FirstOrDefault(i => i.Value.Any(v => v.Name == propertyType.Name)).Key;

        return _supportedFilterTypes.TryGetValue(typeGroup, out var supportedTypes) && supportedTypes.Contains(filter.Type);
    }

    /// <summary>
    /// Gets the operation and value count for the given filter type.
    /// </summary>
    /// <param name="filterType"></param>
    /// <returns></returns>
    private static IOperation GetOperationByFilterType(FilterType filterType) => filterType switch
    {
        FilterType.Between => new Between(),
        FilterType.Contains => new Contains(),
        FilterType.DoesNotContain => new DoesNotContain(),
        FilterType.StartsWith => new StartsWith(),
        FilterType.EndsWith => new EndsWith(),
        FilterType.EqualTo => new EqualTo(),
        FilterType.NotEqualTo => new NotEqualTo(),
        FilterType.GreaterThan => new GreaterThan(),
        FilterType.GreaterThanOrEqualTo => new GreaterThanOrEqualTo(),
        FilterType.LessThan => new LessThan(),
        FilterType.LessThanOrEqualTo => new LessThanOrEqualTo(),
        FilterType.IsEmpty => new IsEmpty(),
        FilterType.IsNotEmpty => new IsNotEmpty(),
        FilterType.IsNull => new IsNull(),
        FilterType.IsNotNull => new IsNotNull(),
        FilterType.IsNullOrWhiteSpace => new IsNullOrWhiteSpace(),
        FilterType.IsNotNullNorWhiteSpace => new IsNotNullNorWhiteSpace(),
        FilterType.In => new In(),
        FilterType.NotIn => new NotIn(),
        FilterType.DateEqualTo => new DateEqualTo(),
        _ => new EqualTo(),
    };
}
