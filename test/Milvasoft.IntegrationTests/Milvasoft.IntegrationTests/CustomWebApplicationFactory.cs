using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Milvasoft.UnitTests.DataAccessTests.EfCoreTests.Fixtures;
using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;

namespace Milvasoft.IntegrationTests;

[CollectionDefinition(nameof(DatabaseTestCollection))]
public class DatabaseTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private Func<IApplicationBuilder, IApplicationBuilder> _appBuilderAction = (app) => app;
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder().WithImage("postgres:latest")
                                                                               .WithDatabase("db")
                                                                               .WithUsername("postgres")
                                                                               .WithPassword("postgres")
                                                                               .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
                                                                               .WithCleanUp(true)
                                                                               .WithPortBinding(5343, 5342)
                                                                               .Build();

    private Respawner _respawner;
    private DbConnection _connection;
    public SomeMilvaDbContextFixture Db { get; private set; } = null!;

    public void SetAppBuilderAction(Func<IApplicationBuilder, IApplicationBuilder> appBuilderAction) => _appBuilderAction = appBuilderAction;

    protected override IHostBuilder CreateHostBuilder()
        => Host.CreateDefaultBuilder()
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.Configure(app =>
                   {
                       _appBuilderAction.Invoke(app);
                   });
               });

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        Db = Services.CreateScope().ServiceProvider.GetRequiredService<SomeMilvaDbContextFixture>();
        _connection = Db.Database.GetDbConnection();
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbContainer.GetConnectionString(), new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<SomeMilvaDbContextFixture>>();
            services.RemoveAll<SomeMilvaDbContextFixture>();

            services.AddDbContext<SomeMilvaDbContextFixture>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });
        });
    }

    public async Task ResetDatabase()
    {
        await _respawner.ResetAsync(_connection);
    }

    public new async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        await _dbContainer.DisposeAsync();
    }

    public string GetConnectionString()
    {
        return _dbContainer.GetConnectionString();
    }
}