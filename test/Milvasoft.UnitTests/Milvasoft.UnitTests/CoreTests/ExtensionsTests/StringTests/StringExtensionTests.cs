using FluentAssertions;
using Milvasoft.Core.Extensions;
using System.Security.Cryptography;
using System.Text;

namespace Milvasoft.UnitTests.CoreTests.ExtensionsTests.StringTests;

public partial class StringExtensionTests
{
    public static IEnumerable<object[]> ValidStringByteArrayPairs()
    {
        yield return new object[] { " ", new byte[] { 32 } };
        yield return new object[] { "hello", new byte[] { 104, 101, 108, 108, 111 } };
        yield return new object[] { "İksir", new byte[] { 196, 176, 107, 115, 105, 114 } };
    }

    #region GetByteArray

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void GetByteArray_InputStringIsNullOrEmpty_ShouldReturnsNull(string input)
    {
        // Arrange

        // Act
        var result = input.GetByteArray();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(ValidStringByteArrayPairs))]
    public void GetByteArray_InputStringIsValidString_ShouldReturnsCorrectByteArray(string input, byte[] expected)
    {
        // Arrange

        // Act
        var result = input.GetByteArray();

        // Assert
        result.Should().Equal(expected);
    }

    #endregion

    #region GetString

    [Fact]
    public void GetString_InputArrayIsEmpty_ShouldReturnsEmptyString()
    {
        // Arrange
        byte[] input = [];
        string expected = string.Empty;

        // Act
        var result = input.GetString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetString_InputArrayIsNull_ShouldReturnsEmptyString()
    {
        // Arrange
        byte[] input = null;
        string expected = string.Empty;

        // Act
        var result = input.GetString();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(ValidStringByteArrayPairs))]
    public void GetString_InputArrayIsValid_ShouldReturnsCorrectString(string expected, byte[] input)
    {
        // Arrange

        // Act
        var result = input.GetString();

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region HashToByteArray

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void HashToByteArray_InputStringIsNullOrEmpty_ShouldReturnsNull(string input)
    {
        // Arrange

        // Act
        var result = input.HashToByteArray();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(ValidStringByteArrayPairs))]
    public void HashToByteArray_InputStringIsValidString_ShouldReturnsCorrectByteArray(string input, byte[] inputAsByteArray)
    {
        // Arrange
        byte[] expected = SHA256.HashData(inputAsByteArray);

        // Act
        var result = input.HashToByteArray();

        // Assert
        result.Should().Equal(expected);
    }

    #endregion

    #region Hash

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Hash_InputStringIsNullOrEmpty_ShouldReturnsEmptyString(string input)
    {
        // Arrange

        // Act
        var result = input.Hash();

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(" ", "36a9e7f1c95b82ffb99743e0c5c4ce95d83c9a430aac59f84ef3cbfab6145068")]
    [InlineData("hello", "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824")]
    [InlineData("İksir", "1f0d02a7fd55d91d18cbf6a81eca5e3c079665bce001aa66d97f959eacab1e6e")]
    public void Hash_InputStringIsValidString_ShouldReturnsCorrectByteArray(string input, string expected)
    {
        // Arrange

        // Act
        var result = input.Hash();

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
