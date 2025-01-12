using FluentAssertions;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Identity.Concrete;
using Milvasoft.Types.Classes;
using Moq;

namespace Milvasoft.UnitTests.IdentityTests;

[Trait("Identity Unit Tests", "Milvasoft.Identity project unit tests.")]
public class MilvaIdentityDescriberTests
{
    private readonly Mock<IMilvaLocalizer> _localizerMock;
    private readonly MilvaIdentityDescriber _describer;

    public MilvaIdentityDescriberTests()
    {
        _localizerMock = new Mock<IMilvaLocalizer>();
        _describer = new MilvaIdentityDescriber(_localizerMock.Object);
    }

    [Fact]
    public void DefaultError_ShouldReturnLocalizedError()
    {
        // Arrange
        _localizerMock.Setup(l => l[LocalizerKeys.IdentityDefaultError]).Returns(new LocalizedValue(LocalizerKeys.IdentityDefaultError, "Localized Default Error Message"));

        // Act
        var error = _describer.DefaultError();

        // Assert
        error.Code.Should().Be("DefaultError");
        error.Description.Should().Be("Localized Default Error Message");
    }

    [Fact]
    public void DuplicateUserName_ShouldReturnLocalizedError()
    {
        // Arrange
        _localizerMock.Setup(l => l[LocalizerKeys.IdentityDuplicateUsername]).Returns(new LocalizedValue(LocalizerKeys.IdentityDuplicateUsername, "Localized Duplicate Username Message"));

        // Act
        var error = _describer.DuplicateUserName("testUser");

        // Assert
        error.Code.Should().Be("DuplicateUserName");
        error.Description.Should().Be("Localized Duplicate Username Message");
    }

    [Fact]
    public void DuplicateEmail_ShouldReturnLocalizedError()
    {
        // Arrange
        _localizerMock.Setup(l => l[LocalizerKeys.IdentityDuplicateEmail]).Returns(new LocalizedValue(LocalizerKeys.IdentityDuplicateEmail, "Localized Duplicate Email Message"));

        // Act
        var error = _describer.DuplicateEmail("test@example.com");

        // Assert
        error.Code.Should().Be("DuplicateEmail");
        error.Description.Should().Be("Localized Duplicate Email Message");
    }

    [Fact]
    public void InvalidUserName_ShouldReturnLocalizedError()
    {
        // Arrange
        _localizerMock.Setup(l => l[LocalizerKeys.IdentityInvalidUserName]).Returns(new LocalizedValue(LocalizerKeys.IdentityInvalidUserName, "Localized Invalid Username Message"));

        // Act
        var error = _describer.InvalidUserName("testUser");

        // Assert
        error.Code.Should().Be("InvalidUserName");
        error.Description.Should().Be("Localized Invalid Username Message");
    }

    [Fact]
    public void InvalidEmail_ShouldReturnLocalizedError()
    {
        // Arrange
        _localizerMock.Setup(l => l[LocalizerKeys.IdentityInvalidEmail]).Returns(new LocalizedValue(LocalizerKeys.IdentityInvalidEmail, "Localized Invalid Email Message"));

        // Act
        var error = _describer.InvalidEmail("test@example.com");

        // Assert
        error.Code.Should().Be("InvalidEmail");
        error.Description.Should().Be("Localized Invalid Email Message");
    }

    [Fact]
    public void ConcurrencyFailure_ShouldReturnLocalizedError()
    {
        // Arrange
        _localizerMock.Setup(l => l[LocalizerKeys.IdentityConcurrencyFailure]).Returns(new LocalizedValue(LocalizerKeys.IdentityConcurrencyFailure, "Localized Concurrency Failure Message"));

        // Act
        var error = _describer.ConcurrencyFailure();

        // Assert
        error.Code.Should().Be("ConcurrencyFailure");
        error.Description.Should().Be("Localized Concurrency Failure Message");
    }

    [Fact]
    public void PasswordTooShort_ShouldReturnLocalizedError()
    {
        // Arrange
        _localizerMock.Setup(l => l[LocalizerKeys.IdentityPasswordTooShort]).Returns(new LocalizedValue(LocalizerKeys.IdentityPasswordTooShort, "Password is too short"));

        // Act
        var error = _describer.PasswordTooShort(6);

        // Assert
        error.Code.Should().Be("PasswordTooShort");
        error.Description.Should().Be("Password is too short");
    }

    [Fact]
    public void PasswordRequiresUpper_ShouldReturnLocalizedError()
    {
        // Arrange
        _localizerMock.Setup(l => l[LocalizerKeys.IdentityPasswordRequiresUpper]).Returns(new LocalizedValue(LocalizerKeys.IdentityPasswordRequiresUpper, "Password must contain an uppercase letter"));

        // Act
        var error = _describer.PasswordRequiresUpper();

        // Assert
        error.Code.Should().Be("PasswordRequiresUpper");
        error.Description.Should().Be("Password must contain an uppercase letter");
    }

    [Fact]
    public void PasswordRequiresLower_ShouldReturnLocalizedError()
    {
        // Arrange
        _localizerMock.Setup(l => l[LocalizerKeys.IdentityPasswordRequiresLower]).Returns(new LocalizedValue(LocalizerKeys.IdentityPasswordRequiresLower, "Password must contain a lowercase letter"));

        // Act
        var error = _describer.PasswordRequiresLower();

        // Assert
        error.Code.Should().Be("PasswordRequiresLower");
        error.Description.Should().Be("Password must contain a lowercase letter");
    }
}