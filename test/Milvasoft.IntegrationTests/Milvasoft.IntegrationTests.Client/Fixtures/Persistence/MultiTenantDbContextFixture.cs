using Microsoft.EntityFrameworkCore;
using Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;
using Milvasoft.MultiTenancy.EfCore;

namespace Milvasoft.IntegrationTests.Client.Fixtures.Persistence;

public class MultiTenantDbContextFixture(DbContextOptions<MultiTenantDbContextFixture> contextOptions) : MilvaMultiTenancyDbContext(contextOptions)
{
    public DbSet<SomeMultiTenantTestEntityFixture> Entities { get; set; }
}