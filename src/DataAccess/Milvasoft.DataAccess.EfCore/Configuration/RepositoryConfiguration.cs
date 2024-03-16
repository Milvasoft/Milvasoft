using Microsoft.EntityFrameworkCore;
using Milvasoft.Helpers.DataAccess.EfCore.Concrete;

namespace Milvasoft.DataAccess.EfCore.Configuration;

/// <summary>
/// <see cref="BaseRepository{TEntity, TContext}"/> configuration.
/// </summary>
public class RepositoryConfiguration
{
    /// <summary>
    /// All repository methods save changes(<see cref="DbContext.SaveChangesAsync(CancellationToken)"/>) after every operation. Except get operations. 
    /// </summary>
    public SaveChangesChoice DefaultSaveChangesChoice { get; set; } = SaveChangesChoice.AfterEveryOperation;

    /// <summary>
    /// Soft delete state. Default value is false.
    /// </summary>
    public bool DefaultSoftDeletedFetchState { get; set; } = false;

    /// <summary>
    /// Soft delete state. Default value is false.
    /// </summary>
    public bool ResetSoftDeletedFetchStateToDefault { get; set; } = true;
}