using FluentAssertions;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.StringTests.Fixtures;
using System.Globalization;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.StringTests;

public partial class StringHelperTests
{
    [Theory]
    [ClassData(typeof(InvalidStringDataWithCultureCodeFixture))]
    public void ToUpperFirst_InputStringIsInvalidWithAnyCulture_ShouldReturnEmptyString(string input, string cultureCode, string expected)
    {
        //Arrange
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        var result = input.ToUpperFirst();

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(SpecialCharacteredStringDataWithCultureCodeFixture))]
    public void ToUpperFirst_InputStringFirstCharacterIsUnableToUppercaseWithAnyCulture_ShouldReturnInputString(string input, string cultureCode, string expected)
    {
        //Arrange
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        var result = input.ToLowerFirst();

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("iksir", "tr-TR", "İksir")]
    [InlineData("ırmak", "tr-TR", "Irmak")]
    [InlineData("iksir", "en-US", "Iksir")]
    [InlineData("ırmak", "en-US", "Irmak")]
    public void ToUpperFirst_InputStringIsValidWithDifferentCultures_ShouldReturnFirstCharacterUppercasedInputString(string input, string cultureCode, string expected)
    {
        //Arrange

        //Act
        var result = input.ToUpperFirst(new CultureInfo(cultureCode));

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(InvalidStringDataWithCultureCodeFixture))]
    public void ToUpperInvariantFirst_InputStringIsInvalidWithDifferentCultures_ShouldReturnEmptyString(string input, string cultureCode, string expected)
    {
        //Arrange
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        var result = input.ToUpperInvariantFirst();

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(SpecialCharacteredStringDataWithCultureCodeFixture))]
    public void ToUpperInvariantFirst_InputStringFirstCharacterIsUnableToUppercaseWithDifferentCultures_ShouldReturnEmptyString(string input, string cultureCode, string expected)
    {
        //Arrange
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        var result = input.ToUpperInvariantFirst();

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-US")]
    public void ToUpperInvariantFirst_InputStringFirstCharacterIsTurkishCharacterWithDifferentCultures_ShouldReturnFirstCharacterUppercasedInputString(string cultureCode)
    {
        //Arrange
        var input = "iksir";
        var expected = "Iksir";
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        var result = input.ToUpperInvariantFirst();

        //Assert
        result.Should().Be(expected);
    }
}
