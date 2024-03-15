using FluentAssertions;
using Milvasoft.Core.Extensions;
using Milvasoft.UnitTests.CoreTests.ExtensionsTests.StringTests.Fixtures;
using System.Globalization;

namespace Milvasoft.UnitTests.CoreTests.ExtensionsTests.StringTests;

public partial class StringExtensionTests
{
    [Theory]
    [ClassData(typeof(InvalidStringDataFixture))]
    public void ToLowerFirst_InputStringIsInvalidOrUnableToLowercaseFirstCharacterWithAnyCulture_ShouldReturnsEmptyString(string input, string cultureCode, string expected)
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
    public void ToLowerFirst_InputStringFirstCharacterIsTurkishCharacterWithTurkishCulture_ShouldReturnsFirstCharacterLowercasedInputString(string input, string cultureCode, string expected)
    {
        //Arrange

        //Act
        var result = input.ToLowerFirst(new CultureInfo(cultureCode));

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Irmak", "en-US", "irmak")]
    [InlineData("İksir", "en-US", "iksir")]
    public void ToLowerFirst_InputStringFirstCharacterIsTurkishCharacterWithEnglishCulture_ShouldReturnsFirstCharacterLowercasedInputString(string input, string cultureCode, string expected)
    {
        //Arrange

        //Act
        var result = input.ToLowerFirst(new CultureInfo(cultureCode));

        //Assert
        result.Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(InvalidStringDataFixture))]
    public void ToLowerInvariantFirst_InputStringIsInvalidOrUnableToLowercaseFirstCharacterWithAnyCulture_ShouldReturnsEmptyString(string input, string cultureCode, string expected)
    {
        //Arrange
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        string result = input.ToLowerInvariantFirst();

        //Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToLowerInvariantFirst_InputStringFirstCharacterIsTurkishCharacterWithTurkishCulture_ShouldReturnsFirstCharacterLowercasedInputString()
    {
        //Arrange
        var input = "Irmak";
        var expected = "irmak";
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

        //Act
        string result = input.ToLowerInvariantFirst();

        //Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToLowerInvariantFirst_InputStringFirstCharacterIsTurkishCharacterWithEnglishCulture_ShouldReturnsFirstCharacterLowercasedInputString()
    {
        //Arrange
        var input = "Irmak";
        var expected = "irmak";
        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        //Act
        string result = input.ToLowerInvariantFirst();

        //Assert
        result.Should().Be(expected);
    }
}
