using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.DataAccess.EfCore.DbContextBase;

namespace Milvasoft.DataAccess.EfCore.Bulk.DbContextBase;

/// <summary>
/// Handles all database operations with new features like soft deletion.
/// </summary>
/// <param name="options"></param>
public abstract class MilvaBulkDbContext(DbContextOptions options) : MilvaDbContext(options), IMilvaBulkDbContextBase
{
    #region SaveChanges Overrides

    /// <summary>
    /// Overrided the BulkSaveChanges method for soft deleting.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public virtual void SaveChangesBulk(BulkConfig bulkConfig = null)
    {
        AuditEntites();

        this.BulkSaveChanges(bulkConfig);
    }

    /// <summary>
    /// Overrided the BulkSaveChangesAsync method for soft deleting.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken())
    {
        AuditEntites();

        await this.BulkSaveChangesAsync(bulkConfig, cancellationToken: cancellationToken);
    }

    #endregion
}