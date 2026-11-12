using Microsoft.IO;

namespace Milvasoft.Storage.Providers;

/// <summary>
/// Write-only stream that uploads its content to a remote store in parts, with bounded memory and bounded upload concurrency.
/// </summary>
/// <remarks>
/// <para>
/// Lifecycle: write with <see cref="WriteAsync(ReadOnlyMemory{byte}, CancellationToken)"/> (or <see cref="Stream.CopyToAsync(Stream, CancellationToken)"/>),
/// then call <see cref="CompleteAsync(CancellationToken)"/> to commit the object. Disposing without completing aborts the upload,
/// so a failed or cancelled copy never commits a partial object. Use <c>await using</c>; synchronous disposal can only start a
/// best-effort background abort.
/// </para>
/// <para>
/// Content that fits in one part is uploaded with a single request. Larger content is uploaded in parts of
/// <c>partLength</c> bytes; at most <c>maxConcurrentUploads</c> parts are in flight, so memory use is bounded by
/// roughly <c>partLength * (maxConcurrentUploads + 1)</c>. The part length doubles every 1000 parts (up to the store's maximum)
/// so that very large uploads stay within the store's part count limit.
/// </para>
/// <para>Synchronous writes are not supported because a full part must be uploaded asynchronously.</para>
/// </remarks>
public abstract class MultipartUploadStream : Stream
{
    private const int _partGrowthInterval = 1000;

    private readonly List<Task> _inFlightUploads = [];
    private readonly int _maxConcurrentUploads;
    private readonly long _maxPartLength;
    private readonly int _maxPartCount;
    private RecyclableMemoryStream _currentPart;
    private long _partLength;
    private int _partCount;
    private long _position;
    private UploadState _state = UploadState.Open;

    private enum UploadState
    {
        Open,
        Completed,
        Aborted
    }

    /// <summary>
    /// Initializes a new instance of <see cref="MultipartUploadStream"/>.
    /// </summary>
    /// <param name="partLength">Length of each part except the last one. Must be at least <paramref name="minPartLength"/>.</param>
    /// <param name="minPartLength">Minimum part length accepted by the store.</param>
    /// <param name="maxPartLength">Maximum part length accepted by the store.</param>
    /// <param name="maxPartCount">Maximum part count accepted by the store.</param>
    /// <param name="maxConcurrentUploads">Maximum number of parts uploaded concurrently.</param>
    protected MultipartUploadStream(long partLength,
                                    long minPartLength,
                                    long maxPartLength,
                                    int maxPartCount,
                                    int maxConcurrentUploads)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(partLength, minPartLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(partLength, maxPartLength);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxConcurrentUploads, 1);

