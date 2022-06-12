using System;
using System.Threading.Tasks;

namespace Milvasoft.DataAccess.EfCore.Abstract;

/// <summary>
/// Abstraction for unit of work pattern.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Asynchronously sends all changes to the data store and commits as a single transaction unless a manual transaction was created using BeginTransaction().
    /// </summary>
    Task CommitAsync();
}
