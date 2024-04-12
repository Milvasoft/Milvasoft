using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.DataAccess.EfCore.Utils;
using Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;
using Milvasoft.UnitTests.CoreTests.HelperTests.CommonTests.Fixtures;
using Milvasoft.UnitTests.TestHelpers;
using System.Linq.Expressions;
using System.Net;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
public class MilvaEfExtensionsTests
{
    #region WithFiltering

    [Fact]
    public void WithFiltering_ForListSource_WithNullQueryable_ShouldReturnNull()
    {
        // Arrange
        IQueryable<RestTestEntityFixture> queryable = null;
        var filterRequest = new FilterRequest
        {
            Criterias =
            [
                new FilterCriteria
                {
                    FilterBy = nameof(RestTestEntityFixture.Name),
                    Type = FilterType.IsNull
                }
            ]
        };

        // Act
        var result = queryable.WithFiltering(filterRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void WithFiltering_ForListSource_WithEmptyQueryable_ShouldReturnEmpty()
    {
        // Arrange
        IQueryable<RestTestEntityFixture> queryable = new List<RestTestEntityFixture>().AsQueryable();
        var filterRequest = new FilterRequest
        {
            Criterias =
            [
                new FilterCriteria
                {
                    FilterBy = nameof(RestTestEntityFixture.Name),
                    Type = FilterType.IsNull
                }
            ]
        };

        // Act
        var result = queryable.WithFiltering(filterRequest);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void WithFiltering_ForEfSource_WithNullQueryable_ShouldReturnNull()
    {
        // Arrange
        using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        dbContextMock.TestEntities = null;
        dbContextMock.SaveChanges();
        var filterRequest = new FilterRequest
        {
            Criterias =
            [
                new FilterCriteria
                {
                    FilterBy = nameof(RestTestEntityFixture.Name),
                    Type = FilterType.IsNull
                }
            ]
        };

        // Act
        var result = dbContextMock.TestEntities.WithFiltering(filterRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void WithFiltering_ForEfSource_WithEmptyQueryable_ShouldReturnEmpty()
    {
        // Arrange
        using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        var filterRequest = new FilterRequest
        {
            Criterias =
            [
                new FilterCriteria
                {
                    FilterBy = nameof(RestTestEntityFixture.Name),
                    Type = FilterType.IsNull
                }
            ]
        };

        // Act
        var result = dbContextMock.TestEntities.WithFiltering(filterRequest);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(ValidListSourceForBuildFilterExpressionMethodData))]
    public void WithFiltering_ForListSource_WithValidFilterRequest_ShouldReturnCorrectResult(IQueryable<RestTestEntityFixture> source, FilterRequest filterRequest, List<int> expectedIdList)
    {
        // Arrange

        // Act
        var result = source.WithFiltering(filterRequest).ToList();

        // Assert
        result.Should().HaveCount(expectedIdList.Count);
        result.Should().AllSatisfy(i => expectedIdList.Should().Contain(i.Id));
    }

    [Theory]
    [ClassData(typeof(ValidEfSourceForBuildFilterExpressionMethodData))]
    public async Task WithFiltering_ForEfSource_WithValidFilterRequest_ShouldReturnCorrectResult(RestDbContextFixture dbContextFixture, FilterRequest filterRequest, List<int> expectedIdList)
    {
        // Arrange

        // Act
        var result = await dbContextFixture.TestEntities.WithFiltering(filterRequest).ToListAsync();

        // Assert
        result.Should().HaveCount(expectedIdList.Count);
        result.Should().AllSatisfy(i => expectedIdList.Should().Contain(i.Id));
    }

    [Fact]
    public async Task WithFiltering_ForListRequestOverload_ForEfSource_WithNullListRequest_ShouldReturnInputQuery()
    {
        // Arrange
        using var dbContextFixture = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        await dbContextFixture.TestEntities.AddAsync(new RestTestEntityFixture { Id = 1, Name = "john" });
        await dbContextFixture.SaveChangesAsync();
        ListRequest listRequest = null;

        // Act
        var result = await dbContextFixture.TestEntities.WithFiltering(listRequest).ToListAsync();

        // Assert
        result.Should().HaveCount(dbContextFixture.TestEntities.Count());
    }

    [Theory]
    [ClassData(typeof(ValidEfSourceForBuildFilterExpressionMethodData))]
    public async Task WithFiltering_ForListRequestOverload_ForEfSource_WithValidListRequest_ShouldReturnCorrectResult(RestDbContextFixture dbContextFixture, FilterRequest filterRequest, List<int> expectedIdList)
    {
        // Arrange
        var listRequest = new ListRequest
        {
            Filtering = filterRequest
        };

        // Act
        var result = await dbContextFixture.TestEntities.WithFiltering(listRequest).ToListAsync();

        // Assert
        result.Should().HaveCount(expectedIdList.Count);
        result.Should().AllSatisfy(i => expectedIdList.Should().Contain(i.Id));
    }

    #endregion

    #region WithSorting

    [Fact]
    public void WithSorting_ForListSource_WithNullQueryable_ShouldReturnNull()
    {
        // Arrange
        IQueryable<RestTestEntityFixture> queryable = null;
        var sortRequest = new SortRequest { SortBy = nameof(RestTestEntityFixture.Name), Type = SortType.Asc };

        // Act
        var result = queryable.WithSorting(sortRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void WithSorting_ForListSource_WithEmptyQueryable_ShouldReturnEmpty()
    {
        // Arrange
        IQueryable<RestTestEntityFixture> queryable = new List<RestTestEntityFixture>().AsQueryable();
        var sortRequest = new SortRequest { SortBy = nameof(RestTestEntityFixture.Name), Type = SortType.Asc };

        // Act
        var result = queryable.WithSorting(sortRequest);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void WithSorting_ForEfSource_WithNullQueryable_ShouldReturnNull()
    {
        // Arrange
        using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        dbContextMock.TestEntities = null;
        dbContextMock.SaveChanges();
        var sortRequest = new SortRequest { SortBy = nameof(RestTestEntityFixture.Name), Type = SortType.Asc };

        // Act
        var result = dbContextMock.TestEntities.WithSorting(sortRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void WithSorting_ForEfSource_WithEmptyQueryable_ShouldReturnEmpty()
    {
        // Arrange
        using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        var sortRequest = new SortRequest { SortBy = nameof(RestTestEntityFixture.Name), Type = SortType.Asc };

        // Act
        var result = dbContextMock.TestEntities.WithSorting(sortRequest);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(ValidAscendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public void WithSorting_ForListSource_WithAscendingSortRequest_ShouldReturnCorrectResult(IQueryable<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange

        // Act
        var result = source.WithSorting(sortRequest).ToList();

        // Assert
        result.Should().BeInAscendingOrder(expectedExpression);
    }

    [Theory]
    [ClassData(typeof(ValidDescendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public void WithSorting_ForListSource_WithDescendingSortRequest_ShouldReturnCorrectResult(IQueryable<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange

        // Act
        var result = source.WithSorting(sortRequest).ToList();

        // Assert
        result.Should().BeInDescendingOrder(expectedExpression);
    }

    [Theory]
    [ClassData(typeof(ValidAscendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public async Task WithSorting_ForEfSource_WithAscendingSortRequest_ShouldReturnCorrectResult(IQueryable<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange
        using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        await dbContextMock.TestEntities.AddRangeAsync(source);
        await dbContextMock.SaveChangesAsync();

        // Act
        var result = await dbContextMock.TestEntities.WithSorting(sortRequest).ToListAsync();

        // Assert
        result.Should().BeInAscendingOrder(expectedExpression);
    }

    [Theory]
    [ClassData(typeof(ValidDescendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public async Task WithSorting_ForEfSource_WithDescendingSortRequest_ShouldReturnCorrectResult(IQueryable<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange
        using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        await dbContextMock.TestEntities.AddRangeAsync(source);
        await dbContextMock.SaveChangesAsync();

        // Act
        var result = await dbContextMock.TestEntities.WithSorting(sortRequest).ToListAsync();

        // Assert
        result.Should().BeInDescendingOrder(expectedExpression);
    }

    [Fact]
    public async Task WithSorting_ForListRequestOverload_ForEfSource_WithNullListRequest_ShouldReturnInputQuery()
    {
        // Arrange
        ListRequest listRequest = null;
        using var dbContextFixture = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        await dbContextFixture.TestEntities.AddAsync(new RestTestEntityFixture { Id = 1, Name = "john" });
        await dbContextFixture.SaveChangesAsync();

        // Act
        var result = await dbContextFixture.TestEntities.WithSorting(listRequest).ToListAsync();

        // Assert
        result.Should().HaveCount(dbContextFixture.TestEntities.Count());
    }

    [Theory]
    [ClassData(typeof(ValidAscendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public async Task WithSorting_ForListRequestOverload_ForEfSource_WithAscendingListRequest_ShouldReturnCorrectResult(IQueryable<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange
        var listRequest = new ListRequest
        {
            Sorting = sortRequest
        };

        using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        await dbContextMock.TestEntities.AddRangeAsync(source);
        await dbContextMock.SaveChangesAsync();

        // Act
        var result = await dbContextMock.TestEntities.WithSorting(listRequest).ToListAsync();

        // Assert
        result.Should().BeInAscendingOrder(expectedExpression);
    }

    [Theory]
    [ClassData(typeof(ValidDescendingSortRequestForBuildPropertySelectorExpressionMethodData))]
    public async Task WithSorting_ForListRequestOverload_ForEfSource_WithDescendingListRequest_ShouldReturnCorrectResult(IQueryable<RestTestEntityFixture> source, SortRequest sortRequest, Expression<Func<RestTestEntityFixture, object>> expectedExpression)
    {
        // Arrange
        var listRequest = new ListRequest
        {
            Sorting = sortRequest
        };
        using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        await dbContextMock.TestEntities.AddRangeAsync(source);
        await dbContextMock.SaveChangesAsync();

        // Act
        var result = await dbContextMock.TestEntities.WithSorting(listRequest).ToListAsync();

        // Assert
        result.Should().BeInDescendingOrder(expectedExpression);
    }

    #endregion

    #region WithFilteringAndSorting

    [Fact]
    public void WithFilteringAndSorting_ForListSource_WithNullListRequest_ShouldReturnInputQuery()
    {
        // Arrange
        IQueryable<RestTestEntityFixture> list = new List<RestTestEntityFixture>
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
        ListRequest listRequest = null;

        // Act
        var result = list.WithFilteringAndSorting(listRequest).ToList();

        // Assert
        result.Should().HaveCount(list.Count());
    }

    [Fact]
    public async Task WithFilteringAndSorting_ForEfSource_WithNullListRequest_ShouldReturnInputQuery()
    {
        // Arrange
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
        ListRequest listRequest = null;

        using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        await dbContextMock.TestEntities.AddRangeAsync(validQueryable);
        await dbContextMock.SaveChangesAsync();

        // Act
        var result = await dbContextMock.TestEntities.WithSorting(listRequest).ToListAsync();

        // Assert
        result.Should().HaveCount(dbContextMock.TestEntities.Count());
    }

    [Fact]
    public void WithFilteringAndSorting_ForListSource_WithFilteringAndSortingRequested_ShouldReturnCorrectResult()
    {
        // Arrange
        IQueryable<RestTestEntityFixture> list = new List<RestTestEntityFixture>
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
        ListRequest listRequest = new()
        {
            Filtering = new FilterRequest
            {
                Criterias =
                [
                    new FilterCriteria
                    {
                        FilterBy = nameof(RestTestEntityFixture.Name),
                        Type = FilterType.IsNotNull
                    }
                ]
            },
            Sorting = new SortRequest { SortBy = nameof(RestTestEntityFixture.Id), Type = SortType.Desc }
        };

        // Act
        var result = list.WithFilteringAndSorting(listRequest).ToList();

        // Assert
        result.Should().HaveCount(4);
        result.Should().BeInDescendingOrder(i => i.Id);
    }

    [Fact]
    public async Task WithFilteringAndSorting_ForEfSource_WithFilteringAndSortingRequested_ShouldReturnCorrectResult()
    {
        // Arrange
        IQueryable<RestTestEntityFixture> list = new List<RestTestEntityFixture>
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
        ListRequest listRequest = new()
        {
            Filtering = new FilterRequest
            {
                Criterias =
                [
                    new FilterCriteria
                    {
                        FilterBy = nameof(RestTestEntityFixture.Name),
                        Type = FilterType.IsNotNull
                    }
                ]
            },
            Sorting = new SortRequest { SortBy = nameof(RestTestEntityFixture.Id), Type = SortType.Desc }
        };

        using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        await dbContextMock.TestEntities.AddRangeAsync(list);
        await dbContextMock.SaveChangesAsync();

        // Act
        var result = await dbContextMock.TestEntities.WithFilteringAndSorting(listRequest).ToListAsync();

        // Assert
        result.Should().HaveCount(4);
        result.Should().BeInDescendingOrder(i => i.Id);
    }

    #endregion

    #region ToListResponseAsync

    /// <summary>
    /// source, list request, expected list response
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> ValidToListResponseMethodData()
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
                Children = new()
                {
                    Name = "Jack"
                },
                Childrens = []
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
                Children = new()
                {
                    Name = "John"
                },
                Childrens =
                [
                    new()
                    {
                        Name = "John"
                    },
                    new()
                    {
                        Name = "Jack"
                    }
                ]
            },
            new() {
                Id = 3,
                Name = "Jack",
                Count = 90,
                Price = 0.50M,
                Number = RestTestEnumFixture.Zero,
                IsActive = false,
                InsertDate = DateTime.Now,
                UpdateDate = null,
                Childrens = []
            },
            new() {
                Id = 4,
                Name = "Mary",
                Count = 101,
                Price = 4M,
                Number = RestTestEnumFixture.One,
                IsActive = true,
                InsertDate = DateTime.Now,
                UpdateDate = null,
                Childrens =
                [
                    new()
                    {

                        Name = "Elise"
                    }
                ]
            },
            new() {
                Id = 5,
                Name = null,
                Count = 999,
                Price = 999M,
                Number = RestTestEnumFixture.Zero,
                IsActive = null,
                InsertDate = DateTime.Now.AddMonths(-5),
                UpdateDate = null,
                Childrens = []
            },
        }.AsQueryable();

        //1 - list request is null
        var response1 = ListResponse<RestTestEntityFixture>.Success([.. validQueryable], LocalizerKeys.Successful, null, null, null);
        yield return new object[]
        {
            validQueryable,
            null,
            response1
        };

        //2 - page number exists but row count not exists
        var response2 = ListResponse<RestTestEntityFixture>.Success([.. validQueryable], LocalizerKeys.Successful, 1, null, null);
        yield return new object[]
        {
            validQueryable,
            new ListRequest
            {
               PageNumber = 1,
               RowCount = null
            },
            response2
        };

        //3 - row count exists but page count not exists
        var response3 = ListResponse<RestTestEntityFixture>.Success([.. validQueryable], LocalizerKeys.Successful, null, null, null);
        yield return new object[]
        {
            validQueryable,
            new ListRequest
            {
               PageNumber = null,
               RowCount = 2
            },
            response3
        };

        //4 - valid page number and row count
        var response4 = ListResponse<RestTestEntityFixture>.Success([.. validQueryable.Skip(0).Take(2)], LocalizerKeys.Successful, 1, 3, 5);
        yield return new object[]
        {
            validQueryable,
            new ListRequest
            {
               PageNumber = 1,
               RowCount = 2
            },
            response4
        };

        //5 - aggregation, filtering, sorting requested
        var response5Data = validQueryable.ToList();
        response5Data.RemoveAll(i => i.Name == null);
        response5Data = [.. response5Data.OrderByDescending(i => i.Id)];
        var response5 = ListResponse<RestTestEntityFixture>.Success([.. response5Data], LocalizerKeys.Successful, null, null, null);
        response5.AggregationResults =
        [
            new AggregationResult { AggregatedBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum, Result = 30M },
        ];
        yield return new object[]
        {
            validQueryable,
            new ListRequest
            {
               Aggregation = new AggregationRequest{ Criterias = [ new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum}] },
               Filtering =  new FilterRequest { Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNotNull }]},
               Sorting = new SortRequest { SortBy = nameof(RestTestEntityFixture.Id), Type = SortType.Desc }
            },
            response5
        };

        //6 - aggregation, filtering, sorting and pagination requested
        var response6Data = validQueryable.ToList();
        response6Data.RemoveAll(i => i.Name == null);
        response6Data = response6Data.OrderByDescending(i => i.Id).Skip(0).Take(2).ToList();
        var response6 = ListResponse<RestTestEntityFixture>.Success([.. response6Data], LocalizerKeys.Successful, 1, 2, 4);
        response6.AggregationResults =
        [
            new AggregationResult { AggregatedBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum, Result = 30M },
        ];
        yield return new object[]
        {
            validQueryable,
            new ListRequest
            {
               PageNumber = 1,
               RowCount = 2,
               Aggregation = new AggregationRequest{ Criterias = [ new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum}] },
               Filtering =  new FilterRequest { Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNotNull }]},
               Sorting = new SortRequest { SortBy = nameof(RestTestEntityFixture.Id), Type = SortType.Desc }
            },
            response6
        };
    }

    [Fact]
    public async Task ToListResponseAsync_WithNullQuerySource_ShouldReturnSuccessResponse()
    {
        // Arrange
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum }] },
            Filtering = new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNotNull }] },
            Sorting = new SortRequest { SortBy = nameof(RestTestEntityFixture.Id), Type = SortType.Desc }
        };
        using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        dbContextMock.TestEntities = null;
        await dbContextMock.SaveChangesAsync();

        // Act
        var result = await dbContextMock.TestEntities.ToListResponseAsync(listRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Messages.Should().HaveCount(1);
        result.Messages[0].Message.Should().Be(LocalizerKeys.Successful);
    }

    [Theory]
    [MemberData(nameof(ValidToListResponseMethodData))]
    public async Task ToListResponseAsync_WithFilteringAndSortingRequested_ShouldReturnCorrectResult(IQueryable<RestTestEntityFixture> source, ListRequest listRequest, ListResponse<RestTestEntityFixture> expectedResponse)
    {
        // Arrange
        using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        await dbContextMock.TestEntities.AddRangeAsync(source);
        await dbContextMock.SaveChangesAsync();

        // Act
        var result = await dbContextMock.TestEntities.ToListResponseAsync(listRequest);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse, opt => opt.RespectingRuntimeTypes());
    }

    #endregion

    #region ToListResponse

    [Fact]
    public void ToListResponse_ForListSource_WithNullQuerySource_ShouldReturnSuccessResponse()
    {
        // Arrange
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum }] },
            Filtering = new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNotNull }] },
            Sorting = new SortRequest { SortBy = nameof(RestTestEntityFixture.Id), Type = SortType.Desc }
        };
        IQueryable<RestTestEntityFixture> list = new List<RestTestEntityFixture>().AsQueryable();

        // Act
        var result = list.ToListResponse(listRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Messages.Should().HaveCount(1);
        result.Messages[0].Message.Should().Be(LocalizerKeys.Successful);
    }

    [Theory]
    [MemberData(nameof(ValidToListResponseMethodData))]
    public void ToListResponse_ForListSource_WithFilteringAndSortingRequested_ShouldReturnCorrectResult(IQueryable<RestTestEntityFixture> source, ListRequest listRequest, ListResponse<RestTestEntityFixture> expectedResponse)
    {
        // Arrange

        // Act
        var result = source.ToListResponse(listRequest);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse, opt => opt.RespectingRuntimeTypes());
    }

