using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.Exceptions;
using Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;
using Milvasoft.UnitTests.TestHelpers;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.RequestTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
public class SortRequestTests
{
    #region Mock Data

    /// <summary>
    /// source , filter request, property selector
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> ValidAscendingSortRequestForBuildPropertySelectorExpressionMethodData()
    {
        IQueryable<RestTestEntityFixture> validQueryable = new List<RestTestEntityFixture>
        {
            new() {
                Id = 1,
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two,
                IsActive = true,
                InsertDate = DateTime.Now,
                UpdateDate = DateTime.Now.AddDays(1),
            },
            new() {
                Id = 2,
                Name = "Elise",
                Count = 10,
                Price = 15.5M,
                Number = RestTestEnumFixture.One,
                IsActive = false,
                InsertDate = DateTime.Now.AddDays(4),
                UpdateDate = DateTime.Now.AddDays(3),
            },
            new() {
                Id = 3,
                Name = "Jack",
                Count = 90,
                Price = 0.50M,
                Number = RestTestEnumFixture.Zero,
                IsActive = false,
                InsertDate = DateTime.Now,
                UpdateDate = null
            },
            new() {
                Id = 4,
                Name = "Mary",
                Count = 101,
                Price = 4M,
                Number = RestTestEnumFixture.One,
                IsActive = true,
                InsertDate = DateTime.Now,
                UpdateDate = null
            },
            new() {
                Id = 5,
                Name = null,
                Count = 999,
                Price = 999M,
                Number = RestTestEnumFixture.Zero,
                IsActive = null,
                InsertDate = DateTime.Now.AddMonths(-5),
                UpdateDate = null
            },
        }.AsQueryable();

        Expression<Func<RestTestEntityFixture, object>> nameSelector = i => (object)i.Name;
        Expression<Func<RestTestEntityFixture, object>> numberSelector = i => (object)i.Number;
        Expression<Func<RestTestEntityFixture, object>> countSelector = i => (object)i.Count;
        Expression<Func<RestTestEntityFixture, object>> priceSelector = i => (object)i.Price;
        Expression<Func<RestTestEntityFixture, object>> isActiveSelector = i => (object)i.IsActive;
        Expression<Func<RestTestEntityFixture, object>> insertDateSelector = i => (object)i.InsertDate;
        Expression<Func<RestTestEntityFixture, object>> updateDateSelector = i => (object)i.UpdateDate;

        yield return new object[]
        {
            validQueryable,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Number), Type = SortType.Asc },
            numberSelector
        };

        yield return new object[]
        {
            validQueryable,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Name), Type = SortType.Asc },
            nameSelector
        };

        yield return new object[]
        {
            validQueryable,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Count), Type = SortType.Asc },
            countSelector
        };

        yield return new object[]
        {
            validQueryable,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Price), Type = SortType.Asc },
            priceSelector
        };

        yield return new object[]
        {
            validQueryable,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.IsActive), Type = SortType.Asc },
            isActiveSelector
        };

        yield return new object[]
        {
            validQueryable,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.InsertDate), Type = SortType.Asc },
            insertDateSelector
        };

        yield return new object[]
        {
            validQueryable,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.UpdateDate), Type = SortType.Asc },
            updateDateSelector
        };

    }

    /// <summary>
    /// source , filter request, property selector
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> ValidDescendingSortRequestForBuildPropertySelectorExpressionMethodData()
    {
        IQueryable<RestTestEntityFixture> validQueryable = new List<RestTestEntityFixture>
        {
            new() {
                Id = 1,
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two,
                IsActive = true,
                InsertDate = DateTime.Now,
                UpdateDate = DateTime.Now.AddDays(1),
            },
            new() {
                Id = 2,
                Name = "Elise",
                Count = 10,
                Price = 15.5M,
                Number = RestTestEnumFixture.One,
                IsActive = false,
                InsertDate = DateTime.Now.AddDays(4),
                UpdateDate = DateTime.Now.AddDays(3),
            },
            new() {
                Id = 3,
                Name = "Jack",
                Count = 90,
                Price = 0.50M,
                Number = RestTestEnumFixture.Zero,
                IsActive = false,
                InsertDate = DateTime.Now,
                UpdateDate = null
            },
            new() {
                Id = 4,
                Name = "Mary",
                Count = 101,
                Price = 4M,
                Number = RestTestEnumFixture.One,
                IsActive = true,
                InsertDate = DateTime.Now,
                UpdateDate = null
            },
            new() {
                Id = 5,
                Name = null,
                Count = 999,
                Price = 999M,
                Number = RestTestEnumFixture.Zero,
                IsActive = null,
                InsertDate = DateTime.Now.AddMonths(-5),
                UpdateDate = null
            },
        }.AsQueryable();

        Expression<Func<RestTestEntityFixture, object>> nameSelector = i => (object)i.Name;
        Expression<Func<RestTestEntityFixture, object>> numberSelector = i => (object)i.Number;
        Expression<Func<RestTestEntityFixture, object>> countSelector = i => (object)i.Count;
        Expression<Func<RestTestEntityFixture, object>> priceSelector = i => (object)i.Price;
        Expression<Func<RestTestEntityFixture, object>> isActiveSelector = i => (object)i.IsActive;
        Expression<Func<RestTestEntityFixture, object>> insertDateSelector = i => (object)i.InsertDate;
        Expression<Func<RestTestEntityFixture, object>> updateDateSelector = i => (object)i.UpdateDate;

        yield return new object[]
        {
            validQueryable,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Number), Type = SortType.Desc },
            numberSelector
        };

        yield return new object[]
        {
            validQueryable,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Name), Type = SortType.Desc },
            nameSelector
        };

        yield return new object[]
        {
            validQueryable,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Count), Type = SortType.Desc },
            countSelector
        };

        yield return new object[]
        {
            validQueryable,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Price), Type = SortType.Desc },
            priceSelector
        };

        yield return new object[]
        {
            validQueryable,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.IsActive), Type = SortType.Desc },
            isActiveSelector
        };

        yield return new object[]
        {
            validQueryable,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.InsertDate), Type = SortType.Desc },
            insertDateSelector
        };

        yield return new object[]
        {
            validQueryable,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.UpdateDate), Type = SortType.Desc },
            updateDateSelector
        };

    }

    #endregion

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
    [MemberData(nameof(ValidAscendingSortRequestForBuildPropertySelectorExpressionMethodData))]
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
    [MemberData(nameof(ValidDescendingSortRequestForBuildPropertySelectorExpressionMethodData))]
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
    [MemberData(nameof(ValidAscendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public async Task BuildPropertySelectorExpression_ForEfSource_WithAscendingSortRequest_ShouldReturnCorrectExpression(IQueryable<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange
        var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(SortRequest)).GetDbContextFixture();
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
    [MemberData(nameof(ValidDescendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public async Task BuildPropertySelectorExpression_ForEfSource_WithDescendingSortRequest_ShouldReturnCorrectExpression(IQueryable<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange
        var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(SortRequest)).GetDbContextFixture();
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
