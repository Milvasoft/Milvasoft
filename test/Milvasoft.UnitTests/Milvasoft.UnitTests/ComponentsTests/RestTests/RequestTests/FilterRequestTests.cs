using ExpressionBuilder.Common;
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

public class FilterRequestTests
{
    #region Mock Data

    /// <summary>
    /// filter request
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> InvalidListSourceForBuildFilterExpressionMethodData()
    {
        yield return new object[]
        {
            new FilterRequest
            {
                Criterias =
                [
                    new()
                    {
                        FilterBy = null
                    },
                    new()
                    {
                        FilterBy = ""
                    },
                    new()
                    {
                        FilterBy = " "
                    },
                    new()
                    {
                        FilterBy = "NotExistsPropertyName"
                    },
                    new()
                    {
                        FilterBy = nameof(RestTestEntityFixture.Name),
                        Type = FilterType.Between
                    },
                    new()
                    {
                        FilterBy = nameof(RestTestEntityFixture.Count),
                        Type = FilterType.Contains
                    },
                    new()
                    {
                        FilterBy = nameof(RestTestEntityFixture.IsActive),
                        Type = FilterType.GreaterThan
                    },
                    new()
                    {
                        FilterBy = nameof(RestTestEntityFixture.InsertDate),
                        Type = FilterType.EndsWith
                    },
                    new()
                    {
                        FilterBy = nameof(RestTestEntityFixture.InsertDate),
                        Type = FilterType.IsNullOrWhiteSpace
                    }
                ]
            }
        };
    }

    /// <summary>
    /// filter request
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> ValidListSourceButFilterValuesNotValidForBuildFilterExpressionMethodData()
    {
        yield return new object[]
        {
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Count), Type = FilterType.EqualTo, Value = null }]}
        };

        yield return new object[]
        {
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Count), Type = FilterType.Between, Value = 90, OtherValue = null }]}
        };
    }

    /// <summary>
    /// source , filter request, expected filtered result id list
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> ValidListSourceForBuildFilterExpressionMethodData()
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

        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNull }]},
            new List<int> { 5 }
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.Contains, Value = "J"}]},
            new List<int> { 1, 3 }
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.Contains, Value = "j"}]},
            new List<int> { 1, 3 }
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.EqualTo, Value = "Mary"}]},
            new List<int> { 4 }
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Count), Type = FilterType.GreaterThanOrEqualTo, Value = 90}]},
            new List<int> { 3, 4, 5 }
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Count), Type = FilterType.Between, Value = 90, OtherValue = 100 }]},
            new List<int> { 3 }
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.IsActive), Type = FilterType.EqualTo, Value = true }]},
            new List<int> { 1 , 4 }
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.IsActive), Type = FilterType.NotEqualTo, Value = true }]},
            new List<int> { 2, 3 }
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.InsertDate), Type = FilterType.Between, Value = DateTime.Now.AddDays(-1), OtherValue = DateTime.Now.AddDays(2)}]},
            new List<int> { 1, 3, 4 }
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.UpdateDate), Type = FilterType.Between, Value = DateTime.Now.AddDays(-1), OtherValue = DateTime.Now.AddDays(2)}]},
            new List<int> { 1 }
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.UpdateDate), Type = FilterType.EqualTo, Value = null }]},
            new List<int> { 3 , 4 , 5}
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest{  Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.IsActive), Type = FilterType.EqualTo, Value = null }]},
            new List<int> { 5 }
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest{  Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNullOrWhiteSpace }]},
            new List<int> { 5 }
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest{  Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Number), Type = FilterType.GreaterThanOrEqualTo, Value = RestTestEnumFixture.One }]},
            new List<int> { 1, 2, 3, 4, 5 }
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest
            {
                Criterias =
                [
                    new FilterCriteria
                    {
                        FilterBy = nameof(RestTestEntityFixture.Count),
                        Type = FilterType.GreaterThanOrEqualTo,
                        Value = 100
                    },
                    new FilterCriteria
                    {
                        FilterBy = nameof(RestTestEntityFixture.Count),
                        Type = FilterType.LessThanOrEqualTo,
                        Value = 900
                    }
                ],
                MergeType = Connector.And
            },
            new List<int> { 4 }
        };

        yield return new object[]
        {
            validQueryable,
            new FilterRequest
            {
                Criterias =
                [
                    new FilterCriteria
                    {
                        FilterBy = nameof(RestTestEntityFixture.Name),
                        Type = FilterType.Contains,
                        Value = "j"
                    },
                    new FilterCriteria
                    {
                        FilterBy = nameof(RestTestEntityFixture.Number),
                        Type = FilterType.EqualTo,
                        Value = RestTestEnumFixture.One
                    }
                ],
                MergeType = Connector.Or
            },
            new List<int> { 1,2,3,4 }
        };
    }

    #endregion

    #region BuildFilterExpression

    [Theory]
    [MemberData(nameof(InvalidListSourceForBuildFilterExpressionMethodData))]
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
    [MemberData(nameof(ValidListSourceButFilterValuesNotValidForBuildFilterExpressionMethodData))]
    public void BuildFilterExpression_WithValidFilterTypeAndFilterByButFilterValuesAreNull_ShouldThrowException(FilterRequest filterRequest)
    {
        // Arrange

        // Act
        Action act = () => filterRequest.BuildFilterExpression<RestTestEntityFixture>();

        // Assert
        act.Should().Throw<MilvaDeveloperException>().WithMessage("Please provide filter values!");
    }

    [Theory]
    [MemberData(nameof(ValidListSourceForBuildFilterExpressionMethodData))]
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
    [MemberData(nameof(ValidListSourceForBuildFilterExpressionMethodData))]
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