    [Fact]
    public void ToListResponse_ForEfSource_WithNullQuerySource_ShouldReturnSuccessResponse()
    {
        // Arrange
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum }] },
            Filtering = new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNotNull }] },
            Sorting = new SortRequest { SortBy = nameof(RestTestEntityFixture.Id), Type = SortType.Desc }
        };
        using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        dbContextMock.TestEntities = null;
        dbContextMock.SaveChanges();

        // Act
        var result = dbContextMock.TestEntities.ToListResponse(listRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Messages.Should().HaveCount(1);
        result.Messages[0].Message.Should().Be(LocalizerKeys.Successful);
    }

    [Theory]
    [MemberData(nameof(ValidToListResponseMethodData))]
    public void ToListResponse_ForEfSourceWithFilteringAndSortingRequested_ShouldReturnCorrectResult(IQueryable<RestTestEntityFixture> source, ListRequest listRequest, ListResponse<RestTestEntityFixture> expectedResponse)
    {
        // Arrange
        using var dbContextMock = new DbContextMock<RestDbContextFixture>(nameof(MilvaEfExtensions)).GetDbContextFixture();
        dbContextMock.TestEntities.AddRange(source);
        dbContextMock.SaveChanges();

        // Act
        var result = dbContextMock.TestEntities.ToListResponse(listRequest);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse, opt => opt.RespectingRuntimeTypes());
    }

    #endregion

    #region GetUpdatablePropertiesBuilder

    [Fact]
    public void GetUpdatablePropertiesBuilder_WithInvalidDto_ShouldReturnNull()
    {
        // Arrange
        UpdatedPropsTestInvalidDto dto = new UpdatedPropsTestInvalidDto
        {
            Id = 1,
            Name = "john",
            Price = 10M
        };
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> expectedExpression = i => i;

        // Act
        var result = dto.GetUpdatablePropertiesBuilder<RestTestEntityFixture, UpdatedPropsTestInvalidDto>();

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expectedExpression, result.SetPropertyCalls);

        equality.Should().BeTrue();
    }

    [Fact]
    public void GetUpdatablePropertiesBuilder_WithValidDtoButNull_ShouldReturnNull()
    {
        // Arrange
        UpdatedPropsTestDto dto = null;

        // Act
        var result = dto.GetUpdatablePropertiesBuilder<RestTestEntityFixture, UpdatedPropsTestDto>();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUpdatablePropertiesBuilder_WithValidDtoUtcFalse_ShouldReturnCorrectResult()
    {
        // Arrange
        var now = DateTime.Now;
        UpdatedPropsTestDto dto = new()
        {
            Id = 1,
            Name = "john",
            UpdateDate = now
        };
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> notExpectedExpression = i => i;

        // Act
        var result = dto.GetUpdatablePropertiesBuilder<RestTestEntityFixture, UpdatedPropsTestDto>();

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(notExpectedExpression, result.SetPropertyCalls);
        equality.Should().BeFalse();
    }

    #endregion
}
