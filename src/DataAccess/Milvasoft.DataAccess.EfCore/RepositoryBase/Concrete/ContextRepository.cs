using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.EntityBases.Abstract.Auditing;
using Milvasoft.Core.EntityBases.Concrete.Auditing;
using Milvasoft.Core.Extensions;
using Milvasoft.Core.MultiLanguage.EntityBases;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.DataAccess.EfCore.RepositoryBase.Concrete;

/// <summary>
/// Helper repository for general DbContext(<typeparamref name="TContext"/>) operations.
/// </summary>
/// <remarks>
/// Constructor of ContextRepository for inject context.
/// </remarks>
/// <param name="dbContext"></param>
public class ContextRepository<TContext>(TContext dbContext) : IContextRepository<TContext> where TContext : DbContext
{
    /// <summary>
    /// DbContext object.
    /// </summary>
    protected TContext _dbContext = dbContext;

    /// <summary>
    /// Ignores soft delete for next process for current <see cref="DbContext"/> instance. Runs correctly, if <typeparamref name="TContext"/> inherit from MilvaDbContext.
    /// </summary>
    public void IgnoreSoftDeleteForNextProcess()
        => _dbContext.GetType().GetMethod("IgnoreSoftDeleteForNextProcess")?.Invoke(_dbContext, null);

    /// <summary>
    /// Activate soft delete for current <see cref="DbContext"/> instance. Runs correctly, if <typeparamref name="TContext"/> inherit from MilvaDbContext.
    /// </summary>
    public void ActivateSoftDelete()
        => _dbContext.GetType().GetMethod("ActivateSoftDelete")?.Invoke(_dbContext, null);

    /// <summary>
    /// Executes sql query to database asynchronously.(e.g. trigger, event).
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public virtual async Task ExecuteQueryAsync(string query) => await _dbContext.Database.ExecuteSqlRawAsync(query).ConfigureAwait(false);

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <param name="function"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <returns></returns>
    public async Task ApplyTransactionAsync(Func<Task> function, bool startTransaction = true)
    {
        if (startTransaction)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

                try
                {
                    await function().ConfigureAwait(false);

                    await transaction.CommitAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    throw;
                }
            });
        }
        else
            await function().ConfigureAwait(false);
    }

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="function"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <returns></returns>
    public async Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, bool startTransaction = true)
    {
        if (startTransaction)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

                try
                {
                    var result = await function().ConfigureAwait(false);

                    await transaction.CommitAsync().ConfigureAwait(false);

                    return result;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    throw;
                }
            });
        }
        else
            return await function().ConfigureAwait(false);
    }

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <returns></returns>
    public async Task ApplyTransactionAsync(Func<Task> function, Func<Task> rollbackFunction, bool startTransaction = true)
    {
        if (startTransaction)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    await function().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    await rollbackFunction().ConfigureAwait(false);
                    throw;
                }
            });
        }
        else
            await function().ConfigureAwait(false);
    }

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <returns></returns>
    public async Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, Func<Task> rollbackFunction, bool startTransaction = true)
    {
        if (startTransaction)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    var result = await function().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);

                    return result;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    await rollbackFunction().ConfigureAwait(false);
                    throw;
                }
            });
        }
        else
            return await function().ConfigureAwait(false);
    }

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <returns></returns>
    public async Task ApplyTransactionAsync(Func<Task> function, Action rollbackFunction, bool startTransaction = true)
    {
        if (startTransaction)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    await function().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    rollbackFunction();
                    throw;
                }
            });
        }
        else
            await function().ConfigureAwait(false);
    }

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <returns></returns>
    public async Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, Action rollbackFunction, bool startTransaction = true)
    {
        if (startTransaction)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    var result = await function().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);

                    return result;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    rollbackFunction();
                    throw;
                }
            });
        }
        else
            return await function().ConfigureAwait(false);
    }

    /// <summary>
    /// User update process.
    /// </summary>
    public void InitializeUpdating<TEntity, TKey>(TEntity entity) where TEntity : class, IBaseEntity<TKey>
                                                  where TKey : struct, IEquatable<TKey>
    {
        var localEntity = _dbContext.Set<TEntity>().Local.FirstOrDefault(u => u.Id.Equals(entity.Id));

        if (localEntity != null)
        {
            _dbContext.Entry(localEntity).State = EntityState.Detached;
        }

        _dbContext.Entry(localEntity).State = EntityState.Modified;
    }

    /// <summary>
    /// Gets requested contents by <typeparamref name="TEntity"/> DbSet.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public async Task<List<TEntity>> GetRequiredContents<TEntity>() where TEntity : class => await _dbContext.Set<TEntity>().ToListAsync().ConfigureAwait(false);

    /// <summary>
    /// Gets requested contents by <paramref name="type"/> DbSet.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<object> GetRequiredContents(Type type)
    {
        var dbSet = GetType().GetMethod("Set").MakeGenericMethod(type).Invoke(this, null);

        var whereMethods = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(mi => mi.Name == "ToListAsync");

        var whereMethod = whereMethods.FirstOrDefault();

        whereMethod = whereMethod.MakeGenericMethod(type);

        var ret = (Task)whereMethod.Invoke(dbSet, new object[] { dbSet, null });
        await ret.ConfigureAwait(false);

        var resultProperty = ret.GetType().GetProperty("Result");
        return resultProperty.GetValue(ret);
    }

    /// <summary>
    /// Gets requested DbSet by <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class => _dbContext.Set<TEntity>();

    /// <summary>
    /// Returns whether or not the entity that satisfies this condition exists. 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual async Task<bool> ExistsAsync<TEntity, TKey>(TKey id)
         where TEntity : class, IBaseEntity<TKey>
         where TKey : struct, IEquatable<TKey>
    {
        var mainCondition = CreateKeyEqualityExpression<TEntity, TKey>(id);

        return await _dbContext.Set<TEntity>().SingleOrDefaultAsync(mainCondition).ConfigureAwait(false) != null;
    }

    private static Expression<Func<TEntity, bool>> CreateKeyEqualityExpression<TEntity, TKey>(TKey key, Expression<Func<TEntity, bool>> conditionExpression = null)
         where TEntity : class, IBaseEntity<TKey>
         where TKey : struct, IEquatable<TKey>
    {
        Expression<Func<TEntity, bool>> idCondition = i => i.Id.Equals(key);
        var mainCondition = idCondition.Append(CreateIsDeletedFalseExpression<TEntity>(), ExpressionType.AndAlso);
        return mainCondition.Append(conditionExpression, ExpressionType.AndAlso);
    }

    /// <summary>
    /// Gets <b>entity => entity.IsDeleted == false</b> expression, if <typeparamref name="TEntity"/> is assignable from <see cref="FullAuditableEntity{TKey}"/>.
    /// </summary>
    /// <returns></returns>
    private static Expression<Func<TEntity, bool>> CreateIsDeletedFalseExpression<TEntity>()
    {
        var entityType = typeof(TEntity);
        if (typeof(ISoftDeletable).IsAssignableFrom(entityType))
        {
            var parameter = Expression.Parameter(entityType, "entity");
            var filterExpression = Expression.Equal(Expression.Property(parameter, entityType.GetProperty(EntityPropertyNames.IsDeleted)), Expression.Constant(false, typeof(bool)));
            return Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
        }
        else
            return null;
    }
}
