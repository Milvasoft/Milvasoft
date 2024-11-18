using EFCore.BulkExtensions;

namespace Milvasoft.DataAccess.EfCore.Bulk;

/// <summary>
/// Interface for base repository.
/// </summary>
public interface IMilvaBulkDbContextBase : IMilvaDbContextBase
{
    /// <summary>
    /// Overrided the BulkSaveChanges method for soft deleting.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    void SaveChangesBulk(BulkConfig bulkConfig = null);

    /// <summary>
    /// Overrided the BulkSaveChangesAsync method for soft deleting.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken());
}
