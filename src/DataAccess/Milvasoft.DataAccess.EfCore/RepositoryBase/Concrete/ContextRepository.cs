using Microsoft.EntityFrameworkCore;
using Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;

namespace Milvasoft.DataAccess.EfCore.RepositoryBase.Concrete;

/// <summary>
/// Helper repository for general DbContext(<typeparamref name="TContext"/>) operations.
/// </summary>
/// <remarks>
/// Constructor of ContextRepository for inject context.
/// </remarks>
/// <param name="dbContext"></param>
public class ContextRepository<TContext>(TContext dbContext) : IContextRepository where TContext : DbContext, IMilvaDbContextBase
{
    /// <summary>
    /// DbContext object.
    /// </summary>
    protected TContext _dbContext = dbContext;

    /// <summary>
    /// Changes soft deletion state.
    /// </summary>
    public void ChangeSoftDeletionState(SoftDeletionState state) => _dbContext.ChangeSoftDeletionState(state);

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
                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    await function();

                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        else
            await function();
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
                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var result = await function();

                    await transaction.CommitAsync();

                    return result;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        else
            return await function();
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
                var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    await function();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    await rollbackFunction();
                    throw;
                }
            });
        }
        else
            await function();
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
                var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var result = await function();
                    await transaction.CommitAsync();

                    return result;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    await rollbackFunction();
                    throw;
                }
            });
        }
        else
            return await function();
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
                var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    await function();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    rollbackFunction();
                    throw;
                }
            });
        }
        else
            await function();
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
                var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var result = await function();
                    await transaction.CommitAsync();

                    return result;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    rollbackFunction();
                    throw;
                }
            });
        }
        else
            return await function();
    }

    /// <summary>
    /// Gets requested DbSet by <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class => _dbContext.Set<TEntity>();
}
