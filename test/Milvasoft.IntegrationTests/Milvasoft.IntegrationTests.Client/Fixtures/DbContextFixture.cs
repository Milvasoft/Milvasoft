using Microsoft.EntityFrameworkCore;
using Milvasoft.DataAccess.EfCore.Bulk.DbContextBase;
using Milvasoft.DataAccess.EfCore.Configuration;

namespace Milvasoft.IntegrationTests.Client.Fixtures;

public class MilvaBulkDbContextFixture(DbContextOptions<MilvaBulkDbContextFixture> contextOptions, IDataAccessConfiguration dataAccessConfiguration, IServiceProvider serviceProvider) : MilvaBulkDbContext(contextOptions, dataAccessConfiguration, serviceProvider)
{
    public DbSet<SomeEntityFixture> Entities { get; set; }
    public DbSet<SomeBaseEntityFixture> BaseEntities { get; set; }
    public DbSet<SomeFullAuditableEntityFixture> FullAuditableEntities { get; set; }
}
