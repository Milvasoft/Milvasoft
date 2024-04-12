using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.Exceptions;
using Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;
using Milvasoft.UnitTests.TestHelpers;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.RequestTests;

public class FilterRequestTests
{
    #region BuildFilterExpression

    [Theory]
    [ClassData(typeof(InvalidListSourceForBuildFilterExpressionMethodData))]
    public void BuildFilterExpression_WithInvalidFilterTypeOrFilterBy_ShouldReturnDefaultExpression(FilterRequest filterRequest)
    {
        // Arrange
        Expression<Func<RestTestEntityFixture, bool>> expectedExpression = x => true;

        // Act
        var result = filterRequest.BuildFilterExpression<RestTestEntityFixture>();

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(result, expectedExpression);
        equality.Should().BeTrue();
    }

    [Theory]
    [ClassData(typeof(ValidListSourceButFilterValuesNotValidForBuildFilterExpressionMethodData))]
    public void BuildFilterExpression_WithValidFilterTypeAndFilterByButFilterValuesAreNull_ShouldThrowException(FilterRequest filterRequest)
    {
        // Arrange

        // Act
        Action act = () => filterRequest.BuildFilterExpression<RestTestEntityFixture>();

        // Assert
        act.Should().Throw<MilvaDeveloperException>().WithMessage("Please provide filter values!");
    }

    [Theory]
    [ClassData(typeof(ValidListSourceForBuildFilterExpressionMethodData))]
    public void BuildFilterExpression_ForListSource_WithValidFilterTypeAndFilterBy_ShouldReturnCorrectExpression(IQueryable<RestTestEntityFixture> source, FilterRequest filterRequest, List<int> expectedIdList)
    {
        // Arrange

        // Act
        var resultExpression = filterRequest.BuildFilterExpression<RestTestEntityFixture>();
        var result = source.Where(resultExpression).ToList();

        // Assert
        result.Should().HaveCount(expectedIdList.Count);
        result.Should().AllSatisfy(i => expectedIdList.Should().Contain(i.Id));
    }

    [Theory]
    [ClassData(typeof(ValidListSourceForBuildFilterExpressionMethodData))]
    public async Task BuildFilterExpression_ForEfSource_WithValidFilterTypeAndFilterBy_ShouldReturnCorrectExpression(IQueryable<RestTestEntityFixture> source, FilterRequest filterRequest, List<int> expectedIdList)
    {
        // Arrange
        using var dbContextFixture = new DbContextMock<RestDbContextFixture>(nameof(FilterRequest)).GetDbContextFixture();
        await dbContextFixture.TestEntities.AddRangeAsync(source);
        await dbContextFixture.SaveChangesAsync();

        // Act
        var resultExpression = filterRequest.BuildFilterExpression<RestTestEntityFixture>();
        var result = await dbContextFixture.TestEntities.Where(resultExpression).ToListAsync();

        // Assert
        result.Should().HaveCount(expectedIdList.Count);
        result.Should().AllSatisfy(i => expectedIdList.Should().Contain(i.Id));
    }

    #endregion
}
