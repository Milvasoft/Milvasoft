using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Cryptography.Builder;
using Milvasoft.Cryptography.Concrete;
using Milvasoft.DataAccess.EfCore.Utils.Converters;
using System.Security.Cryptography;
using System.Text;

namespace Milvasoft.UnitTests.CryptographyTests;

[Trait("Cryptography Unit Tests", "Milvasoft.Cryptography project unit tests.")]
public class MilvaCryptographyTests
{
    private const string _key16 = "8RvJjg-oye5NhwqK";
    private const string _key32 = "bc3voatyGOwvN/yfoX32pPfNJhLnVTBQ";

    #region Key validation

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("bc3voatyGOwvN/yfoX32pPfNJhLnVTBQEG9PIzKQ7I88l+qc7Tf4Z0wUqkVCUy5K")]
    public void Constructor_WithInvalidKeySize_ShouldThrowCryptographicException(string key)
    {
        // Act
        Action act = () => _ = CreateProvider(key);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void WithOptions_WithInvalidKey_ShouldThrowAtRegistration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Action act = () => services.AddMilvaCryptography().WithOptions(opt => opt.Key = "short");

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void WithOptions_WithMissingConfigurationSection_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationManager();

        // Act
        Action act = () => services.AddMilvaCryptography(configuration).WithOptions();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Base64 raw keys

    [Fact]
    public void Base64Key_ShouldBeUsedDirectlyAsAesGcmKey()
    {
        // Arrange
        var rawKey = RandomNumberGenerator.GetBytes(32);
        var provider = CreateProvider("base64:" + Convert.ToBase64String(rawKey));

        // Act
        var encrypted = provider.Encrypt("securedata");

        // Assert: decrypt the v2 payload with the raw key itself.
        var payload = Convert.FromBase64String(encrypted["v2:".Length..]);
        var plain = new byte[payload.Length - 12 - 16];
        using var aesGcm = new AesGcm(rawKey, 16);
        aesGcm.Decrypt(payload.AsSpan(0, 12), payload.AsSpan(12, plain.Length), payload.AsSpan(12 + plain.Length), plain);
        Encoding.UTF8.GetString(plain).Should().Be("securedata");
        provider.Decrypt(encrypted).Should().Be("securedata");
    }

    [Fact]
    public async Task GenerateKey_ShouldProduceValidBase64KeyThatRoundTrips()
    {
        // Arrange
        var key = MilvaCryptographyProvider.GenerateKey();
        var provider = CreateProvider(key);
        var bytes = RandomNumberGenerator.GetBytes(100);

        // Act & Assert
        key.Should().StartWith(MilvaCryptographyProvider.Base64KeyPrefix);
        Convert.FromBase64String(key[MilvaCryptographyProvider.Base64KeyPrefix.Length..]).Should().HaveCount(32);
        MilvaCryptographyProvider.GenerateKey().Should().NotBe(key);
        provider.Decrypt(provider.Encrypt("securedata")).Should().Be("securedata");
        (await provider.DecryptAsync(await provider.EncryptAsync("securedata"))).Should().Be("securedata");
        provider.DecryptBytes(provider.EncryptBytes(bytes)).Should().Equal(bytes);
    }

    [Fact]
    public void Base64Key_ShouldNotDecryptValuesOfUtf8KeyWithSameBytes()
    {
        // Arrange: same 32 bytes used as a UTF-8 key (HKDF derived) and as a raw key.
        var encrypted = CreateProvider(_key32).Encrypt("securedata");
        var rawKeyProvider = CreateProvider("base64:" + Convert.ToBase64String(Encoding.UTF8.GetBytes(_key32)));

        // Act
        Action act = () => rawKeyProvider.Decrypt(encrypted);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Base64Key_WithLegacyCbcValue_ShouldThrowCryptographicException()
    {
        // Arrange
        var provider = CreateProvider(MilvaCryptographyProvider.GenerateKey());

        // Act
        Action act = () => provider.Decrypt("evmtSvW8sFXeLueyevYMWtJID+xgtMqj4TDUT4NQ4wQ=");

        // Assert
        act.Should().Throw<CryptographicException>().WithMessage("*legacy format*");
    }

    [Theory]
    [InlineData("base64:")]
    [InlineData("base64:not base64 at all !!!!!!!!!!!!!!!!!!!!!!!!!!!")]
    [InlineData("base64:AAAAAAAAAAAAAAAAAAAAAA==")] // 16 bytes
    [InlineData("base64:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==")] // 34 bytes
    [InlineData("BASE64:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")] // prefix is case sensitive
    public void Base64Key_WithInvalidValue_ShouldThrowAtRegistration(string key)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Action act = () => services.AddMilvaCryptography().WithOptions(opt => opt.Key = key);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void WithOptions_WithBase64Key_ShouldRegisterProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var key = MilvaCryptographyProvider.GenerateKey();

        // Act
        services.AddMilvaCryptography().WithOptions(opt => opt.Key = key);
        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<Milvasoft.Cryptography.Abstract.IMilvaCryptographyProvider>();

        // Assert
        provider.Decrypt(provider.Encrypt("securedata")).Should().Be("securedata");
    }

    /// <summary>
    /// A UTF-8 key that happens to start with "base64:" is 16, 24 or 32 bytes and can never be a Base64 encoded 32 byte key,
    /// so it keeps working as a UTF-8 key (HKDF derived GCM key and legacy CBC key).
    /// </summary>
    [Fact]
    public void Utf8KeyStartingWithBase64Prefix_ShouldStillBeTreatedAsUtf8Key()
    {
        // Arrange
        const string utf8Key = "base64:abcdefghi"; // 16 bytes
        var provider = CreateProvider(utf8Key);
        var legacyValue = EncryptLegacyCbc(utf8Key, "securedata");

        // Act & Assert
        provider.Decrypt(legacyValue).Should().Be("securedata");
        provider.Decrypt(provider.Encrypt("securedata")).Should().Be("securedata");
    }

    #endregion

    #region String encryption

    [Theory]
    [InlineData("securedata")]
    [InlineData("")]
    [InlineData("Işık ğüşöç İstanbul 😀")]
    [InlineData("trailing\0nul\0")]
    public async Task EncryptAndDecrypt_ShouldRoundTrip(string data)
    {
        // Arrange
        var provider = CreateProvider(_key16);

        // Act
        var encrypted = provider.Encrypt(data);
        var encryptedAsync = await provider.EncryptAsync(data);

        // Assert
        encrypted.Should().StartWith("v2:");
        encrypted.Should().NotBe(encryptedAsync);
        provider.Decrypt(encrypted).Should().Be(data);
        (await provider.DecryptAsync(encryptedAsync)).Should().Be(data);
    }

    [Fact]
    public void Decrypt_WithTamperedValue_ShouldThrowCryptographicException()
    {
        // Arrange
        var provider = CreateProvider(_key16);
        var payload = Convert.FromBase64String(provider.Encrypt("securedata")[3..]);
        payload[15] ^= 0x01;
        var tampered = "v2:" + Convert.ToBase64String(payload);

        // Act
        Action act = () => provider.Decrypt(tampered);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_WithAnotherKey_ShouldThrowCryptographicException()
    {
        // Arrange
        var encrypted = CreateProvider(_key16).Encrypt("securedata");

        // Act
        Action act = () => CreateProvider("A=YI=+B_LfoK%V4r").Decrypt(encrypted);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    /// <summary>
    /// Values produced by the previous AES-CBC implementation (generated with the old code) must still be readable.
    /// </summary>
    [Theory]
    [InlineData(_key16, "evmtSvW8sFXeLueyevYMWtJID+xgtMqj4TDUT4NQ4wQ=", "securedata")]
    [InlineData(_key16, "vgs6vLmkgORFXvniUa3XVtO1H73QghymihpEzM5mAifI5/2pir8UCyo7A3HO9k1oxHpBY70nefh8AgcLbCSFXg==", "Işık ğüşöç İstanbul 😀")]
    [InlineData(_key16, "49TMQD0oANtuqV583EHiLiMZDwl8Md+tmzz7cdo8wrA=", "")]
    [InlineData(_key32, "Bt4NQRS3UACkzKYG8JWjDUIeqwXUr7Jy9sPjqdmkfWY=", "{\"a\":1}")]
    public async Task Decrypt_WithLegacyCbcValue_ShouldReturnPlainText(string key, string legacyValue, string expected)
    {
        // Arrange
        var provider = CreateProvider(key);

        // Act & Assert
        provider.Decrypt(legacyValue).Should().Be(expected);
        (await provider.DecryptAsync(legacyValue)).Should().Be(expected);
    }

    [Fact]
    public void MilvaEncryptionConverter_ShouldReadLegacyAndNewValues()
    {
        // Arrange
        var converter = new MilvaEncryptionConverter(CreateProvider(_key16));
        var toProvider = converter.ConvertToProviderExpression.Compile();
        var fromProvider = converter.ConvertFromProviderExpression.Compile();

        // Act
        var legacy = fromProvider("enc::evmtSvW8sFXeLueyevYMWtJID+xgtMqj4TDUT4NQ4wQ=");
        var stored = toProvider("securedata");
        var roundTrip = fromProvider(stored);

        // Assert
        legacy.Should().Be("securedata");
        stored.Should().StartWith("enc::v2:");
        roundTrip.Should().Be("securedata");
    }

    #endregion

    #region Bytes and files

    [Fact]
    public void EncryptBytesAndDecryptBytes_ShouldRoundTripArbitraryBytes()
    {
        // Arrange
        var provider = CreateProvider(_key32);
        var data = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();

        // Act
        var encrypted = provider.EncryptBytes(data);
        var decrypted = provider.DecryptBytes(encrypted);

        // Assert
        Encoding.ASCII.GetString(encrypted, 0, 5).Should().Be("MVENC");
        decrypted.Should().Equal(data);
    }

    [Fact]
    public async Task EncryptFileAndDecryptFile_WithBinaryContent_ShouldRestoreExactBytes()
    {
        // Arrange
        var provider = CreateProvider(_key16);
        var directory = CreateTempDirectory();
        var filePath = Path.Combine(directory, "binary.bin");
        var data = new byte[4096];
        new Random(42).NextBytes(data);
        data[0] = 0xFF; // invalid UTF-8
        data[1] = 0xFE;
        await File.WriteAllBytesAsync(filePath, data, TestContext.Current.CancellationToken);

        try
        {
            // Act
            await provider.EncryptFileAsync(filePath);
            var encrypted = await File.ReadAllBytesAsync(filePath, TestContext.Current.CancellationToken);
            await provider.DecryptFileAsync(filePath);
            var decryptedAsync = await File.ReadAllBytesAsync(filePath, TestContext.Current.CancellationToken);
            provider.EncryptFile(filePath);
            provider.DecryptFile(filePath);
            var decryptedSync = await File.ReadAllBytesAsync(filePath, TestContext.Current.CancellationToken);

            // Assert
            encrypted.Should().NotEqual(data);
            decryptedAsync.Should().Equal(data);
            decryptedSync.Should().Equal(data);
            Directory.GetFiles(directory).Should().ContainSingle();
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void DecryptFile_WithLegacyEncryptedFile_ShouldRestoreText()
    {
        // Arrange (file produced by the previous implementation's EncryptFile: UTF-8 BOM + Base64 text)
        var provider = CreateProvider(_key16);
        var directory = CreateTempDirectory();
        var filePath = Path.Combine(directory, "legacy.txt");
        File.WriteAllBytes(filePath, Convert.FromBase64String("77u/ejhFZGIwZnI0cEZFa3FDN3pkc2hxVTM2aUFvVytlT3lvUytNR1h1NUl3L01Rd2svWm9WMi8zUE1sUlJuVGtrdg=="));

        try
        {
            // Act
            provider.DecryptFile(filePath);

            // Assert
            File.ReadAllText(filePath).Should().Be("line1\nsatır 2 ışık\n");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void DecryptFile_WithWrongKey_ShouldLeaveOriginalFileUntouched()
    {
        // Arrange
        var directory = CreateTempDirectory();
        var filePath = Path.Combine(directory, "data.txt");
        File.WriteAllText(filePath, "secret");
        CreateProvider(_key16).EncryptFile(filePath);
        var encrypted = File.ReadAllBytes(filePath);

        try
        {
            // Act
            Action act = () => CreateProvider("A=YI=+B_LfoK%V4r").DecryptFile(filePath);

            // Assert
            act.Should().Throw<CryptographicException>();
            File.ReadAllBytes(filePath).Should().Equal(encrypted);
            Directory.GetFiles(directory).Should().ContainSingle();
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    #endregion

    /// <summary>
    /// Produces the legacy format: Base64(iv[16] | AES-CBC-PKCS7 ciphertext) with the UTF-8 key.
    /// </summary>
    private static string EncryptLegacyCbc(string key, string value)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.GenerateIV();
        var cipherText = aes.EncryptCbc(Encoding.UTF8.GetBytes(value), aes.IV, PaddingMode.PKCS7);

        return Convert.ToBase64String([.. aes.IV, .. cipherText]);
    }

    private static MilvaCryptographyProvider CreateProvider(string key) => new(new MilvaCryptographyOptions { Key = key }, new Microsoft.IO.RecyclableMemoryStreamManager());

    private static string CreateTempDirectory() => Directory.CreateTempSubdirectory("milva-crypto-tests-").FullName;
}
