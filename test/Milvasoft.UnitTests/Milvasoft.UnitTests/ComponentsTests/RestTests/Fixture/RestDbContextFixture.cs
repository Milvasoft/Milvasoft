using Microsoft.EntityFrameworkCore;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;
public class RestDbContextFixture(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
{
    public DbSet<RestTestEntityFixture> TestEntities { get; set; }
}
