using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Identity.Abstract;
using Milvasoft.Identity.Concrete;
using Milvasoft.Identity.Concrete.Entity;
using Milvasoft.Identity.Concrete.Options;
using Milvasoft.Identity.TokenProvider;
using Milvasoft.Identity.TokenProvider.AuthToken;
using Moq;
using System.Security.Cryptography;
using System.Text;
using static Milvasoft.UnitTests.IdentityTests.MilvaUserValidationTests;

namespace Milvasoft.UnitTests.IdentityTests;

[Trait("Identity Unit Tests", "Milvasoft.Identity project unit tests.")]
public class SecurityStampTests
{
    private readonly Mock<IMilvaPasswordHasher> _passwordHasherMock = new();
    private readonly EphemeralDataProtectionProvider _dataProtectionProvider = new();
    private readonly MilvaUserManager<TestUser, Guid> _userManager;

    public SecurityStampTests()
    {
        _passwordHasherMock.Setup(x => x.HashPassword(It.IsAny<string>())).Returns<string>(p => "hash:" + p);

        var options = new MilvaIdentityOptions
        {
            Lockout = new MilvaLockoutOptions { AllowedForNewUsers = true, MaxFailedAccessAttempts = 3 },
            Token = new TokenConfig { UseUtcForDateTimes = true, DigitTokenSecret = new string('s', 32) }
        };

        _userManager = new MilvaUserManager<TestUser, Guid>(new Lazy<IDataProtectionProvider>(() => _dataProtectionProvider),
                                                            options,
                                                            new Lazy<IMilvaPasswordHasher>(() => _passwordHasherMock.Object));
    }

    #region Computation

    /// <summary>
    /// Users whose row has no stamp yet (column added by a migration, value null) must get the same stamp as before the column existed,
    /// so tokens issued before the upgrade keep validating.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Compute_WithoutPersistedStamp_ShouldEqualStampOfPreviousVersion(string persistedStamp)
    {
        // Arrange
        var user = CreateUser();
        user.SecurityStamp = persistedStamp;

        // Act
        var stamp = UserSecurityStamp.ComputeString(user);

        // Assert
        stamp.Should().Be(ComputePreviousVersionStamp(user));
    }

    [Fact]
    public void Compute_WithPersistedStamp_ShouldDependOnIt()
    {
        // Arrange
        var user = CreateUser();
        var derivedOnly = UserSecurityStamp.ComputeString(user);

        // Act
        user.SecurityStamp = "stamp-1";
        var withStamp1 = UserSecurityStamp.ComputeString(user);
        user.SecurityStamp = "stamp-2";
        var withStamp2 = UserSecurityStamp.ComputeString(user);

        // Assert
        withStamp1.Should().NotBe(derivedOnly);
        withStamp2.Should().NotBe(withStamp1);
    }

    [Fact]
    public void Compute_WithPersistedStamp_ShouldStillDependOnSecurityRelevantColumns()
    {
        // Arrange: an application changes the email directly on the entity without going through the user manager.
        var user = CreateUser();
        user.SecurityStamp = "stamp";
        var before = UserSecurityStamp.ComputeString(user);

        // Act
        user.Email = "other@example.com";

        // Assert
        UserSecurityStamp.ComputeString(user).Should().NotBe(before);
    }

    [Fact]
    public void NewStamp_ShouldBeRandomBase64UrlThatFitsTheColumn()
    {
        // Act
        var stamp1 = UserSecurityStamp.NewStamp();
        var stamp2 = UserSecurityStamp.NewStamp();

        // Assert
        stamp1.Should().NotBe(stamp2);
        stamp1.Should().MatchRegex("^[A-Za-z0-9_-]{43}$");
        stamp1.Length.Should().BeLessThanOrEqualTo(MilvaUser<Guid>.SecurityStampMaxLength);
    }

    [Fact]
    public void EfModel_ShouldMapSecurityStampAsNullableWithMaxLength()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<UserDbContext>().UseNpgsql("Host=localhost").Options;
        using var context = new UserDbContext(dbOptions);

        // Act
        var property = context.Model.FindEntityType(typeof(TestUser)).FindProperty(nameof(MilvaUser<Guid>.SecurityStamp));

