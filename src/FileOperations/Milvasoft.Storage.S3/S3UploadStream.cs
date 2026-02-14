using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.IO;
using Microsoft.Net.Http.Headers;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Utils.Constants;
using System.Collections.Concurrent;
using System.Net;

namespace Milvasoft.Storage.S3;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class S3UploadStream : Stream
{
    internal class Metadata
    {
        public string BucketName;
        public string Key;
        public long PartLength = _defaultPartLength;
        public bool FileValidated = false;

        public int PartCount = 0;
        public string UploadId;
        public MemoryStream CurrentStream;

        public long Position = 0; // based on bytes written
        public long Length = 0; // based on bytes written or SetLength, whichever is larger (no truncation)

        public List<Task> Tasks = [];
        public ConcurrentDictionary<int, string> PartETags = new();
    }

    /* Note the that maximum size (as of now) of a file in S3 is 5TB so it isn't
     * safe to assume all uploads will work here.  MAX_PART_SIZE times MAX_PART_COUNT
     * is ~50TB, which is too big for S3. */
    private const long _minPartLength = 5L * 1024 * 1024; // all parts but the last this size or greater
    private const long _maxPartLength = 5L * 1024 * 1024 * 1024; // 5GB max per PUT
    private const long _maxPartCount = 10000; // no more than 10,000 parts total
    private const long _defaultPartLength = _minPartLength;
    private Metadata _metadata = new();
    private readonly IAmazonS3 _s3Client = null;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager = new();
    private const string _contentDispositonHeader = "Content-Disposition";

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => _metadata.Length = Math.Max(_metadata.Length, _metadata.Position);

    public override long Position
    {
        get => _metadata.Position;
        set => throw new NotImplementedException();
    }

    public S3UploadStream(IAmazonS3 s3, string bucket, string key, long partLength = _defaultPartLength)
    {
        _s3Client = s3;
        _metadata.BucketName = bucket;
        _metadata.Key = key;
        _metadata.PartLength = partLength;
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

    public override void SetLength(long value)
    {
        _metadata.Length = Math.Max(_metadata.Length, value);
        _metadata.PartLength = Math.Max(_minPartLength, Math.Min(_maxPartLength, _metadata.Length / _maxPartCount));
    }

    public override void Flush() => FlushAsync(false).Wait();
    public override Task FlushAsync(CancellationToken cancellationToken) => FlushAsync(false, cancellationToken);

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (buffer.Length == 0)
            return;

        var offset = 0;
        var remaining = buffer.Length;

        do
        {
            if (_metadata.CurrentStream == null || _metadata.CurrentStream.Length >= _metadata.PartLength)
                await StartNewPartAsync(cancellationToken);

            var availableSpace = _metadata.PartLength - _metadata.CurrentStream.Length;
            var writeLength = Math.Min(remaining, (int)availableSpace);

            await _metadata.CurrentStream.WriteAsync(buffer.Slice(offset, writeLength), cancellationToken);

            _metadata.Position += writeLength;

            remaining -= writeLength;
            offset += writeLength;

        } while (remaining > 0);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken);

    public override void Write(byte[] buffer, int offset, int count) => WriteAsync(buffer, offset, count, default).Wait();

    private async Task StartNewPartAsync(CancellationToken cancellationToken = default)
    {
        if (_metadata.CurrentStream != null)
        {
            await FlushAsync(false, cancellationToken);
        }

        _metadata.CurrentStream = _recyclableMemoryStreamManager.GetStream();
        _metadata.PartLength = Math.Min(_maxPartLength, Math.Max(_metadata.PartLength, (_metadata.PartCount / 2 + 1) * _minPartLength));
    }

