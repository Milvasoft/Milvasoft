using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Milvasoft.IntegrationTests;

[CollectionDefinition(nameof(DatabaseTestCollection))]
public class DatabaseTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private Respawner _respawner;
    private NpgsqlConnection _connection;
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder().WithImage("postgres:latest")
                                                                               .WithDatabase("testDb")
                                                                               .WithUsername("root")
                                                                               .WithPassword("postgres")
                                                                               .WithCleanUp(true)
                                                                               .WithPortBinding(5344, 5342)
                                                                               .Build();

    public async Task InitializeAsync()
    {
        if (_dbContainer.State != TestcontainersStates.Running)
        {
            await _dbContainer.StartAsync();

            _connection = new NpgsqlConnection($"{_dbContainer.GetConnectionString()};Timeout=30;");

            await _connection.OpenAsync();
        }
    }

    public async Task CreateRespawner()
    {
        if (_respawner == null)
        {
            _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore = ["__EFMigrationsHistory"]
            });
        }
    }

    public async Task ResetDatabase()
    {
        if (_respawner != null)
        {
            await _respawner.ResetAsync(_connection);
        }
    }

    public new async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        await _dbContainer.DisposeAsync();
    }

    public string GetConnectionString() => _dbContainer.GetConnectionString();
}