using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Milvasoft.Core.Helpers;
using Milvasoft.Identity.Concrete.Entity;
using Milvasoft.Identity.TokenProvider.AuthToken;
using System.Text;

namespace Milvasoft.UnitTests.PropertyTests.Identity;

[Trait("Property Getter Setters Unit Tests", "Library's models property getter setter unit tests.")]
public class IdentityModelTests
{
    #region TokenConfig

    [Fact]
    public void TokenConfig_ShouldSetAndGetProperties()
    {
        // Arrange
        var tokenConfig = new TokenConfig
        {
            UseUtcForDateTimes = true,
            ExpirationMinute = 30,
            SecurityKeyType = SecurityKeyType.Symmetric,
            SymmetricPublicKey = "TestSymmetricKey",
            RsaPublicKey = new RsaPublicKey
            {
                Kid = "test-kid",
                Kty = "RSA",
                Alg = "RS256",
                Use = "sig",
                N = "test-modulus",
                E = "test-exponent"
            },
            TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            }
        };

        // Assert
        tokenConfig.UseUtcForDateTimes.Should().BeTrue();
        tokenConfig.ExpirationMinute.Should().Be(30);
        tokenConfig.SecurityKeyType.Should().Be(SecurityKeyType.Symmetric);
        tokenConfig.SymmetricPublicKey.Should().Be("TestSymmetricKey");
        tokenConfig.RsaPublicKey.Should().NotBeNull();
        tokenConfig.RsaPublicKey.Kid.Should().Be("test-kid");
        tokenConfig.TokenValidationParameters.Should().NotBeNull();
        tokenConfig.TokenValidationParameters.ValidateIssuer.Should().BeTrue();
    }

    [Fact]
    public void GetSecurityKey_WithSymmetricKey_ShouldReturnSymmetricSecurityKey()
    {
        // Arrange
        var symmetricKey = "TestSymmetricKey";
        var tokenConfig = new TokenConfig
        {
            SecurityKeyType = SecurityKeyType.Symmetric,
            SymmetricPublicKey = symmetricKey
        };

        // Act
        var securityKey = tokenConfig.GetSecurityKey();

        // Assert
        securityKey.Should().NotBeNull();
        securityKey.Should().BeOfType<SymmetricSecurityKey>();
        var symmetricSecurityKey = securityKey as SymmetricSecurityKey;
        symmetricSecurityKey.Key.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(symmetricKey));
    }

    [Fact]
    public void GetSecurityKey_WithRsaKey_ShouldReturnRsaSecurityKey()
    {
        // Arrange
        var rsaPublicKey = new RsaPublicKey
        {
            Kid = "test-kid",
            Kty = "RSA",
            Alg = "RS256",
            Use = "sig",
            N = "test-modulus",
            E = "test-exponent"
        };
        var tokenConfig = new TokenConfig
        {
            SecurityKeyType = SecurityKeyType.Rsa,
            RsaPublicKey = rsaPublicKey
        };

        // Act
        var securityKey = tokenConfig.GetSecurityKey();

        // Assert
        securityKey.Should().NotBeNull();
        securityKey.Should().BeOfType<JsonWebKey>();
        var rsaSecurityKey = securityKey as JsonWebKey;
        rsaSecurityKey.Kid.Should().Be(rsaPublicKey.Kid);
    }

    [Fact]
    public void GetSecurityAlgorithm_WithSymmetricKey_ShouldReturnHmacSha256()
    {
        // Arrange
        var tokenConfig = new TokenConfig
        {
            SecurityKeyType = SecurityKeyType.Symmetric
        };

        // Act
        var algorithm = tokenConfig.GetSecurityAlgorithm();

        // Assert
        algorithm.Should().Be(SecurityAlgorithms.HmacSha256Signature);
    }

    [Fact]
    public void GetSecurityAlgorithm_WithRsaKey_ShouldReturnRsaSha256()
    {
        // Arrange
        var tokenConfig = new TokenConfig
        {
            SecurityKeyType = SecurityKeyType.Rsa
        };

        // Act
        var algorithm = tokenConfig.GetSecurityAlgorithm();

        // Assert
        algorithm.Should().Be(SecurityAlgorithms.RsaSha256Signature);
    }

    [Fact]
    public void GetSecurityAlgorithm_WithInvalidKeyType_ShouldReturnNull()
    {
        // Arrange
        var tokenConfig = new TokenConfig
        {
            SecurityKeyType = (SecurityKeyType)999 // Invalid Key Type
        };

        // Act
        var algorithm = tokenConfig.GetSecurityAlgorithm();

        // Assert
        algorithm.Should().BeNull();
    }

    #endregion

    #region MilvaUser

    [Fact]
    public void MilvaUser_ShouldGetSetPropertiesCorrectly()
    {
        // Arrange
        var user = new MilvaUser<Guid>();
        var userId = Guid.NewGuid();
        var userName = "testUser";
        var email = "test@example.com";
        var phoneNumber = "1234567890";
        var passwordHash = "hashedPassword";
        var lockoutEnd = DateTimeOffset.UtcNow.AddDays(1);

        // Act
        user.Id = userId;
        user.UserName = userName;
        user.Email = email;
        user.PhoneNumber = phoneNumber;
        user.PasswordHash = passwordHash;
        user.EmailConfirmed = true;
        user.PhoneNumberConfirmed = true;
        user.TwoFactorEnabled = true;
        user.LockoutEnd = lockoutEnd;
        user.LockoutEnabled = true;
        user.AccessFailedCount = 3;

        // Assert
        user.Id.Should().Be(userId);
        user.UserName.Should().Be(userName);
        user.NormalizedUserName.Should().Be(userName.MilvaNormalize());
        user.Email.Should().Be(email);
        user.NormalizedEmail.Should().Be(email.MilvaNormalize());
        user.PhoneNumber.Should().Be(phoneNumber);
        user.PasswordHash.Should().Be(passwordHash);
        user.EmailConfirmed.Should().BeTrue();
        user.PhoneNumberConfirmed.Should().BeTrue();
        user.TwoFactorEnabled.Should().BeTrue();
        user.LockoutEnd.Should().Be(lockoutEnd);
        user.LockoutEnabled.Should().BeTrue();
        user.AccessFailedCount.Should().Be(3);
    }

    #endregion

    [Fact]
    public void RsaPublicKey_ShouldSetAndGetProperties()
    {
        // Arrange
        var rsaPublicKey = new RsaPublicKey();
        var kid = "test-kid";
        var kty = "RSA";
        var alg = "RS256";
        var use = "sig";
        var n = "test-modulus";
        var e = "test-exponent";
        var x5c = new List<string> { "cert1", "cert2" };
        var x5t = "test-x5t";
        var x5tS256 = "test-x5tS256";

        // Act
        rsaPublicKey.Kid = kid;
        rsaPublicKey.Kty = kty;
        rsaPublicKey.Alg = alg;
        rsaPublicKey.Use = use;
        rsaPublicKey.N = n;
        rsaPublicKey.E = e;
        rsaPublicKey.X5c = x5c;
        rsaPublicKey.X5t = x5t;
        rsaPublicKey.X5tS256 = x5tS256;

        // Assert
        rsaPublicKey.Kid.Should().Be(kid);
        rsaPublicKey.Kty.Should().Be(kty);
        rsaPublicKey.Alg.Should().Be(alg);
        rsaPublicKey.Use.Should().Be(use);
        rsaPublicKey.N.Should().Be(n);
        rsaPublicKey.E.Should().Be(e);
        rsaPublicKey.X5c.Should().BeEquivalentTo(x5c);
        rsaPublicKey.X5t.Should().Be(x5t);
        rsaPublicKey.X5tS256.Should().Be(x5tS256);
    }
}
