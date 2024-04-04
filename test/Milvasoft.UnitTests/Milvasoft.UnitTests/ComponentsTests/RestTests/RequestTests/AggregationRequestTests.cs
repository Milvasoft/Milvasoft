﻿using FluentAssertions;
using Milvasoft.Components.Rest.Request;
using Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;
using Milvasoft.UnitTests.TestHelpers;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.RequestTests;

public class AggregationRequestTests
{
    #region With List Query Provider Source

    [Fact]
    public async Task ApplyAggregationAsync_WithListQueryProviderSourceAndSyncRunAndACriteriasAreNull_ShouldReturnNull()
    {
        // Arrange
        var request = new AggregationRequest
        {
            Criterias = null
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
        var result = await request.ApplyAggregationAsync(validQueryable, false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ApplyAggregationAsync_WithListQueryProviderSourceAndSyncRunAndACriteriasAreEmpty_ShouldReturnNull()
    {
        // Arrange
        var request = new AggregationRequest
        {
            Criterias = []
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
        var result = await request.ApplyAggregationAsync(validQueryable, false);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region With Ef Query Provider Source

    [Fact]
    public async Task ApplyAggregationAsync_WithEfQueryProviderSourceAndAsyncRunAndACriteriasAreNull_ShouldReturnNull()
    {
        // Arrange
        var request = new AggregationRequest
        {
            Criterias = null
        };
        var dbContextMock = new DbContextMock(nameof(AggregationRequest)).GetDbContextFixture();
        dbContextMock.TestEntities = null;
        await dbContextMock.SaveChangesAsync();

        // Act
        var result = await request.ApplyAggregationAsync(dbContextMock.TestEntities, true);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ApplyAggregationAsync_WithEfQueryProviderSourceAndAsyncRunAndACriteriasAreEmpty_ShouldReturnNull()
    {
        // Arrange
        var request = new AggregationRequest
        {
            Criterias = []
        };
        var dbContextMock = new DbContextMock(nameof(AggregationRequest)).GetDbContextFixture();
        dbContextMock.TestEntities = null;
        await dbContextMock.SaveChangesAsync();

        // Act
        var result = await request.ApplyAggregationAsync(dbContextMock.TestEntities, true);

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
