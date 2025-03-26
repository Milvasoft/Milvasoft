using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Milvasoft.DataAccess.EfCore.Utils;

internal static class MilvaDbContextCache
{
    private static readonly ConcurrentDictionary<Type, LambdaExpression> _isDeletedExpressionCache = new();
    private static readonly ConcurrentDictionary<Type, HashSet<string>> _propertyNamesCache = new();

    /// <summary>
    /// Gets the filter expression for the IsDeleted property of the entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TEntity, bool>> GetIsDeletedFilter<TEntity>() => (Expression<Func<TEntity, bool>>)_isDeletedExpressionCache.GetOrAdd(typeof(TEntity), type =>
    {
        return CommonHelper.CreateIsDeletedFalseExpression<TEntity>() ?? (entity => true);
    });

    public static bool HasProperty(this EntityEntry entry, string propName)
    {
        var type = entry.Entity.GetType();

        var cached = _propertyNamesCache.GetOrAdd(type, t => [.. entry.Metadata.GetProperties().Select(p => p.Name)]);

        return cached.Contains(propName);
    }
}
