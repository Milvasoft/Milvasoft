using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Identity.Abstract;
using Milvasoft.Identity.Concrete;
using Milvasoft.Identity.Concrete.Options;
using Milvasoft.Identity.TokenProvider;
using Milvasoft.Identity.TokenProvider.AuthToken;
using Moq;
using System.Text;
using static Milvasoft.UnitTests.IdentityTests.MilvaUserValidationTests;

namespace Milvasoft.UnitTests.IdentityTests;

[Trait("Identity Unit Tests", "Milvasoft.Identity project unit tests.")]
public class MilvaUserManagerTests
{
    private readonly Mock<IDataProtectionProvider> _dataProtectorMock;
    private readonly Mock<IMilvaPasswordHasher> _passwordHasherMock;
    private readonly MilvaIdentityOptions _identityOptions;

    public MilvaUserManagerTests()
    {
        _dataProtectorMock = new Mock<IDataProtectionProvider>();
        _passwordHasherMock = new Mock<IMilvaPasswordHasher>();

        _identityOptions = new MilvaIdentityOptions
        {
            Lockout = new MilvaLockoutOptions
            {
                AllowedForNewUsers = true,
                MaxFailedAccessAttempts = 3
            },
            Password = new MilvaPasswordOptions
            {
                RequiredLength = 8,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = true,
                RequiredUniqueChars = 2
            },
            SignIn = new MilvaSignInOptions
            {
                RequireConfirmedEmail = true,
                RequireConfirmedPhoneNumber = false
            },
            User = new MilvaUserOptions
            {
                RequireUniqueEmail = true,
                AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
            },
            Token = new TokenConfig { UseUtcForDateTimes = true }
        };
    }

    private MilvaUserManager<TestUser, Guid> CreateUserManager() => new(new Lazy<IDataProtectionProvider>(() => _dataProtectorMock.Object),
                                                                         _identityOptions,
                                                                         new Lazy<IMilvaPasswordHasher>(() => _passwordHasherMock.Object));

    [Fact]
    public void ConfigureForCreate_ShouldSetPasswordHashAndNormalizeFields()
    {
        // Arrange
        var user = new TestUser { UserName = "TestUser", Email = "test@example.com" };
        var password = "Password123!";

        _passwordHasherMock.Setup(h => h.HashPassword(password)).Returns("hashedPassword");

        var userManager = CreateUserManager();

        // Act
        userManager.ConfigureForCreate(ref user, password);

        // Assert
        user.PasswordHash.Should().Be("hashedPassword");
        user.LockoutEnabled.Should().BeTrue();
        user.NormalizedUserName.Should().Be("TESTUSER");
        user.NormalizedEmail.Should().Be("TEST@EXAMPLE.COM");
    }

