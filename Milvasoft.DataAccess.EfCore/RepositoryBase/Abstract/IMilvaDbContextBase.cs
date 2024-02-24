using EFCore.BulkExtensions;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.Utils.Enums;

namespace Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;

/// <summary>
/// Interface for base repository.
/// </summary>
public interface IMilvaDbContextBase
{
    /// <summary>
    /// Overrided the BulkSaveChanges method for soft deleting.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    void SaveChangesBulk(BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken());

    /// <summary>
    /// Overrided the BulkSaveChangesAsync method for soft deleting.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken());

    /// <summary>
    /// Sets milva db context specific configuration.
    /// </summary>
    /// <param name="dbContextConfiguration"></param>
    public void SetDataAccessConfiguration(IDataAccessConfiguration dbContextConfiguration);

    /// <summary>
    /// Gets milva db context specific configuration.
    /// </summary>
    public IDataAccessConfiguration GetDataAccessConfiguration();

    /// <summary>
    /// Gets current soft deletion state.
    /// </summary>
    /// <returns></returns>
    SoftDeletionState GetCurrentSoftDeletionState();
}