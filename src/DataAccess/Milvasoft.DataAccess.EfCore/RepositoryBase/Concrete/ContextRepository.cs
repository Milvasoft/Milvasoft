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
    /// <remarks>
    /// The function runs with the execution strategy of the context. With a retrying strategy (e.g. <c>EnableRetryOnFailure</c>) a failed attempt is
    /// rolled back and the whole function runs again, so it must be self-contained and idempotent: read the data it needs inside the function
    /// and do not depend on state changed by a previous attempt. Entities tracked by a failed attempt stay in the change tracker unless
    /// <c>clearChangeTrackerOnRetry</c> is true (it detaches every tracked entity of the context, also the ones tracked before the call).
    /// </remarks>
    /// <param name="function"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <param name="clearChangeTrackerOnRetry"> When true, the change tracker is cleared before each retry attempt, so entities tracked by a failed attempt are not saved again. Default is false. </param>
    /// <returns></returns>
    public Task ApplyTransactionAsync(Func<Task> function, bool startTransaction = true, CancellationToken cancellationToken = default, bool clearChangeTrackerOnRetry = false)
        => startTransaction
            ? ExecuteInTransactionAsync(async () => { await function(); return true; }, null, clearChangeTrackerOnRetry, cancellationToken)
            : function();

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <remarks>
    /// The function runs with the execution strategy of the context. With a retrying strategy (e.g. <c>EnableRetryOnFailure</c>) a failed attempt is
    /// rolled back and the whole function runs again, so it must be self-contained and idempotent: read the data it needs inside the function
    /// and do not depend on state changed by a previous attempt. Entities tracked by a failed attempt stay in the change tracker unless
    /// <c>clearChangeTrackerOnRetry</c> is true (it detaches every tracked entity of the context, also the ones tracked before the call).
    /// </remarks>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="function"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <param name="clearChangeTrackerOnRetry"> When true, the change tracker is cleared before each retry attempt, so entities tracked by a failed attempt are not saved again. Default is false. </param>
    /// <returns></returns>
    public Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, bool startTransaction = true, CancellationToken cancellationToken = default, bool clearChangeTrackerOnRetry = false)
        => startTransaction
            ? ExecuteInTransactionAsync(function, null, clearChangeTrackerOnRetry, cancellationToken)
            : function();

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <remarks>
    /// The function runs with the execution strategy of the context. With a retrying strategy (e.g. <c>EnableRetryOnFailure</c>) a failed attempt is
    /// rolled back and the whole function runs again, so it must be self-contained and idempotent: read the data it needs inside the function
    /// and do not depend on state changed by a previous attempt. Entities tracked by a failed attempt stay in the change tracker unless
    /// <c>clearChangeTrackerOnRetry</c> is true (it detaches every tracked entity of the context, also the ones tracked before the call).
    /// </remarks>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"> Runs after the transaction is rolled back. </param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <param name="clearChangeTrackerOnRetry"> When true, the change tracker is cleared before each retry attempt, so entities tracked by a failed attempt are not saved again. Default is false. </param>
    /// <returns></returns>
    public Task ApplyTransactionAsync(Func<Task> function, Func<Task> rollbackFunction, bool startTransaction = true, CancellationToken cancellationToken = default, bool clearChangeTrackerOnRetry = false)
        => startTransaction
            ? ExecuteInTransactionAsync(async () => { await function(); return true; }, rollbackFunction, clearChangeTrackerOnRetry, cancellationToken)
            : function();

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <remarks>
    /// The function runs with the execution strategy of the context. With a retrying strategy (e.g. <c>EnableRetryOnFailure</c>) a failed attempt is
    /// rolled back and the whole function runs again, so it must be self-contained and idempotent: read the data it needs inside the function
    /// and do not depend on state changed by a previous attempt. Entities tracked by a failed attempt stay in the change tracker unless
    /// <c>clearChangeTrackerOnRetry</c> is true (it detaches every tracked entity of the context, also the ones tracked before the call).
    /// </remarks>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"> Runs after the transaction is rolled back. </param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <param name="clearChangeTrackerOnRetry"> When true, the change tracker is cleared before each retry attempt, so entities tracked by a failed attempt are not saved again. Default is false. </param>
    /// <returns></returns>
    public Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, Func<Task> rollbackFunction, bool startTransaction = true, CancellationToken cancellationToken = default, bool clearChangeTrackerOnRetry = false)
        => startTransaction
            ? ExecuteInTransactionAsync(function, rollbackFunction, clearChangeTrackerOnRetry, cancellationToken)
            : function();

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <remarks>
    /// The function runs with the execution strategy of the context. With a retrying strategy (e.g. <c>EnableRetryOnFailure</c>) a failed attempt is
    /// rolled back and the whole function runs again, so it must be self-contained and idempotent: read the data it needs inside the function
    /// and do not depend on state changed by a previous attempt. Entities tracked by a failed attempt stay in the change tracker unless
    /// <c>clearChangeTrackerOnRetry</c> is true (it detaches every tracked entity of the context, also the ones tracked before the call).
    /// </remarks>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"> Runs after the transaction is rolled back. </param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <param name="clearChangeTrackerOnRetry"> When true, the change tracker is cleared before each retry attempt, so entities tracked by a failed attempt are not saved again. Default is false. </param>
    /// <returns></returns>
    public Task ApplyTransactionAsync(Func<Task> function, Action rollbackFunction, bool startTransaction = true, CancellationToken cancellationToken = default, bool clearChangeTrackerOnRetry = false)
        => startTransaction
            ? ExecuteInTransactionAsync(async () => { await function(); return true; }, ToAsync(rollbackFunction), clearChangeTrackerOnRetry, cancellationToken)
            : function();

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <remarks>
    /// The function runs with the execution strategy of the context. With a retrying strategy (e.g. <c>EnableRetryOnFailure</c>) a failed attempt is
    /// rolled back and the whole function runs again, so it must be self-contained and idempotent: read the data it needs inside the function
    /// and do not depend on state changed by a previous attempt. Entities tracked by a failed attempt stay in the change tracker unless
    /// <c>clearChangeTrackerOnRetry</c> is true (it detaches every tracked entity of the context, also the ones tracked before the call).
    /// </remarks>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"> Runs after the transaction is rolled back. </param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <param name="clearChangeTrackerOnRetry"> When true, the change tracker is cleared before each retry attempt, so entities tracked by a failed attempt are not saved again. Default is false. </param>
    /// <returns></returns>
    public Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, Action rollbackFunction, bool startTransaction = true, CancellationToken cancellationToken = default, bool clearChangeTrackerOnRetry = false)
        => startTransaction
            ? ExecuteInTransactionAsync(function, ToAsync(rollbackFunction), clearChangeTrackerOnRetry, cancellationToken)
            : function();

    /// <summary>
    /// Runs <paramref name="function"/> in a transaction with the execution strategy of the context.
    /// The transaction is always disposed. If <paramref name="function"/> or commit fails, the transaction is rolled back,
    /// <paramref name="rollbackFunction"/> runs and the original exception is rethrown (a failing rollback does not hide it).
    /// If the execution strategy retries and <paramref name="clearChangeTrackerOnRetry"/> is true, the change tracker is cleared before each retry attempt.
    /// </summary>
    private Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> function, Func<Task> rollbackFunction, bool clearChangeTrackerOnRetry, CancellationToken cancellationToken)
    {
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

        var attempt = 0;

        return executionStrategy.ExecuteAsync(async cancellation =>
        {
            if (attempt++ > 0 && clearChangeTrackerOnRetry)
                _dbContext.ChangeTracker.Clear();

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
