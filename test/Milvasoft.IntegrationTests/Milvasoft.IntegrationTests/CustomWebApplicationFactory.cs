using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Milvasoft.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private Respawner _respawner;
    private NpgsqlConnection _connection;
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:latest").WithDatabase("testDb")
                                                                                                .WithUsername("root")
                                                                                                .WithPassword("postgres")
                                                                                                .WithCleanUp(true)
                                                                                                .Build();

    private const string _resetAutoIncrementQuery = @"
                DO $$
                DECLARE
                    seq RECORD;
                BEGIN
                    -- Iterate over all sequences in the current database
                    FOR seq IN
                        SELECT c.oid::regclass::text AS sequence_name
                        FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE c.relkind = 'S'  -- 'S' represents sequences
                    LOOP
                        -- Reset each sequence to start from 1
                        EXECUTE 'ALTER SEQUENCE ' || seq.sequence_name || ' RESTART WITH 1';
                    END LOOP;
                END
                $$;
            ";

    public async Task InitializeAsync()
    {
        if (_dbContainer.State != TestcontainersStates.Running)
        {
            await _dbContainer.StartAsync();

            _connection = new NpgsqlConnection($"{_dbContainer.GetConnectionString()};Timeout=30;");

            await _connection.OpenAsync();
        }
    }

    public async Task CreateRespawner() => _respawner ??= await Respawner.CreateAsync(_connection, new RespawnerOptions
    {
        DbAdapter = DbAdapter.Postgres,
        SchemasToInclude = ["public"],
        TablesToIgnore = ["__EFMigrationsHistory"]
    });

    public async Task ResetDatabase()
    {
        if (_respawner != null)
        {
            await _respawner.ResetAsync(_connection);
            await _dbContainer.ExecScriptAsync(_resetAutoIncrementQuery);
        }
    }

    public new async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        await _dbContainer.DisposeAsync();
    }

    public string GetConnectionString() => _dbContainer.GetConnectionString();

    async ValueTask IAsyncLifetime.InitializeAsync() => await InitializeAsync();
}