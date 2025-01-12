using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Milvasoft.Core.Abstractions;
using Milvasoft.Identity.Concrete.Options;
using Milvasoft.Identity.TokenProvider.AuthToken;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Milvasoft.UnitTests.IdentityTests;

[Trait("Identity Unit Tests", "Milvasoft.Identity project unit tests.")]
public class MilvaTokenManagerTests
{
    private static MilvaIdentityOptions CreateDefaultOptions()
    {
        var identityOptions = new MilvaIdentityOptions
        {
            Token = new TokenConfig
            {
                UseUtcForDateTimes = true,
                ExpirationMinute = 10,
                SymmetricPublicKey = "9321acf6145bc0a5d573a835be473682992d17a17830de85a122c715633ae5e9",
                SecurityKeyType = SecurityKeyType.Symmetric,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "TestIssuer",
                    ValidAudience = "TestAudience",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("w!z%C*F-JaNdRgUk"))
                }
            }
        };

        identityOptions.Token.TokenValidationParameters.IssuerSigningKey = identityOptions.Token.GetSecurityKey();

        return identityOptions;
    }

    [Fact]
    public void GetClaimsPrincipalIfValid_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        var options = CreateDefaultOptions();
        var loggerMock = new Mock<IMilvaLogger>();
        var tokenManager = new MilvaTokenManager(options, loggerMock.Object);

        var token = tokenManager.GenerateToken(claims: new Claim(ClaimTypes.Name, "TestUser"));

        // Act
        var principal = tokenManager.GetClaimsPrincipalIfValid(token);

        // Assert
        principal.Should().NotBeNull();
        principal.Identity.Should().NotBeNull();
        principal.Identity.Name.Should().Be("TestUser");
    }

    [Fact]
    public void GetClaimsPrincipalIfValid_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var options = CreateDefaultOptions();
        var loggerMock = new Mock<IMilvaLogger>();
        var tokenManager = new MilvaTokenManager(options, loggerMock.Object);

        var invalidToken = "InvalidToken";

        // Act
        var principal = tokenManager.GetClaimsPrincipalIfValid(invalidToken);

        // Assert
        principal.Should().BeNull();
        loggerMock.Verify(logger => logger.Warning(It.IsAny<Exception>(), "Token validation error!"), Times.Once);
    }

    [Fact]
    public void GetClaimsPrincipalIfValid_WithOverriddenIssuers_ShouldValidateSuccessfully()
    {
        // Arrange
        var options = CreateDefaultOptions();
        var loggerMock = new Mock<IMilvaLogger>();
        var tokenManager = new MilvaTokenManager(options, loggerMock.Object);

        var token = tokenManager.GenerateToken(claims: new Claim(ClaimTypes.Name, "TestUser"));
        tokenManager.OverrideValidIssuer("NewIssuer");

        // Act
        var principal = tokenManager.GetClaimsPrincipalIfValid(token);
        // Assert
        principal.Should().BeNull(); // ValidIssuer is overridden and does not match
        loggerMock.Verify(logger => logger.Warning(It.IsAny<SecurityTokenInvalidIssuerException>(), "Token validation error!"), Times.Once);
    }

    [Fact]
    public void GenerateToken_WithDefaultExpiration_ShouldGenerateTokenSuccessfully()
    {
        // Arrange
        var options = CreateDefaultOptions();
        var loggerMock = new Mock<IMilvaLogger>();
        var tokenManager = new MilvaTokenManager(options, loggerMock.Object);

        // Act
        var token = tokenManager.GenerateToken(claims: new Claim(ClaimTypes.Name, "TestUser"));

        // Assert
        token.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        var principal = tokenManager.GetClaimsPrincipalIfValid(token);

        jwtToken.Should().NotBeNull();
        principal.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.Name && c.Value == "TestUser");
    }

    [Fact]
    public void GenerateToken_WithCustomExpiration_ShouldGenerateTokenSuccessfully()
    {
        // Arrange
        var options = CreateDefaultOptions();
        var loggerMock = new Mock<IMilvaLogger>();
        var tokenManager = new MilvaTokenManager(options, loggerMock.Object);

        var customExpiration = DateTime.UtcNow.AddHours(1);

        // Act
        var token = tokenManager.GenerateToken(customExpiration, claims: new Claim(ClaimTypes.Name, "TestUser"));

        // Assert
        token.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        var principal = tokenManager.GetClaimsPrincipalIfValid(token);

        jwtToken.Should().NotBeNull();
        principal.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.Name && c.Value == "TestUser");
        jwtToken.ValidTo.Should().BeCloseTo(customExpiration, TimeSpan.FromSeconds(5));
    }
}
