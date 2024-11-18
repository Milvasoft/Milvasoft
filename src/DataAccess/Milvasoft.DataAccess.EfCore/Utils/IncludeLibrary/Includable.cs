using Microsoft.EntityFrameworkCore.Query;

namespace Milvasoft.DataAccess.EfCore.Utils.IncludeLibrary;

/// <summary>
/// Supports queryable Include/ThenInclude chaining operators. 
/// </summary>
public interface IIncludable;

/// <summary>
/// Supports queryable Include/ThenInclude chaining operators.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IIncludable<out TEntity> : IIncludable;

/// <summary>
/// Supports queryable Include/ThenInclude chaining operators.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TProperty"></typeparam>
public interface IIncludable<out TEntity, out TProperty> : IIncludable<TEntity>;

/// <summary>
/// Supports queryable Include/ThenInclude chaining operators.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
internal class Includable<TEntity> : IIncludable<TEntity> where TEntity : class
{
    internal IQueryable<TEntity> Input { get; }

    internal Includable(IQueryable<TEntity> queryable)
    {
        // C# 7 syntax, just rewrite it "old style" if you do not have Visual Studio 2017
        Input = queryable ?? throw new ArgumentNullException(nameof(queryable));
    }
}

/// <summary>
/// Supports queryable Include/ThenInclude chaining operators.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TProperty"></typeparam>
internal class Includable<TEntity, TProperty> :
    Includable<TEntity>, IIncludable<TEntity, TProperty>
    where TEntity : class
{
    internal IIncludableQueryable<TEntity, TProperty> IncludableInput { get; }

    internal Includable(IIncludableQueryable<TEntity, TProperty> queryable) : base(queryable)
    {
        IncludableInput = queryable;
    }
}
