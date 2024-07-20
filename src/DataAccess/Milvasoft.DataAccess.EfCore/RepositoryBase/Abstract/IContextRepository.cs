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
    /// <param name="function"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <returns></returns>
    Task ApplyTransactionAsync(Func<Task> function, bool startTransaction = true);

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="function"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <returns></returns>
    Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, bool startTransaction = true);

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <returns></returns>
    Task ApplyTransactionAsync(Func<Task> function, Action rollbackFunction, bool startTransaction = true);

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <returns></returns>
    Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, Action rollbackFunction, bool startTransaction = true);

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <returns></returns>
    Task ApplyTransactionAsync(Func<Task> function, Func<Task> rollbackFunction, bool startTransaction = true);

    /// <summary>
    /// Applies transaction process to requested function.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="function"></param>
    /// <param name="rollbackFunction"></param>
    /// <param name="startTransaction"> When nested conditional transactions are desired, a transaction cannot be started for the transaction it contains. </param>
    /// <returns></returns>
    Task<TResult> ApplyTransactionAsync<TResult>(Func<Task<TResult>> function, Func<Task> rollbackFunction, bool startTransaction = true);
}
