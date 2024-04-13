using Microsoft.EntityFrameworkCore;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.DbContextBase;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests.Fixtures;

public class SomeMilvaDbContextFixture(DbContextOptions<SomeMilvaDbContextFixture> contextOptions, IDataAccessConfiguration dataAccessConfiguration) : MilvaDbContext(contextOptions, dataAccessConfiguration)
{
    public DbSet<SomeEntityFixture> Entities { get; set; }
    public DbSet<SomeBaseEntityFixture> BaseEntities { get; set; }
    public DbSet<SomeFullAuditableEntityFixture> FullAuditableEntities { get; set; }
}
