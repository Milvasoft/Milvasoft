using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.DataAccess.EfCore.Utils;

internal static class CursorExtensions
{
    /// <summary>
    /// Returns a new <see cref="SortRequest"/> with the opposite sort direction, used for backward cursor traversal.
    /// </summary>
    internal static SortRequest ReversedSorting(SortRequest sorting)
        => new() { SortBy = sorting.SortBy, Type = sorting.Type == SortType.Asc ? SortType.Desc : SortType.Asc };

    // Per-entity-type sort property metadata; resolved once via reflection then cached.
    internal static readonly ConcurrentDictionary<(Type EntityType, string SortBy), CursorPropertyMetadata> _cursorMetadataCache = new();

    /// <summary>
    /// Builds cursor filter expression: prop > cursor (forward+asc / backward+desc) or prop &lt; cursor (forward+desc / backward+asc).
    /// Logic: useGreaterThan = (isAscending != isBackward)
    /// </summary>
    internal static IQueryable<TEntity> ApplyCursorCondition<TEntity>(IQueryable<TEntity> query, CursorData cursorData, bool isBackward) where TEntity : class
    {
        if (cursorData == null || string.IsNullOrWhiteSpace(cursorData.SortBy))
            return query;

        var meta = GetCursorPropertyMetadata(typeof(TEntity), cursorData.SortBy);

        if (meta == null)
            return query;

        var isAscending = cursorData.SortType == SortType.Asc;
        var useGreaterThan = isAscending != isBackward;

        var param = Expression.Parameter(typeof(TEntity), "e");
        var propertyAccess = Expression.Property(param, meta.Property);
        var underlyingType = meta.UnderlyingType;
        var rawValue = System.Text.Json.JsonSerializer.Deserialize(cursorData.Value, underlyingType);

        Expression body;

        if (underlyingType == typeof(string))
        {
            var compareMethod = typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string), typeof(StringComparison)]);
            var call = Expression.Call(compareMethod, propertyAccess, Expression.Constant(rawValue, typeof(string)), Expression.Constant(StringComparison.Ordinal));
            body = useGreaterThan
                ? Expression.GreaterThan(call, Expression.Constant(0))
                : Expression.LessThan(call, Expression.Constant(0));
        }
        else
        {
            var valueExpr = meta.IsNullable
                ? (Expression)Expression.Convert(Expression.Constant(rawValue, underlyingType), meta.Property.PropertyType)
                : Expression.Constant(rawValue, meta.Property.PropertyType);

            body = useGreaterThan
                ? Expression.GreaterThan(propertyAccess, valueExpr)
                : Expression.LessThan(propertyAccess, valueExpr);
        }

        return query.Where(Expression.Lambda<Func<TEntity, bool>>(body, param));
    }

    /// <summary>
    /// Builds a single EF-translatable SELECT that returns both the sort-property value and the projected result.
    /// Avoids a second round-trip: fetch entities → project in memory.
    /// </summary>
    internal static Expression<Func<TEntity, CursorProjectionCarrier<TResult>>> BuildCombinedProjection<TEntity, TResult>(CursorPropertyMetadata meta, Expression<Func<TEntity, TResult>> projection)
    {
        var param = Expression.Parameter(typeof(TEntity), "e");

        // box the sort property value → object
        var sortValueAccess = Expression.Convert(Expression.Property(param, meta.Property), typeof(object));

        // rebind projection to same parameter
        var reboundProjectionBody = new ParameterReplacer(projection.Parameters[0], param).Visit(projection.Body);

        var carrierType = typeof(CursorProjectionCarrier<TResult>);

        var bindings = new MemberBinding[]
        {
            Expression.Bind(carrierType.GetProperty(nameof(CursorProjectionCarrier<>.SortValue))!, sortValueAccess),
            Expression.Bind(carrierType.GetProperty(nameof(CursorProjectionCarrier<>.Result))!, reboundProjectionBody),
        };

        var body = Expression.MemberInit(Expression.New(carrierType), bindings);

        return Expression.Lambda<Func<TEntity, CursorProjectionCarrier<TResult>>>(body, param);
    }

    internal static string BuildCursor<TEntity>(TEntity item, SortRequest sorting, bool isBackward)
    {
        if (item == null || string.IsNullOrWhiteSpace(sorting?.SortBy))
            return null;

        var meta = GetCursorPropertyMetadata(typeof(TEntity), sorting.SortBy);

        if (meta == null)
            return null;

        return CursorData.Encode(meta.Property.GetValue(item), sorting.SortBy, sorting.Type, isBackward);
    }

    internal static string BuildCursorFromBoxedValue(object sortValue, SortRequest sorting, bool isBackward)
    {
        if (sortValue == null || string.IsNullOrWhiteSpace(sorting?.SortBy))
            return null;

        return CursorData.Encode(sortValue, sorting.SortBy, sorting.Type, isBackward);
    }

    internal static CursorPropertyMetadata GetCursorPropertyMetadata(Type entityType, string sortBy) => _cursorMetadataCache.GetOrAdd((entityType, sortBy), key =>
    {
        var prop = key.EntityType.GetProperty(key.SortBy, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (prop == null)
            return null;

        var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
        return new CursorPropertyMetadata(prop, underlyingType, Nullable.GetUnderlyingType(prop.PropertyType) != null);
    });
}

/// <summary>Cached sort-property reflection data per (EntityType, SortBy) pair.</summary>
internal sealed record CursorPropertyMetadata(PropertyInfo Property, Type UnderlyingType, bool IsNullable);

/// <summary>Carrier projected in a single EF query: sort value (for cursor building) + projected result.</summary>
internal sealed class CursorProjectionCarrier<TResult>
{
    public object SortValue { get; set; }
    public TResult Result { get; set; }
}

/// <summary>Replaces a specific parameter expression node inside an expression tree.</summary>
internal sealed class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
{
    protected override Expression VisitParameter(ParameterExpression node)
        => node == oldParam ? newParam : base.VisitParameter(node);
}

internal sealed record CursorData
{
    public string SortBy { get; init; }
    public string Value { get; init; }
    public SortType SortType { get; init; }

    /// <summary>True when this cursor is meant to navigate to a previous page.</summary>
    public bool IsBackward { get; init; }

    public static string Encode(object lastSortValue, string sortBy, SortType sortType, bool isBackward)
    {
        var data = new CursorData
        {
            SortBy = sortBy,
            Value = System.Text.Json.JsonSerializer.Serialize(lastSortValue),
            SortType = sortType,
            IsBackward = isBackward
        };

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(data)));
    }

    public static CursorData Decode(string cursor)
    {
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
        return System.Text.Json.JsonSerializer.Deserialize<CursorData>(json);
    }
}