using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.IntegrationTests.DataAccessTests;

public abstract class DataAccessIntegrationTestBase(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory _factory = factory;
    protected IServiceProvider _serviceProvider;
    protected IServiceScope _serviceScope;

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual Task InitializeAsync(Action<IServiceCollection> configureServices = null, Action<IApplicationBuilder> configureApp = null)
    {
        var waf = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                configureServices?.Invoke(services);
            });

            builder.Configure(app =>
            {
                configureApp?.Invoke(app);
            });
        });

        _serviceProvider = waf.Services.CreateScope().ServiceProvider;

        return _factory.CreateRespawner();
    }

    public virtual Task DisposeAsync() => _factory.ResetDatabase();
}