        // Assert
        property.Should().NotBeNull();
        property.IsNullable.Should().BeTrue();
        property.GetMaxLength().Should().Be(64);
    }

    #endregion

    #region Tokens in flight

    [Fact]
    public void UserToken_IssuedBeforeUpgrade_ShouldStayValidWhileStampIsNull()
    {
        // Arrange: token produced by the previous version (stamp derived from the columns only).
        var user = CreateUser();
        var token = _dataProtectionProvider.Generate($"Totp:{Purpose.PasswordReset}{user.Id}", user.Id, ComputePreviousVersionStamp(user));

        // Act & Assert
        _userManager.VerifyUserToken(user, Purpose.PasswordReset, token).Should().BeTrue();
    }

    [Fact]
    public void UserToken_IssuedBeforeFirstStampAssignment_ShouldBeInvalidAfterIt()
    {
        // Arrange
        var user = CreateUser();
        var token = _userManager.GenerateUserToken(user, Purpose.PasswordReset);

        // Act
        _userManager.RotateSecurityStamp(user);

        // Assert
        _userManager.VerifyUserToken(user, Purpose.PasswordReset, token).Should().BeFalse();
    }

    #endregion

    #region Rotation

    [Fact]
    public void ConfigureForCreate_ShouldAssignRandomStamp()
    {
        // Arrange
        var user1 = new TestUser { UserName = "user1", Email = "user1@example.com" };
        var user2 = new TestUser { UserName = "user2", Email = "user2@example.com" };

        // Act
        _userManager.ConfigureForCreate(ref user1, "Password123!");
        _userManager.ConfigureForCreate(ref user2, "Password123!");

        // Assert
        user1.SecurityStamp.Should().NotBeNullOrEmpty();
        user2.SecurityStamp.Should().NotBeNullOrEmpty();
        user1.SecurityStamp.Should().NotBe(user2.SecurityStamp);
    }

    [Fact]
    public void RotateSecurityStamp_ShouldRevokeOutstandingTokensAndCodes()
    {
        // Arrange
        var user = CreateUser();
        user.SecurityStamp = UserSecurityStamp.NewStamp();
        var token = _userManager.GenerateUserToken(user, Purpose.EmailConfirm);
        var code = _userManager.GenerateDigitToken(user, Purpose.PasswordReset);
        var otherState = Snapshot(user);

        // Act
        _userManager.RotateSecurityStamp(user);

        // Assert: nothing but the stamp changed, yet both are revoked.
        Snapshot(user).Should().Be(otherState);
        _userManager.VerifyUserToken(user, Purpose.EmailConfirm, token).Should().BeFalse();
        _userManager.VerifyDigitToken(user, Purpose.PasswordReset, code).Should().BeFalse();
    }

    [Fact]
    public void RotateSecurityStamp_WithNullUser_ShouldThrow()
    {
        // Act
        Action act = () => _userManager.RotateSecurityStamp(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    public static TheoryData<string> RotatingOperations => [nameof(IMilvaUserManager<TestUser, Guid>.SetPasswordHash),
                                                             nameof(IMilvaUserManager<TestUser, Guid>.ValidateAndSetPasswordHash),
                                                             nameof(IMilvaUserManager<TestUser, Guid>.SetEmail),
                                                             nameof(IMilvaUserManager<TestUser, Guid>.ConfirmEmail),
                                                             nameof(IMilvaUserManager<TestUser, Guid>.SetPhoneNumber),
                                                             nameof(IMilvaUserManager<TestUser, Guid>.ConfirmPhoneNumber)];

    [Theory]
    [MemberData(nameof(RotatingOperations))]
    public void CredentialOrIdentityChange_ShouldRotateStamp(string operation)
    {
        // Arrange
        var user = CreateUser();
        user.SecurityStamp = "previous";

        // Act
        Apply(operation, user);

        // Assert
        user.SecurityStamp.Should().NotBeNullOrEmpty().And.NotBe("previous");
    }

    [Fact]
    public void SetPasswordHash_ShouldRevokeTokenEvenIfHashIsUnchanged()
    {
        // Arrange: a hasher that would produce the same hash for the same password must still revoke tokens.
        var user = CreateUser();
        user.PasswordHash = "hash:Password123!";
        var token = _userManager.GenerateUserToken(user, Purpose.PasswordReset);

        // Act
        _userManager.SetPasswordHash(user, "Password123!");

        // Assert
        user.PasswordHash.Should().Be("hash:Password123!");
        _userManager.VerifyUserToken(user, Purpose.PasswordReset, token).Should().BeFalse();
    }

    [Fact]
    public void SetEmail_ShouldSetEmailNormalizedEmailAndConfirmation()
    {
        // Arrange
        var user = CreateUser();
        user.EmailConfirmed = true;

        // Act
        _userManager.SetEmail(user, "new@example.com");

        // Assert
        user.Email.Should().Be("new@example.com");
        user.NormalizedEmail.Should().Be("NEW@EXAMPLE.COM");
        user.EmailConfirmed.Should().BeFalse();

        // Act
        _userManager.SetEmail(user, "confirmed@example.com", emailConfirmed: true);

        // Assert
        user.Email.Should().Be("confirmed@example.com");
        user.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public void ConfirmEmail_ShouldSetEmailConfirmed()
    {
        // Arrange
        var user = CreateUser();

        // Act
        _userManager.ConfirmEmail(user);

        // Assert
        user.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public void SetPhoneNumberAndConfirmPhoneNumber_ShouldSetPhoneNumberAndConfirmation()
    {
        // Arrange
        var user = CreateUser();
        user.PhoneNumberConfirmed = true;

        // Act
        _userManager.SetPhoneNumber(user, "+905550000000");

        // Assert
        user.PhoneNumber.Should().Be("+905550000000");
        user.PhoneNumberConfirmed.Should().BeFalse();

        // Act
        _userManager.ConfirmPhoneNumber(user);

        // Assert
        user.PhoneNumberConfirmed.Should().BeTrue();

        // Act
        _userManager.SetPhoneNumber(user, "+905551111111", phoneNumberConfirmed: true);

        // Assert
        user.PhoneNumber.Should().Be("+905551111111");
        user.PhoneNumberConfirmed.Should().BeTrue();
    }

    [Fact]
    public void RehashPassword_ShouldNotRotateStamp()
    {
        // Arrange
        var user = CreateUser();
        user.SecurityStamp = "stamp";
        _passwordHasherMock.Setup(x => x.VerifyHashedPasswordWithRehashCheck(user.PasswordHash, "Password123!"))
                           .Returns(PasswordVerificationResult.SuccessRehashNeeded);

        // Act
        var rehashed = _userManager.RehashPassword(user, "Password123!");

        // Assert
        rehashed.Should().BeTrue();
        user.SecurityStamp.Should().Be("stamp");
    }

    [Fact]
    public void ConfigureLockout_ShouldNotRotateStamp()
    {
        // Arrange
        var user = CreateUser();
        user.SecurityStamp = "stamp";
        user.LockoutEnabled = true;

        // Act
        _userManager.ConfigureLockout(user, accessFailed: true);
        _userManager.ConfigureLockout(user, accessFailed: false);

        // Assert
        user.SecurityStamp.Should().Be("stamp");
    }

    [Fact]
    public void GenerateTokens_ShouldNotModifyUser()
    {
        // Arrange
        var user = CreateUser();
        var before = Snapshot(user);

        // Act
        _userManager.GenerateUserToken(user, Purpose.EmailConfirm);
        _userManager.GenerateDigitToken(user, Purpose.EmailConfirm);

        // Assert
        Snapshot(user).Should().Be(before);
        user.SecurityStamp.Should().BeNull();
    }

    #endregion

    #region Single-use verification

    [Theory]
    [InlineData(Purpose.EmailConfirm)]
    [InlineData(Purpose.EmailChange)]
    [InlineData(Purpose.PasswordReset)]
    public void VerifyUserToken_WhenValid_ShouldConsumeTokenAndRevokeOtherTokens(Purpose purpose)
    {
        // Arrange
        var user = CreateUser();
        var token = _userManager.GenerateUserToken(user, purpose, "new@example.com");
        var otherToken = _userManager.GenerateUserToken(user, purpose == Purpose.PasswordReset ? Purpose.EmailConfirm : Purpose.PasswordReset);

        // Act
        var first = _userManager.VerifyUserToken(user, purpose, token, "new@example.com");
        var second = _userManager.VerifyUserToken(user, purpose, token, "new@example.com");

        // Assert
        first.Should().BeTrue();
        second.Should().BeFalse();
        user.SecurityStamp.Should().NotBeNullOrEmpty();
        _userManager.VerifyUserToken(user, purpose == Purpose.PasswordReset ? Purpose.EmailConfirm : Purpose.PasswordReset, otherToken).Should().BeFalse();
    }

    [Fact]
    public void VerifyUserToken_WhenInvalid_ShouldNotRotateStamp()
    {
        // Arrange
        var user = CreateUser();
        user.SecurityStamp = "stamp";
        var token = _userManager.GenerateUserToken(user, Purpose.EmailConfirm);

        // Act
        var wrongPurpose = _userManager.VerifyUserToken(user, Purpose.PasswordReset, token);
        var garbage = _userManager.VerifyUserToken(user, Purpose.EmailConfirm, "garbage");

        // Assert
        wrongPurpose.Should().BeFalse();
        garbage.Should().BeFalse();
        user.SecurityStamp.Should().Be("stamp");
        _userManager.VerifyUserToken(user, Purpose.EmailConfirm, token).Should().BeTrue();
    }

    [Theory]
    [InlineData(Purpose.EmailConfirm)]
    [InlineData(Purpose.EmailChange)]
    [InlineData(Purpose.PasswordReset)]
    public void VerifyDigitToken_WhenValid_ShouldConsumeCode(Purpose purpose)
    {
        // Arrange
        var user = CreateUser();
        var code = _userManager.GenerateDigitToken(user, purpose);

        // Act
        var first = _userManager.VerifyDigitToken(user, purpose, code);
        var second = _userManager.VerifyDigitToken(user, purpose, code);

        // Assert
        first.Should().BeTrue();
        second.Should().BeFalse();
    }

    [Fact]
    public void VerifyDigitToken_WhenInvalid_ShouldNotRotateStamp()
    {
        // Arrange
        var user = CreateUser();
        user.SecurityStamp = "stamp";
        var code = _userManager.GenerateDigitToken(user, Purpose.PasswordReset);
        var wrongCode = code == "000000" ? "000001" : "000000";

        // Act
        var result = _userManager.VerifyDigitToken(user, Purpose.PasswordReset, wrongCode);

        // Assert
        result.Should().BeFalse();
        user.SecurityStamp.Should().Be("stamp");
    }

    #endregion

    private static TestUser CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        UserName = "user",
        Email = "user@example.com",
        PhoneNumber = "+905559999999",
        PasswordHash = "hash:old",
    };

    private void Apply(string operation, TestUser user)
    {
        switch (operation)
        {
            case nameof(IMilvaUserManager<TestUser, Guid>.SetPasswordHash):
                _userManager.SetPasswordHash(user, "Password123!");
                break;
            case nameof(IMilvaUserManager<TestUser, Guid>.ValidateAndSetPasswordHash):
                _userManager.ValidateAndSetPasswordHash(user, "Password123!");
                break;
            case nameof(IMilvaUserManager<TestUser, Guid>.SetEmail):
                _userManager.SetEmail(user, "new@example.com");
                break;
            case nameof(IMilvaUserManager<TestUser, Guid>.ConfirmEmail):
                _userManager.ConfirmEmail(user);
                break;
            case nameof(IMilvaUserManager<TestUser, Guid>.SetPhoneNumber):
                _userManager.SetPhoneNumber(user, "+905550000000");
                break;
            case nameof(IMilvaUserManager<TestUser, Guid>.ConfirmPhoneNumber):
                _userManager.ConfirmPhoneNumber(user);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(operation));
        }
    }

    private static string Snapshot(TestUser user)
        => $"{user.Id}|{user.PasswordHash}|{user.Email}|{user.EmailConfirmed}|{user.PhoneNumber}|{user.PhoneNumberConfirmed}|{user.AccessFailedCount}|{user.LockoutEnd}";

    /// <summary>
    /// The stamp computation of the version before the persisted SecurityStamp column existed.
    /// </summary>
    private static string ComputePreviousVersionStamp(TestUser user)
    {
        using var ms = new MemoryStream();

        using (var writer = new BinaryWriter(ms, new UTF8Encoding(false, true), leaveOpen: true))
        {
            writer.Write(user.Id.ToString());
            writer.Write(user.PasswordHash ?? string.Empty);
            writer.Write(user.Email ?? string.Empty);
            writer.Write(user.EmailConfirmed);
            writer.Write(user.PhoneNumber ?? string.Empty);
            writer.Write(user.PhoneNumberConfirmed);
        }

        return Convert.ToBase64String(SHA256.HashData(ms.ToArray()));
    }

    private sealed class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<TestUser> Users { get; set; }
    }
}
