using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Milvasoft.Storage.Providers;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace Milvasoft.Storage.Abs.AzureAbs;

/// <summary>
/// Write-only stream that uploads its content to an Azure block blob. Content up to one block is uploaded with a single request,
/// larger content is staged block by block and committed with a block list.
/// </summary>
/// <remarks>
/// Call <see cref="MultipartUploadStream.CompleteAsync(CancellationToken)"/> after writing to commit the blob, and dispose with
/// <c>await using</c>. Disposing without completing leaves the staged blocks uncommitted; Azure discards them after 7 days and the
/// blob is not changed.
/// </remarks>
public class AbsUploadStream : MultipartUploadStream
{
    /// <summary>
    /// Default block length.
    /// </summary>
    public const long DefaultPartLength = 8L * 1024 * 1024;

    /// <summary>
    /// Maximum block length accepted by Azure.
    /// </summary>
    public const long MaxPartLength = 4000L * 1024 * 1024;

    /// <summary>
    /// Maximum committed block count accepted by Azure.
    /// </summary>
    public const int MaxPartCount = 50000;

    /// <summary>
    /// Default number of blocks staged concurrently.
    /// </summary>
    public const int DefaultMaxConcurrentUploads = 4;

    private readonly BlockBlobClient _absClient;
    private readonly string _contentType;
    private readonly ConcurrentDictionary<int, string> _blockIds = new();

    /// <summary>
    /// Initializes a new instance of <see cref="AbsUploadStream"/>.
    /// </summary>
    /// <param name="absBlobClient">Block blob client of the target blob.</param>
    /// <param name="contentType">Content type of the blob.</param>
    /// <param name="partLength">Block length. Must be between 1 byte and <see cref="MaxPartLength"/>.</param>
    /// <param name="maxConcurrentUploads">Maximum number of blocks staged concurrently.</param>
    public AbsUploadStream(BlockBlobClient absBlobClient,
                           string contentType,
                           long partLength = DefaultPartLength,
                           int maxConcurrentUploads = DefaultMaxConcurrentUploads)
        : base(partLength, 1, MaxPartLength, MaxPartCount, maxConcurrentUploads)
    {
        _absClient = absBlobClient ?? throw new ArgumentNullException(nameof(absBlobClient));
        _contentType = contentType;
    }

    /// <inheritdoc/>
    protected override Task InitiateMultipartUploadAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc/>
    protected override async Task UploadPartAsync(int partNumber, Stream content, bool isLastPart, CancellationToken cancellationToken)
    {
        // All block ids of a blob must have the same length.
        var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(partNumber.ToString("d6", CultureInfo.InvariantCulture)));

        await _absClient.StageBlockAsync(blockId, content, cancellationToken: cancellationToken);

        _blockIds[partNumber] = blockId;
    }

    /// <inheritdoc/>
    protected override Task CompleteMultipartUploadAsync(CancellationToken cancellationToken)
        => _absClient.CommitBlockListAsync(_blockIds.OrderBy(b => b.Key).Select(b => b.Value),
                                           new BlobHttpHeaders { ContentType = _contentType },
                                           cancellationToken: cancellationToken);

    /// <inheritdoc/>
    protected override Task UploadSingleAsync(Stream content, CancellationToken cancellationToken)
        => _absClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = _contentType } }, cancellationToken);

    /// <inheritdoc/>
    protected override Task AbortMultipartUploadAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
