using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Milvasoft.FileOperations.Concrete;
using Milvasoft.FileOperations.Enums;
using Milvasoft.Storage.Models;
using System.Globalization;
using System.Text;

namespace Milvasoft.UnitTests.FileOperationsTests;

[Trait("FileOperations Unit Tests", "Milvasoft.FileOperations project unit tests.")]
public class FormFileOperationsTests : IDisposable
{
    private readonly string _basePath;

    public class Product
    {
        public Guid Id { get; set; }
    }

    public FormFileOperationsTests()
    {
        _basePath = Path.Combine(Path.GetTempPath(), "milva-formfile-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_basePath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
            Directory.Delete(_basePath, true);

        GC.SuppressFinalize(this);
    }

    private static FormFile CreateFormFile(string content, string fileName)
    {
        var bytes = Encoding.UTF8.GetBytes(content);

        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName) { Headers = new HeaderDictionary() };
    }

    [Theory]
    [InlineData(FileType.Image, "a.png")]
    [InlineData(FileType.Document, "a.pdf")]
    [InlineData(FileType.Video, "a.mp4")]
    public void ValidateFile_WithDefaultExtensions_ShouldNotThrow(FileType fileType, string fileName)
    {
        var result = CreateFormFile("x", fileName).ValidateFile(1024, null, fileType);

        result.Should().Be(FileValidationResult.Valid);
    }

    [Fact]
    public void ValidateFile_UnderTurkishCulture_ShouldMatchUppercaseExtensions()
    {
        var previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            CreateFormFile("x", "IMAGE.GIF").ValidateFile(1024, [".gif"]).Should().Be(FileValidationResult.Valid);
            CreateFormFile("x", "image.gif").ValidateFile(1024, [".GIF"]).Should().Be(FileValidationResult.Valid);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public async Task GetFileFromPathAsync_ShouldReturnReadableFile()
    {
        var path = Path.Combine(_basePath, "a.txt");
        await File.WriteAllTextAsync(path, "content");

        var file = await FormFileOperations.GetFileFromPathAsync(path, FileType.Document);

        using var reader = new StreamReader(file.OpenReadStream());

        (await reader.ReadToEndAsync()).Should().Be("content");
    }

    [Fact]
    public void GetFileUrlPathSectionFromFilePath_ShouldKeepSlashes()
    {
        var path = Path.Combine(_basePath, "Product", "1", "1 a.jpeg");

        var url = FormFileOperations.GetFileUrlPathSectionFromFilePath(path, "api/ImageLibrary");

        url.Should().Be("api/ImageLibrary/Product/1/1%20a.jpeg");
    }

    [Fact]
    public async Task SaveFilesToPathAsync_ShouldKeepEachFilesExtensionAndIgnoreNonNumericNames()
    {
        var entity = new Product { Id = Guid.NewGuid() };
        var itemFolder = Path.Combine(_basePath, "Product", entity.Id.ToString());

        Directory.CreateDirectory(itemFolder);
        await File.WriteAllTextAsync(Path.Combine(itemFolder, $"{entity.Id}.png"), "existing");

        var paths = await new List<IFormFile> { CreateFormFile("a", "a.png"), CreateFormFile("b", "b.pdf") }
            .SaveFilesToPathAsync(entity, _basePath, t => t.Name, nameof(Product.Id));

        paths.Select(Path.GetExtension).Should().Equal(".png", ".pdf");
        File.Exists(Path.Combine(itemFolder, $"{entity.Id}.png")).Should().BeTrue();
    }

    [Fact]
    public async Task SaveFileToPathAsync_WhenWriteFails_ShouldKeepPreviousFile()
    {
        var entity = new Product { Id = Guid.NewGuid() };

        var first = await CreateFormFile("old", "a.png").SaveFileToPathAsync(entity, _basePath, t => t.Name, nameof(Product.Id));

        var failing = new FormFile(new FailingStream(), 0, 10, "file", "b.png");

        var act = () => failing.SaveFileToPathAsync(entity, _basePath, t => t.Name, nameof(Product.Id));

        await act.Should().ThrowAsync<IOException>();
        (await File.ReadAllTextAsync(first)).Should().Be("old");
    }

    private sealed class FailingStream : MemoryStream
    {
        public override int Read(byte[] buffer, int offset, int count) => throw new IOException("read failed");

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => throw new IOException("read failed");

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new IOException("read failed");
    }
}
