﻿using FluentAssertions;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.StringTests.Fixtures;
using System.Globalization;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.StringTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public partial class StringHelperTests
{
    [Theory]
    [ClassData(typeof(InvalidStringDataWithCultureCodeFixture))]
    [ClassData(typeof(SpecialCharacteredStringDataWithCultureCodeFixture))]
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
    [InlineData("Irmak", "tr-TR", "ırmak")]
    [InlineData("İksir", "tr-TR", "iksir")]
    [InlineData("Irmak", "en-US", "irmak")]
    [InlineData("İksir", "en-US", "iksir")]
    [InlineData("I", "tr-TR", "ı")]
    [InlineData("I", "en-US", "i")]
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
    [ClassData(typeof(SpecialCharacteredStringDataWithCultureCodeFixture))]
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

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-US")]
    public void ToLowerInvariantFirst_WithInputStringHasOneCharacterFirstCharacterIsTurkishCharacterWithDifferentCultures_ShouldReturnFirstCharacterLowercasedInputString(string cultureCode)
    {
        //Arrange
        var input = "I";
        var expected = "i";
        CultureInfo.CurrentCulture = new CultureInfo(cultureCode);

        //Act
        string result = input.ToLowerInvariantFirst();

        //Assert
        result.Should().Be(expected);
    }
}
