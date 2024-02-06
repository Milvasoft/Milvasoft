using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using ExpressionBuilder.Interfaces;
using ExpressionBuilder.Operations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Core;
using Milvasoft.Core.Extensions;
using System.Linq.Expressions;

namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// Filter specs.
/// </summary>
public class FilterRequest
{
    /// <summary>
    /// Filter details
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
            var property = typeof(TEntity).ThrowIfPropertyNotExists(filter.FilterBy);

            var propertyType = property.PropertyType;

            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                propertyType = propertyType.GetGenericArguments()[0];

            var operation = GetOperation(filter.FilterType);

            object value;

            if (propertyType == typeof(Guid))
                value = Guid.Parse(filter.Value.ToString());
            else
                value = Convert.ChangeType(filter.Value, propertyType);

            if (propertyType == typeof(DateTime))
            {
                var dateValue = ((DateTime)value).Date;

                if (filter.FilterType == FilterType.Equal)
                {
                    expression.By(filter.FilterBy, Operation.GreaterThanOrEqualTo, dateValue, Connector.And);
                    expression.By(filter.FilterBy, Operation.LessThan, dateValue.AddDays(1), Connector.And);
                }
                else if (filter.FilterType == FilterType.LessEqual)
                    expression.By(filter.FilterBy, Operation.LessThanOrEqualTo, dateValue.AddDays(1), Connector.And);
                else
                    expression.By(filter.FilterBy, operation, dateValue, Connector.And);
            }
            else
            {
                expression.By(filter.FilterBy, operation, value, Connector.And);
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
}
