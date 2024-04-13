using FluentAssertions;
using Milvasoft.Core.Helpers;
using System.Text.RegularExpressions;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.RegexTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public partial class RegexHelperTests
{
    #region MatchRegex

    [Fact]
    public void MatchRegex_ForRegexObjectParameter_WithNullRegexObject_ShouldReturnFalse()
    {
        // Arrange
        var input = "123-456-7890";
        Regex regex = null;

        // Act
        var result = input.MatchRegex(regex);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void MatchRegex_ForRegexObjectParameter_WithInvalidInputString_ShouldReturnFalse(string input)
    {
        // Arrange
        var regexPattern = @"^\d{3}-\d{3}-\d{4}$";
        var regex = new Regex(regexPattern);

        // Act
        var result = input.MatchRegex(regex);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("123-456-7890", true)]
    [InlineData("1234567890", false)]
    public void MatchRegex_ForRegexObjectParameter_WithValidInput_ShouldReturnCorrectResult(string input, bool expected)
    {
        // Arrange
        var regexPattern = @"^\d{3}-\d{3}-\d{4}$";
        var regex = new Regex(regexPattern);

        // Act
        var result = input.MatchRegex(regex);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(" ", false)]
    [InlineData("", true)]
    public void MatchRegex_ForRegexStringParameter_WithInvalidRegexString_ShouldReturnFalse(string regexPattern, bool expected)
    {
        // Arrange
        var input = "123-456-7890";

        // Act
        var result = input.MatchRegex(regexPattern);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void MatchRegex_ForRegexStringParameter_WithInvalidInputString_ShouldReturnFalse(string input)
    {
        // Arrange
        var regexPattern = @"^\d{3}-\d{3}-\d{4}$";

        // Act
        var result = input.MatchRegex(regexPattern);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("123-456-7890", true)]
    [InlineData("1234567890", false)]
    public void MatchRegex_ForRegexStringParameter_WithValidInput_ShouldReturnCorrectResult(string input, bool expected)
    {
        // Arrange
        var regexPattern = @"^\d{3}-\d{3}-\d{4}$";

        // Act
        var result = input.MatchRegex(regexPattern);

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
