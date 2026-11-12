using Microsoft.EntityFrameworkCore;
using Milvasoft.BulkExtensions.PostgreSql;
using Milvasoft.BulkExtensions.Tests.Infrastructure;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(PostgresFixture))]

namespace Milvasoft.BulkExtensions.Tests.Infrastructure;

/// <summary>
/// One PostgreSQL container for the whole test run. Every test gets its own database.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17").WithDatabase("postgres")
                                                                                          .WithUsername("postgres")
                                                                                          .WithPassword("postgres")
                                                                                          .WithCommand("-c", "max_connections=1000")
                                                                                          .WithCleanUp(true)
                                                                                          .Build();

    public string GetConnectionString(string database)
        => new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = database,
            IncludeErrorDetail = true,

            // Every test has its own database and so its own pool; keep pools small and short lived.
            MaxPoolSize = 20,
            ConnectionIdleLifetime = 2,
            ConnectionPruningInterval = 1,
        }.ConnectionString;

    public ValueTask InitializeAsync() => new(_container.StartAsync());

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}

/// <summary>
/// Base class of the tests. Creates a fresh database for each test.
/// </summary>
public abstract class BulkTestBase(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly string _database = $"bulk_{Guid.NewGuid():N}";

    protected PostgresFixture Fixture { get; } = fixture;

    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    /// <summary>
    /// Creates a context on the test database.
    /// </summary>
    /// <param name="registerBulkProvider">Whether to register the provider with <c>UseBulkExtensions()</c>. Without it the provider is found by convention.</param>
    /// <param name="configure">More Npgsql options.</param>
    /// <param name="configureOptions">More context options, such as a logger factory.</param>
    protected TestDbContext CreateContext(bool registerBulkProvider = true, Action<NpgsqlDbContextOptionsBuilder> configure = null, Action<DbContextOptionsBuilder> configureOptions = null)
    {
        var builder = new DbContextOptionsBuilder<TestDbContext>().UseNpgsql(Fixture.GetConnectionString(_database), o =>
                                                                  {
                                                                      o.MapEnum<Mood>("mood");

                                                                      if (registerBulkProvider)
                                                                          o.UseBulkExtensions();

                                                                      configure?.Invoke(o);
                                                                  });

        configureOptions?.Invoke(builder);

        return new TestDbContext(builder.Options);
    }

    /// <summary>
    /// Creates a context with mappings that bulk operations reject, on its own database.
    /// </summary>
    protected async Task<UnsupportedDbContext> CreateUnsupportedContextAsync()
    {
        var options = new DbContextOptionsBuilder<UnsupportedDbContext>().UseNpgsql(Fixture.GetConnectionString(_database + "_unsupported"), o => o.UseBulkExtensions()).Options;
        var context = new UnsupportedDbContext(options);

        await context.Database.EnsureCreatedAsync(CancellationToken);

        return context;
    }

    public async ValueTask InitializeAsync()
    {
        await using var context = CreateContext();

        await context.Database.EnsureCreatedAsync(CancellationToken);
    }

    /// <summary>
    /// Databases are removed with the container.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    protected static Product NewProduct(string sku, decimal price = 10m, string name = null) => new()
    {
        Sku = sku,
        Name = name ?? $"Product {sku}",
        Price = price,
        Status = ProductStatus.Active,
        Tags = ["a", "b"],
        ReleaseDate = new DateOnly(2026, 1, 15),
        Mood = Mood.Happy,
        Size = new Dimensions { Width = 1.5, Height = 2.5 },
    };

    /// <summary>
    /// Inserts entities with regular EF Core and returns them.
    /// </summary>
    protected async Task<List<T>> SeedAsync<T>(params T[] entities) where T : class
    {
        await using var context = CreateContext();

        context.AddRange(entities);
        await context.SaveChangesAsync(CancellationToken);

        return [.. entities];
    }
}