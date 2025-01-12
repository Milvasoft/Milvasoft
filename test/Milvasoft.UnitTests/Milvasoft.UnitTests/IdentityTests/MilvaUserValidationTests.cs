using FluentAssertions;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Identity.Concrete;
using Milvasoft.Identity.Concrete.Entity;
using Milvasoft.Types.Classes;
using Moq;

namespace Milvasoft.UnitTests.IdentityTests;

[Trait("Identity Unit Tests", "Milvasoft.Identity project unit tests.")]
public class MilvaUserValidationTests
{
    public class TestUser : MilvaUser<Guid>
    {
        public TestUser()
        {

        }
    }

    private readonly Mock<IMilvaLocalizer> _localizerMock;
    private readonly MilvaUserValidation<TestUser, Guid> _validator;

    public MilvaUserValidationTests()
    {
        _localizerMock = new Mock<IMilvaLocalizer>();
        _localizerMock.Setup(l => l[LocalizerKeys.UserValidationUserNameNumberStartWith])
                      .Returns(new LocalizedValue(LocalizerKeys.UserValidationUserNameNumberStartWith, "Username cannot start with a number."));
        _localizerMock.Setup(l => l[LocalizerKeys.UserValidationUserNameLength])
                      .Returns(new LocalizedValue(LocalizerKeys.UserValidationUserNameLength, "Username must be between 3 and 25 characters."));
        _localizerMock.Setup(l => l[LocalizerKeys.UserValidationEmailLength])
                      .Returns(new LocalizedValue(LocalizerKeys.UserValidationEmailLength, "Email cannot exceed 70 characters."));

        _validator = new MilvaUserValidation<TestUser, Guid>(_localizerMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnError_WhenUserNameStartsWithNumber()
    {
        // Arrange
        var user = new TestUser { UserName = "1TestUser", Email = "test@example.com" };

        // Act
        var result = await _validator.ValidateAsync(user);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "UserNameNumberStartWith" &&
                                                   e.Description == "Username cannot start with a number.");
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnError_WhenUserNameLengthIsInvalid()
    {
        // Arrange
        var user = new TestUser { UserName = "Us", Email = "test@example.com" };

        // Act
        var result = await _validator.ValidateAsync(user);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "UserNameLength" &&
                                                   e.Description == "Username must be between 3 and 25 characters.");
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnError_WhenEmailExceedsMaxLength()
    {
        // Arrange
        var user = new TestUser { UserName = "ValidUser", Email = new string('a', 71) + "@example.com" };

        // Act
        var result = await _validator.ValidateAsync(user);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "EmailLength" &&
                                                   e.Description == "Email cannot exceed 70 characters.");
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenUserIsValid()
    {
        // Arrange
        var user = new TestUser { UserName = "ValidUser", Email = "test@example.com" };

        // Act
        var result = await _validator.ValidateAsync(user);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}