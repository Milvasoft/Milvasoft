using FluentAssertions;
using Milvasoft.Core.Extensions;
using Milvasoft.UnitTests.CoreTests.ExtensionsTests.StringTests.Fixtures;
using System.Globalization;

namespace Milvasoft.UnitTests.CoreTests.ExtensionsTests.StringTests;

public partial class StringExtensionTests
{
    [Theory]
    [ClassData(typeof(InvalidStringDataFixture))]
    public void ToUpperFirst_InputStringIsInvalidOrUnableToUppercaseFirstCharacterWithAnyCulture_ShouldReturnsEmptyString(string input, string cultureCode, string expected)
    {
        //Arrange
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        var result = input.ToUpperFirst();

        //Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToUpperFirst_InputStringFirstCharacterIsTurkishCharacterWithTurkishCulture_ShouldReturnsFirstCharacterUppercasedInputString()
    {
        //Arrange
        var input = "iksir";
        var expected = "İksir";

        //Act
        var result = input.ToUpperFirst(new CultureInfo("tr-TR"));

        //Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToUpperFirst_InputStringFirstCharacterIsTurkishCharacterWithEnglishCulture_ShouldReturnsFirstCharacterUppercasedInputString()
    {
        //Arrange
        var input = "iksir";
        var expected = "Iksir";

        //Act
        var result = input.ToUpperFirst(new CultureInfo("en-US"));

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(InvalidStringDataFixture))]
    public void ToUpperInvariantFirst_InputStringIsInvalidOrUnableToUppercaseFirstCharacterWithAnyCulture_ShouldReturnsEmptyString(string input, string cultureCode, string expected)
    {
        //Arrange
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        var result = input.ToUpperInvariantFirst();

        //Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToUpperInvariantFirst_InputStringFirstCharacterIsTurkishCharacterWithTurkishCulture_ShouldReturnsFirstCharacterUppercasedInputString()
    {
        //Arrange
        var input = "iksir";
        var expected = "Iksir";
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

        //Act
        var result = input.ToUpperInvariantFirst();

        //Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToUpperInvariantFirst_InputStringFirstCharacterIsTurkishCharacterWithEnglishCulture_ShouldReturnsFirstCharacterUppercasedInputString()
    {
        //Arrange
        var input = "iksir";
        var expected = "Iksir";
        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        //Act
        var result = input.ToUpperInvariantFirst();

        //Assert
        result.Should().Be(expected);
    }
}
