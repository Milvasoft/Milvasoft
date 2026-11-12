using FluentAssertions;
using Microsoft.IO;
using Milvasoft.FileOperations.Builder;
using Milvasoft.FileOperations.Concrete;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Milvasoft.UnitTests.FileOperationsTests;

[Trait("FileOperations Unit Tests", "Milvasoft.FileOperations project unit tests.")]
public class JsonOperationsTests : IDisposable
{
    private const string _key = "1234567890123456";
    private readonly string _basePath;

    public class Poco
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public JsonOperationsTests()
    {
        _basePath = Path.Combine(Path.GetTempPath(), "milva-json-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_basePath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
            Directory.Delete(_basePath, true);

        GC.SuppressFinalize(this);
    }

    private JsonOperations CreateOperations() => new(new JsonFileOperationsOptions
    {
        BasePath = _basePath,
        EncryptionKey = _key,
        CultureInfo = CultureInfo.InvariantCulture,
        Encoding = Encoding.UTF8
    }, new RecyclableMemoryStreamManager());

    /// <summary>
    /// Writes a file the way the synchronous encryption methods of previous versions did: AES-CBC, IV = key, ASCII plaintext, raw bytes.
    /// </summary>
    private void WriteLegacySyncEncryptedFile(string fileName, string content)
    {
        using var aes = Aes.Create();

        var keyBytes = Encoding.UTF8.GetBytes(_key);

        aes.Key = keyBytes;

        File.WriteAllBytes(Path.Combine(_basePath, fileName), aes.EncryptCbc(Encoding.ASCII.GetBytes(content), keyBytes, PaddingMode.PKCS7));
    }

    [Fact]
    public async Task CryptedContent_WithTurkishCharacters_ShouldRoundTripInSyncAndAsyncMethods()
    {
        var operations = CreateOperations();
        List<Poco> content = [new Poco { Id = 1, Name = "İğüşöç ıĞÜŞÖÇ" }];

        operations.ReplaceCryptedOldContentWithNew(content, "sync.json");
        await operations.ReplaceCryptedOldContentWithNewAsync(content, "async.json");

        // Sync and async formats are now the same, so each can read what the other wrote.
        (await operations.GetCryptedContentAsync<List<Poco>>("sync.json"))[0].Name.Should().Be("İğüşöç ıĞÜŞÖÇ");
        operations.GetCryptedContent<List<Poco>>("async.json")[0].Name.Should().Be("İğüşöç ıĞÜŞÖÇ");
    }

    [Fact]
    public async Task CryptedContent_WrittenByLegacySyncFormat_ShouldStayReadableAndBeMigratedOnWrite()
    {
        WriteLegacySyncEncryptedFile("legacy.json", "[{\"Id\":1,\"Name\":\"poco1\"}]");

        var operations = CreateOperations();

        operations.GetCryptedContent<List<Poco>>("legacy.json")[0].Name.Should().Be("poco1");
        (await operations.GetCryptedContentAsync<List<Poco>>("legacy.json"))[0].Name.Should().Be("poco1");

        await operations.AddCryptedContentAsync(new Poco { Name = "poco2" }, "legacy.json", contentsHasId: true);

        var result = operations.GetCryptedContent<List<Poco>>("legacy.json");

        result.Select(p => p.Name).Should().Equal("poco1", "poco2");
        result[1].Id.Should().Be(2);
    }

    [Fact]
    public void CryptedContent_WithWrongKey_ShouldThrowWithoutLeakingKey()
    {
        var operations = CreateOperations();

        operations.ReplaceCryptedOldContentWithNew(new List<Poco> { new() { Id = 1 } }, "a.json");
        operations.SetEncryptionKey("6543210987654321");

        var act = () => operations.GetCryptedContent<List<Poco>>("a.json");

        act.Should().Throw<CryptographicException>().Which.ToString().Should().NotContain("6543210987654321").And.NotContain(_key);
    }

    [Fact]
    public void AddCryptedContent_ShouldUseBasePath()
    {
        var operations = CreateOperations();

        operations.ReplaceCryptedOldContentWithNew(new List<Poco>(), "based.json");
        operations.AddCryptedContent(new Poco { Id = 5 }, "based.json", contentsHasId: false);

        File.Exists(Path.Combine(_basePath, "based.json")).Should().BeTrue();
        operations.GetCryptedContent<List<Poco>>("based.json").Should().ContainSingle(p => p.Id == 5);
    }

    [Fact]
    public async Task DeleteContent_WithSingleValueList_ShouldDeleteMatchingRecord()
    {
        var operations = CreateOperations();

        operations.ReplaceOldContentWithNew(new List<Poco> { new() { Id = 1 }, new() { Id = 2 } }, "delete.json");

        operations.DeleteContent<Poco>([2], "delete.json", p => p.Id);

        operations.GetContent<List<Poco>>("delete.json").Select(p => p.Id).Should().Equal(1);

        await operations.DeleteContentAsync<Poco>([1], "delete.json", p => p.Id);

        operations.GetContent<List<Poco>>("delete.json").Should().BeEmpty();
    }

    [Fact]
    public async Task AddContentAsync_Concurrent_ShouldNotLoseUpdates()
    {
        var operations = CreateOperations();

        operations.ReplaceOldContentWithNew(new List<Poco>(), "concurrent.json");

        await Task.WhenAll(Enumerable.Range(0, 40).Select(i => Task.Run(() => operations.AddContentAsync(new Poco { Name = $"p{i}" }, "concurrent.json", contentsHasId: true))));

        var result = operations.GetContent<List<Poco>>("concurrent.json");

        result.Should().HaveCount(40);
        result.Select(p => p.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void PlainContent_ShouldBeWrittenWithConfiguredEncodingWithoutBom()
    {
        var operations = CreateOperations();

        operations.ReplaceOldContentWithNew(new List<Poco> { new() { Name = "ş" } }, "plain.json");
        operations.AddContent(new Poco { Name = "ğ" }, "plain.json");

        var bytes = File.ReadAllBytes(Path.Combine(_basePath, "plain.json"));

        bytes.Take(3).Should().NotEqual(new byte[] { 0xEF, 0xBB, 0xBF });
        operations.GetContent<List<Poco>>("plain.json").Select(p => p.Name).Should().Equal("ş", "ğ");
        Directory.GetFiles(_basePath, "*.tmp").Should().BeEmpty();
    }
}
