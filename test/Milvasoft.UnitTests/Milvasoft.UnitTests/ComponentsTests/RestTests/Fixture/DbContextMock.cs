using Microsoft.EntityFrameworkCore;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;

public class DbContextMock
{
    private readonly RestDbContextFixture _testDbContext;
    public DbContextMock()
    {
        var builder = new DbContextOptionsBuilder<RestDbContextFixture>();

        builder.UseInMemoryDatabase(databaseName: $"ComponentsRestTestDbInMemory_{Guid.NewGuid}");

        var dbContextOptions = builder.Options;

        _testDbContext = new RestDbContextFixture(dbContextOptions);

        // Delete existing db before creating a new one
        _testDbContext.Database.EnsureDeleted();
        _testDbContext.Database.EnsureCreated();
    }

    public RestDbContextFixture GetDbContextFixture() => _testDbContext;
}
