using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Milvasoft.Core.Helpers;
using Milvasoft.Identity.TokenProvider;
using Moq;
using System.Text;

namespace Milvasoft.UnitTests.IdentityTests;

[Trait("Identity Unit Tests", "Milvasoft.Identity project unit tests.")]
public class DataProtectorExtensionsTests
{
    private const string _stamp = "c3RhbXA=";

    private static (Mock<IDataProtectionProvider> Provider, string Token) CreateToken(string purpose, Guid userId, string stamp, DateTimeOffset creationTime)
    {
        var mockProtectorProvider = new Mock<IDataProtectionProvider>();
        var mockProtector = new Mock<IDataProtector>();

        var memoryStream = new MemoryStream();
        using (var writer = memoryStream.CreateWriter())
        {
            writer.Write(creationTime);
            writer.Write(userId.ToString());
            writer.Write(purpose);
            writer.Write(stamp);
        }

        var tokenBytes = memoryStream.ToArray();
        mockProtectorProvider.Setup(p => p.CreateProtector(purpose)).Returns(mockProtector.Object);
        mockProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns(tokenBytes);

        return (mockProtectorProvider, Convert.ToBase64String(tokenBytes));
    }

    /// <summary>
    /// Tokens issued by the previous version carried an empty stamp and were accepted for any user state. They must be rejected.
    /// </summary>
    [Fact]
    public void Validate_ShouldReturnFalseForTokenWithEmptyStamp()
    {
        var userId = Guid.NewGuid();
        var (provider, token) = CreateToken("TestPurpose", userId, string.Empty, DateTimeOffset.UtcNow);

        DataProtectorExtensions.Validate(provider.Object, "TestPurpose", token, userId, _stamp).Should().BeFalse();
        DataProtectorExtensions.Validate(provider.Object, "TestPurpose", token, userId, string.Empty).Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldReturnFalseForChangedStamp()
    {
        var userId = Guid.NewGuid();
        var (provider, token) = CreateToken("TestPurpose", userId, _stamp, DateTimeOffset.UtcNow);

        DataProtectorExtensions.Validate(provider.Object, "TestPurpose", token, userId, _stamp).Should().BeTrue();
        DataProtectorExtensions.Validate(provider.Object, "TestPurpose", token, userId, "b3RoZXI=").Should().BeFalse();
    }

    [Fact]
    public void Generate_WithEmptyStamp_ShouldThrow()
    {
        var provider = new Mock<IDataProtectionProvider>();

        Action act = () => DataProtectorExtensions.Generate(provider.Object, "TestPurpose", Guid.NewGuid(), string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    #region Generate Tests

    [Fact]
    public void Generate_ShouldReturnProtectedToken()
    {
        // Arrange
        var mockProtectorProvider = new Mock<IDataProtectionProvider>();
        var mockProtector = new Mock<IDataProtector>();

        mockProtectorProvider.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(mockProtector.Object);
        mockProtector.Setup(p => p.Protect(It.IsAny<byte[]>())).Returns<byte[]>(b => b);

        var purpose = "TestPurpose";
        var userId = Guid.NewGuid();
        var useUtcForDateTimes = true;

        // Act
        var token = DataProtectorExtensions.Generate(mockProtectorProvider.Object, purpose, userId, _stamp);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Validate Tests

    [Fact]
    public void Validate_ShouldReturnTrueForValidToken()
    {
        // Arrange
        var mockProtectorProvider = new Mock<IDataProtectionProvider>();
        var mockProtector = new Mock<IDataProtector>();

        var purpose = "TestPurpose";
        var userId = Guid.NewGuid();
        var useUtcForDateTimes = true;

        var memoryStream = new MemoryStream();
        using (var writer = memoryStream.CreateWriter())
        {
            writer.Write(CommonHelper.GetDateTimeOffsetNow(useUtcForDateTimes));
            writer.Write(userId.ToString());
            writer.Write(purpose);
            writer.Write(_stamp);
        }

        var tokenBytes = memoryStream.ToArray();
        mockProtectorProvider.Setup(p => p.CreateProtector(purpose)).Returns(mockProtector.Object);
        mockProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns(tokenBytes);

        var token = Convert.ToBase64String(tokenBytes);

        // Act
        var isValid = DataProtectorExtensions.Validate(mockProtectorProvider.Object, purpose, token, userId, _stamp);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldReturnFalseForExpiredToken()
    {
        // Arrange
        var mockProtectorProvider = new Mock<IDataProtectionProvider>();
        var mockProtector = new Mock<IDataProtector>();

        var purpose = "TestPurpose";
        var userId = Guid.NewGuid();
        var useUtcForDateTimes = true;

        var memoryStream = new MemoryStream();
        using (var writer = memoryStream.CreateWriter())
        {
            writer.Write(CommonHelper.GetDateTimeOffsetNow(useUtcForDateTimes).AddDays(-2));
            writer.Write(userId.ToString());
            writer.Write(purpose);
            writer.Write(_stamp);
        }

        var tokenBytes = memoryStream.ToArray();
        mockProtectorProvider.Setup(p => p.CreateProtector(purpose)).Returns(mockProtector.Object);
        mockProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns(tokenBytes);

        var token = Convert.ToBase64String(tokenBytes);

        // Act
        var isValid = DataProtectorExtensions.Validate(mockProtectorProvider.Object, purpose, token, userId, _stamp);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldReturnFalseForInvalidUserId()
    {
        // Arrange
        var mockProtectorProvider = new Mock<IDataProtectionProvider>();
        var mockProtector = new Mock<IDataProtector>();

        var purpose = "TestPurpose";
        var validUserId = Guid.NewGuid();
        var invalidUserId = Guid.NewGuid();
        var useUtcForDateTimes = true;

        var memoryStream = new MemoryStream();
        using (var writer = memoryStream.CreateWriter())
        {
            writer.Write(CommonHelper.GetDateTimeOffsetNow(useUtcForDateTimes));
            writer.Write(validUserId.ToString());
            writer.Write(purpose);
            writer.Write(_stamp);
        }

        var tokenBytes = memoryStream.ToArray();
        mockProtectorProvider.Setup(p => p.CreateProtector(purpose)).Returns(mockProtector.Object);
        mockProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns(tokenBytes);

        var token = Convert.ToBase64String(tokenBytes);

        // Act
        var isValid = DataProtectorExtensions.Validate(mockProtectorProvider.Object, purpose, token, invalidUserId, _stamp);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldReturnFalseForInvalidPurpose()
    {
        // Arrange
        var mockProtectorProvider = new Mock<IDataProtectionProvider>();
        var mockProtector = new Mock<IDataProtector>();

        var validPurpose = "ValidPurpose";
        var invalidPurpose = "InvalidPurpose";
        var userId = Guid.NewGuid();
        var useUtcForDateTimes = true;

        var memoryStream = new MemoryStream();
        using (var writer = memoryStream.CreateWriter())
        {
            writer.Write(CommonHelper.GetDateTimeOffsetNow(useUtcForDateTimes));
            writer.Write(userId.ToString());
            writer.Write(validPurpose);
            writer.Write(_stamp);
        }

        var tokenBytes = memoryStream.ToArray();
        mockProtectorProvider.Setup(p => p.CreateProtector(validPurpose)).Returns(mockProtector.Object);
        mockProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns(tokenBytes);

        var token = Convert.ToBase64String(tokenBytes);

        // Act
        var isValid = DataProtectorExtensions.Validate(mockProtectorProvider.Object, invalidPurpose, token, userId, _stamp);

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion
}
internal static class StreamExtensions
{
    internal static readonly Encoding _defaultEncoding = new UTF8Encoding(false, true);

    public static BinaryReader CreateReader(this Stream stream) => new(stream, _defaultEncoding, true);

    public static BinaryWriter CreateWriter(this Stream stream) => new(stream, _defaultEncoding, true);

    public static DateTimeOffset ReadDateTimeOffset(this BinaryReader reader) => new(reader.ReadInt64(), TimeSpan.Zero);

    public static void Write(this BinaryWriter writer, DateTimeOffset value) => writer.Write(value.UtcTicks);
}
