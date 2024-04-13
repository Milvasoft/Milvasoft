using FluentAssertions;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.StringTests.Fixtures;
using System.Globalization;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.StringTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public partial class StringHelperTests
{
    [Theory]
    [ClassData(typeof(InvalidStringDataWithCultureCodeFixture))]
    public void ToUpperFirst_WithInputStringIsInvalidWithAnyCulture_ShouldReturnEmptyString(string input, string cultureCode, string expected)
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
    public void ToUpperFirst_WithInputStringFirstCharacterIsUnableToUppercaseWithAnyCulture_ShouldReturnInputString(string input, string cultureCode, string expected)
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
    public void ToUpperFirst_WithInputStringIsValidWithDifferentCultures_ShouldReturnFirstCharacterUppercasedInputString(string input, string cultureCode, string expected)
    {
        //Arrange

        //Act
        var result = input.ToUpperFirst(new CultureInfo(cultureCode));

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(InvalidStringDataWithCultureCodeFixture))]
    [ClassData(typeof(SpecialCharacteredStringDataWithCultureCodeFixture))]
    public void ToUpperInvariantFirst_WithInputStringIsInvalidWithDifferentCultures_ShouldReturnEmptyString(string input, string cultureCode, string expected)
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
    public void ToUpperInvariantFirst_WithInputStringFirstCharacterIsTurkishCharacterWithDifferentCultures_ShouldReturnFirstCharacterUppercasedInputString(string cultureCode)
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
