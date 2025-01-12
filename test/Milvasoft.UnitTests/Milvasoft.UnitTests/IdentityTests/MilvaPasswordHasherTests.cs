using FluentAssertions;
using Microsoft.Extensions.Options;
using Milvasoft.Identity.Concrete;
using Milvasoft.Identity.Concrete.Options;
using System.Security.Cryptography;

namespace Milvasoft.UnitTests.IdentityTests;

[Trait("Identity Unit Tests", "Milvasoft.Identity project unit tests.")]
public class MilvaPasswordHasherTests
{
    private readonly MilvaPasswordHasher _hasher;

    public MilvaPasswordHasherTests()
    {
        var options = Options.Create(new MilvaPasswordHasherOptions
        {
            IterationCount = 10000,
            RandomNumberGenerator = RandomNumberGenerator.Create()
        });
        _hasher = new MilvaPasswordHasher(options);
    }

    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "testPassword";

        // Act
        var hashedPassword = _hasher.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_WithNullPassword_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _hasher.HashPassword(null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*password*");
    }

    [Fact]
    public void VerifyHashedPassword_ShouldReturnTrueForValidPassword()
    {
        // Arrange
        var password = "testPassword";
        var hashedPassword = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyHashedPassword(hashedPassword, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyHashedPassword_ShouldReturnFalseForInvalidPassword()
    {
        // Arrange
        var password = "testPassword";
        var wrongPassword = "wrongPassword";
        var hashedPassword = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyHashedPassword(hashedPassword, wrongPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyHashedPassword_WithNullHashedPassword_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _hasher.VerifyHashedPassword(null, "testPassword");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*hashedPassword*");
    }

    [Fact]
    public void VerifyHashedPassword_WithNullProvidedPassword_ShouldThrowArgumentNullException()
    {
        // Arrange
        var hashedPassword = _hasher.HashPassword("testPassword");

        // Act
        Action act = () => _hasher.VerifyHashedPassword(hashedPassword, null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*providedPassword*");
    }

    [Fact]
    public void VerifyHashedPassword_ShouldReturnFalseForInvalidFormat()
    {
        // Arrange
        var invalidHashedPassword = "invalidFormat";

        // Act
        var result = _hasher.VerifyHashedPassword(invalidHashedPassword, "testPassword");

        // Assert
        result.Should().BeFalse();
    }
}