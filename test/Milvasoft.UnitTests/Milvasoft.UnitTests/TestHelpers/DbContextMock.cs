using Microsoft.EntityFrameworkCore;

namespace Milvasoft.UnitTests.TestHelpers;

public class DbContextMock<TContext> where TContext : DbContext
{
    private readonly TContext _testDbContext;

    public DbContextMock(string dbName)
    {
        var builder = new DbContextOptionsBuilder<TContext>();

        builder.UseInMemoryDatabase(databaseName: $"{dbName}TestDbInMemory_{Guid.NewGuid}_{DateTime.Now.Nanosecond}");

        var dbContextOptions = builder.Options;

        _testDbContext = (TContext)Activator.CreateInstance(typeof(TContext), dbContextOptions);

        // Delete existing db before creating a new one
        _testDbContext.Database.EnsureDeleted();
        _testDbContext.Database.EnsureCreated();
    }

    public TContext GetDbContextFixture() => _testDbContext;
}
