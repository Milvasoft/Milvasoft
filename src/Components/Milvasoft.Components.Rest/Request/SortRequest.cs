using Milvasoft.Components.Rest.Enums;
using System.Linq.Expressions;

namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// Sort specs.
/// </summary>
public class SortRequest
{
    /// <summary>
    /// Sort column name
    /// </summary>
    /// <example>{columnName}</example>
    public virtual string SortBy { get; set; }

    /// <summary>
    /// Sort type
    /// Ascendind, Descending
    /// </summary>
    /// <example>Asc</example>
    public virtual SortType Type { get; set; }

    /// <summary>
    /// Builds an expression that selects the property to sort by based on the specified <see cref="SortBy"/> value.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>An expression that selects the property to order by.</returns>
    public Expression<Func<TEntity, object>> BuildPropertySelectorExpression<TEntity>()
        => !string.IsNullOrWhiteSpace(SortBy) ? CommonHelper.CreateRequiredPropertySelector<TEntity>(SortBy) : null;

}
