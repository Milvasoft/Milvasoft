using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.Utils.Constants;
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
    /// Builds the cursor filter. Primary comparison is prop &gt; cursor (forward+asc / backward+desc) or
    /// prop &lt; cursor (forward+desc / backward+asc); useGreaterThan = (isAscending != isBackward). When the
    /// cursor also carries the primary-key (Id) value it is used as a tie-break — primaryStrict OR
    /// (prop == value AND idStrict) — so rows sharing the same SortBy value are neither skipped nor duplicated
    /// across pages. Legacy cursors without an Id value fall back to the single-column filter.
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
        var rawValue = System.Text.Json.JsonSerializer.Deserialize(cursorData.Value, meta.UnderlyingType);

        var strictBody = BuildComparison(propertyAccess, meta, rawValue, useGreaterThan);

        // Tie-break on the primary key so equal SortBy values keep a total order. Skipped when the cursor
        // predates this change (no Id value) or when SortBy already is the primary key (comparison already total).
        var idMeta = string.Equals(cursorData.SortBy, EntityPropertyNames.Id, StringComparison.OrdinalIgnoreCase)
            ? null
            : GetCursorPropertyMetadata(typeof(TEntity), EntityPropertyNames.Id);

        if (!string.IsNullOrEmpty(cursorData.IdValue) && idMeta != null)
        {
            var idAccess = Expression.Property(param, idMeta.Property);
            var idRawValue = System.Text.Json.JsonSerializer.Deserialize(cursorData.IdValue, idMeta.UnderlyingType);
            var idStrictBody = BuildComparison(idAccess, idMeta, idRawValue, useGreaterThan);
            var equalBody = BuildEquality(propertyAccess, meta, rawValue);

            // primaryStrict OR (primaryEqual AND idStrict)
            var composite = Expression.OrElse(strictBody, Expression.AndAlso(equalBody, idStrictBody));

            return query.Where(Expression.Lambda<Func<TEntity, bool>>(composite, param));
        }

        return query.Where(Expression.Lambda<Func<TEntity, bool>>(strictBody, param));
    }

    /// <summary>Builds a strict greater-than / less-than comparison against a boxed value, handling string (ordinal) and (nullable) value types.</summary>
    private static Expression BuildComparison(MemberExpression propertyAccess, CursorPropertyMetadata meta, object rawValue, bool useGreaterThan)
    {
        if (meta.UnderlyingType == typeof(string))
        {
            var compareMethod = typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string), typeof(StringComparison)]);
            var call = Expression.Call(compareMethod, propertyAccess, Expression.Constant(rawValue, typeof(string)), Expression.Constant(StringComparison.Ordinal));

            return useGreaterThan
                ? Expression.GreaterThan(call, Expression.Constant(0))
                : Expression.LessThan(call, Expression.Constant(0));
        }

        var valueExpr = meta.IsNullable
            ? (Expression)Expression.Convert(Expression.Constant(rawValue, meta.UnderlyingType), meta.Property.PropertyType)
            : Expression.Constant(rawValue, meta.Property.PropertyType);

        return useGreaterThan
            ? Expression.GreaterThan(propertyAccess, valueExpr)
            : Expression.LessThan(propertyAccess, valueExpr);
    }

    /// <summary>Builds an equality comparison against a boxed value, handling string (ordinal) and (nullable) value types.</summary>
    private static Expression BuildEquality(MemberExpression propertyAccess, CursorPropertyMetadata meta, object rawValue)
    {
        if (meta.UnderlyingType == typeof(string))
        {
            var compareMethod = typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string), typeof(StringComparison)]);
            var call = Expression.Call(compareMethod, propertyAccess, Expression.Constant(rawValue, typeof(string)), Expression.Constant(StringComparison.Ordinal));

            return Expression.Equal(call, Expression.Constant(0));
        }

        var valueExpr = meta.IsNullable
            ? (Expression)Expression.Convert(Expression.Constant(rawValue, meta.UnderlyingType), meta.Property.PropertyType)
            : Expression.Constant(rawValue, meta.Property.PropertyType);

        return Expression.Equal(propertyAccess, valueExpr);
    }

    /// <summary>
    /// Applies the cursor ordering: the requested sort followed by a primary-key (Id) tie-break in the same
    /// direction, so equal SortBy values have a stable, total order (matching <see cref="ApplyCursorCondition"/>).
    /// Skipped tie-break when sorting already is by the primary key or the entity has no Id property.
    /// </summary>
    internal static IQueryable<TEntity> ApplyCursorSorting<TEntity>(IQueryable<TEntity> query, SortRequest sorting) where TEntity : class
    {
        var primaryExpression = sorting?.BuildPropertySelectorExpression<TEntity>();

        if (primaryExpression == null)
            return query;

        var sortingById = string.Equals(sorting.SortBy, EntityPropertyNames.Id, StringComparison.OrdinalIgnoreCase);

        // Only tie-break when the entity actually has an Id property (guards against keyless/Id-less entities).
        var idExpression = sortingById || GetCursorPropertyMetadata(typeof(TEntity), EntityPropertyNames.Id) == null
            ? null
            : new SortRequest { SortBy = EntityPropertyNames.Id, Type = sorting.Type }.BuildPropertySelectorExpression<TEntity>();

        if (sorting.Type == SortType.Asc)
        {
            var ordered = query.OrderBy(primaryExpression);

            return idExpression == null ? ordered : ordered.ThenBy(idExpression);
        }
        else
        {
            var ordered = query.OrderByDescending(primaryExpression);

            return idExpression == null ? ordered : ordered.ThenByDescending(idExpression);
        }
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

        var bindings = new List<MemberBinding>
        {
            Expression.Bind(carrierType.GetProperty(nameof(CursorProjectionCarrier<>.SortValue))!, sortValueAccess),
            Expression.Bind(carrierType.GetProperty(nameof(CursorProjectionCarrier<>.Result))!, reboundProjectionBody),
        };

        // Carry the primary-key (Id) tie-break value so the built cursor can disambiguate equal sort values.
        var idMeta = GetCursorPropertyMetadata(typeof(TEntity), EntityPropertyNames.Id);

        if (idMeta != null)
        {
            var idValueAccess = Expression.Convert(Expression.Property(param, idMeta.Property), typeof(object));

            bindings.Add(Expression.Bind(carrierType.GetProperty(nameof(CursorProjectionCarrier<>.IdValue))!, idValueAccess));
        }

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

        return CursorData.Encode(meta.Property.GetValue(item), GetTieBreakValue(typeof(TEntity), sorting.SortBy, item), sorting.SortBy, sorting.Type, isBackward);
    }

    internal static string BuildCursorFromBoxedValue(object sortValue, object idValue, SortRequest sorting, bool isBackward)
    {
        if (sortValue == null || string.IsNullOrWhiteSpace(sorting?.SortBy))
            return null;

        return CursorData.Encode(sortValue, idValue, sorting.SortBy, sorting.Type, isBackward);
    }

    /// <summary>Reads the primary-key (Id) tie-break value from an entity; null when SortBy already is the primary key or the entity has no Id.</summary>
    private static object GetTieBreakValue(Type entityType, string sortBy, object item)
    {
        if (string.Equals(sortBy, EntityPropertyNames.Id, StringComparison.OrdinalIgnoreCase))
            return null;

        var idMeta = GetCursorPropertyMetadata(entityType, EntityPropertyNames.Id);

        return idMeta?.Property.GetValue(item);
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

/// <summary>Carrier projected in a single EF query: sort value + primary-key tie-break value (for cursor building) + projected result.</summary>
internal sealed class CursorProjectionCarrier<TResult>
{
    public object SortValue { get; set; }
    public object IdValue { get; set; }
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

    /// <summary>JSON-serialized primary-key (Id) value of the boundary row; a tie-break so equal SortBy values are not skipped or duplicated. Null on legacy cursors (single-column behavior).</summary>
    public string IdValue { get; init; }

    public static string Encode(object lastSortValue, object lastIdValue, string sortBy, SortType sortType, bool isBackward)
    {
        var data = new CursorData
        {
            SortBy = sortBy,
            Value = System.Text.Json.JsonSerializer.Serialize(lastSortValue),
            SortType = sortType,
            IsBackward = isBackward,
            IdValue = lastIdValue != null ? System.Text.Json.JsonSerializer.Serialize(lastIdValue) : null
        };

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(data)));
    }

    public static CursorData Decode(string cursor)
    {
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
        return System.Text.Json.JsonSerializer.Deserialize<CursorData>(json);
    }
}
