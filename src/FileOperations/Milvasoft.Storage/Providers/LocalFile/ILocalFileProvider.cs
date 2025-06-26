using Milvasoft.Storage.Abstract;
using Milvasoft.Storage.Models;

namespace Milvasoft.Storage.Providers.LocalFile;

/// <summary>
/// File provider.
/// </summary>
public interface ILocalFileProvider : IStorageProvider
{
    /// <summary>
    /// Remove all files in provider.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public FileOperationResult ClearFileSource(CancellationToken cancellationToken = default);
}
