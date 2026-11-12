using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.EntityBases.Concrete.Auditing;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.DbContextBase;
using Npgsql;

namespace Milvasoft.IntegrationTests.DataAccessTests.EfCoreTests;

/// <summary>
/// Soft delete and tenant id query filters of <see cref="ModelBuilderExtensions"/> against PostgreSQL.
/// </summary>
[Collection(nameof(UtcTrueDatabaseTestCollection))]
[Trait("ModelBuilderExtensions Query Filter Integration Tests", "Query filter extensions of ModelBuilderExtensions.")]
public class QueryFilterTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private static readonly TenantId _firstTenant = new("milvasoft", 1);
    private static readonly TenantId _secondTenant = new("milvasoft", 2);

    private readonly string _connectionString = new NpgsqlConnectionStringBuilder(factory.GetConnectionString())
    {
        Database = $"qf_{Guid.NewGuid():N}"
    }.ConnectionString;

    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        await using var context = CreateContext(_firstTenant);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        context.Items.AddRange(new QfItem { Name = "first-active", TenantId = _firstTenant },
                               new QfItem { Name = "first-deleted", TenantId = _firstTenant, IsDeleted = true },
                               new QfSpecialItem { Name = "first-special", TenantId = _firstTenant, Extra = "extra" },
                               new QfItem { Name = "second-active", TenantId = _secondTenant },
                               new QfItem { Name = "second-deleted", TenantId = _secondTenant, IsDeleted = true });

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await using var context = CreateContext(_firstTenant);
        await context.Database.EnsureDeletedAsync();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task QueryFilters_WithSoftDeleteAndContextTenantFiltersOnHierarchy_ShouldApplyBothFiltersForEachContextTenant()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await using var firstTenantContext = CreateContext(_firstTenant);
        await using var secondTenantContext = CreateContext(_secondTenant);

        // Act
        var firstTenantItems = await firstTenantContext.Items.Select(i => i.Name).OrderBy(n => n).ToListAsync(ct);
        var secondTenantItems = await secondTenantContext.Items.Select(i => i.Name).OrderBy(n => n).ToListAsync(ct);
        var firstTenantSpecialItems = await firstTenantContext.Set<QfSpecialItem>().Select(i => i.Name).ToListAsync(ct);
        var allItemCount = await firstTenantContext.Items.IgnoreQueryFilters().CountAsync(ct);

        // Assert
        firstTenantItems.Should().Equal("first-active", "first-special");
        secondTenantItems.Should().Equal("second-active");
        firstTenantSpecialItems.Should().Equal("first-special");
        allItemCount.Should().Be(5);
    }

    private QueryFilterTestDbContext CreateContext(TenantId tenantId)
    {
        var options = new DbContextOptionsBuilder<QueryFilterTestDbContext>().UseNpgsql(_connectionString).Options;

        var configuration = new DataAccessConfiguration { DbContext = new DbContextConfiguration { UseUtcForDateTime = true } };

        return new QueryFilterTestDbContext(options, configuration) { CurrentTenantId = tenantId };
    }
}

public class QueryFilterTestDbContext(DbContextOptions<QueryFilterTestDbContext> options, IDataAccessConfiguration configuration) : MilvaDbContext(options, configuration)
{
    public TenantId CurrentTenantId { get; set; }

    public DbSet<QfItem> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<QfSpecialItem>();

        modelBuilder.UseTenantIdQueryFilter(this, c => c.CurrentTenantId);
        modelBuilder.UseSoftDeleteQueryFilter();
    }
}

public class QfItem : FullAuditableEntityWithTenantId<int>
{
    public string Name { get; set; }
}

public class QfSpecialItem : QfItem
{
    public string Extra { get; set; }
}
