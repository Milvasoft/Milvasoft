using Microsoft.EntityFrameworkCore;
using Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;
using Milvasoft.MultiTenancy.EfCore;

namespace Milvasoft.IntegrationTests.Client.Fixtures.Persistence;

public class MultiTenantDbContextFixture(DbContextOptions<MultiTenantDbContextFixture> contextOptions) : MilvaMultiTenancyDbContext(contextOptions)
{
    public DbSet<SomeMultiTenantTestEntityFixture> Entities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Same table as MilvaBulkDbContextFixture.SomeMultiTenantEntities (created by the test database migration).
        modelBuilder.Entity<SomeMultiTenantTestEntityFixture>().ToTable("SomeMultiTenantEntities");
    }
}
