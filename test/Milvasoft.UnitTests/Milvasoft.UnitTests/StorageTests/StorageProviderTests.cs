using Amazon.S3;
using Amazon.S3.Model;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Storage;
using Milvasoft.Storage.Models;
using Milvasoft.Storage.Providers;
using Milvasoft.Storage.Providers.LocalFile;
using Milvasoft.Storage.S3;
using Moq;
using System.Text;

namespace Milvasoft.UnitTests.StorageTests;

[Trait("Storage Unit Tests", "Milvasoft.Storage project unit tests.")]
public class StorageProviderTests : IDisposable
{
    private readonly string _root;
    private readonly string _basePath;

    public StorageProviderTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "milva-storage-tests-" + Guid.NewGuid().ToString("N"));
        _basePath = Path.Combine(_root, "files");
        Directory.CreateDirectory(_basePath);
        File.WriteAllText(Path.Combine(_root, "secret.txt"), "secret");
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, true);

        GC.SuppressFinalize(this);
    }

    private LocalFileProvider CreateLocalProvider(Action<StorageProviderOptions> configure = null)
    {
        var options = new StorageProviderOptions { LocalFile = new LocalFileConfiguration { BasePath = _basePath, RoutePrefix = "/files" } };

        configure?.Invoke(options);

        return new LocalFileProvider(options);
    }

    private static FormFile CreateFormFile(string content, string fileName = "test.txt")
    {
        var bytes = Encoding.UTF8.GetBytes(content);

        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName) { Headers = new HeaderDictionary(), ContentType = "text/plain" };
    }

    #region LocalFileProvider path containment

    [Theory]
    [InlineData("../secret.txt")]
    [InlineData("..\\secret.txt")]
    [InlineData("sub/../../secret.txt")]
    [InlineData("sub\\..\\..\\secret.txt")]
    [InlineData("sub/..\\../secret.txt")]
    public async Task LocalFileProvider_TraversalPaths_ShouldBeRejected(string path)
    {
        // '\' is a directory separator only on Windows; elsewhere it is a valid file name character.
        Assert.SkipWhen(path.Contains('\\') && !OperatingSystem.IsWindows(), "Backslash is not a separator on this OS.");

        var provider = CreateLocalProvider();

        (await provider.DownloadAsync(path)).IsSuccess.Should().BeFalse();
        (await provider.DeleteAsync(path)).Message.Should().Be(LocalFileProvider.InvalidFilePath);
        (await provider.UploadAsync(CreateFormFile("x"), path)).Message.Should().Be(LocalFileProvider.InvalidFilePath);

        File.ReadAllText(Path.Combine(_root, "secret.txt")).Should().Be("secret");
    }

    [Fact]
    public async Task LocalFileProvider_RootedPathOutsideBase_ShouldBeRejected()
    {
        var provider = CreateLocalProvider();
        var outside = Path.Combine(_root, "secret.txt");

        (await provider.DownloadAsync(outside)).Message.Should().Be(LocalFileProvider.InvalidFilePath);
        (await provider.DeleteAsync(outside)).Message.Should().Be(LocalFileProvider.InvalidFilePath);
        (await provider.CopyAsync(outside, "copy.txt", default)).Message.Should().Be(LocalFileProvider.InvalidFilePath);

        File.Exists(outside).Should().BeTrue();
    }

    [Fact]
    public async Task LocalFileProvider_SiblingFolderWithSamePrefix_ShouldBeRejected()
    {
        var sibling = _basePath + "-other";
        Directory.CreateDirectory(sibling);
        File.WriteAllText(Path.Combine(sibling, "a.txt"), "a");

        var provider = CreateLocalProvider();

        (await provider.DownloadAsync(Path.Combine("..", "files-other", "a.txt"))).Message.Should().Be(LocalFileProvider.InvalidFilePath);
        (await provider.DownloadAsync(Path.Combine(sibling, "a.txt"))).Message.Should().Be(LocalFileProvider.InvalidFilePath);
    }

    [Fact]
    public async Task LocalFileProvider_PathsInsideBase_ShouldWork()
    {
        var provider = CreateLocalProvider();

        var upload = await provider.UploadAsync(CreateFormFile("hello"), "sub/dir/a.txt");

        upload.IsSuccess.Should().BeTrue();

        var download = await provider.DownloadAsync(upload.FullPath);

        download.IsSuccess.Should().BeTrue();

        using (var reader = new StreamReader(download.Stream))
            (await reader.ReadToEndAsync()).Should().Be("hello");

        (await provider.DownloadAsync("sub\\dir\\a.txt")).Stream.Dispose();
        provider.CreateAccessUrl(upload.FullPath).Should().Be("/files/sub/dir/a.txt");
        (await provider.DeleteAsync("sub/./dir/a.txt")).IsSuccess.Should().BeTrue();
        File.Exists(Path.Combine(_basePath, "sub", "dir", "a.txt")).Should().BeFalse();
    }

    [Fact]
    public async Task LocalFileProvider_NoBufferUpload_ShouldWriteOnlyFileContentNotMultipartEnvelope()
    {
        var provider = CreateLocalProvider(o => o.AllowedFileExtensions = [".txt"]);
        var context = CreateMultipartContext("file", "a.txt", "file content");

        var result = await provider.NoBufferUploadAsync(context, "uploads/a");

        result.IsSuccess.Should().BeTrue();
        File.ReadAllText(Path.Combine(_basePath, "uploads", "a.txt")).Should().Be("file content");
    }

    [Fact]
    public async Task LocalFileProvider_NoBufferUpload_WithDisallowedExtension_ShouldFail()
    {
        var provider = CreateLocalProvider(o => o.AllowedFileExtensions = [".png"]);
        var context = CreateMultipartContext("file", "a.html", "<script></script>");

        var result = await provider.NoBufferUploadAsync(context, "uploads/a");

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be(LocalizerKeys.UnsupportedMediaType);
        File.Exists(Path.Combine(_basePath, "uploads", "a.html")).Should().BeFalse();
    }

    [Fact]
    public async Task LocalFileProvider_NoBufferUpload_TooLarge_ShouldFailAndLeaveNoFile()
    {
        var provider = CreateLocalProvider(o =>
        {
            o.AllowedFileExtensions = [".txt"];
            o.MaxFileSize = 4;
        });
        var context = CreateMultipartContext("file", "a.txt", "0123456789");

        var result = await provider.NoBufferUploadAsync(context, "uploads/a");

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be(UploadValidationStream.FileSizeTooBig);
        Directory.GetFiles(_basePath, "*", SearchOption.AllDirectories).Should().BeEmpty();
    }

    private static DefaultHttpContext CreateMultipartContext(string name, string fileName, string content)
    {
        const string boundary = "----milvaboundary";
        var body = $"--{boundary}\r\nContent-Disposition: form-data; name=\"{name}\"; filename=\"{fileName}\"\r\nContent-Type: text/plain\r\n\r\n{content}\r\n--{boundary}--\r\n";
        var context = new DefaultHttpContext();

        context.Request.ContentType = $"multipart/form-data; boundary={boundary}";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

        return context;
    }

    #endregion

    #region StorageHelper / validation

    [Fact]
    public void ValidateFileSignature_WithFileShorterThanHeader_ShouldNotThrow()
    {
        StorageHelper.RegisterFileSignature(".mtst", [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08]);

        var file = new FormFile(new MemoryStream([0x01, 0x02]), 0, 2, "file", "a.mtst");

        var act = () => StorageHelper.ValidateFileSignature(file, ".mtst");

        act.Should().NotThrow();
        StorageHelper.ValidateFileSignature(file, ".mtst").Should().BeFalse();
    }

    [Fact]
    public void ValidateFileSignature_WithSignatureLongerThanEightBytes_ShouldCompareWholeSignature()
    {
        var signature = Enumerable.Range(1, 12).Select(i => (byte)i).ToArray();
        StorageHelper.RegisterFileSignature(".mtst12", signature);

        var wrong = signature.ToArray();
        wrong[10] = 0xFF;

        StorageHelper.ValidateFileSignature(new FormFile(new MemoryStream(signature), 0, 12, "file", "a.mtst12"), ".mtst12").Should().BeTrue();
        StorageHelper.ValidateFileSignature(new FormFile(new MemoryStream(wrong), 0, 12, "file", "a.mtst12"), ".mtst12").Should().BeFalse();
    }

    [Theory]
    [InlineData("a.png", "text/html", "image/png")]
    [InlineData("a.unknownext", "text/html", "application/octet-stream")]
    [InlineData("a.unknownext", "image/svg+xml", "application/octet-stream")]
    [InlineData("a.unknownext", "image/heic", "image/heic")]
    [InlineData("a", null, "application/octet-stream")]
    public void GetSafeContentType_ShouldNotTrustActiveClientContentTypes(string fileName, string clientContentType, string expected)
        => StorageHelper.GetSafeContentType(fileName, clientContentType).Should().Be(expected);

    [Fact]
    public async Task UploadValidationStream_ShouldRejectWrongSignatureBeforeForwardingAnyByte()
    {
        var inner = new MemoryStream();
        var stream = new UploadValidationStream(inner, null, [[0x89, 0x50, 0x4E, 0x47]]);

        var act = async () =>
        {
            await stream.WriteAsync(Encoding.ASCII.GetBytes("<html>"));
            await stream.CompleteAsync();
        };

        await act.Should().ThrowAsync<InvalidDataException>().WithMessage(LocalizerKeys.UnsupportedMediaType);
        inner.Length.Should().Be(0);
    }

    [Fact]
    public async Task UploadValidationStream_ShouldForwardValidContent()
    {
        var inner = new MemoryStream();
        var stream = new UploadValidationStream(inner, 100, [[0x89, 0x50, 0x4E, 0x47]]);
        byte[] content = [0x89, 0x50, 0x4E, 0x47, 1, 2, 3];

        await stream.WriteAsync(content.AsMemory(0, 2));
        await stream.WriteAsync(content.AsMemory(2));
        await stream.CompleteAsync();

        inner.ToArray().Should().Equal(content);
    }

    #endregion

    #region Models

    [Fact]
    public void FileInformation_FromUploadRequestWithId_ShouldKeepId()
    {
        var info = new FileInformation(new FileUploadRequest { Id = "file-1", FileName = "a" });

        info.Id.Should().Be("file-1");
    }

    [Fact]
    public void FileInformation_FromUploadRequestWithoutId_ShouldGenerateId()
    {
        var info = new FileInformation(new FileUploadRequest { FileName = "a" });

        Guid.TryParse(info.Id, out _).Should().BeTrue();
    }

    [Fact]
    public void FileUploadRequest_WithMalformedDataUri_ShouldNotThrow()
    {
        var request = new FileUploadRequest { FileAsBase64 = "not-a-data-uri", FileName = "a" };

        request.File.Should().BeEmpty();
        request.MediaAsFormFile.Should().BeNull();
        request.ValidateExtension(FileType.Image).IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void FileUploadRequest_WithDataUri_ShouldDecodeContentTypeAndBytes()
    {
        var request = new FileUploadRequest { FileAsBase64 = "data:image/png;base64," + Convert.ToBase64String([1, 2, 3]), FileName = "a" };

        request.File.Should().Equal(1, 2, 3);
        request.MediaAsFormFile.ContentType.Should().Be("image/png");
        request.MediaAsFormFile.FileName.Should().Be("a.png");
        request.Should().Be(request with { });
    }

    #endregion

    #region S3Provider

    [Fact]
    public void S3Provider_GeneratePreSignedUrl_ShouldSignContentTypeAndLength()
    {
        using var client = new AmazonS3Client(new Amazon.Runtime.BasicAWSCredentials("access", "secret"), Amazon.RegionEndpoint.EUCentral1);
        var provider = new S3Provider(client, new StorageProviderOptions { AwsS3 = new AwsS3Configuration { BucketName = "bucket" } });

        var url = provider.GeneratePreSignedUrl("a/b.png", contentType: "image/png", contentLength: 42);

        var signedHeaders = Uri.UnescapeDataString(url.Split("X-Amz-SignedHeaders=")[1].Split('&')[0]);

        signedHeaders.Split(';').Should().Contain(["content-type", "content-length"]);
    }

    [Fact]
    public async Task S3Provider_DeleteAsync_ShouldBatchKeysByThousandAndReportErrors()
    {
        var s3 = new Mock<IAmazonS3>();
        var batchSizes = new List<int>();

        s3.Setup(c => c.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync((DeleteObjectsRequest r, CancellationToken _) =>
          {
              batchSizes.Add(r.Objects.Count);

              return new DeleteObjectsResponse
              {
                  DeleteErrors = batchSizes.Count == 2 ? [new DeleteError { Key = r.Objects[0].Key }] : []
              };
          });

        var provider = new S3Provider(s3.Object, new StorageProviderOptions { AwsS3 = new AwsS3Configuration { BucketName = "bucket" } });

        var result = await provider.DeleteAsync([.. Enumerable.Range(0, 2500).Select(i => $"key-{i}")]);

        batchSizes.Should().Equal(1000, 1000, 500);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task S3Provider_ClearBucketAsync_ShouldDeleteAllPages()
    {
        var s3 = new Mock<IAmazonS3>();
        var deleted = 0;

        s3.SetupSequence(c => c.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(new ListObjectsV2Response { S3Objects = [.. Enumerable.Range(0, 1000).Select(i => new S3Object { Key = $"a-{i}" })], IsTruncated = true, NextContinuationToken = "t" })
          .ReturnsAsync(new ListObjectsV2Response { S3Objects = [.. Enumerable.Range(0, 1000).Select(i => new S3Object { Key = $"b-{i}" })], IsTruncated = true, NextContinuationToken = "t2" })
          .ReturnsAsync(new ListObjectsV2Response { S3Objects = [new S3Object { Key = "c" }], IsTruncated = false });
        s3.Setup(c => c.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync((DeleteObjectsRequest r, CancellationToken _) =>
          {
              deleted += r.Objects.Count;
              return new DeleteObjectsResponse { DeleteErrors = [] };
          });

        var provider = new S3Provider(s3.Object, new StorageProviderOptions { AwsS3 = new AwsS3Configuration { BucketName = "bucket" } });

        var result = await provider.ClearBucketAsync();

        result.IsSuccess.Should().BeTrue();
        deleted.Should().Be(2001);
    }

    #endregion
}
