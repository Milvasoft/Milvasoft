using FluentAssertions;
using Milvasoft.Cryptography.Builder;
using Milvasoft.Cryptography.Concrete;
using System.Security.Cryptography;

namespace Milvasoft.UnitTests.CryptographyTests;

[Trait("Cryptography Unit Tests", "Milvasoft.Cryptography project unit tests.")]
public class MilvaCryptographyTests
{
    #region Encrypt & Decrypt Async

    [Fact]
    public Task MilvaCryptographyProviderAsync_WithIncorrectKeySize_ShouldThrowException()
    {
        // Arrange
        var data = "securedata";
        var options = new MilvaCryptographyOptions
        {
            Key = "bc3voatyGOwvN/yfoX32pPfNJhLnVTBQEG9PIzKQ7I88l+qc7Tf4Z0wUqkVCUy5K"
        };
        var provider = new MilvaCryptographyProvider(options, new Microsoft.IO.RecyclableMemoryStreamManager());

        // Act
        Func<Task<string>> act = async () => await provider.EncryptAsync(data);

        // Assert
        return act.Should().ThrowAsync<CryptographicException>();
    }

    [Fact]
    public async Task MilvaCryptographyProviderAsync_WithCorrectKeySize_ShouldDecryptCorrectly()
    {
        // Arrange
        var data = "securedata";
        var options = new MilvaCryptographyOptions
        {
            Key = "A=YI=+B_LfoK%V4r"
        };
        var provider = new MilvaCryptographyProvider(options, new Microsoft.IO.RecyclableMemoryStreamManager());

        // Act
        var encryptedData = await provider.EncryptAsync(data);
        Func<Task<string>> act = async () => await provider.DecryptAsync(encryptedData);
        var decryptedData = await provider.DecryptAsync(encryptedData);

        // Assert
        await act.Should().NotThrowAsync();
        decryptedData.Should().Be(data);
    }

    #endregion

    #region Encrypt & Decrypt

    [Fact]
    public void MilvaCryptographyProvider_WithIncorrectKeySize_ShouldThrowException()
    {
        // Arrange
        var data = "securedata";
        var options = new MilvaCryptographyOptions
        {
            Key = "bc3voatyGOwvN/yfoX32pPfNJhLnVTBQEG9PIzKQ7I88l+qc7Tf4Z0wUqkVCUy5K"
        };
        var provider = new MilvaCryptographyProvider(options, new Microsoft.IO.RecyclableMemoryStreamManager());

        // Act
        Func<string> act = () => provider.Encrypt(data);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void MilvaCryptographyProvider_WithCorrectKeySize_ShouldDecryptCorrectly()
    {
        // Arrange
        var data = "securedata";
        var options = new MilvaCryptographyOptions
        {
            Key = "8RvJjg-oye5NhwqK"
        };
        var provider = new MilvaCryptographyProvider(options, new Microsoft.IO.RecyclableMemoryStreamManager());

        // Act
        var encryptedData = provider.Encrypt(data);
        Func<string> act = () => provider.Decrypt(encryptedData);
        var decryptedData = provider.Decrypt(encryptedData);

        // Assert
        act.Should().NotThrow();
        decryptedData.Should().Be(data);
    }

    #endregion
}
