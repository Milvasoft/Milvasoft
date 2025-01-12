using FluentAssertions;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Identity.Concrete;

namespace Milvasoft.UnitTests.IdentityTests;

[Trait("Identity Unit Tests", "Milvasoft.Identity project unit tests.")]
public class MilvaIdentityExceptionThrowerTests
{
    [Fact]
    public void ThrowDefaultError_ShouldThrowMilvaUserFriendlyException()
    {
        // Act
        Action act = () => MilvaIdentityExceptionThrower.ThrowDefaultError();

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>()
           .WithMessage(LocalizerKeys.IdentityDefaultError);
    }

    [Fact]
    public void ThrowDuplicateUserName_ShouldThrowMilvaUserFriendlyException()
    {
        // Act
        Action act = () => MilvaIdentityExceptionThrower.ThrowDuplicateUserName();

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>()
           .WithMessage(LocalizerKeys.IdentityDuplicateUsername);
    }

    [Fact]
    public void ThrowDuplicateEmail_ShouldThrowMilvaUserFriendlyException()
    {
        // Act
        Action act = () => MilvaIdentityExceptionThrower.ThrowDuplicateEmail();

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>()
           .WithMessage(LocalizerKeys.IdentityDuplicateEmail);
    }

    [Fact]
    public void ThrowInvalidUserName_ShouldThrowMilvaUserFriendlyException()
    {
        // Act
        Action act = () => MilvaIdentityExceptionThrower.ThrowInvalidUserName();

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>()
           .WithMessage(LocalizerKeys.IdentityInvalidUserName);
    }

    [Fact]
    public void ThrowInvalidEmail_ShouldThrowMilvaUserFriendlyException()
    {
        // Act
        Action act = () => MilvaIdentityExceptionThrower.ThrowInvalidEmail();

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>()
           .WithMessage(LocalizerKeys.IdentityInvalidEmail);
    }

    [Fact]
    public void ThrowConcurrencyFailure_ShouldThrowMilvaUserFriendlyException()
    {
        // Act
        Action act = () => MilvaIdentityExceptionThrower.ThrowConcurrencyFailure();

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>()
           .WithMessage(LocalizerKeys.IdentityConcurrencyFailure);
    }

    [Fact]
    public void ThrowDuplicateRoleName_ShouldThrowMilvaUserFriendlyException()
    {
        // Act
        Action act = () => MilvaIdentityExceptionThrower.ThrowDuplicateRoleName();

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>()
           .WithMessage(LocalizerKeys.IdentityDuplicateRoleName);
    }

    [Fact]
    public void ThrowPasswordRequiresUpper_ShouldThrowMilvaUserFriendlyException()
    {
        // Act
        Action act = () => MilvaIdentityExceptionThrower.ThrowPasswordRequiresUpper();

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>()
           .WithMessage(LocalizerKeys.IdentityPasswordRequiresUpper);
    }

    [Fact]
    public void ThrowPasswordTooShort_ShouldThrowMilvaUserFriendlyException()
    {
        // Act
        Action act = () => MilvaIdentityExceptionThrower.ThrowPasswordTooShort();

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>()
           .WithMessage(LocalizerKeys.IdentityPasswordTooShort);
    }

    [Fact]
    public void ThrowNotAllowed_ShouldThrowMilvaUserFriendlyException()
    {
        // Act
        Action act = () => MilvaIdentityExceptionThrower.ThrowNotAllowed();

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>()
           .WithMessage(LocalizerKeys.NotAllowed);
    }

    [Fact]
    public void ThrowLocked_ShouldThrowMilvaUserFriendlyException()
    {
        // Act
        Action act = () => MilvaIdentityExceptionThrower.ThrowLocked();

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>()
           .WithMessage(LocalizerKeys.Locked);
    }

    [Fact]
    public void ThrowLockedWarning_ShouldThrowMilvaUserFriendlyException()
    {
        // Act
        Action act = () => MilvaIdentityExceptionThrower.ThrowLockedWarning();

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>()
           .WithMessage(LocalizerKeys.LockWarning);
    }
}