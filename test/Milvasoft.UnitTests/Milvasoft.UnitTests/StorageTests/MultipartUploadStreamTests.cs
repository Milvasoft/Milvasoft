using Amazon.S3;
using Amazon.S3.Model;
using FluentAssertions;
using Milvasoft.Storage.Providers;
using Milvasoft.Storage.S3;
using Moq;

namespace Milvasoft.UnitTests.StorageTests;

[Trait("Storage Unit Tests", "Milvasoft.Storage project unit tests.")]
public class MultipartUploadStreamTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private const int _partLength = 1024;

    private sealed class RecordingUploadStream(int maxConcurrentUploads = 2, Func<int, Task> onUploadPart = null)
        : MultipartUploadStream(_partLength, 1, 1024 * 1024, 100, maxConcurrentUploads)
    {
        private int _inFlight;

        public List<(int PartNumber, byte[] Content, bool IsLast)> Parts { get; } = [];
        public byte[] SingleContent { get; private set; }
        public bool Initiated { get; private set; }
        public bool Completed { get; private set; }
        public bool Aborted { get; private set; }
        public int MaxObservedConcurrency { get; private set; }

        protected override Task InitiateMultipartUploadAsync(CancellationToken cancellationToken)
        {
            Initiated = true;
            return Task.CompletedTask;
        }

        protected override async Task UploadPartAsync(int partNumber, Stream content, bool isLastPart, CancellationToken cancellationToken)
        {
            var inFlight = Interlocked.Increment(ref _inFlight);

            lock (Parts)
                MaxObservedConcurrency = Math.Max(MaxObservedConcurrency, inFlight);

            using var ms = new MemoryStream();

            await content.CopyToAsync(ms, cancellationToken);

            await Task.Delay(5, cancellationToken);

            if (onUploadPart != null)
                await onUploadPart(partNumber);

            lock (Parts)
                Parts.Add((partNumber, ms.ToArray(), isLastPart));

            Interlocked.Decrement(ref _inFlight);
        }

        protected override Task CompleteMultipartUploadAsync(CancellationToken cancellationToken)
        {
            Completed = true;
            return Task.CompletedTask;
        }

        protected override async Task UploadSingleAsync(Stream content, CancellationToken cancellationToken)
        {
            using var ms = new MemoryStream();

            await content.CopyToAsync(ms, cancellationToken);

            SingleContent = ms.ToArray();
            Completed = true;
        }

        protected override Task AbortMultipartUploadAsync(CancellationToken cancellationToken)
        {
            Aborted = true;
            return Task.CompletedTask;
        }
    }

    private static byte[] CreateContent(int length) => [.. Enumerable.Range(0, length).Select(i => (byte)(i % 251))];

    [Fact]
    public async Task CompleteAsync_WithContentSmallerThanPart_ShouldUploadSingleRequest()
    {
        var content = CreateContent(100);
        await using var stream = new RecordingUploadStream();

        await new MemoryStream(content).CopyToAsync(stream, Ct);
        await stream.CompleteAsync(Ct);

        stream.SingleContent.Should().Equal(content);
        stream.Initiated.Should().BeFalse();
        stream.Parts.Should().BeEmpty();
    }

    [Fact]
    public async Task CompleteAsync_WithEmptyContent_ShouldUploadEmptyObjectWithoutMultipart()
    {
        await using var stream = new RecordingUploadStream();

        await stream.CompleteAsync(Ct);

        stream.SingleContent.Should().BeEmpty();
        stream.Initiated.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteAsync_WithLargeContent_ShouldUploadOrderedPartsAndComplete()
    {
        var content = CreateContent(_partLength * 5 + 10);
        await using var stream = new RecordingUploadStream();

        await new MemoryStream(content).CopyToAsync(stream, 333, Ct);
        await stream.CompleteAsync(Ct);

        stream.Completed.Should().BeTrue();
        stream.Aborted.Should().BeFalse();
        stream.Parts.Should().HaveCount(6);

        var ordered = stream.Parts.OrderBy(p => p.PartNumber).ToList();

        ordered.Take(5).Should().OnlyContain(p => p.Content.Length == _partLength && !p.IsLast);
        ordered[^1].IsLast.Should().BeTrue();
        ordered.SelectMany(p => p.Content).Should().Equal(content);
    }

    [Fact]
    public async Task WriteAsync_ShouldNotExceedMaxConcurrentUploads()
    {
        var content = CreateContent(_partLength * 20);
        await using var stream = new RecordingUploadStream(maxConcurrentUploads: 2);

        await new MemoryStream(content).CopyToAsync(stream, _partLength, Ct);
        await stream.CompleteAsync(Ct);

        stream.MaxObservedConcurrency.Should().BeLessThanOrEqualTo(2);
    }

    /// <summary>
    /// The previous streams committed the upload from an async void Dispose, so an interrupted copy still committed a partial object
    /// and failures crashed the process. Disposing without completing must abort.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_WithoutComplete_ShouldAbortAndNotCommit()
    {
        var stream = new RecordingUploadStream();

        await stream.WriteAsync(CreateContent(_partLength * 3), Ct);
        await stream.DisposeAsync();

        stream.Completed.Should().BeFalse();
        stream.Aborted.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteAsync_WhenPartUploadFails_ShouldAbortAndThrow()
    {
        await using var stream = new RecordingUploadStream(onUploadPart: part => part == 2 ? throw new IOException("boom") : Task.CompletedTask);

        var act = async () =>
        {
            await stream.WriteAsync(CreateContent(_partLength * 4), Ct);
            await stream.CompleteAsync(Ct);
        };

        await act.Should().ThrowAsync<IOException>().WithMessage("boom");
        stream.Completed.Should().BeFalse();
        stream.Aborted.Should().BeTrue();
    }

    [Fact]
    public void Write_Synchronous_ShouldThrowNotSupported()
    {
        using var stream = new RecordingUploadStream();

        var act = () => stream.Write(new byte[10], 0, 10);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public async Task S3UploadStream_DisposeWithoutComplete_ShouldAbortMultipartUpload()
    {
        var s3 = new Mock<IAmazonS3>();

        s3.Setup(c => c.InitiateMultipartUploadAsync(It.IsAny<InitiateMultipartUploadRequest>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(new InitiateMultipartUploadResponse { UploadId = "upload-1" });
        s3.Setup(c => c.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(new UploadPartResponse { ETag = "etag" });
        s3.Setup(c => c.AbortMultipartUploadAsync(It.IsAny<AbortMultipartUploadRequest>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(new AbortMultipartUploadResponse());

        var stream = new S3UploadStream(s3.Object, "bucket", "key", "image/png");

        await stream.WriteAsync(new byte[S3UploadStream.MinPartLength + 1], Ct);
        await stream.DisposeAsync();

        s3.Verify(c => c.AbortMultipartUploadAsync(It.Is<AbortMultipartUploadRequest>(r => r.UploadId == "upload-1"), It.IsAny<CancellationToken>()), Times.Once);
        s3.Verify(c => c.CompleteMultipartUploadAsync(It.IsAny<CompleteMultipartUploadRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task S3UploadStream_Complete_ShouldUseMinimumPartSizeAndOrderedETags()
    {
        var s3 = new Mock<IAmazonS3>();
        var partSizes = new List<long>();
        CompleteMultipartUploadRequest completeRequest = null;

        s3.Setup(c => c.InitiateMultipartUploadAsync(It.IsAny<InitiateMultipartUploadRequest>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(new InitiateMultipartUploadResponse { UploadId = "upload-1" });
        s3.Setup(c => c.UploadPartAsync(It.IsAny<UploadPartRequest>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync((UploadPartRequest r, CancellationToken _) =>
          {
              lock (partSizes)
                  partSizes.Add(r.InputStream.Length);

              return new UploadPartResponse { ETag = $"etag-{r.PartNumber}" };
          });
        s3.Setup(c => c.CompleteMultipartUploadAsync(It.IsAny<CompleteMultipartUploadRequest>(), It.IsAny<CancellationToken>()))
          .Callback((CompleteMultipartUploadRequest r, CancellationToken _) => completeRequest = r)
          .ReturnsAsync(new CompleteMultipartUploadResponse());

        await using (var stream = new S3UploadStream(s3.Object, "bucket", "key", "image/png"))
        {
            await stream.WriteAsync(new byte[S3UploadStream.MinPartLength * 2 + 100], Ct);
            await stream.CompleteAsync(Ct);
        }

        partSizes.Should().HaveCount(3);
        partSizes.Count(s => s == S3UploadStream.MinPartLength).Should().Be(2);
        completeRequest.PartETags.Select(p => p.PartNumber).Should().Equal(1, 2, 3);
        s3.Verify(c => c.AbortMultipartUploadAsync(It.IsAny<AbortMultipartUploadRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
