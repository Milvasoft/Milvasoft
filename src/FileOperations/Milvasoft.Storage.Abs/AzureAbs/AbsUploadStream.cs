using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.IO;
using Microsoft.Net.Http.Headers;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Utils.Constants;
using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Milvasoft.Storage.Abs.AzureAbs;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class AbsUploadStream : Stream
{
    internal class Metadata
    {
        public string Key;
        public long PartLength = _defaultPartLength;
        public bool FileValidated = false;
        public string ContentType;

        public int PartCount = 0;
        public List<string> BlockIdlist = [];
        public MemoryStream CurrentStream;

        public long Position = 0; // based on bytes written
        public long Length = 0; // based on bytes written or SetLength, whichever is larger (no truncation)

        public List<Task> Tasks = [];
        public ConcurrentDictionary<int, string> PartETags = new();
    }

    /* Note the that maximum size (as of now) of a file in S3 is 5TB so it isn't
     * safe to assume all uploads will work here.  MAX_PART_SIZE times MAX_PART_COUNT
     * is ~50TB, which is too big for S3. */
    private const long _minPartLength = 20L * 1024 * 1024; // all parts but the last this size or greater
    private const long _maxPartLength = 5L * 1024 * 1024 * 1024; // 5GB max per PUT
    private const long _maxPartCount = 10000; // no more than 10,000 parts total
    private const long _defaultPartLength = _minPartLength;
    private const string _contentDispositonHeader = "Content-Disposition";
    private Metadata _metadata = new();
    private readonly BlockBlobClient _absClient = null;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager = new();

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => _metadata.Length = Math.Max(_metadata.Length, _metadata.Position);

    public override long Position
    {
        get => _metadata.Position;
        set => throw new NotImplementedException();
    }

    public AbsUploadStream(BlockBlobClient absContainerClient, string contentType, string key, long partLength = _defaultPartLength)
    {
        _absClient = absContainerClient;
        _metadata.Key = key;
        _metadata.PartLength = partLength;
        _metadata.ContentType = contentType;
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

        if (_metadata.CurrentStream != null)
        {
            _metadata.PartCount++;

            var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(_metadata.PartCount.ToString("d6")));

            _metadata.CurrentStream.Seek(0, SeekOrigin.Begin);

            await _absClient.StageBlockAsync(blockId, _metadata.CurrentStream, cancellationToken: cancellationToken);

            await _metadata.CurrentStream.DisposeAsync();

            _metadata.BlockIdlist.Add(blockId);

            _metadata.CurrentStream = null;
        }
    }

    private async Task CompleteUploadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (Length > 0)
            {
                var headers = new BlobHttpHeaders()
                {
                    ContentType = _metadata.ContentType
                };

                await _absClient.CommitBlockListAsync(_metadata.BlockIdlist, headers, cancellationToken: cancellationToken);
            }
        }
        catch (Exception)
        {
            throw new MilvaUserFriendlyException(LocalizerKeys.UploadFailed);
        }
    }

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