    [Fact]
    public void CheckPassword_ShouldVerifyPasswordHash()
    {
        // Arrange
        var user = new TestUser { PasswordHash = "hashedPassword" };
        var password = "Password123!";

        _passwordHasherMock.Setup(h => h.VerifyHashedPassword("hashedPassword", password)).Returns(true);

        var userManager = CreateUserManager();

        // Act
        var result = userManager.CheckPassword(user, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidatePassword_ShouldReturnErrorMessageIfPasswordInvalid()
    {
        // Arrange
        var invalidPassword = "short";
        var userManager = CreateUserManager();

        // Act
        var result = userManager.ValidatePassword(invalidPassword);

        // Assert
        result.Should().Be(LocalizerKeys.IdentityPasswordTooShort);
    }

    [Fact]
    public void ValidatePassword_ShouldReturnNullIfPasswordValid()
    {
        // Arrange
        var validPassword = "Password123!";
        var userManager = CreateUserManager();

        // Act
        var result = userManager.ValidatePassword(validPassword);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void IsLockedOut_ShouldReturnTrueIfLockoutEndIsInFuture()
    {
        // Arrange
        var user = new TestUser { LockoutEnabled = true, LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10) };
        var userManager = CreateUserManager();

        // Act
        var result = userManager.IsLockedOut(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLockedOut_ShouldReturnFalseIfLockoutDisabled()
    {
        // Arrange
        var user = new TestUser { LockoutEnabled = false };
        var userManager = CreateUserManager();

        // Act
        var result = userManager.IsLockedOut(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateEmail_ShouldReturnErrorIfEmailInvalid()
    {
        // Arrange
        var invalidEmail = "invalid-email";
        var userManager = CreateUserManager();

        // Act
        var result = userManager.ValidateEmail(invalidEmail);

        // Assert
        result.Should().Be(LocalizerKeys.IdentityInvalidEmail);
    }

    [Fact]
    public void ValidateEmail_ShouldReturnNullIfEmailValid()
    {
        // Arrange
        var validEmail = "test@example.com";
        var userManager = CreateUserManager();

        // Act
        var result = userManager.ValidateEmail(validEmail);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ConfigureLockout_ShouldLockUserAfterMaxFailedAttempts()
    {
        // Arrange
        var user = new TestUser { LockoutEnabled = true, AccessFailedCount = 2 };
        var userManager = CreateUserManager();

        // Act
        var isLockedOut = userManager.ConfigureLockout(user, accessFailed: true);

        // Assert
        isLockedOut.Should().BeTrue();
        user.LockoutEnd.Should().NotBeNull();
        user.AccessFailedCount.Should().Be(0);
    }

    [Fact]
    public void ConfigureLockout_ShouldNotLockUserIfUnderThreshold()
    {
        // Arrange
        var user = new TestUser { LockoutEnabled = true, AccessFailedCount = 1 };
        var userManager = CreateUserManager();

        // Act
        var isLockedOut = userManager.ConfigureLockout(user, accessFailed: true);

        // Assert
        isLockedOut.Should().BeFalse();
        user.AccessFailedCount.Should().Be(2);
    }

    [Fact]
    public void ThrowIfUserInvalid_ShouldThrowIfValidationFails()
    {
        // Arrange
        var invalidUser = new TestUser { UserName = "!InvalidUser", Email = "invalid-email" };
        var userManager = CreateUserManager();

        // Act
        Action act = () => userManager.ThrowIfUserInvalid(invalidUser);

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>();
    }

    [Fact]
    public void ThrowIfUserInvalid_ShouldNotThrowIfUserValid()
    {
        // Arrange
        var validUser = new TestUser { UserName = "ValidUser", Email = "test@example.com" };
        var userManager = CreateUserManager();

        // Act
        Action act = () => userManager.ThrowIfUserInvalid(validUser);

        // Assert
        act.Should().NotThrow<MilvaUserFriendlyException>();
    }

    [Fact]
    public void GenerateUserToken_ShouldGenerateValidToken()
    {
        // Arrange
        var user = new TestUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "TestUser"
        };

        var purpose = Purpose.EmailConfirm;
        var mockProtector = new Mock<IDataProtector>();
        var generatedToken = "mocked-token";

        mockProtector
            .Setup(p => p.Protect(It.IsAny<byte[]>()))
            .Returns(Encoding.UTF8.GetBytes(generatedToken));

        _dataProtectorMock
            .Setup(dp => dp.CreateProtector(It.IsAny<string>()))
            .Returns(mockProtector.Object);

        var userManager = CreateUserManager();

        // Act
        var token = userManager.GenerateUserToken(user, purpose, true);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Be(Convert.ToBase64String(Encoding.UTF8.GetBytes(generatedToken)));
    }

    [Fact]
    public void VerifyUserToken_ShouldReturnFalseForInvalidToken()
    {
        // Arrange
        var user = new TestUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "TestUser"
        };

        var purpose = Purpose.EmailConfirm;
        var token = "invalid-token";

        var mockProtector = new Mock<IDataProtector>();
        mockProtector
            .Setup(p => p.Unprotect(It.IsAny<byte[]>()))
            .Throws(new Exception("Invalid token"));

        _dataProtectorMock
            .Setup(dp => dp.CreateProtector(It.IsAny<string>()))
            .Returns(mockProtector.Object);
        var userManager = CreateUserManager();

        // Act
        var isValid = userManager.VerifyUserToken(user, purpose, token, true);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyUserToken_ShouldReturnFalseForNullToken()
    {
        // Arrange
        var user = new TestUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "TestUser"
        };

        var purpose = Purpose.EmailConfirm;
        var userManager = CreateUserManager();

        // Act
        var isValid = userManager.VerifyUserToken(user, purpose, null, true);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void GenerateUserToken_ShouldThrowExceptionForNullUser()
    {
        // Arrange
        TestUser user = null;
        var purpose = Purpose.EmailConfirm;
        var userManager = CreateUserManager();

        // Act
        Action act = () => userManager.GenerateUserToken(user, purpose, true);

        // Assert
        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void VerifyUserToken_ShouldThrowExceptionForNullUser()
    {
        // Arrange
        TestUser user = null;
        var purpose = Purpose.EmailConfirm;
        var token = "valid-token";
        var userManager = CreateUserManager();

        // Act
        Action act = () => userManager.VerifyUserToken(user, purpose, token, true);

        // Assert
        act.Should().Throw<NullReferenceException>();
    }
}