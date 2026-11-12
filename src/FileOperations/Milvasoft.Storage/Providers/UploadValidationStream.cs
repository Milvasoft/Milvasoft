using Milvasoft.Core.Utils.Constants;

namespace Milvasoft.Storage.Providers;

/// <summary>
/// Write-only wrapper that enforces a maximum length and a file signature (magic number) on content written to an inner stream.
/// </summary>
/// <remarks>
/// When signatures are required, the first bytes are held back until enough bytes arrived to check them, so nothing reaches the
/// inner stream before the signature is validated. Call <see cref="CompleteAsync(CancellationToken)"/> after the last write to
/// validate short content and forward the held bytes. Violations throw <see cref="InvalidDataException"/> with the message
/// <c>FileSizeTooBig</c> or <see cref="LocalizerKeys.UnsupportedMediaType"/>. The inner stream is not disposed.
/// </remarks>
public sealed class UploadValidationStream : Stream
{
    /// <summary>
    /// Message of the exception thrown when the content is longer than allowed.
    /// </summary>
    public const string FileSizeTooBig = nameof(FileSizeTooBig);

    private readonly Stream _inner;
    private readonly long? _maxLength;
    private readonly IReadOnlyList<byte[]> _signatures;
    private readonly byte[] _header;
    private int _headerLength;
    private bool _validated;
    private long _position;

    /// <summary>
    /// Initializes a new instance of <see cref="UploadValidationStream"/>.
    /// </summary>
    /// <param name="inner">Stream the validated content is written to.</param>
    /// <param name="maxLength">Maximum content length in bytes. Null means no limit.</param>
    /// <param name="signatures">Accepted signatures. Null or empty means no signature check.</param>
    public UploadValidationStream(Stream inner, long? maxLength, IEnumerable<byte[]> signatures)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _maxLength = maxLength;
        _signatures = signatures?.Where(s => s?.Length > 0).ToList() ?? [];
        _validated = _signatures.Count == 0;
        _header = _validated ? [] : new byte[_signatures.Max(s => s.Length)];
    }

    /// <inheritdoc/>
    public override bool CanRead => false;

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override bool CanWrite => true;

    /// <inheritdoc/>
    public override long Length => _position;

    /// <inheritdoc/>
    public override long Position { get => _position; set => throw new NotSupportedException(); }

    /// <inheritdoc/>
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_maxLength.HasValue && _position + buffer.Length > _maxLength.Value)
            throw new InvalidDataException(FileSizeTooBig);

        _position += buffer.Length;

        if (!_validated)
        {
            var headerPart = Math.Min(buffer.Length, _header.Length - _headerLength);

            buffer.Span[..headerPart].CopyTo(_header.AsSpan(_headerLength));
            _headerLength += headerPart;
            buffer = buffer[headerPart..];

            if (_headerLength < _header.Length)
                return;

            await ValidateAndForwardHeaderAsync(cancellationToken);
        }

        if (buffer.Length > 0)
            await _inner.WriteAsync(buffer, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    /// <summary>
    /// Validates content shorter than the longest signature and forwards the held back bytes. Call after the last write.
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        if (!_validated)
            await ValidateAndForwardHeaderAsync(cancellationToken);
    }

    private async Task ValidateAndForwardHeaderAsync(CancellationToken cancellationToken)
    {
        if (!StorageHelper.MatchesAnySignature(_header.AsSpan(0, _headerLength), _signatures))
            throw new InvalidDataException(LocalizerKeys.UnsupportedMediaType);

        _validated = true;

        if (_headerLength > 0)
            await _inner.WriteAsync(_header.AsMemory(0, _headerLength), cancellationToken);
    }

    /// <summary>
    /// Not supported. Use <see cref="WriteAsync(ReadOnlyMemory{byte}, CancellationToken)"/>.
    /// </summary>
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Flush()
    {
    }

    /// <inheritdoc/>
    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();
}
