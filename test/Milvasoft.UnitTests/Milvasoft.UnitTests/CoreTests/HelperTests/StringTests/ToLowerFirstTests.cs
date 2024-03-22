using FluentAssertions;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.StringTests.Fixtures;
using System.Globalization;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.StringTests;

public partial class StringHelperTests
{
    [Theory]
    [ClassData(typeof(InvalidStringDataWithCultureCodeFixture))]
    public void ToLowerFirst_WithInputStringIsInvalidWithDifferentCultures_ShouldReturnEmptyString(string input, string cultureCode, string expected)
    {
        //Arrange
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        var result = input.ToLowerFirst();

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(SpecialCharacteredStringDataWithCultureCodeFixture))]
    public void ToLowerFirst_WithInputStringFirstCharacterIsUnableToLowercaseWithDifferentCultures_ShouldReturnInputString(string input, string cultureCode, string expected)
    {
        //Arrange
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        var result = input.ToLowerFirst();

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Irmak", "tr-TR", "ırmak")]
    [InlineData("İksir", "tr-TR", "iksir")]
    [InlineData("Irmak", "en-US", "irmak")]
    [InlineData("İksir", "en-US", "iksir")]
    public void ToLowerFirst_WithInputStringIsValidWithDifferentCultures_ShouldReturnFirstCharacterLowercasedInputString(string input, string cultureCode, string expected)
    {
        //Arrange

        //Act
        var result = input.ToLowerFirst(new CultureInfo(cultureCode));

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(InvalidStringDataWithCultureCodeFixture))]
    public void ToLowerInvariantFirst_WithInputStringIsInvalidWithDifferentCultures_ShouldReturnEmptyString(string input, string cultureCode, string expected)
    {
        //Arrange
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        string result = input.ToLowerInvariantFirst();

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(SpecialCharacteredStringDataWithCultureCodeFixture))]
    public void ToLowerInvariantFirst_WithInputStringFirstCharacterIsUnableToLowercaseWithDifferentCultures_ShouldReturnEmptyString(string input, string cultureCode, string expected)
    {
        //Arrange
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        string result = input.ToLowerInvariantFirst();

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-US")]
    public void ToLowerInvariantFirst_WithInputStringFirstCharacterIsTurkishCharacterWithDifferentCultures_ShouldReturnFirstCharacterLowercasedInputString(string cultureCode)
    {
        //Arrange
        var input = "Irmak";
        var expected = "irmak";
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        string result = input.ToLowerInvariantFirst();

        //Assert
        result.Should().Be(expected);
    }
}
