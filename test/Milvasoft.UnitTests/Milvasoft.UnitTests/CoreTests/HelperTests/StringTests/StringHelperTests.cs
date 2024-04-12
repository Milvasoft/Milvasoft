using FluentAssertions;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.StringTests.Fixtures;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.StringTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
public partial class StringHelperTests
{
    public static IEnumerable<object[]> ValidStringByteArrayPairs()
    {
#pragma warning disable IDE0230 // Use UTF-8 string literal
        yield return new object[] { " ", new byte[] { 32 } };
        yield return new object[] { "hello", new byte[] { 104, 101, 108, 108, 111 } };
        yield return new object[] { "İksir", new byte[] { 196, 176, 107, 115, 105, 114 } };
#pragma warning restore IDE0230 // Use UTF-8 string literal
    }

    #region GetByteArray

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void GetByteArray_WithInputStringIsNullOrEmpty_ShouldReturnEmpty(string input)
    {
        // Arrange

        // Act
        var result = input.GetByteArray();

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(ValidStringByteArrayPairs))]
    public void GetByteArray_WithInputStringIsValidString_ShouldReturnCorrectByteArray(string input, byte[] expected)
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
    public void GetString_WithInputArrayIsEmpty_ShouldReturnEmptyString()
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
    public void GetString_WithInputArrayIsNull_ShouldReturnEmptyString()
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
    public void GetString_WithInputArrayIsValid_ShouldReturnCorrectString(string expected, byte[] input)
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
    public void HashToByteArray_WithInputStringIsNullOrEmpty_ShouldReturnEmpty(string input)
    {
        // Arrange

        // Act
        var result = input.HashToByteArray();

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(ValidStringByteArrayPairs))]
    public void HashToByteArray_WithInputStringIsValidString_ShouldReturnCorrectByteArray(string input, byte[] inputAsByteArray)
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
    public void Hash_WithInputStringIsNullOrEmpty_ShouldReturnEmptyString(string input)
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
    public void Hash_WithInputStringIsValidString_ShouldReturnCorrectByteArray(string input, string expected)
    {
        // Arrange

        // Act
        var result = input.Hash();

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region MilvaNormalize

    [Theory]
    [ClassData(typeof(InvalidStringDataWithCultureCodeFixture))]
    public void MilvaNormalize_WithInputStringIsInvalidWithAnyCulture_ShouldReturnEmptyString(string input, string cultureCode, string expected)
    {
        //Arrange
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        var result = input.MilvaNormalize();

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-US")]
    public void MilvaNormalize_WithInputStringIsValidButContainsSpecialCharacterWithDifferentCulture_ShouldReturnNormalizedString(string cultureCode)
    {
        //Arrange
        var input = "İKsir!";
        var expected = "IKSIR!";
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        var result = input.MilvaNormalize();

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-US")]
    public void MilvaNormalize_WithInputStringIsValidWithDifferentCulture_ShouldReturnNormalizedString(string cultureCode)
    {
        //Arrange
        var input = "İKsir";
        var expected = "IKSIR";
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        var result = input.MilvaNormalize();

        //Assert
        result.Should().Be(expected);
    }

    #endregion
}
