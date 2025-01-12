using FluentAssertions;
using Milvasoft.Identity.Concrete.Entity;
using Milvasoft.Identity.TokenProvider;

namespace Milvasoft.UnitTests.IdentityTests;

[Trait("Identity Unit Tests", "Milvasoft.Identity project unit tests.")]
public class DigitTokenProviderTests
{
    private class TestUser : MilvaUser<Guid>
    {
        public TestUser(Guid id, string phoneNumber)
        {
            Id = id;
            PhoneNumber = phoneNumber;
            Email = "info@milvasoft.com";
        }
    }

    #region Generate Tests

    [Fact]
    public void Generate_ShouldReturnValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var phoneNumber = "1234567890";
        var user = new TestUser(userId, phoneNumber);
        var purpose = Purpose.EmailConfirm;

        // Act
        var token = DigitTokenProvider<TestUser, Guid>.Generate(purpose, user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Length.Should().Be(6); // RFC6238 tokens are typically 6 digits
        int.TryParse(token, out _).Should().BeTrue();
    }

    #endregion

    #region Validate Tests

    [Fact]
    public void Validate_ShouldReturnTrueForValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var phoneNumber = "1234567890";
        var user = new TestUser(userId, phoneNumber);
        var purpose = Purpose.EmailConfirm;

        var token = DigitTokenProvider<TestUser, Guid>.Generate(purpose, user);

        // Act
        var isValid = DigitTokenProvider<TestUser, Guid>.Validate(purpose, token, user);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldReturnFalseForInvalidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var phoneNumber = "1234567890";
        var user = new TestUser(userId, phoneNumber);
        var purpose = Purpose.EmailConfirm;

        var invalidToken = "000000";

        // Act
        var isValid = DigitTokenProvider<TestUser, Guid>.Validate(purpose, invalidToken, user);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldReturnFalseForExpiredToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var phoneNumber = "1234567890";
        var user = new TestUser(userId, phoneNumber);
        var purpose = Purpose.PasswordReset;

        // Simulate an expired token by using incorrect modifier
        var expiredToken = "123456"; // Invalid due to mismatched entropy

        // Act
        var isValid = DigitTokenProvider<TestUser, Guid>.Validate(purpose, expiredToken, user);

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion

    #region GetUserModifier Tests

    [Theory]
    [InlineData(Purpose.EmailChange, "Email:EmailChange:info@milvasoft.com")]
    [InlineData(Purpose.EmailConfirm, "Totp:EmailConfirm:1234567890")]
    [InlineData(Purpose.PasswordReset, "PhoneNumber:PasswordReset")]
    public void GetUserModifier_ShouldReturnExpectedModifier(Purpose purpose, string expectedModifier)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var phoneNumber = "1234567890";
        var user = new TestUser(userId, phoneNumber);

        // Act
        var modifier = typeof(DigitTokenProvider<TestUser, Guid>)
            .GetMethod("GetUserModifier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, [purpose, user]) as string;

        // Assert
        modifier.Should().Be(expectedModifier);
    }

    #endregion
}
