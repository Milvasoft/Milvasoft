using FluentAssertions;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.StringTests.Fixtures;
using System.Globalization;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.StringTests;

public partial class StringHelperTests
{
    [Theory]
    [ClassData(typeof(InvalidStringDataWithCultureCodeFixture))]
    public void ToLowerFirst_InputStringIsInvalidWithDifferentCultures_ShouldReturnsEmptyString(string input, string cultureCode, string expected)
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
    public void ToLowerFirst_InputStringFirstCharacterIsUnableToLowercaseWithDifferentCultures_ShouldReturnsInputString(string input, string cultureCode, string expected)
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
    public void ToLowerFirst_InputStringIsValidWithDifferentCultures_ShouldReturnsFirstCharacterLowercasedInputString(string input, string cultureCode, string expected)
    {
        //Arrange

        //Act
        var result = input.ToLowerFirst(new CultureInfo(cultureCode));

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(InvalidStringDataWithCultureCodeFixture))]
    public void ToLowerInvariantFirst_InputStringIsInvalidWithDifferentCultures_ShouldReturnsEmptyString(string input, string cultureCode, string expected)
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
    public void ToLowerInvariantFirst_InputStringFirstCharacterIsUnableToLowercaseWithDifferentCultures_ShouldReturnsEmptyString(string input, string cultureCode, string expected)
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
    public void ToLowerInvariantFirst_InputStringFirstCharacterIsTurkishCharacterWithDifferentCultures_ShouldReturnsFirstCharacterLowercasedInputString(string cultureCode)
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
