namespace Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;

/// <summary>
/// Helper repository for general DbContext operations.
/// </summary>
public interface IContextRepository
{
    /// <summary>
    /// Changes soft deletion state.
    /// </summary>
    void ChangeSoftDeletionState(SoftDeletionState state);

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
    Task ApplyTransactionAsync(Func<Task> function, bool startTransaction = true, CancellationToken cancellationToken = default, bool clearChangeTrackerOnRetry = false);

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
    Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, bool startTransaction = true, CancellationToken cancellationToken = default, bool clearChangeTrackerOnRetry = false);

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
    /// <param name="rollbackFunction"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <param name="clearChangeTrackerOnRetry"> When true, the change tracker is cleared before each retry attempt, so entities tracked by a failed attempt are not saved again. Default is false. </param>
    /// <returns></returns>
    Task ApplyTransactionAsync(Func<Task> function, Action rollbackFunction, bool startTransaction = true, CancellationToken cancellationToken = default, bool clearChangeTrackerOnRetry = false);

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
    /// <param name="rollbackFunction"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <param name="clearChangeTrackerOnRetry"> When true, the change tracker is cleared before each retry attempt, so entities tracked by a failed attempt are not saved again. Default is false. </param>
    /// <returns></returns>
    Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, Action rollbackFunction, bool startTransaction = true, CancellationToken cancellationToken = default, bool clearChangeTrackerOnRetry = false);

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
    /// <param name="rollbackFunction"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <param name="clearChangeTrackerOnRetry"> When true, the change tracker is cleared before each retry attempt, so entities tracked by a failed attempt are not saved again. Default is false. </param>
    /// <returns></returns>
    Task ApplyTransactionAsync(Func<Task> function, Func<Task> rollbackFunction, bool startTransaction = true, CancellationToken cancellationToken = default, bool clearChangeTrackerOnRetry = false);

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
    /// <param name="rollbackFunction"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <param name="cancellationToken"> Cancels starting and committing the transaction. </param>
    /// <param name="clearChangeTrackerOnRetry"> When true, the change tracker is cleared before each retry attempt, so entities tracked by a failed attempt are not saved again. Default is false. </param>
    /// <returns></returns>
    Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, Func<Task> rollbackFunction, bool startTransaction = true, CancellationToken cancellationToken = default, bool clearChangeTrackerOnRetry = false);
}