        _partLength = partLength;
        _maxPartLength = maxPartLength;
        _maxPartCount = maxPartCount;
        _maxConcurrentUploads = maxConcurrentUploads;
    }

    /// <summary>
    /// Number of parts dispatched so far.
    /// </summary>
    protected int PartCount => _partCount;

    /// <inheritdoc/>
    public override bool CanRead => false;

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override bool CanWrite => _state == UploadState.Open;

    /// <inheritdoc/>
    public override long Length => _position;

    /// <inheritdoc/>
    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// Starts a multipart upload. Called once, before the first part is uploaded.
    /// </summary>
    protected abstract Task InitiateMultipartUploadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Uploads one part. <paramref name="content"/> is positioned at 0 and is disposed by the caller after this task completes.
    /// </summary>
    protected abstract Task UploadPartAsync(int partNumber, Stream content, bool isLastPart, CancellationToken cancellationToken);

    /// <summary>
    /// Commits the uploaded parts in part number order.
    /// </summary>
    protected abstract Task CompleteMultipartUploadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Uploads the whole content with a single request. Used when the content fits in one part (including empty content).
    /// </summary>
    protected abstract Task UploadSingleAsync(Stream content, CancellationToken cancellationToken);

    /// <summary>
    /// Aborts a started multipart upload and releases its parts in the store.
    /// </summary>
    protected abstract Task AbortMultipartUploadAsync(CancellationToken cancellationToken);

    /// <inheritdoc/>
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ThrowIfNotOpen();

        while (buffer.Length > 0)
        {
            // A full part is dispatched only when more data arrives, so the last part is always uploaded by CompleteAsync.
            if (_currentPart != null && _currentPart.Length >= _partLength)
                await DispatchCurrentPartAsync(isLastPart: false, cancellationToken);

            _currentPart ??= StorageHelper.MemoryStreamManager.GetStream(nameof(MultipartUploadStream));

            var writeLength = (int)Math.Min(buffer.Length, _partLength - _currentPart.Length);

            _currentPart.Write(buffer.Span[..writeLength]);

            buffer = buffer[writeLength..];
            _position += writeLength;
        }
    }

    /// <inheritdoc/>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    /// <summary>
    /// Not supported. Use <see cref="WriteAsync(ReadOnlyMemory{byte}, CancellationToken)"/>.
    /// </summary>
    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException($"{GetType().Name} supports only asynchronous writes.");

    /// <summary>
    /// Does nothing. Parts are uploaded when they are full and on <see cref="CompleteAsync(CancellationToken)"/>.
    /// </summary>
    public override void Flush()
    {
    }

    /// <summary>
    /// Does nothing. Parts are uploaded when they are full and on <see cref="CompleteAsync(CancellationToken)"/>.
    /// </summary>
    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <summary>
    /// Uploads the remaining content and commits the object. If anything fails, the upload is aborted and the exception is rethrown.
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfNotOpen();

        try
        {
            if (_partCount == 0)
            {
                _currentPart ??= StorageHelper.MemoryStreamManager.GetStream(nameof(MultipartUploadStream));
                _currentPart.Position = 0;

                await UploadSingleAsync(_currentPart, cancellationToken);
            }
            else
            {
                if (_currentPart is { Length: > 0 })
                    await DispatchCurrentPartAsync(isLastPart: true, cancellationToken);

                await Task.WhenAll(_inFlightUploads);

                _inFlightUploads.Clear();

                await CompleteMultipartUploadAsync(cancellationToken);
            }

            _state = UploadState.Completed;
        }
        catch
        {
            await AbortAsync();

            throw;
        }
        finally
        {
            ReleaseCurrentPart();
        }
    }

    /// <summary>
    /// Aborts the upload if it was not completed and releases buffers.
    /// </summary>
    public override async ValueTask DisposeAsync()
    {
        if (_state == UploadState.Open)
            await AbortAsync();

        ReleaseCurrentPart();

        await base.DisposeAsync();
    }

    /// <summary>
    /// Releases buffers. If the upload was neither completed nor aborted, starts a best-effort background abort.
    /// Prefer <see cref="DisposeAsync"/>.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && _state == UploadState.Open)
        {
            _state = UploadState.Aborted;

            if (_partCount > 0)
                _ = AbortInBackgroundAsync();
        }

        if (disposing)
            ReleaseCurrentPart();

        base.Dispose(disposing);
    }

    private async Task AbortInBackgroundAsync()
    {
        try
        {
            await WaitForInFlightUploadsAsync();
            await AbortMultipartUploadAsync(CancellationToken.None);
        }
        catch
        {
            // Best effort. Configure a lifecycle rule in the store to clean up incomplete uploads.
        }
    }

    private async Task AbortAsync()
    {
        if (_state != UploadState.Open)
            return;

        _state = UploadState.Aborted;

        if (_partCount == 0)
            return;

        try
        {
            // Parts still uploading could reappear after an abort, so wait for them first.
            await WaitForInFlightUploadsAsync();
            await AbortMultipartUploadAsync(CancellationToken.None);
        }
        catch
        {
            // The original failure is more relevant to the caller than a failed abort.
        }
    }

    private async Task WaitForInFlightUploadsAsync()
    {
        try
        {
            await Task.WhenAll(_inFlightUploads);
        }
        catch
        {
            // Failures are reported by the operation that observed them first.
        }

        _inFlightUploads.Clear();
    }

    private async Task DispatchCurrentPartAsync(bool isLastPart, CancellationToken cancellationToken)
    {
        await WaitForUploadSlotAsync();

        if (_partCount == 0)
            await InitiateMultipartUploadAsync(cancellationToken);

        if (_partCount >= _maxPartCount)
            throw new InvalidDataException("FileSizeTooBig");

        var partNumber = ++_partCount;
        var part = _currentPart;

        _currentPart = null;
        part.Position = 0;

        _inFlightUploads.Add(UploadPartAndReleaseAsync(partNumber, part, isLastPart, cancellationToken));

        if (partNumber % _partGrowthInterval == 0)
            _partLength = Math.Min(_partLength * 2, _maxPartLength);
    }

    private async Task UploadPartAndReleaseAsync(int partNumber, RecyclableMemoryStream part, bool isLastPart, CancellationToken cancellationToken)
    {
        try
        {
            await UploadPartAsync(partNumber, part, isLastPart, cancellationToken);
        }
        finally
        {
            await part.DisposeAsync();
        }
    }

    private async Task WaitForUploadSlotAsync()
    {
        // Surface failures of already finished uploads as early as possible.
        for (var i = _inFlightUploads.Count - 1; i >= 0; i--)
        {
            if (_inFlightUploads[i].IsCompleted)
            {
                var completed = _inFlightUploads[i];

                _inFlightUploads.RemoveAt(i);

                await completed;
            }
        }

        while (_inFlightUploads.Count >= _maxConcurrentUploads)
        {
            var completed = await Task.WhenAny(_inFlightUploads);

            _inFlightUploads.Remove(completed);

            await completed;
        }
    }

    private void ReleaseCurrentPart()
    {
        _currentPart?.Dispose();
        _currentPart = null;
    }

    private void ThrowIfNotOpen()
    {
        if (_state != UploadState.Open)
            throw new InvalidOperationException("The upload stream is already completed or aborted.");
    }
}
