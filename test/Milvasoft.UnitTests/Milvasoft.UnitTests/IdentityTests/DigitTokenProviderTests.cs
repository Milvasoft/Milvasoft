using FluentAssertions;
using Milvasoft.Core.Exceptions;
using Milvasoft.Identity.Concrete.Entity;
using Milvasoft.Identity.TokenProvider;
using System.Security.Cryptography;
using System.Text;

namespace Milvasoft.UnitTests.IdentityTests;

[Trait("Identity Unit Tests", "Milvasoft.Identity project unit tests.")]
public class DigitTokenProviderTests
{
    private static readonly byte[] _secret = RandomNumberGenerator.GetBytes(32);

    private class TestUser : MilvaUser<Guid>
    {
        public TestUser(Guid id, string phoneNumber)
        {
            Id = id;
            PhoneNumber = phoneNumber;
            Email = "info@milvasoft.com";
            PasswordHash = "hash";
        }
    }

    [Fact]
    public void Generate_ShouldReturnSixDigitToken()
    {
        // Arrange
        var user = new TestUser(Guid.NewGuid(), "1234567890");

        // Act
        var token = DigitTokenProvider<TestUser, Guid>.Generate(Purpose.EmailConfirm, user, _secret);

        // Assert
        token.Should().HaveLength(6);
        token.Should().MatchRegex("^[0-9]{6}$");
    }

    [Fact]
    public void Validate_ShouldReturnTrueForValidToken()
    {
        // Arrange
        var user = new TestUser(Guid.NewGuid(), "1234567890");
        var token = DigitTokenProvider<TestUser, Guid>.Generate(Purpose.EmailConfirm, user, _secret);

        // Act
        var isValid = DigitTokenProvider<TestUser, Guid>.Validate(Purpose.EmailConfirm, token, user, _secret);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldReturnFalseForMalformedToken()
    {
        var user = new TestUser(Guid.NewGuid(), "1234567890");

        DigitTokenProvider<TestUser, Guid>.Validate(Purpose.EmailConfirm, null, user, _secret).Should().BeFalse();
        DigitTokenProvider<TestUser, Guid>.Validate(Purpose.EmailConfirm, "12345", user, _secret).Should().BeFalse();
        DigitTokenProvider<TestUser, Guid>.Validate(Purpose.EmailConfirm, "-12345", user, _secret).Should().BeFalse();
        DigitTokenProvider<TestUser, Guid>.Validate(Purpose.EmailConfirm, "abcdef", user, _secret).Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldReturnFalseForOtherPurpose()
    {
        // Arrange
        var user = new TestUser(Guid.NewGuid(), "1234567890");
        var token = DigitTokenProvider<TestUser, Guid>.Generate(Purpose.EmailConfirm, user, _secret);

        // Act & Assert
        DigitTokenProvider<TestUser, Guid>.Validate(Purpose.PasswordReset, token, user, _secret).Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldReturnFalseForOtherBoundValue()
    {
        // Arrange
        var user = new TestUser(Guid.NewGuid(), "1234567890");
        var token = DigitTokenProvider<TestUser, Guid>.Generate(Purpose.EmailChange, user, _secret, "new@milvasoft.com");

        // Act & Assert
        DigitTokenProvider<TestUser, Guid>.Validate(Purpose.EmailChange, token, user, _secret, "new@milvasoft.com").Should().BeTrue();
        DigitTokenProvider<TestUser, Guid>.Validate(Purpose.EmailChange, token, user, _secret, "attacker@milvasoft.com").Should().BeFalse();
    }

    /// <summary>
    /// The previous implementation keyed the HMAC with the user id and a public modifier only, so anyone who knew a user's id
    /// could compute that user's code. Codes must depend on a server-side secret.
    /// </summary>
    [Fact]
    public void Token_ShouldNotBeComputableFromPublicUserDataOnly()
    {
        // Arrange
        var user = new TestUser(Guid.NewGuid(), "1234567890");
        var otherSecret = RandomNumberGenerator.GetBytes(32);
        var forgedCodes = Enumerable.Range(-2, 5).Select(step => LegacyForgedCode(user, step)).ToList();

        // Act
        var token = DigitTokenProvider<TestUser, Guid>.Generate(Purpose.PasswordReset, user, _secret);

        // Assert
        DigitTokenProvider<TestUser, Guid>.Validate(Purpose.PasswordReset, token, user, otherSecret).Should().BeFalse();
        forgedCodes.Should().NotContain(token);
    }

    [Fact]
    public void Validate_ShouldReturnFalseAfterSecurityRelevantChange()
    {
        // Arrange
        var user = new TestUser(Guid.NewGuid(), "1234567890");
        var token = DigitTokenProvider<TestUser, Guid>.Generate(Purpose.EmailConfirm, user, _secret);

        // Act
        user.EmailConfirmed = true;

        // Assert
        DigitTokenProvider<TestUser, Guid>.Validate(Purpose.EmailConfirm, token, user, _secret).Should().BeFalse();
    }

    [Fact]
    public void Generate_WithShortSecret_ShouldThrow()
    {
        var user = new TestUser(Guid.NewGuid(), "1234567890");

        Action act = () => DigitTokenProvider<TestUser, Guid>.Generate(Purpose.EmailConfirm, user, new byte[16]);

        act.Should().Throw<MilvaDeveloperException>();
    }

    /// <summary>
    /// Code an attacker could compute with the previous implementation: HMACSHA512 keyed by the user id (UTF-16) over the time step and a public modifier.
    /// </summary>
    private static string LegacyForgedCode(TestUser user, int stepOffset)
    {
        var step = (ulong)((DateTimeOffset.UtcNow.UtcTicks - DateTimeOffset.UnixEpoch.UtcTicks) / TimeSpan.FromMinutes(3).Ticks) + (ulong)stepOffset;
        var stepBytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder((long)step));
        var modifier = Encoding.UTF8.GetBytes("PhoneNumber:PasswordReset");
        var hash = HMACSHA512.HashData(Encoding.Unicode.GetBytes(user.Id.ToString()), stepBytes.Concat(modifier).ToArray());
        var offset = hash[^1] & 0xf;
        var binaryCode = (hash[offset] & 0x7f) << 24 | (hash[offset + 1] & 0xff) << 16 | (hash[offset + 2] & 0xff) << 8 | hash[offset + 3] & 0xff;

        return (binaryCode % 1000000).ToString("D6");
    }
}
