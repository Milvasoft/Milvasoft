using Amazon.S3;
using Amazon.S3.Model;
using Milvasoft.Storage.Providers;
using System.Collections.Concurrent;

namespace Milvasoft.Storage.S3;

/// <summary>
/// Write-only stream that uploads its content to S3. Content up to one part is sent with a single PutObject request,
/// larger content with a multipart upload (parts of at least 5 MiB except the last one, at most 10,000 parts).
/// </summary>
/// <remarks>
/// Call <see cref="MultipartUploadStream.CompleteAsync(CancellationToken)"/> after writing to commit the object, and dispose with
/// <c>await using</c>. Disposing without completing aborts the multipart upload so no orphan parts are left behind.
/// </remarks>
public class S3UploadStream : MultipartUploadStream
{
    /// <summary>
    /// Minimum S3 part size (except the last part).
    /// </summary>
    public const long MinPartLength = 5L * 1024 * 1024;

    /// <summary>
    /// Maximum S3 part size.
    /// </summary>
    public const long MaxPartLength = 5L * 1024 * 1024 * 1024;

    /// <summary>
    /// Maximum S3 part count.
    /// </summary>
    public const int MaxPartCount = 10000;

    /// <summary>
    /// Default number of parts uploaded concurrently.
    /// </summary>
    public const int DefaultMaxConcurrentUploads = 4;

    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _key;
    private readonly string _contentType;
    private readonly ConcurrentDictionary<int, PartETag> _partETags = new();
    private string _uploadId;

    /// <summary>
    /// Initializes a new instance of <see cref="S3UploadStream"/>.
    /// </summary>
    /// <param name="s3">S3 client.</param>
    /// <param name="bucket">Bucket name.</param>
    /// <param name="key">Object key.</param>
    /// <param name="contentType">Content type of the object. If null, S3 default is used.</param>
    /// <param name="partLength">Part length. Must be between <see cref="MinPartLength"/> and <see cref="MaxPartLength"/>.</param>
    /// <param name="maxConcurrentUploads">Maximum number of parts uploaded concurrently.</param>
    public S3UploadStream(IAmazonS3 s3,
                          string bucket,
                          string key,
                          string contentType = null,
                          long partLength = MinPartLength,
                          int maxConcurrentUploads = DefaultMaxConcurrentUploads)
        : base(partLength, MinPartLength, MaxPartLength, MaxPartCount, maxConcurrentUploads)
    {
        _s3Client = s3 ?? throw new ArgumentNullException(nameof(s3));
        _bucketName = bucket;
        _key = key;
        _contentType = contentType;
    }

    /// <inheritdoc/>
    protected override async Task InitiateMultipartUploadAsync(CancellationToken cancellationToken)
    {
        var response = await _s3Client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest
        {
            BucketName = _bucketName,
            Key = _key,
            ContentType = _contentType,
        }, cancellationToken);

        _uploadId = response.UploadId;
    }

    /// <inheritdoc/>
    protected override async Task UploadPartAsync(int partNumber, Stream content, bool isLastPart, CancellationToken cancellationToken)
    {
        var response = await _s3Client.UploadPartAsync(new UploadPartRequest
        {
            BucketName = _bucketName,
            Key = _key,
            UploadId = _uploadId,
            PartNumber = partNumber,
            PartSize = content.Length,
            IsLastPart = isLastPart,
            InputStream = content,
        }, cancellationToken);

        _partETags[partNumber] = new PartETag(partNumber, response.ETag);
    }

    /// <inheritdoc/>
    protected override Task CompleteMultipartUploadAsync(CancellationToken cancellationToken)
        => _s3Client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest
        {
            BucketName = _bucketName,
            Key = _key,
            UploadId = _uploadId,
            PartETags = [.. _partETags.Values.OrderBy(p => p.PartNumber)],
        }, cancellationToken);

    /// <inheritdoc/>
    protected override Task UploadSingleAsync(Stream content, CancellationToken cancellationToken)
        => _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = _key,
            ContentType = _contentType,
            InputStream = content,
            AutoCloseStream = false,
        }, cancellationToken);

    /// <inheritdoc/>
    protected override async Task AbortMultipartUploadAsync(CancellationToken cancellationToken)
    {
        if (_uploadId == null)
            return;

        await _s3Client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest
        {
            BucketName = _bucketName,
            Key = _key,
            UploadId = _uploadId,
        }, cancellationToken);
    }
}