    private async Task FlushAsync(bool disposing, CancellationToken cancellationToken = default)
    {
        if ((_metadata.CurrentStream == null || _metadata.CurrentStream.Length < _minPartLength) && !disposing)
            return;

        if (_metadata.UploadId == null)
        {
            var initResponse = await _s3Client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest()
            {
                BucketName = _metadata.BucketName,
                Key = _metadata.Key
            }, cancellationToken);

            _metadata.UploadId = initResponse.UploadId;
        }

        if (_metadata.CurrentStream != null)
        {
            var i = ++_metadata.PartCount;

            _metadata.CurrentStream.Seek(0, SeekOrigin.Begin);

            var request = new UploadPartRequest()
            {
                BucketName = _metadata.BucketName,
                Key = _metadata.Key,
                UploadId = _metadata.UploadId,
                PartNumber = i,
                IsLastPart = disposing,
                InputStream = _metadata.CurrentStream
            };

            _metadata.CurrentStream = null;

            var upload = Task.Run(async () =>
            {
                var response = await _s3Client.UploadPartAsync(request, cancellationToken);

                _metadata.PartETags.AddOrUpdate(i, response.ETag, (n, s) => response.ETag);

                await request.InputStream.DisposeAsync();
            }, cancellationToken);

            _metadata.Tasks.Add(upload);
        }
    }

    private async Task CompleteUploadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.WhenAll(_metadata.Tasks);

            if (Length > 0)
            {
                await _s3Client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest()
                {
                    BucketName = _metadata.BucketName,
                    Key = _metadata.Key,
                    PartETags = [.. _metadata.PartETags.Select(e => new PartETag(e.Key, e.Value))],
                    UploadId = _metadata.UploadId
                }, cancellationToken);
            }
        }
        catch (Exception)
        {
            var abortRequest = new AbortMultipartUploadRequest
            {
                BucketName = _metadata.BucketName,
                Key = _metadata.Key,
                UploadId = _metadata.UploadId
            };

            await _s3Client.AbortMultipartUploadAsync(abortRequest, cancellationToken);

            throw new MilvaUserFriendlyException(LocalizerKeys.UnsupportedMediaType);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("AsyncUsage", "AsyncFixer03:Fire-and-forget async-void methods or delegates", Justification = "<Pending>")]
    protected override async void Dispose(bool disposing)
    {
        if (disposing && _metadata != null)
        {
            await FlushAsync(true);
            await CompleteUploadAsync();
        }

        _metadata = null;

        base.Dispose(disposing);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "This may useful in future.")]
    private async Task ValidateFileAsync(CancellationToken cancellationToken = default)
    {
        if (_metadata.FileValidated)
            return;

        var memoryStream = _metadata.CurrentStream;

        var initialPosition = memoryStream.Position;

        memoryStream.Position = 0;

        // Stream'i bir StreamReader ile okuyun
        using var reader = new StreamReader(memoryStream, leaveOpen: true);

        string line;
        string contentDisposition = null;

        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (line.StartsWith(_contentDispositonHeader, StringComparison.OrdinalIgnoreCase))
            {
                contentDisposition = line[21..];
                break;
            }
            else if (string.IsNullOrEmpty(line))
            {
                // When there is a blank line, we can assume that this line is a separator between the headings and the body.
                break;
            }
        }

        if (contentDisposition != null)
        {
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(contentDisposition, out var dispositionHeader);

            if (!hasContentDispositionHeader)
                throw new MilvaUserFriendlyException(LocalizerKeys.UnsupportedMediaType);

            var trustedFileName = WebUtility.HtmlEncode(dispositionHeader.FileName.Value);

            var extension = Path.GetExtension(trustedFileName).ToLowerInvariant();

            var isSupportedExtension = StorageHelper.FileSignatures.TryGetValue(extension, out List<byte[]> signatures);

            if (!isSupportedExtension)
                throw new MilvaUserFriendlyException(LocalizerKeys.UnsupportedMediaType);

            _metadata.Key += extension;
            _metadata.FileValidated = true;
        }

        memoryStream.Position = initialPosition;
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member