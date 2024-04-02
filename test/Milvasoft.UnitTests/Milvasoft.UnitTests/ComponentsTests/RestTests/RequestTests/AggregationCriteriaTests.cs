using FluentAssertions;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Core.Exceptions;
using Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.RequestTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
public class AggregationCriteriaTests
{
    #region With List Query Provider Source

    /// <summary>
    /// source , aggregation criteria, expected aggregation result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> InvalidListSourceForApplyAggregationAsyncMethodData()
    {
        IQueryable<RestTestEntityFixture> validQueryable = new List<RestTestEntityFixture>
        {
            new() {
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two
            },
            new() {
                Name = "Elise",
                Count = 10,
                Price = 15.5M,
                Number = RestTestEnumFixture.One
            },
            new() {
                Name = "Jack",
                Count = 90,
                Price = 0.50M,
                Number = RestTestEnumFixture.Zero
            },
            new() {
                Name = "Mary",
                Count = 101,
                Price = 4M,
                Number = RestTestEnumFixture.One
            },
        }.AsQueryable();

        yield return new object[]
        {
            validQueryable,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Name), Type = AggregationType.Avg}
        };

        yield return new object[]
        {
            validQueryable,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Name), Type = AggregationType.Sum}
        };

        yield return new object[]
        {
            validQueryable,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Number), Type = AggregationType.Avg}
        };

        yield return new object[]
        {
            validQueryable,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Number), Type = AggregationType.Sum}
        };
    }
    /// <summary>
    /// source , aggregation criteria, expected aggregation result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> ValidListSourceForApplyAggregationAsyncMethodData()
    {
        IQueryable<RestTestEntityFixture> validQueryable = new List<RestTestEntityFixture>
        {
            new() {
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two
            },
            new() {
                Name = "Elise",
                Count = 10,
                Price = 15.5M,
                Number = RestTestEnumFixture.One
            },
            new() {
                Name = "Jack",
                Count = 90,
                Price = 0.50M,
                Number = RestTestEnumFixture.Zero
            },
            new() {
                Name = "Mary",
                Count = 101,
                Price = 4M,
                Number = RestTestEnumFixture.One
            },
        }.AsQueryable();

        yield return new object[]
        {
            validQueryable,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Name), Type = AggregationType.Count},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Name) , Type = AggregationType.Count, Result = 4 }
        };
        yield return new object[]
        {
            validQueryable,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Count},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Price) , Type = AggregationType.Count, Result = 4 }
        };
        yield return new object[]
        {
            validQueryable,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Avg},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Price) , Type = AggregationType.Avg, Result = 7.5 }
        };
        yield return new object[]
        {
            validQueryable,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Price) , Type = AggregationType.Sum, Result = 30M }
        };
        yield return new object[]
        {
            validQueryable,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Count), Type = AggregationType.Min},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Count) , Type = AggregationType.Min, Result = 1 }
        };
        yield return new object[]
        {
            validQueryable,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Max},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Price) , Type = AggregationType.Max, Result =   15.5M }
        };
        yield return new object[]
        {
            validQueryable,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Name), Type = AggregationType.Min},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Name) , Type = AggregationType.Min, Result =  "Elise" }
        };
        yield return new object[]
        {
            validQueryable,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Number), Type = AggregationType.Min},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Number) , Type = AggregationType.Min, Result = RestTestEnumFixture.Zero }
        };
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ApplyAggregationAsync_WithListQueryProviderSourceAndSyncRunAndAggregateByIsNullOrEmptyOrWhiteSpace_ShouldReturnExpectedAggregationResult(string aggregateBy)
    {
        // Arrange
        var criteria = new AggregationCriteria
        {
            AggregateBy = aggregateBy,
            Type = AggregationType.Avg
        };
        IQueryable<RestTestEntityFixture> validQueryable = new List<RestTestEntityFixture>
        {
            new() {
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two
            }
        }.AsQueryable();

        // Act
        var result = await criteria.ApplyAggregationAsync(validQueryable, false);

        // Assert
        result.Result.Should().BeNull();
    }

    [Fact]
    public async Task ApplyAggregationAsync_WithListQueryProviderSourceAndSyncRunAndValidParameters_ShouldReturnExpectedAggregationResult()
    {
        // Arrange
        var criteria = new AggregationCriteria
        {
            AggregateBy = nameof(RestTestEntityFixture.Name),
            Type = AggregationType.Avg
        };
        IQueryable<RestTestEntityFixture> validQueryable = null;

        // Act
        var result = await criteria.ApplyAggregationAsync(validQueryable, false);

        // Assert
        result.Result.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(InvalidListSourceForApplyAggregationAsyncMethodData))]
    public async Task ApplyAggregationAsync_WithListQueryProviderSourceAndSyncRunAndUnsupportedAggregateByPropetyTypeWithAggregationMethod_ShouldThrowException(IQueryable<RestTestEntityFixture> query, AggregationCriteria criteria)
    {
        // Arrange

        // Act
        Func<Task<AggregationResult>> act = async () => await criteria.ApplyAggregationAsync(query, false);

        // Assert
        await act.Should().ThrowAsync<MilvaDeveloperException>();
    }

    [Theory]
    [MemberData(nameof(ValidListSourceForApplyAggregationAsyncMethodData))]
    public async Task ApplyAggregationAsync_WithListQueryProviderSourceAndSyncRunAndValidParameters_ShouldReturnExpectedResult(IQueryable<RestTestEntityFixture> query, AggregationCriteria criteria, AggregationResult expectedResult)
    {
        // Arrange

        // Act
        var result = await criteria.ApplyAggregationAsync(query, false);

        // Assert
        result.Should().BeEquivalentTo(expectedResult, options => options.RespectingRuntimeTypes());
    }

    [Theory]
    [MemberData(nameof(ValidListSourceForApplyAggregationAsyncMethodData))]
    public async Task ApplyAggregationAsync_WithListQueryProviderSourceAndAsyncRunAndValidParameters_ShouldReturnExpectedResult(IQueryable<RestTestEntityFixture> query, AggregationCriteria criteria, AggregationResult expectedResult)
    {
        // Arrange

        // Act
        Func<Task<AggregationResult>> act = async () => await criteria.ApplyAggregationAsync(query, true);

        // Assert
        await act.Should().ThrowAsync<MilvaDeveloperException>().WithMessage("Query provider type is 'EnumerableQuery' cannot run asyncrhonosly!");
    }

    #endregion

    #region With Ef Query Provider Source

    /// <summary>
    /// source , aggregation criteria, expected aggregation result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> InvalidEfQuerySourceForApplyAggregationAsyncMethodData()
    {
        var entities = new List<RestTestEntityFixture>
        {
            new() {
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two
            },
            new() {
                Name = "Elise",
                Count = 10,
                Price = 15.5M,
                Number = RestTestEnumFixture.One
            },
            new() {
                Name = "Jack",
                Count = 90,
                Price = 0.50M,
                Number = RestTestEnumFixture.Zero
            },
            new() {
                Name = "Mary",
                Count = 101,
                Price = 4M,
                Number = RestTestEnumFixture.One
            },
        };

        var dbContextMock = new DbContextMock().GetDbContextFixture();
        dbContextMock.TestEntities.AddRange(entities);
        dbContextMock.SaveChanges();

        yield return new object[]
        {
            dbContextMock,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Name), Type = AggregationType.Avg}
        };

        yield return new object[]
        {
            dbContextMock,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Name), Type = AggregationType.Sum}
        };

        yield return new object[]
        {
            dbContextMock,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Number), Type = AggregationType.Avg}
        };

        yield return new object[]
        {
            dbContextMock,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Number), Type = AggregationType.Sum}
        };
    }
    /// <summary>
    /// source , aggregation criteria, expected aggregation result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> ValidEfQuerySourceForApplyAggregationAsyncMethodData()
    {
        var entities = new List<RestTestEntityFixture>
        {
            new() {
                Id = 1,
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two
            },
            new() {
                Id = 2,
                Name = "Elise",
                Count = 10,
                Price = 15.5M,
                Number = RestTestEnumFixture.One
            },
            new() {
                Id = 3,
                Name = "Jack",
                Count = 90,
                Price = 0.50M,
                Number = RestTestEnumFixture.Zero
            },
            new() {
                Id = 4,
                Name = "Mary",
                Count = 101,
                Price = 4M,
                Number = RestTestEnumFixture.One
            },
        };

        var dbContextMock = new DbContextMock().GetDbContextFixture();
        dbContextMock.TestEntities.AddRange(entities);
        dbContextMock.SaveChanges();

        yield return new object[]
        {
            dbContextMock,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Name), Type = AggregationType.Count},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Name) , Type = AggregationType.Count, Result = 4 }
        };
        yield return new object[]
        {
            dbContextMock,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Count},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Price) , Type = AggregationType.Count, Result = 4 }
        };
        yield return new object[]
        {
            dbContextMock,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Avg},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Price) , Type = AggregationType.Avg, Result = 7.5 }
        };
        yield return new object[]
        {
            dbContextMock,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Sum},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Price) , Type = AggregationType.Sum, Result = 30M }
        };
        yield return new object[]
        {
            dbContextMock,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Count), Type = AggregationType.Min},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Count) , Type = AggregationType.Min, Result = 1 }
        };
        yield return new object[]
        {
            dbContextMock,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Price), Type = AggregationType.Max},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Price) , Type = AggregationType.Max, Result =   15.5M }
        };
        yield return new object[]
        {
            dbContextMock,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Name), Type = AggregationType.Min},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Name) , Type = AggregationType.Min, Result =  "Elise" }
        };
        yield return new object[]
        {
            dbContextMock,
            new AggregationCriteria { AggregateBy = nameof(RestTestEntityFixture.Number), Type = AggregationType.Min},
            new AggregationResult {   AggregatedBy = nameof(RestTestEntityFixture.Number) , Type = AggregationType.Min, Result = RestTestEnumFixture.Zero }
        };
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ApplyAggregationAsync_WithEfQueryProviderSourceAndAsyncRunAndAggregateByIsNullOrEmptyOrWhiteSpace_ShouldReturnExpectedAggregationResult(string aggregateBy)
    {
        // Arrange
        var criteria = new AggregationCriteria
        {
            AggregateBy = aggregateBy,
            Type = AggregationType.Avg
        };
        var entities = new List<RestTestEntityFixture>
        {
            new() {
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two
            }
        };
        var dbContextMock = new DbContextMock().GetDbContextFixture();
        dbContextMock.TestEntities.AddRange(entities);
        dbContextMock.SaveChanges();

        // Act
        var result = await criteria.ApplyAggregationAsync(dbContextMock.TestEntities, true);

        // Assert
        result.Result.Should().BeNull();
    }

    [Fact]
    public async Task ApplyAggregationAsync_WithEfQueryProviderSourceAndAsyncRunAndValidParameters_ShouldReturnNull()
    {
        // Arrange
        var criteria = new AggregationCriteria
        {
            AggregateBy = nameof(RestTestEntityFixture.Name),
            Type = AggregationType.Avg
        };
        var dbContextMock = new DbContextMock().GetDbContextFixture();
        dbContextMock.TestEntities = null;
        await dbContextMock.SaveChangesAsync();

        // Act
        var result = await criteria.ApplyAggregationAsync(dbContextMock.TestEntities, true);

        // Assert
        result.Result.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(InvalidEfQuerySourceForApplyAggregationAsyncMethodData))]
    public async Task ApplyAggregationAsync_WithEfQueryProviderSourceAndAsyncRunAndUnsupportedAggregateByPropetyTypeWithAggregationMethod_ShouldThrowException(RestDbContextFixture dbContextMock, AggregationCriteria criteria)
    {
        // Arrange

        // Act
        Func<Task<AggregationResult>> act = async () => await criteria.ApplyAggregationAsync(dbContextMock.TestEntities, true);

        // Assert
        await act.Should().ThrowAsync<MilvaDeveloperException>();
    }

    [Theory]
    [MemberData(nameof(ValidEfQuerySourceForApplyAggregationAsyncMethodData))]
    public async Task ApplyAggregationAsync_WithEfQueryProviderSourceAndAsyncRunAndValidParameters_ShouldReturnExpectedResult(RestDbContextFixture dbContextMock, AggregationCriteria criteria, AggregationResult expectedResult)
    {
        // Arrange

        // Act
        var result = await criteria.ApplyAggregationAsync(dbContextMock.TestEntities, true);

        // Assert
        result.Should().BeEquivalentTo(expectedResult, options => options.RespectingRuntimeTypes());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ApplyAggregationAsync_WithEfQueryProviderSourceAndSyncRunAndAggregateByIsNullOrEmptyOrWhiteSpace_ShouldReturnExpectedAggregationResult(string aggregateBy)
    {
        // Arrange
        var criteria = new AggregationCriteria
        {
            AggregateBy = aggregateBy,
            Type = AggregationType.Avg
        };
        var entities = new List<RestTestEntityFixture>
        {
            new() {
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two
            }
        };
        var dbContextMock = new DbContextMock().GetDbContextFixture();
        dbContextMock.TestEntities.AddRange(entities);
        dbContextMock.SaveChanges();

        // Act
        var result = await criteria.ApplyAggregationAsync(dbContextMock.TestEntities, false);

        // Assert
        result.Result.Should().BeNull();
    }

    [Fact]
    public async Task ApplyAggregationAsync_WithEfQueryProviderSourceAndSyncRunAndValidParameters_ShouldReturnExpectedAggregationResult()
    {
        // Arrange
        var criteria = new AggregationCriteria
        {
            AggregateBy = nameof(RestTestEntityFixture.Name),
            Type = AggregationType.Avg
        };
        var dbContextMock = new DbContextMock().GetDbContextFixture();
        dbContextMock.TestEntities = null;
        await dbContextMock.SaveChangesAsync();

        // Act
        var result = await criteria.ApplyAggregationAsync(dbContextMock.TestEntities, false);

        // Assert
        result.Result.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(InvalidEfQuerySourceForApplyAggregationAsyncMethodData))]
    public async Task ApplyAggregationAsync_WithEfQueryProviderSourceAndSyncRunAndUnsupportedAggregateByPropetyTypeWithAggregationMethod_ShouldThrowException(RestDbContextFixture dbContextMock, AggregationCriteria criteria)
    {
        // Arrange

        // Act
        Func<Task<AggregationResult>> act = async () => await criteria.ApplyAggregationAsync(dbContextMock.TestEntities, false);

        // Assert
        await act.Should().ThrowAsync<MilvaDeveloperException>();
    }

    [Theory]
    [MemberData(nameof(ValidEfQuerySourceForApplyAggregationAsyncMethodData))]
    public async Task ApplyAggregationAsync_WithEfQueryProviderSourceAndSyncRunAndValidParameters_ShouldReturnExpectedResult(RestDbContextFixture dbContextMock, AggregationCriteria criteria, AggregationResult expectedResult)
    {
        // Arrange

        // Act
        var result = await criteria.ApplyAggregationAsync(dbContextMock.TestEntities, false);

        // Assert
        result.Should().BeEquivalentTo(expectedResult, options => options.RespectingRuntimeTypes());
    }

    #endregion
}
