using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
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
    private readonly Mock<IDataProtectionProvider> _dataProtectionProviderMock;
    private readonly Mock<IMilvaPasswordHasher> _passwordHasherMock;
    private readonly MilvaIdentityOptions _identityOptions;
    private readonly MilvaUserManager<TestUser, Guid> _userManager;

    public MilvaUserManagerTests()
    {
        _dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
        _passwordHasherMock = new Mock<IMilvaPasswordHasher>();
        _identityOptions = new MilvaIdentityOptions
        {
            Lockout = new MilvaLockoutOptions { AllowedForNewUsers = true, MaxFailedAccessAttempts = 3 },
            Password = new MilvaPasswordOptions { RequiredLength = 8, RequireNonAlphanumeric = true },
            SignIn = new MilvaSignInOptions { RequireConfirmedEmail = true },
            Token = new TokenConfig { UseUtcForDateTimes = true }
        };

        _userManager = new MilvaUserManager<TestUser, Guid>(
            new Lazy<IDataProtectionProvider>(() => _dataProtectionProviderMock.Object),
            _identityOptions,
            new Lazy<IMilvaPasswordHasher>(() => _passwordHasherMock.Object)
        );
    }

    [Fact]
    public void ConfigureForCreate_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var user = new TestUser
        {
            UserName = "TestUser",
            Email = "test@example.com"
        };
        var password = "Password@123";
        var hashedPassword = "hashedPassword";

        _passwordHasherMock.Setup(ph => ph.HashPassword(password)).Returns(hashedPassword);

        // Act
        _userManager.ConfigureForCreate(ref user, password);

        // Assert
        user.PasswordHash.Should().Be(hashedPassword);
        user.LockoutEnabled.Should().BeTrue();
        user.NormalizedUserName.Should().Be("TESTUSER");
        user.NormalizedEmail.Should().Be("TEST@EXAMPLE.COM");
    }

    [Fact]
    public void ConfigureForCreate_ShouldNotEnableLockoutIfOptionIsDisabled()
    {
        // Arrange
        var user = new TestUser
        {
            UserName = "TestUser",
            Email = "test@example.com"
        };
        var password = "Password@123";
        var hashedPassword = "hashedPassword";

        _identityOptions.Lockout.AllowedForNewUsers = false;
        _passwordHasherMock.Setup(ph => ph.HashPassword(password)).Returns(hashedPassword);

        // Act
        _userManager.ConfigureForCreate(ref user, password);

        // Assert
        user.LockoutEnabled.Should().BeFalse();
    }

    [Fact]
    public void ConfigureForCreate_ShouldThrowIfPasswordIsNull()
    {
        // Arrange
        var user = new TestUser
        {
            UserName = "TestUser",
            Email = "test@example.com"
        };

        var userManager = new MilvaUserManager<TestUser, Guid>(
            new Lazy<IDataProtectionProvider>(() => _dataProtectionProviderMock.Object),
            _identityOptions,
            new Lazy<IMilvaPasswordHasher>(() => new MilvaPasswordHasher(Options.Create(new MilvaPasswordHasherOptions
            {
                IterationCount = 10000
            }))));

        // Act
        Action act = () => userManager.ConfigureForCreate(ref user, null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ConfigureForCreate_ShouldThrowIfUserIsNull()
    {
        // Arrange
        TestUser user = null;
        var password = "Password@123";

        // Act
        Action act = () => _userManager.ConfigureForCreate(ref user, password);

        // Assert
        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void CheckPassword_WithValidPassword_ShouldReturnTrue()
    {
        // Arrange
        var user = new TestUser { PasswordHash = "hashedPassword" };
        var password = "Password123!";
        _passwordHasherMock.Setup(x => x.VerifyHashedPasswordWithRehashCheck(user.PasswordHash, password))
                          .Returns(PasswordVerificationResult.Success);

        // Act
        var result = _userManager.CheckPassword(user, password);

        // Assert
        result.Should().BeTrue();
        user.PasswordHash.Should().Be("hashedPassword");
        _passwordHasherMock.Verify(x => x.VerifyHashedPasswordWithRehashCheck(user.PasswordHash, password), Times.Once);
    }

    [Fact]
    public void CheckPassword_WithInvalidPassword_ShouldReturnFalse()
    {
        // Arrange
        var user = new TestUser { PasswordHash = "hashedPassword" };
        var password = "WrongPassword123!";
        _passwordHasherMock.Setup(x => x.VerifyHashedPasswordWithRehashCheck(user.PasswordHash, password))
                          .Returns(PasswordVerificationResult.Failed);

        // Act
        var result = _userManager.CheckPassword(user, password);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckPassword_WithLegacyHash_ShouldReturnTrueAndUpgradeHash()
    {
        // Arrange: hash created with the previous defaults (HMAC-SHA256, 10,000 iterations).
        var legacyHasher = new MilvaPasswordHasher(new MilvaPasswordHasherOptions
        {
            IterationCount = 10000,
            Prf = Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivationPrf.HMACSHA256
        });
        var currentHasher = new MilvaPasswordHasher(new MilvaPasswordHasherOptions { IterationCount = 20000 });
        var userManager = new MilvaUserManager<TestUser, Guid>(new Lazy<IDataProtectionProvider>(() => _dataProtectionProviderMock.Object),
                                                               _identityOptions,
                                                               new Lazy<IMilvaPasswordHasher>(() => currentHasher));
        var password = "Password123!";
        var legacyHash = legacyHasher.HashPassword(password);
        var user = new TestUser { PasswordHash = legacyHash };

        // Act
        var result = userManager.CheckPassword(user, password);

        // Assert
        result.Should().BeTrue();
        user.PasswordHash.Should().NotBe(legacyHash);
        currentHasher.VerifyHashedPasswordWithRehashCheck(user.PasswordHash, password).Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void CheckPassword_WithWrongPassword_ShouldNotChangeHash()
    {
        // Arrange
        var user = new TestUser { PasswordHash = "hashedPassword" };
        _passwordHasherMock.Setup(x => x.VerifyHashedPasswordWithRehashCheck(user.PasswordHash, "wrong"))
                           .Returns(PasswordVerificationResult.Failed);

        // Act
        var result = _userManager.CheckPassword(user, "wrong");

        // Assert
        result.Should().BeFalse();
        user.PasswordHash.Should().Be("hashedPassword");
        _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ValidateUser_WithRequireUniqueEmailAndValidEmail_ShouldStillValidatePassword()
    {
        // Arrange
        var user = new TestUser
        {
            UserName = "validuser",
            Email = "valid@email.com"
        };
        _identityOptions.User.RequireUniqueEmail = true;

        // Act
        var result = _userManager.ValidateUser(user, "short");

        // Assert
        result.Should().Be(LocalizerKeys.IdentityPasswordTooShort);
    }

    [Fact]
    public void UserToken_ShouldRoundTripAndBeInvalidatedByPasswordChange()
    {
        // Arrange
        var userManager = CreateUserManagerWithRealDataProtection();
        var user = new TestUser { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "TestUser", PasswordHash = "hash-1" };

        // Act
        var token = userManager.GenerateUserToken(user, Purpose.PasswordReset);
        var validBeforeChange = userManager.VerifyUserToken(user, Purpose.PasswordReset, token);

        user.PasswordHash = "hash-2";

        var validAfterChange = userManager.VerifyUserToken(user, Purpose.PasswordReset, token);

        // Assert
        validBeforeChange.Should().BeTrue();
        validAfterChange.Should().BeFalse();
    }

    [Fact]
    public void UserToken_EmailConfirm_ShouldBeSingleUse()
    {
        // Arrange
        var userManager = CreateUserManagerWithRealDataProtection();
        var user = new TestUser { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "TestUser" };
        var token = userManager.GenerateUserToken(user, Purpose.EmailConfirm);

        // Act & Assert
        userManager.VerifyUserToken(user, Purpose.EmailConfirm, token).Should().BeTrue();

        user.EmailConfirmed = true;

        userManager.VerifyUserToken(user, Purpose.EmailConfirm, token).Should().BeFalse();
    }

    [Fact]
    public void UserToken_IssuedForAnotherUser_ShouldBeInvalid()
    {
        // Arrange
        var userManager = CreateUserManagerWithRealDataProtection();
        var user = new TestUser { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "TestUser" };
        var otherUser = new TestUser { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "TestUser" };
        var token = userManager.GenerateUserToken(user, Purpose.EmailConfirm);

        // Act & Assert
        userManager.VerifyUserToken(otherUser, Purpose.EmailConfirm, token).Should().BeFalse();
    }

    [Fact]
    public void DigitToken_ShouldRoundTripWithConfiguredSecretAndBeSingleUse()
    {
        // Arrange
        _identityOptions.Token.DigitTokenSecret = new string('s', 32);
        var user = new TestUser { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "TestUser", PasswordHash = "hash" };

        // Act
        var code = _userManager.GenerateDigitToken(user, Purpose.PasswordReset);

        // Assert
        _userManager.VerifyDigitToken(user, Purpose.PasswordReset, code).Should().BeTrue();

        user.PasswordHash = "new-hash";

        _userManager.VerifyDigitToken(user, Purpose.PasswordReset, code).Should().BeFalse();
    }

    [Fact]
    public void DigitToken_WithoutSecretOrSymmetricKey_ShouldThrow()
    {
        // Arrange
        var user = new TestUser { Id = Guid.NewGuid() };

        // Act
        Action act = () => _userManager.GenerateDigitToken(user, Purpose.PasswordReset);

        // Assert
        act.Should().Throw<MilvaDeveloperException>();
    }

    private MilvaUserManager<TestUser, Guid> CreateUserManagerWithRealDataProtection()
        => new(new Lazy<IDataProtectionProvider>(() => new EphemeralDataProtectionProvider()),
               _identityOptions,
               new Lazy<IMilvaPasswordHasher>(() => _passwordHasherMock.Object));

    [Fact]
    public void ValidatePassword_WithValidPassword_ShouldReturnNull()
    {
        // Arrange
        var password = "ValidP@ssw0rd";
        _identityOptions.Password.RequiredLength = 8;
        _identityOptions.Password.RequireNonAlphanumeric = true;
        _identityOptions.Password.RequireDigit = true;
        _identityOptions.Password.RequireLowercase = true;
        _identityOptions.Password.RequireUppercase = true;

        // Act
        var result = _userManager.ValidatePassword(password);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidatePassword_WithShortPassword_ShouldReturnError()
    {
        // Arrange
        var password = "Short1!";
        _identityOptions.Password.RequiredLength = 8;

        // Act
        var result = _userManager.ValidatePassword(password);

        // Assert
        result.Should().Be(LocalizerKeys.IdentityPasswordTooShort);
    }

    [Fact]
    public void ConfigureLockout_WhenAccessFailedAndMaxAttemptsReached_ShouldLockUser()
    {
        // Arrange
        var user = new TestUser { LockoutEnabled = true, AccessFailedCount = 2 };
        _identityOptions.Lockout.MaxFailedAccessAttempts = 3;
        _identityOptions.Token.UseUtcForDateTimes = true;

        // Act
        var result = _userManager.ConfigureLockout(user, true);

        // Assert
        result.Should().BeTrue();
        user.LockoutEnd.Should().NotBeNull();
        user.AccessFailedCount.Should().Be(0);
    }

    [Fact]
    public void ConfigureLockout_WhenAccessSucceeded_ShouldResetLockout()
    {
        // Arrange
        var user = new TestUser
        {
            LockoutEnabled = true,
            AccessFailedCount = 2,
            LockoutEnd = DateTimeOffset.UtcNow.AddHours(1)
        };

        // Act
        var result = _userManager.ConfigureLockout(user, false);

        // Assert
        result.Should().BeFalse();
        user.LockoutEnd.Should().BeNull();
        user.AccessFailedCount.Should().Be(0);
    }

    [Fact]
    public void CanSignIn_WhenEmailConfirmationRequired_AndEmailNotConfirmed_ShouldReturnFalse()
    {
        // Arrange
        var user = new TestUser { EmailConfirmed = false };
        _identityOptions.SignIn.RequireConfirmedEmail = true;

        // Act
        var result = _userManager.CanSignIn(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsLockedOut_WhenUserLocked_ShouldReturnTrue()
    {
        // Arrange
        var user = new TestUser
        {
            LockoutEnabled = true,
            LockoutEnd = DateTimeOffset.UtcNow.AddHours(1)
        };
        _identityOptions.Token.UseUtcForDateTimes = true;

        // Act
        var result = _userManager.IsLockedOut(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateEmail_WithValidEmail_ShouldReturnNull()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var result = _userManager.ValidateEmail(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateEmail_WithInvalidEmail_ShouldReturnError()
    {
        // Arrange
        var email = "invalid-email";

        // Act
        var result = _userManager.ValidateEmail(email);

        // Assert
        result.Should().Be(LocalizerKeys.IdentityInvalidEmail);
    }

    [Fact]
    public void SetPasswordHash_ShouldSetHashedPassword()
    {
        // Arrange
        var user = new TestUser();
        var password = "Password123!";
        var hashedPassword = "hashedPassword";
        _passwordHasherMock.Setup(x => x.HashPassword(password))
                          .Returns(hashedPassword);

        // Act
        _userManager.SetPasswordHash(user, password);

        // Assert
        user.PasswordHash.Should().Be(hashedPassword);
    }

    [Fact]
    public void ValidateAndSetPasswordHash_WithValidPassword_ShouldSetHash()
    {
        // Arrange
        var user = new TestUser();
        var password = "ValidP@ssw0rd";
        var hashedPassword = "hashedPassword";
        _passwordHasherMock.Setup(x => x.HashPassword(password))
                          .Returns(hashedPassword);

        // Act
        _userManager.ValidateAndSetPasswordHash(user, password);

        // Assert
        user.PasswordHash.Should().Be(hashedPassword);
    }

    [Fact]
    public void ValidateAndSetPasswordHash_WithInvalidPassword_ShouldThrowException()
    {
        // Arrange
        var user = new TestUser();
        var password = "short";

        // Act
        Action act = () => _userManager.ValidateAndSetPasswordHash(user, password);

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>();
    }

    [Fact]
    public void CheckPreLogin_WithValidUser_ShouldReturnNull()
    {
        // Arrange
        var user = new TestUser
        {
            EmailConfirmed = true,
            LockoutEnabled = true,
            LockoutEnd = null
        };
        _identityOptions.SignIn.RequireConfirmedEmail = true;

        // Act
        var result = _userManager.CheckPreLogin(user);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CheckPreLogin_WithLockedUser_ShouldReturnNotAllowed()
    {
        // Arrange
        var user = new TestUser
        {
            EmailConfirmed = true,
            LockoutEnabled = true,
            LockoutEnd = DateTimeOffset.UtcNow.AddHours(1)
        };
        _identityOptions.Token.UseUtcForDateTimes = true;

        // Act
        var result = _userManager.CheckPreLogin(user);

        // Assert
        result.Should().Be(LocalizerKeys.NotAllowed);
    }

    [Fact]
    public void CheckPreLoginAndThrowIfInvalid_WithLockedUser_ShouldThrowException()
    {
        // Arrange
        var user = new TestUser
        {
            EmailConfirmed = true,
            LockoutEnabled = true,
            LockoutEnd = DateTimeOffset.UtcNow.AddHours(1)
        };
        _identityOptions.Token.UseUtcForDateTimes = true;

        // Act
        Action act = () => _userManager.CheckPreLoginAndThrowIfInvalid(user);

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>();
    }

    [Fact]
    public void ValidateUser_WithValidUserAndPassword_ShouldReturnNull()
    {
        // Arrange
        var user = new TestUser
        {
            UserName = "validuser",
            Email = "valid@email.com"
        };
        var password = "ValidP@ssw0rd";
        _identityOptions.User.RequireUniqueEmail = true;

        // Act
        var result = _userManager.ValidateUser(user, password);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateUserName_WithValidUserName_ShouldReturnNull()
    {
        // Arrange
        var userName = "validusername";
        _identityOptions.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz";

        // Act
        var result = _userManager.ValidateUserName(userName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateUserName_WithInvalidCharacters_ShouldReturnError()
    {
        // Arrange
        var userName = "invalid@username";
        _identityOptions.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz";

        // Act
        var result = _userManager.ValidateUserName(userName);

        // Assert
        result.Should().Be(LocalizerKeys.IdentityInvalidUserName);
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

        _dataProtectionProviderMock
            .Setup(dp => dp.CreateProtector(It.IsAny<string>()))
            .Returns(mockProtector.Object);

        // Act
        var token = _userManager.GenerateUserToken(user, purpose);

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

        _dataProtectionProviderMock
            .Setup(dp => dp.CreateProtector(It.IsAny<string>()))
            .Returns(mockProtector.Object);

        // Act
        var isValid = _userManager.VerifyUserToken(user, purpose, token);

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

        // Act
        var isValid = _userManager.VerifyUserToken(user, purpose, null);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void GenerateUserToken_ShouldThrowExceptionForNullUser()
    {
        // Arrange
        TestUser user = null;
        var purpose = Purpose.EmailConfirm;

        // Act
        Action act = () => _userManager.GenerateUserToken(user, purpose);

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

        // Act
        Action act = () => _userManager.VerifyUserToken(user, purpose, token);

        // Assert
        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void ThrowIfUserInvalid_ShouldThrowIfValidationFails()
    {
        // Arrange
        var invalidUser = new TestUser { UserName = "!InvalidUser", Email = "invalid-email" };

        // Act
        Action act = () => _userManager.ThrowIfUserInvalid(invalidUser);

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>();
    }

    [Fact]
    public void ThrowIfUserInvalid_ShouldNotThrowIfUserValid()
    {
        // Arrange
        var validUser = new TestUser { UserName = "ValidUser", Email = "test@example.com" };

        // Act
        Action act = () => _userManager.ThrowIfUserInvalid(validUser);

        // Assert
        act.Should().NotThrow<MilvaUserFriendlyException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("123")]
    [InlineData("password")]
    public void ThrowIfPasswordIsInvalid_ShouldThrowIfValidationFails(string invalidPassword)
    {
        // Act
        Action act = () => _userManager.ThrowIfPasswordIsInvalid(invalidPassword);

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>();
    }

    [Fact]
    public void ThrowIfPasswordIsInvalid_ShouldNotThrowIfPasswordValid()
    {
        // Arrange
        var validPassword = "Password123!";

        // Act
        Action act = () => _userManager.ThrowIfPasswordIsInvalid(validPassword);

        // Assert
        act.Should().NotThrow<MilvaUserFriendlyException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("milvasoft!")]
    public void ThrowIfUserNameIsInvalid_ShouldThrowIfValidationFails(string invalidUserName)
    {
        // Act
        Action act = () => _userManager.ThrowIfUserNameIsInvalid(invalidUserName);

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>();
    }

    [Fact]
    public void ThrowIfUserNameIsInvalid_ShouldNotThrowIfUserNameValid()
    {
        // Arrange
        var validUserName = "milvasoft";

        // Act
        Action act = () => _userManager.ThrowIfUserNameIsInvalid(validUserName);

        // Assert
        act.Should().NotThrow<MilvaUserFriendlyException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("milvasoft!")]
    [InlineData("milvasoft@g")]
    [InlineData("milvasoft@gmail.")]
    public void ThrowIfEmailIsInvalid_ShouldThrowIfValidationFails(string invalidEmail)
    {
        // Act
        Action act = () => _userManager.ThrowIfEmailIsInvalid(invalidEmail);

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>();
    }

    [Fact]
    public void ThrowIfEmailIsInvalid_ShouldNotThrowIfEmailValid()
    {
        // Arrange
        var validUserName = "milvasoft@gmail.com";

        // Act
        Action act = () => _userManager.ThrowIfEmailIsInvalid(validUserName);

        // Assert
        act.Should().NotThrow<MilvaUserFriendlyException>();
    }
}