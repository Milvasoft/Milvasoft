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
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <returns></returns>
    public Task ApplyTransactionAsync(Func<Task> function, bool startTransaction = true, CancellationToken cancellationToken = default)
        => startTransaction
            ? ExecuteInTransactionAsync(async () => { await function(); return true; }, null, cancellationToken)
            : function();

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="function"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <returns></returns>
    public Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, bool startTransaction = true, CancellationToken cancellationToken = default)
        => startTransaction
            ? ExecuteInTransactionAsync(function, null, cancellationToken)
            : function();

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"> Runs after the transaction is rolled back. </param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <returns></returns>
    public Task ApplyTransactionAsync(Func<Task> function, Func<Task> rollbackFunction, bool startTransaction = true, CancellationToken cancellationToken = default)
        => startTransaction
            ? ExecuteInTransactionAsync(async () => { await function(); return true; }, rollbackFunction, cancellationToken)
            : function();

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"> Runs after the transaction is rolled back. </param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <returns></returns>
    public Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, Func<Task> rollbackFunction, bool startTransaction = true, CancellationToken cancellationToken = default)
        => startTransaction
            ? ExecuteInTransactionAsync(function, rollbackFunction, cancellationToken)
            : function();

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"> Runs after the transaction is rolled back. </param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <returns></returns>
    public Task ApplyTransactionAsync(Func<Task> function, Action rollbackFunction, bool startTransaction = true, CancellationToken cancellationToken = default)
        => startTransaction
            ? ExecuteInTransactionAsync(async () => { await function(); return true; }, ToAsync(rollbackFunction), cancellationToken)
            : function();

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"> Runs after the transaction is rolled back. </param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <returns></returns>
    public Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, Action rollbackFunction, bool startTransaction = true, CancellationToken cancellationToken = default)
        => startTransaction
            ? ExecuteInTransactionAsync(function, ToAsync(rollbackFunction), cancellationToken)
            : function();

    /// <summary>
    /// Runs <paramref name="function"/> in a transaction with the execution strategy of the context.
    /// The transaction is always disposed. If <paramref name="function"/> or commit fails, the transaction is rolled back,
    /// <paramref name="rollbackFunction"/> runs and the original exception is rethrown (a failing rollback does not hide it).
    /// </summary>
    private Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> function, Func<Task> rollbackFunction, CancellationToken cancellationToken)
    {
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

        return executionStrategy.ExecuteAsync(async cancellation =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellation);

            try
            {
                var result = await function();

                await transaction.CommitAsync(cancellation);

                return result;
            }
            catch (Exception)
            {
                try
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                }
                catch (Exception)
                {
                    // The original exception is more relevant. Disposing the transaction releases it anyway.
                }

                if (rollbackFunction is not null)
                    await rollbackFunction();

                throw;
            }
        }, cancellationToken);
    }

    private static Func<Task> ToAsync(Action rollbackFunction) => rollbackFunction is null ? null : () =>
    {
        rollbackFunction();
        return Task.CompletedTask;
    };

    /// <summary>
    /// Gets requested DbSet by <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class => _dbContext.Set<TEntity>();
}
