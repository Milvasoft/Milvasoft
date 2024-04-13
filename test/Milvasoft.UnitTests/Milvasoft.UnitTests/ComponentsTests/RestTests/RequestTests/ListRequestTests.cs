using FluentAssertions;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.Exceptions;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.RequestTests;

[Trait("Rest Components Unit Tests", "Milvasoft.Components.Rest project unit tests.")]
public class ListRequestTests
{
    #region CalculatePageCountAndCompareWithRequested

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, 1)]
    [InlineData(1, null)]
    public void CalculatePageCountAndCompareWithRequested_WithPageNumberOrPageCountIsNull_ShouldReturnZero(int? pageNumber, int? rowCount)
    {
        // Arrange
        var request = new ListRequest
        {
            PageNumber = pageNumber,
            RowCount = rowCount,
        };

        // Act
        var result = request.CalculatePageCountAndCompareWithRequested(30);

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(1, -10)]
    public void CalculatePageCountAndCompareWithRequested_WithInvalidPageNumberOrPageCount_ShouldThrowException(int? pageNumber, int? rowCount)
    {
        // Arrange
        var request = new ListRequest
        {
            PageNumber = pageNumber,
            RowCount = rowCount,
        };

        // Act
        Action act = () => request.CalculatePageCountAndCompareWithRequested(30);

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>();
    }

    [Fact]
    public void CalculatePageCountAndCompareWithRequested_WithTotalDataCountIsNull_ShouldReturnZero()
    {
        // Arrange
        var request = new ListRequest
        {
            PageNumber = 1,
            RowCount = 10,
        };

        // Act
        var result = request.CalculatePageCountAndCompareWithRequested(null);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculatePageCountAndCompareWithRequested_WithPageNumberBiggerThanActualPageCount_ShouldThrowException()
    {
        // Arrange
        var request = new ListRequest
        {
            PageNumber = 2,
            RowCount = 10,
        };

        // Act
        Action act = () => request.CalculatePageCountAndCompareWithRequested(10);

        // Assert
        act.Should().Throw<MilvaUserFriendlyException>();
    }

    [Theory]
    [InlineData(1, 10, 29, 3)]
    [InlineData(1, 10, 33, 4)]
    [InlineData(1, 12, 11, 1)]
    [InlineData(1, 10, 40, 4)]
    [InlineData(1, 20, 40, 2)]
    [InlineData(1, 20, 0, 0)]
    public void CalculatePageCountAndCompareWithRequested_WithValidParameters_ShouldReturnCorrectPageCount(int? pageNumber, int? rowCount, int? totalDataCount, int expectedResult)
    {
        // Arrange
        var request = new ListRequest
        {
            PageNumber = pageNumber,
            RowCount = rowCount,
        };

        // Act
        var result = request.CalculatePageCountAndCompareWithRequested(totalDataCount);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion
}
