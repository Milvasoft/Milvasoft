using FluentAssertions;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.PropertyTests.Fixtures;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.PropertyTests;

public class PropertyHelperTests
{
    #region PropertyExists

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    public void PropertyExists_ForOverloadWithCollection_WithNullOrEmptyOrWhiteSpacePropertyNameInput_ShouldReturnFalse(string propertyName, bool expected)
    {
        // Arrange
        IQueryable<PropertyExistsTestModelFixture> inputList = null;

        // Act
        var result = inputList.PropertyExists(propertyName);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("poco", true)]
    [InlineData("Poco", true)]
    [InlineData("NotExistsPropName", false)]
    public void PropertyExists__ForOverloadWithCollection_WithValidPropertyNameInput_ShouldReturnExpected(string propertyName, bool expected)
    {
        // Arrange
        IQueryable<PropertyExistsTestModelFixture> inputList = null;

        // Act
        var result = inputList.PropertyExists(propertyName);

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
