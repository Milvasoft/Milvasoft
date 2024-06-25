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
    /// </summary>
    /// <param name="hasMetadataObject">The object to generate metadata for.</param>
    /// <returns>The list of generated metadata.</returns>
    public void GenerateMetadata(IHasMetadata hasMetadataObject);
}