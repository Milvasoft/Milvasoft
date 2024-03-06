using ExpressionBuilder.Common;
using ExpressionBuilder.Generics;
using ExpressionBuilder.Interfaces;
using ExpressionBuilder.Operations;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Core;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.Extensions;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// Filter specs.
/// </summary>
public class FilterRequest
{
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

        var entityType = typeof(TEntity);

        PropertyInfo languagesPropertyInfo = null;

        if (entityType.IsAssignableTo(typeof(IHasMultiLanguage)))
            languagesPropertyInfo = entityType.GetProperty("Languages");

        foreach (var filter in Criterias)
        {
            var property = typeof(TEntity).GetProperty(filter.FilterBy);

            var operation = GetOperation(filter.FilterType);

            if (languagesPropertyInfo != null)
            {
                var propType = languagesPropertyInfo.PropertyType.GenericTypeArguments.FirstOrDefault();

                propType.ThrowIfPropertyNotExists(filter.FilterBy);

                expression.By($"Languages[{filter.FilterBy}]", operation, filter.Value, Connector.And);

                Expression<Func<TEntity, bool>> ex = expression;

                continue;
            }

            var propertyType = property.PropertyType;

            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                propertyType = propertyType.GetGenericArguments()[0];

            object value;

            if (propertyType == typeof(Guid))
                value = Guid.Parse(filter.Value);
            else if (propertyType.IsGenericType && typeof(IList).IsAssignableFrom(propertyType))
            {
                return expression;
            }
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
    }

    private static IOperation GetOperation(FilterType filterType) => filterType switch
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
