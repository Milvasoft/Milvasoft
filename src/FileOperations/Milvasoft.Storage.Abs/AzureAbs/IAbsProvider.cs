using Milvasoft.Storage.Abstract;
using Milvasoft.Storage.Models;

namespace Milvasoft.Storage.Abs.AzureAbs;

/// <summary>
/// Amazon ABS provider.
/// </summary>
public interface IAbsProvider : IStorageProvider
{
    /// <summary>
    /// Remove all files in azure container.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<FileOperationResult> ClearContainerAsync(CancellationToken cancellationToken = default);
}
