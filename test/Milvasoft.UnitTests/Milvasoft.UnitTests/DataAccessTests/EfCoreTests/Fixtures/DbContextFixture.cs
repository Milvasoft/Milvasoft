using Microsoft.EntityFrameworkCore;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.DbContextBase;
using Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests.Fixtures;

public class SomeMilvaDbContextFixture(DbContextOptions<SomeMilvaDbContextFixture> contextOptions, IDataAccessConfiguration dataAccessConfiguration) : MilvaDbContext(contextOptions, dataAccessConfiguration)
{
    public DbSet<RestTestEntityFixture> RestTestEntities { get; set; }
    public DbSet<SomeEntityFixture> Entities { get; set; }
    public DbSet<SomeBaseEntityFixture> BaseEntities { get; set; }
    public DbSet<SomeFullAuditableEntityFixture> FullAuditableEntities { get; set; }
}
