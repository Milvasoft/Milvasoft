using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using ExpressionBuilder.Interfaces;
using ExpressionBuilder.Operations;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Core;
using Milvasoft.Core.Extensions;
using System.Linq.Expressions;

namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// Helper filter components.
/// </summary>
public class FilterableAndSortable
{
    /// <summary>
    /// Sort column name
    /// </summary>
    /// <example>{columnName}</example>
    public string Sort { get; set; }

    /// <summary>
    /// Sort type
    /// Ascendind, Descending
    /// </summary>
    /// <example>Asc</example>
    public SortType SortType { get; set; }

    /// <summary>
    /// Filter details
    /// </summary>
    public List<FilterCriteria> Filters { get; set; }

    /// <summary>
    /// Builds filter expression according to <see cref="Filters"/>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Expression<Func<TEntity, bool>> BuildFilterExpression<TEntity>() where TEntity : class
    {
        if (Filters.IsNullOrEmpty())
            return null;

        var expression = new Filter<TEntity>();

        foreach (var filter in Filters)
        {
            object value;

            var property = typeof(TEntity).GetProperty(filter.ColumnName) ?? throw new Exception($"Cannot find column '{filter.ColumnName}'");

            var propertyType = property.PropertyType;

            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                propertyType = propertyType.GetGenericArguments()[0];

            var operation = GetOperation(filter.FilterType);

            if (propertyType == typeof(Guid))
                value = Guid.Parse(filter.Value.ToString());
            else
                value = Convert.ChangeType(filter.Value, propertyType);

            if (propertyType == typeof(DateTime))
            {
                var dateValue = ((DateTime)value).Date;

                if (filter.FilterType == FilterType.Equal)
                {
                    expression.By(filter.ColumnName, Operation.GreaterThanOrEqualTo, dateValue, Connector.And);
                    expression.By(filter.ColumnName, Operation.LessThan, dateValue.AddDays(1), Connector.And);
                }
                else if (filter.FilterType == FilterType.LessEqual)
                    expression.By(filter.ColumnName, Operation.LessThanOrEqualTo, dateValue.AddDays(1), Connector.And);
                else
                    expression.By(filter.ColumnName, operation, dateValue, Connector.And);
            }
            else
            {
                expression.By(filter.ColumnName, operation, value, Connector.And);
            }
        }

        return expression;

        static IOperation GetOperation(FilterType filterType) => filterType switch
        {
            FilterType.Equal => Operation.EqualTo,
            FilterType.NotEqual => Operation.NotEqualTo,
            FilterType.Greater => Operation.GreaterThan,
            FilterType.GreaterEqual => Operation.GreaterThanOrEqualTo,
            FilterType.Less => Operation.LessThan,
            FilterType.LessEqual => Operation.LessThanOrEqualTo,
            FilterType.Contains => Operation.Contains,
            FilterType.NotContains => Operation.DoesNotContain,
            FilterType.StartsWith => Operation.StartsWith,
            FilterType.EndsWith => Operation.EndsWith,
            _ => Operation.EqualTo,
        };
    }

    /// <summary>
    /// If <see cref="Sort"/> property is not null or empty apply sorting with <see cref="Sort"/> and <see cref="SortType"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    public IQueryable<TEntity> ApplySort<TEntity>(IQueryable<TEntity> query)
    {
        if (string.IsNullOrEmpty(Sort))
            return query;

        return SortType switch
        {
            SortType.Asc => query.OrderBy(CommonHelper.CreateOrderByKeySelector<TEntity>(Sort)),
            SortType.Desc => query.OrderByDescending(CommonHelper.CreateOrderByKeySelector<TEntity>(Sort)),
            _ => query,
        };
    }

}
