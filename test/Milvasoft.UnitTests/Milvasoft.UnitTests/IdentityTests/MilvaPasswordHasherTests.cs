using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Milvasoft.Identity.Concrete;
using Milvasoft.Identity.Concrete.Options;
using System.Security.Cryptography;

namespace Milvasoft.UnitTests.IdentityTests;

[Trait("Identity Unit Tests", "Milvasoft.Identity project unit tests.")]
public class MilvaPasswordHasherTests
{
    private readonly MilvaPasswordHasher _hasher;

    public MilvaPasswordHasherTests()
    {
        var options = Options.Create(new MilvaPasswordHasherOptions
        {
            IterationCount = 10000,
            RandomNumberGenerator = RandomNumberGenerator.Create()
        });
        _hasher = new MilvaPasswordHasher(options);
    }

    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "testPassword";

        // Act
        var hashedPassword = _hasher.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_WithNullPassword_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _hasher.HashPassword(null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*password*");
    }

    [Fact]
    public void VerifyHashedPassword_ShouldReturnTrueForValidPassword()
    {
        // Arrange
        var password = "testPassword";
        var hashedPassword = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyHashedPassword(hashedPassword, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyHashedPassword_ShouldReturnFalseForInvalidPassword()
    {
        // Arrange
        var password = "testPassword";
        var wrongPassword = "wrongPassword";
        var hashedPassword = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyHashedPassword(hashedPassword, wrongPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyHashedPassword_WithNullHashedPassword_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _hasher.VerifyHashedPassword(null, "testPassword");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*hashedPassword*");
    }

    [Fact]
    public void VerifyHashedPassword_WithNullProvidedPassword_ShouldThrowArgumentNullException()
    {
        // Arrange
        var hashedPassword = _hasher.HashPassword("testPassword");

        // Act
        Action act = () => _hasher.VerifyHashedPassword(hashedPassword, null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*providedPassword*");
    }

    [Fact]
    public void VerifyHashedPassword_ShouldReturnFalseForInvalidFormat()
    {
        // Arrange
        var invalidHashedPassword = "invalidFormat";

        // Act
        var result = _hasher.VerifyHashedPassword(invalidHashedPassword, "testPassword");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Hash produced by the previous implementation (format 0x01, HMAC-SHA256, 10,000 iterations, 16 byte salt) for "testPassword".
    /// </summary>
    private static string CreateLegacyHash(string password)
    {
        var salt = Enumerable.Range(1, 16).Select(i => (byte)i).ToArray();
        var subkey = Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivation.Pbkdf2(password, salt, Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivationPrf.HMACSHA256, 10000, 32);
        var output = new byte[13 + salt.Length + subkey.Length];
        output[0] = 0x01;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(output.AsSpan(1), 1); // HMACSHA256
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(output.AsSpan(5), 10000);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(output.AsSpan(9), 16);
        salt.CopyTo(output, 13);
        subkey.CopyTo(output, 13 + salt.Length);
        return Convert.ToBase64String(output);
    }

    [Fact]
    public void VerifyHashedPasswordWithRehashCheck_WithLegacyHash_ShouldSucceedAndRequestRehash()
    {
        // Arrange
        var hasher = new MilvaPasswordHasher(new MilvaPasswordHasherOptions());
        var legacyHash = CreateLegacyHash("testPassword");

        // Act & Assert
        hasher.VerifyHashedPassword(legacyHash, "testPassword").Should().BeTrue();
        hasher.VerifyHashedPasswordWithRehashCheck(legacyHash, "testPassword").Should().Be(PasswordVerificationResult.SuccessRehashNeeded);
        hasher.VerifyHashedPasswordWithRehashCheck(legacyHash, "wrong").Should().Be(PasswordVerificationResult.Failed);
    }

    [Fact]
    public void VerifyHashedPasswordWithRehashCheck_WithCurrentSettings_ShouldNotRequestRehash()
    {
        // Arrange
        var hasher = new MilvaPasswordHasher(new MilvaPasswordHasherOptions { IterationCount = 1000 });
        var hash = hasher.HashPassword("testPassword");

        // Act & Assert
        hasher.VerifyHashedPasswordWithRehashCheck(hash, "testPassword").Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void HashPassword_ShouldUseConfiguredIterationCountAndPrf()
    {
        // Arrange
        var hasher = new MilvaPasswordHasher(new MilvaPasswordHasherOptions { IterationCount = 1234, Prf = Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivationPrf.HMACSHA512 });

        // Act
        var bytes = Convert.FromBase64String(hasher.HashPassword("testPassword"));

        // Assert
        System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(1)).Should().Be(2u);
        System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(5)).Should().Be(1234u);
    }

    [Fact]
    public void DefaultOptions_ShouldBeStrongerThanLegacyDefaults()
    {
        var options = new MilvaPasswordHasherOptions();

        options.IterationCount.Should().BeGreaterThan(10000);
        options.Prf.Should().Be(Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivationPrf.HMACSHA512);
    }

    [Fact]
    public void IdentityBuilder_ShouldRegisterHasherWithConfiguredOptions()
    {
        // Arrange
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        new Milvasoft.Identity.Builder.MilvaIdentityBuilder<MilvaUserValidationTests.TestUser, Guid>(services, null)
            .WithOptions(opt =>
            {
                opt.Password.Hasher.IterationCount = 4321;
                opt.Token.SymmetricPublicKey = new string('k', 64);
                opt.Token.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters();
            });

        using var provider = Microsoft.Extensions.DependencyInjection.ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(services);
        using var scope = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.CreateScope(provider);

        var hasher = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<Milvasoft.Identity.Abstract.IMilvaPasswordHasher>(scope.ServiceProvider);

        // Act
        var bytes = Convert.FromBase64String(hasher.HashPassword("testPassword"));

        // Assert
        System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(5)).Should().Be(4321u);
    }

    [Fact]
    public void VerifyHashedPassword_WithTruncatedHash_ShouldReturnFalse()
    {
        var hash = Convert.FromBase64String(_hasher.HashPassword("testPassword"));

        _hasher.VerifyHashedPassword(Convert.ToBase64String(hash[..12]), "testPassword").Should().BeFalse();
        _hasher.VerifyHashedPassword(Convert.ToBase64String(hash[..20]), "testPassword").Should().BeFalse();
    }
}