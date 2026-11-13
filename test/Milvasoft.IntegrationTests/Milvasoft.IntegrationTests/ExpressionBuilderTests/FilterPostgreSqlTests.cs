using ExpressionBuilder.Configuration;
using ExpressionBuilder.Generics;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.IntegrationTests.DataAccessTests;
using Milvasoft.UnitTests.ComponentsTests.ExpressionBuilderTests.Fixture;
using Npgsql;

namespace Milvasoft.IntegrationTests.ExpressionBuilderTests;

/// <summary>
/// Runs the cases of the unit tests against PostgreSQL. The cases and the entities are linked from the unit test project.
/// </summary>
[Collection(nameof(UtcTrueDatabaseTestCollection))]
[Trait("ExpressionBuilder Integration Tests", "Milvasoft.ExpressionBuilder against PostgreSQL.")]
#pragma warning disable xUnit1045 // Avoid using TheoryData type arguments that might not be serializable
public class FilterPostgreSqlTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private static readonly SemaphoreSlim _seedLock = new(1, 1);
    private static bool _seeded;

    private FilterNpgsqlDbContextFixture _context;

    public async ValueTask InitializeAsync()
    {
        // The collection fixture has started the container. A separate database, so the respawner of the other tests does not touch it.
        var connectionString = new NpgsqlConnectionStringBuilder(factory.GetConnectionString()) { Database = "expression_builder_tests" }.ConnectionString;

        _context = new FilterNpgsqlDbContextFixture(new DbContextOptionsBuilder<FilterNpgsqlDbContextFixture>().UseNpgsql(connectionString).Options);

        await _seedLock.WaitAsync();

        try
        {
            if (!_seeded)
            {
                await _context.Database.EnsureDeletedAsync();
                await _context.Database.EnsureCreatedAsync();
                await _context.Entities.AddRangeAsync(FilterDataFixture.Create());
                await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear();
                _seeded = true;
            }
        }
        finally
        {
            _seedLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    [Theory]
    [ClassData(typeof(FilterCaseData))]
    public async Task Filter_ForPostgreSql_ShouldReturnExpectedEntities(string description, Action<Filter<FilterEntityFixture>> setup, int[] expectedIds)
    {
        // Arrange
        Settings.UseUtcConversionInDateTypes.Should().BeTrue("PostgreSQL timestamptz columns accept only UTC values");
        var filter = new Filter<FilterEntityFixture>();
        setup(filter);

        // Act
        var result = await _context.Entities.Where(filter.ToExpression()).Select(e => e.Id).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(expectedIds, description);
    }
}
#pragma warning restore xUnit1045 // Avoid using TheoryData type arguments that might not be serializable
