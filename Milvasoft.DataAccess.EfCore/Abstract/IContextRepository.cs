using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.EntityBases.Abstract;

namespace Milvasoft.DataAccess.EfCore.Abstract;

/// <summary>
/// Helper repository for general DbContext(<typeparamref name="TContext"/>) operations.
/// </summary>
public interface IContextRepository<TContext> where TContext : DbContext
{
    /// <summary>
    /// Ignores soft delete for next process. Runs correctly, if <typeparamref name="TContext"/> inherit from MilvaDbContext.
    /// </summary>
    void IgnoreSoftDeleteForNextProcess();

    /// <summary>
    /// Activate soft delete. Runs correctly, if <typeparamref name="TContext"/> inherit from MilvaDbContext.
    /// </summary>
    void ActivateSoftDelete();

    /// <summary>
    /// Executes sql query to database asynchronously.(e.g. trigger, event)
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    Task ExecuteQueryAsync(string query);

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

    /// <summary>
    /// User update process.
    /// </summary>
    void InitializeUpdating<TEntity, TKey>(TEntity entity) where TEntity : class, IBaseEntity<TKey>
                                                           where TKey : struct, IEquatable<TKey>;

    /// <summary>
    /// Gets requested DbSet by <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class;

    /// <summary>
    /// Returns whether or not the entity that satisfies this condition exists. 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> ExistsAsync<TEntity, TKey>(TKey id) where TEntity : class, IBaseEntity<TKey> where TKey : struct, IEquatable<TKey>;
}
