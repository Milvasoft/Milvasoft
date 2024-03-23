using FluentAssertions;
using Milvasoft.Core.Helpers.GeoLocation;
using Milvasoft.Core.Helpers.GeoLocation.Models;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.RegexTests;

public partial class GeoLocationExtensionsTests
{
    [Fact]
    public void CalculateDistance_ShouldReturnZero_WhenPointsAreSame()
    {
        // Arrange
        var point = new GeoPoint(40.1234, 29.5678);

        // Act
        var distance = point.CalculateDistance(point);

        // Assert
        distance.Should().Be(0);
    }

    [Fact]
    public void CalculateDistance_ShouldReturnCorrectDistance_WhenPointsAreDifferent()
    {
        // Arrange
        var point1 = new GeoPoint(40.1234, 29.5678);
        var point2 = new GeoPoint(41.4321, 30.8765);
        var expectedDistance = 182.530; // Expected distance between the points in kilometers with 3 decimal places precision

        // Act
        var distance = point1.CalculateDistance(point2);

        // Assert
        distance.Should().BeApproximately(expectedDistance, 0.001);
    }
}
