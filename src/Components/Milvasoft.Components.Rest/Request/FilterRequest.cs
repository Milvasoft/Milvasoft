using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using ExpressionBuilder.Interfaces;
using ExpressionBuilder.Operations;
using Milvasoft.Components.Rest.Enums;
using System.Linq.Expressions;
using System.Text.Json;

namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// Filter specs.
/// </summary>
public class FilterRequest
{
    private static readonly Dictionary<TypeGroup, HashSet<Type>> _typeGroups = new()
    {
        { TypeGroup.Text, new HashSet<Type> { typeof(string), typeof(char)} },
        { TypeGroup.Number, new HashSet<Type> { typeof(int), typeof(uint), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(long), typeof(ulong), typeof(Single), typeof(double), typeof(decimal) } },
        { TypeGroup.Boolean, new HashSet<Type> { typeof(bool) } },
        { TypeGroup.Date, new HashSet<Type> { typeof(DateTime) } },
        { TypeGroup.Nullable, new HashSet<Type> { typeof(Nullable<>), typeof(string) } }
    };

    private static readonly Dictionary<TypeGroup, HashSet<FilterType>> _supportedFilterTypes = new()
    {
        { TypeGroup.Default, new HashSet<FilterType>  { FilterType.EqualTo , FilterType.NotEqualTo } },
        { TypeGroup.Text, new HashSet<FilterType> { FilterType.Contains, FilterType.DoesNotContain, FilterType.EndsWith, FilterType.EqualTo, FilterType.IsEmpty, FilterType.IsNotEmpty, FilterType.IsNotNull, FilterType.IsNotNullNorWhiteSpace, FilterType.IsNull, FilterType.IsNullOrWhiteSpace, FilterType.NotEqualTo, FilterType.StartsWith, } },
        { TypeGroup.Number, new HashSet<FilterType> { FilterType.Between, FilterType.EqualTo, FilterType.Between,FilterType.GreaterThan, FilterType.GreaterThanOrEqualTo, FilterType.LessThan,FilterType.LessThanOrEqualTo, FilterType.NotEqualTo } },
        { TypeGroup.Boolean, new HashSet<FilterType>  { FilterType.EqualTo , FilterType.NotEqualTo } },
        { TypeGroup.Date, new HashSet<FilterType> { FilterType.Between, FilterType.EqualTo, FilterType.GreaterThan, FilterType.GreaterThanOrEqualTo, FilterType.LessThan, FilterType.LessThanOrEqualTo, FilterType.NotEqualTo } }
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
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to filter.</typeparam>
    /// <returns>The filter expression.</returns>
    /// <exception cref="Exception">Thrown when the <typeparamref name="TEntity"/> does not contain the value sent in the FilterBy property of one of the criteria.</exception>
    public virtual Expression<Func<TEntity, bool>> BuildFilterExpression<TEntity>() where TEntity : class
    {
        if (Criterias.IsNullOrEmpty())
            return null;

        var expression = new Filter<TEntity>();

        foreach (var filter in Criterias)
        {
            var propertyType = CheckPropertyAndGetType<TEntity>(filter);

            if (propertyType == null || !IsFilterTypeSupported(propertyType, filter))
                continue;

            (IOperation operation, int valueCount) = GetOperationAndValueCount(filter.Type);

            if (valueCount == 0)
            {
                expression.By(filter.FilterBy, operation, MergeType);
            }
            else if (valueCount == 1)
            {
                var value = GetValueWithCorrectType<TEntity>(filter, filter.Value, propertyType, filter.FilterBy);

                expression.By(filter.FilterBy, operation, value, MergeType);
            }
            else
            {
                var value = GetValueWithCorrectType<TEntity>(filter, filter.Value, propertyType, filter.FilterBy);
                var otherValue = GetValueWithCorrectType<TEntity>(filter, filter.OtherValue, propertyType, filter.FilterBy);

                expression.By(filter.FilterBy, operation, value, otherValue, MergeType);
            }
        }

        return expression;

    }

    /// <summary>
    /// Gets the value with the correct type for the given property.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="value"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    private static object GetValueWithCorrectType<TEntity>(FilterCriteria filter, object value, Type propertyUnderlyingType, string propertyName)
    {
        var propType = filter.FilterByContainsSpecialChars() ? propertyUnderlyingType : typeof(TEntity).GetPublicPropertyIgnoreCase(propertyName).PropertyType;

        if (value == null)
        {
            if (propType.IsNonNullableValueType())
                throw new MilvaDeveloperException("Please provide filter values!");

            if (propType == typeof(string))
                value = string.Empty;
        }

        if (value is JsonElement jsonElementOfValue)
            value = jsonElementOfValue.Deserialize(propType);

        return value;
    }

    /// <summary>
    /// Checks the property and gets its type for the given filter criteria.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="filter"></param>
    /// <returns></returns>
    private static Type CheckPropertyAndGetType<TEntity>(FilterCriteria filter)
    {
        string propertyName = filter.FilterBy;

        if (filter.FilterByContainsSpecialChars())
            propertyName = filter.GetUntilSpecialCharFromFilterBy();

        var property = typeof(TEntity).GetPublicPropertyIgnoreCase(propertyName);

        if (property == null)
            return null;

        var propertyType = property.PropertyType;

        var underlyingNullableType = Nullable.GetUnderlyingType(propertyType);

        if (underlyingNullableType != null)
            propertyType = underlyingNullableType;

        if (propertyType != typeof(string) && (propertyType.IsArray || propertyType.IsEnumerableType()))
        {
            if (propertyType.IsGenericType)
            {
                var listPropType = propertyType.GetGenericArguments()[0];

                var childrenPropertyName = filter.GetChildrenPropertyNameFromFilterBy();

                var prop = listPropType.ThrowIfPropertyNotExists(childrenPropertyName);

                propertyType = prop.PropertyType;
            }
            else
                propertyType = propertyType.GetElementType();
        }

        return propertyType;
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

        var tempType = propertyType;

        if (_typeGroups.Any(i => i.Value.Any(v => v.Name == tempType.Name)))
            typeGroup = _typeGroups.FirstOrDefault(i => i.Value.Any(v => v.Name == tempType.Name)).Key;

        return _supportedFilterTypes[typeGroup].Any(ft => ft == filter.Type);
    }

    /// <summary>
    /// Gets the operation and value count for the given filter type.
    /// </summary>
    /// <param name="filterType"></param>
    /// <returns></returns>
    private static (IOperation operation, int valueCount) GetOperationAndValueCount(FilterType filterType) => filterType switch
    {
        FilterType.Between => (new Between(), 2),
        FilterType.Contains => (new Contains(), 1),
        FilterType.DoesNotContain => (new DoesNotContain(), 1),
        FilterType.StartsWith => (new StartsWith(), 1),
        FilterType.EndsWith => (new EndsWith(), 1),
        FilterType.EqualTo => (new EqualTo(), 1),
        FilterType.NotEqualTo => (new NotEqualTo(), 1),
        FilterType.GreaterThan => (new GreaterThan(), 1),
        FilterType.GreaterThanOrEqualTo => (new GreaterThanOrEqualTo(), 1),
        FilterType.LessThan => (new LessThan(), 1),
        FilterType.LessThanOrEqualTo => (new LessThanOrEqualTo(), 1),
        FilterType.IsEmpty => (new IsEmpty(), 0),
        FilterType.IsNotEmpty => (new IsNotEmpty(), 0),
        FilterType.IsNull => (new IsNull(), 0),
        FilterType.IsNotNull => (new IsNotNull(), 0),
        FilterType.IsNullOrWhiteSpace => (new IsNullOrWhiteSpace(), 0),
        FilterType.IsNotNullNorWhiteSpace => (new IsNotNullNorWhiteSpace(), 0),
        FilterType.In => (new In(), 1),
        FilterType.NotIn => (new NotIn(), 1),
        _ => (new EqualTo(), 1),
    };
}
