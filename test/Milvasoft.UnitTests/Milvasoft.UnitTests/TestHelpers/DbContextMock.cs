using Microsoft.EntityFrameworkCore;
using Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;

namespace Milvasoft.UnitTests.TestHelpers;

public class DbContextMock
{
    private readonly RestDbContextFixture _testDbContext;

    public DbContextMock(string dbName)
    {
        var builder = new DbContextOptionsBuilder<RestDbContextFixture>();

        builder.UseInMemoryDatabase(databaseName: $"{dbName}TestDbInMemory_{Guid.NewGuid}_{DateTime.Now.Nanosecond}");

        var dbContextOptions = builder.Options;

        _testDbContext = new RestDbContextFixture(dbContextOptions);

        // Delete existing db before creating a new one
        _testDbContext.Database.EnsureDeleted();
        _testDbContext.Database.EnsureCreated();
    }

    public RestDbContextFixture GetDbContextFixture() => _testDbContext;
}
