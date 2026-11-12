using Milvasoft.Components.Rest.MilvaResponse;

namespace Milvasoft.Interception.Interceptors.Response;

/// <summary>
/// Generates metadata for the response data.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ResponseMetadataGenerator"/> class.
/// </remarks>
public interface IResponseMetadataGenerator
{
    /// <summary>
    /// Generates metadata for the specified object.
    /// Asynchronous options fetchers are waited synchronously; prefer <see cref="GenerateMetadataAsync(IHasMetadata)"/>.
    /// </summary>
    /// <param name="hasMetadataObject">The object to generate metadata for.</param>
    public void GenerateMetadata(IHasMetadata hasMetadataObject);

    /// <summary>
    /// Generates metadata for the specified object. Asynchronous options fetchers are awaited.
    /// </summary>
    /// <param name="hasMetadataObject">The object to generate metadata for.</param>
    public Task GenerateMetadataAsync(IHasMetadata hasMetadataObject);
}
