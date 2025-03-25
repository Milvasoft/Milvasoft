using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.DbContextBase;

namespace Milvasoft.DataAccess.EfCore.Bulk.DbContextBase;

/// <summary>
/// Handles all database operations with new features like soft deletion.
/// </summary>
/// <param name="options"></param>
public abstract class MilvaBulkDbContext(DbContextOptions options) : MilvaDbContext(options), IMilvaBulkDbContextBase
{
    /// <summary>
    /// Initializes new instance.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="dbContextConfiguration"></param>
    protected MilvaBulkDbContext(DbContextOptions options, IDataAccessConfiguration dbContextConfiguration) : this(options)
    {
        SetDataAccessConfiguration(dbContextConfiguration);
    }

    /// <summary>
    /// Initializes new instance.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="dbContextConfiguration"></param>
    /// <param name="serviceProvider"></param>
    protected MilvaBulkDbContext(DbContextOptions options, IDataAccessConfiguration dbContextConfiguration, IServiceProvider serviceProvider) : this(options)
    {
        SetDataAccessConfiguration(dbContextConfiguration);
        ServiceProvider = serviceProvider;
    }

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
    public virtual Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken())
    {
        AuditEntites();

        return this.BulkSaveChangesAsync(bulkConfig, cancellationToken: cancellationToken);
    }

    #endregion
}