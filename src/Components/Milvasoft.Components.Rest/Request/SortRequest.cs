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
    /// Builds property selector by <see cref="SortBy"/>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public Expression<Func<TEntity, object>> BuildPropertySelectorExpression<TEntity>()
        => !string.IsNullOrWhiteSpace(SortBy) ? CommonHelper.CreateRequiredPropertySelector<TEntity>(SortBy) : null;

}
