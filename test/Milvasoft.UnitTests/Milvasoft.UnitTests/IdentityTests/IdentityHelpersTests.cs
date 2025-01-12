using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Milvasoft.Core.Exceptions;
using Milvasoft.Identity.Concrete;

namespace Milvasoft.UnitTests.IdentityTests;

[Trait("Identity Unit Tests", "Milvasoft.Identity project unit tests.")]
public class IdentityHelpersTests
{
    #region DescriptionJoin Tests

    [Fact]
    public void DescriptionJoin_WithIdentityResult_ShouldJoinDescriptionsWithDefaultSeparator()
    {
        // Arrange
        var errors = new List<IdentityError>
        {
            new() { Description = "Error1" },
            new() { Description = "Error2" },
            new() { Description = "Error3" }
        };
        var result = IdentityResult.Failed([.. errors]);

        // Act
        var joinedDescriptions = result.DescriptionJoin();

        // Assert
        joinedDescriptions.Should().Be("Error1~Error2~Error3");
    }

    [Fact]
    public void DescriptionJoin_WithIdentityResult_ShouldJoinDescriptionsWithCustomSeparator()
    {
        // Arrange
        var errors = new List<IdentityError>
        {
            new() { Description = "Error1" },
            new() { Description = "Error2" },
            new() { Description = "Error3" }
        };
        var result = IdentityResult.Failed([.. errors]);

        // Act
        var joinedDescriptions = result.DescriptionJoin(", ");

        // Assert
        joinedDescriptions.Should().Be("Error1, Error2, Error3");
    }

    [Fact]
    public void DescriptionJoin_WithErrorList_ShouldJoinDescriptionsWithDefaultSeparator()
    {
        // Arrange
        var errors = new List<IdentityError>
        {
            new() { Description = "Error1" },
            new() { Description = "Error2" }
        };

        // Act
        var joinedDescriptions = errors.DescriptionJoin();

        // Assert
        joinedDescriptions.Should().Be("Error1~Error2");
    }

    [Fact]
    public void DescriptionJoin_WithErrorList_ShouldJoinDescriptionsWithCustomSeparator()
    {
        // Arrange
        var errors = new List<IdentityError>
        {
            new() { Description = "Error1" },
            new() { Description = "Error2" }
        };

        // Act
        var joinedDescriptions = errors.DescriptionJoin(" | ");

        // Assert
        joinedDescriptions.Should().Be("Error1 | Error2");
    }

    #endregion

    #region ThrowErrorMessagesIfNotSuccess Tests

    [Fact]
    public void ThrowErrorMessagesIfNotSuccess_ShouldThrowExceptionWhenResultNotSucceeded()
    {
        // Arrange
        var errors = new List<IdentityError>
        {
            new() { Description = "Error1" },
            new() { Description = "Error2" }
        };
        var result = IdentityResult.Failed([.. errors]);

        // Act
        Action act = () => result.ThrowErrorMessagesIfNotSuccess();

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>()
           .WithMessage("Error1~Error2");
    }

    [Fact]
    public void ThrowErrorMessagesIfNotSuccess_ShouldNotThrowExceptionWhenResultSucceeded()
    {
        // Arrange
        var result = IdentityResult.Success;

        // Act
        Action act = () => result.ThrowErrorMessagesIfNotSuccess();

        // Assert
        act.Should().NotThrow<MilvaUserFriendlyException>();
    }

    #endregion

    #region GenerateRandomPassword Tests

    [Fact]
    public void GenerateRandomPassword_ShouldReturnPasswordWithRequiredProperties()
    {
        // Arrange
        var options = new PasswordOptions
        {
            RequiredLength = 10,
            RequireDigit = true,
            RequireLowercase = true,
            RequireUppercase = true,
            RequireNonAlphanumeric = true
        };

        // Act
        var password = options.GenerateRandomPassword();

        // Assert
        password.Length.Should().BeGreaterOrEqualTo(10);
        password.Any(char.IsDigit).Should().BeTrue();
        password.Any(char.IsLower).Should().BeTrue();
        password.Any(char.IsUpper).Should().BeTrue();
        password.Any(c => !char.IsLetterOrDigit(c)).Should().BeTrue();
    }

    [Fact]
    public void GenerateRandomPassword_WithDefaultValues_ShouldReturnValidPassword()
    {
        // Act
        var password = IdentityHelpers.GenerateRandomPassword();

        // Assert
        password.Length.Should().BeGreaterOrEqualTo(8);
    }

    #endregion

    #region CreateRefreshToken Tests

    [Fact]
    public void CreateRefreshToken_ShouldReturnValidBase64String()
    {
        // Act
        var refreshToken = IdentityHelpers.CreateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        Action act = () => Convert.FromBase64String(refreshToken);
        act.Should().NotThrow<FormatException>();
    }

    #endregion
}
