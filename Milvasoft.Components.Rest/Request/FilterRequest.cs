using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using ExpressionBuilder.Interfaces;
using ExpressionBuilder.Operations;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Core;
using Milvasoft.Core.Extensions;
using Milvasoft.Core.Utils.Constants;
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
        { TypeGroup.Text, new HashSet<FilterType> { FilterType.Contains, FilterType.DoesNotContain, FilterType.EndsWith, FilterType.EqualTo, FilterType.IsEmpty, FilterType.IsNotEmpty, FilterType.IsNotNull, FilterType.IsNotNullNorWhiteSpace, FilterType.IsNull, FilterType.IsNullOrWhiteSpace, FilterType.NotEqualTo, FilterType.StartsWith, } },
        { TypeGroup.Number, new HashSet<FilterType> { FilterType.Between,FilterType.EqualTo, FilterType.Between,FilterType.GreaterThan, FilterType.GreaterThanOrEqualTo, FilterType.LessThan,FilterType.LessThanOrEqualTo, FilterType.NotEqualTo } },
        { TypeGroup.Boolean, new HashSet<FilterType>  { FilterType.EqualTo , FilterType.NotEqualTo } },
        { TypeGroup.Date, new HashSet<FilterType> { FilterType.Between,FilterType.EqualTo, FilterType.GreaterThan, FilterType.GreaterThanOrEqualTo, FilterType.LessThan, FilterType.LessThanOrEqualTo, FilterType.NotEqualTo } }
    };

    /// <summary>
    /// Filter details.
    /// </summary>
    public virtual List<FilterCriteria> Criterias { get; set; }

    /// <summary>
    /// Builds filter expression according to <see cref="Criterias"/>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public virtual Expression<Func<TEntity, bool>> BuildFilterExpression<TEntity>() where TEntity : class
    {
        if (Criterias.IsNullOrEmpty())
            return null;

        var expression = new Filter<TEntity>();

        foreach (var filter in Criterias)
        {
            var propertyType = CheckPropertyAndGetType<TEntity>(filter);

            if (!IsFilterTypeSupported(propertyType, filter))
                throw new Exception($"{filter.FilterType} is not supported for {propertyType.Name}.");

            (IOperation operation, int valueCount) = GetOperationAndValueCount(filter.FilterType);

            if (valueCount == 0)
            {
                expression.By(filter.FilterBy, operation, Connector.And);
            }
            else if (valueCount == 1)
            {
                var value = ((JsonElement)filter.Value).Deserialize(propertyType);

                expression.By(filter.FilterBy, operation, value, Connector.And);
            }
            else
            {
                var value = ((JsonElement)filter.Value).Deserialize(propertyType);
                var value2 = ((JsonElement)filter.OtherValue).Deserialize(propertyType);

                expression.By(filter.FilterBy, operation, value, value2, Connector.And);
            }
        }

        return expression;
    }

    private static Type CheckPropertyAndGetType<TEntity>(FilterCriteria filter)
    {
        string propertyName = filter.FilterBy;

        if (filter.FilterByContainsSpecialChars())
            propertyName = filter.GetUntilSpecialCharFromFilterBy();

        var property = typeof(TEntity).ThrowIfPropertyNotExists(propertyName);

        var propertyType = property.PropertyType;

        var underlyingNullableType = Nullable.GetUnderlyingType(propertyType);

        if (underlyingNullableType != null)
            propertyType = underlyingNullableType;

        if (propertyType.IsArray || propertyType.IsEnumerableType())
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

    private static bool IsFilterTypeSupported(Type propertyType, FilterCriteria filter)
    {
        var typeGroup = TypeGroup.Default;

        if (_typeGroups.Any(i => i.Value.Any(v => v.Name == propertyType.Name)))
        {
            typeGroup = _typeGroups.FirstOrDefault(i => i.Value.Any(v => v.Name == propertyType.Name)).Key;
        }

        return _supportedFilterTypes[typeGroup].Any(ft => ft == filter.FilterType);
    }

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
