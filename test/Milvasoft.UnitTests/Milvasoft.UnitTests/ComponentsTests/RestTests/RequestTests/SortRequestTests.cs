using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.Exceptions;
using Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;
using Milvasoft.UnitTests.TestHelpers;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.RequestTests;

[Trait("Rest Components Unit Tests", "Milvasoft.Components.Rest project unit tests.")]
public class SortRequestTests
{
    #region BuildPropertySelectorExpression

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void BuildPropertySelectorExpression_WithInvalidSortBy_ShouldReturnNull(string sortBy)
    {
        // Arrange
        var sortRequest = new SortRequest()
        {
            SortBy = sortBy
        };

        // Act
        var result = sortRequest.BuildPropertySelectorExpression<RestTestEntityFixture>();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void BuildPropertySelectorExpression_WithValidSortRequestsButEntityNotContainsPropertyName_ShouldThrowException()
    {
        // Arrange
        var sortRequest = new SortRequest()
        {
            SortBy = "NotExistsPropertyName"
        };

        // Act
        Action act = () => sortRequest.BuildPropertySelectorExpression<RestTestEntityFixture>();

        // Assert
        act.Should().Throw<MilvaDeveloperException>();
    }

    [Theory]
    [ClassData(typeof(ValidAscendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public void BuildPropertySelectorExpression_ForListSource_WithAscendingSortRequest_ShouldReturnCorrectExpression(IQueryable<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange

        // Act
        var resultExpression = sortRequest.BuildPropertySelectorExpression<RestTestEntityFixture>();
        var result = source.OrderBy(resultExpression).ToList();

        // Assert
        var expressionEquality = ExpressionEqualityComparer.Instance.Equals(resultExpression, expectedExpression);
        expressionEquality.Should().BeTrue();
        result.Should().BeInAscendingOrder(expectedExpression);
    }

    [Theory]
    [ClassData(typeof(ValidDescendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public void BuildPropertySelectorExpression_ForListSource_WithDescendingSortRequest_ShouldReturnCorrectExpression(IQueryable<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange

        // Act
        var resultExpression = sortRequest.BuildPropertySelectorExpression<RestTestEntityFixture>();
        var result = source.OrderByDescending(resultExpression).ToList();

        // Assert
        var expressionEquality = ExpressionEqualityComparer.Instance.Equals(resultExpression, expectedExpression);
        expressionEquality.Should().BeTrue();
        result.Should().BeInDescendingOrder(expectedExpression);
    }

    [Theory]
    [ClassData(typeof(ValidAscendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public async Task BuildPropertySelectorExpression_ForEfSource_WithAscendingSortRequest_ShouldReturnCorrectExpression(IQueryable<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange
        await using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(SortRequest)).GetDbContextFixture();
        await dbContextMock.TestEntities.AddRangeAsync(source);
        await dbContextMock.SaveChangesAsync();

        // Act
        var resultExpression = sortRequest.BuildPropertySelectorExpression<RestTestEntityFixture>();
        var result = await dbContextMock.TestEntities.OrderBy(resultExpression).ToListAsync();

        // Assert
        var expressionEquality = ExpressionEqualityComparer.Instance.Equals(resultExpression, expectedExpression);
        expressionEquality.Should().BeTrue();
        result.Should().BeInAscendingOrder(expectedExpression);
    }

    [Theory]
    [ClassData(typeof(ValidDescendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public async Task BuildPropertySelectorExpression_ForEfSource_WithDescendingSortRequest_ShouldReturnCorrectExpression(IQueryable<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange
        await using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(SortRequest)).GetDbContextFixture();
        await dbContextMock.TestEntities.AddRangeAsync(source);
        await dbContextMock.SaveChangesAsync();

        // Act
        var resultExpression = sortRequest.BuildPropertySelectorExpression<RestTestEntityFixture>();
        var result = await dbContextMock.TestEntities.OrderByDescending(resultExpression).ToListAsync();

        // Assert
        var expressionEquality = ExpressionEqualityComparer.Instance.Equals(resultExpression, expectedExpression);
        expressionEquality.Should().BeTrue();
        result.Should().BeInDescendingOrder(expectedExpression);
    }

    #endregion
}
