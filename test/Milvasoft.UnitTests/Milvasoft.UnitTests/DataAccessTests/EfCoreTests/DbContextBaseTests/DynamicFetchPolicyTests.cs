using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.Exceptions;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.Utils.LookupModels;
using Milvasoft.UnitTests.DataAccessTests.EfCoreTests.Fixtures;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests.DbContextBaseTests;

/// <summary>
/// Dynamic lookups and property value requests use <see cref="RequestPropertyPolicy"/> and are limited in row count.
/// </summary>
[Trait("EF Core Data Access Unit Tests", "Unit tests for Milvasoft.DataAccess.EfCore unit testable parts.")]
public class DynamicFetchPolicyTests
{
    [Theory]
    [InlineData(nameof(LookupTestEntityFixture.PasswordHash))]
    [InlineData(nameof(LookupTestEntityFixture.EncryptedValue))]
    public async Task GetLookupsAsync_WithSensitiveProperty_ShouldThrow(string propertyName)
    {
        // Arrange
        var dbContext = await CreateContextAsync(rowCount: 1);

        var request = new LookupRequest { Parameters = [new() { EntityName = nameof(LookupTestEntityFixture), RequestedPropertyNames = [propertyName] }] };

        // Act
        var act = () => dbContext.GetLookupsAsync(request, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<MilvaUserFriendlyException>();
    }

    [Fact]
    public async Task GetLookupsAsync_ShouldReturnAtMostMaxRowCountRows()
    {
        // Arrange
        var dbContext = await CreateContextAsync(rowCount: 5, maxRowCount: 2);

        var request = new LookupRequest
        {
            Parameters =
            [
                new()
                {
                    EntityName = nameof(LookupTestEntityFixture),
                    RequestedPropertyNames = [nameof(LookupTestEntityFixture.Name)],
                    Sorting = new SortRequest { SortBy = nameof(LookupTestEntityFixture.Id) }
                }
            ]
        };

        // Act
        var result = await dbContext.GetLookupsAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().ContainSingle().Which.As<LookupResult>().Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLookupsAsync_WithoutConfiguredMaxRowCount_ShouldUseRequestQueryOptionsMaxRowCount()
    {
        // Arrange
        var dbContext = await CreateContextAsync(rowCount: 3);

        var request = new LookupRequest { Parameters = [new() { EntityName = nameof(LookupTestEntityFixture), RequestedPropertyNames = [nameof(LookupTestEntityFixture.Name)] }] };

        // Act
        var result = await dbContext.GetLookupsAsync(request, TestContext.Current.CancellationToken);

        // Assert
        new DynamicFetchConfiguration().GetEffectiveMaxRowCount().Should().Be(RequestQueryOptions.MaxRowCount);
        result.Should().ContainSingle().Which.As<LookupResult>().Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetLookupsAsync_WithCanceledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var dbContext = await CreateContextAsync(rowCount: 1);

        var request = new LookupRequest { Parameters = [new() { EntityName = nameof(LookupTestEntityFixture), RequestedPropertyNames = [nameof(LookupTestEntityFixture.Name)] }] };

        // Act
        var act = () => dbContext.GetLookupsAsync(request, new CancellationToken(canceled: true));

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(nameof(LookupTestEntityFixture.PasswordHash))]
    [InlineData(nameof(LookupTestEntityFixture.EncryptedValue))]
    public async Task GetPropertyValuesAsync_WithSensitiveProperty_ShouldThrow(string propertyName)
    {
        // Arrange
        var dbContext = await CreateContextAsync(rowCount: 1);

        var request = new EntityPropertyValuesRequest { EntityName = nameof(LookupTestEntityFixture), PropertyName = propertyName };

        // Act
        var act = () => dbContext.GetPropertyValuesAsync(request, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<MilvaUserFriendlyException>();
    }

    [Fact]
    public async Task GetPropertyValuesAsync_ShouldReturnAtMostMaxRowCountValues()
    {
        // Arrange
        var dbContext = await CreateContextAsync(rowCount: 5, maxRowCount: 2);

        var request = new EntityPropertyValuesRequest { EntityName = nameof(LookupTestEntityFixture), PropertyName = nameof(LookupTestEntityFixture.Name) };

        // Act
        var result = await dbContext.GetPropertyValuesAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().HaveCount(2);
    }

    private static async Task<SomeMilvaDbContextFixture> CreateContextAsync(int rowCount, int? maxRowCount = null)
    {
        var configuration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DynamicFetch = new DynamicFetchConfiguration
                {
                    EntityAssemblyName = "Milvasoft.UnitTests",
                    AllowedEntityNamesForLookup = [nameof(LookupTestEntityFixture)],
                    MaxRowCount = maxRowCount
                }
            }
        };

        var services = new ServiceCollection();

        services.AddSingleton<IDataAccessConfiguration>(configuration);
        services.AddDbContext<SomeMilvaDbContextFixture>(opt => opt.UseInMemoryDatabase($"DynamicFetchPolicyTests_{Guid.NewGuid()}"));

        var dbContext = services.BuildServiceProvider().GetRequiredService<SomeMilvaDbContextFixture>();

        for (int i = 1; i <= rowCount; i++)
            dbContext.LookupTestEntities.Add(new LookupTestEntityFixture { Id = i, Name = $"name{i}", PasswordHash = $"hash{i}", EncryptedValue = $"secret{i}" });

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        dbContext.ChangeTracker.Clear();

        return dbContext;
    }
}
