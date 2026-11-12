using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Helpers;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.Persistence;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Ef;
using Milvasoft.Interception.Ef.Transaction;

namespace Milvasoft.IntegrationTests.DataAccessTests.EfCoreTests;

/// <summary>
/// The transaction interceptor commits only completed, successful work and rolls back everything else.
/// </summary>
[Collection(nameof(UtcTrueDatabaseTestCollection))]
[Trait("Interception Integration Tests", "Integration tests for Milvasoft.Interception.Ef.")]
public class TransactionInterceptorTests(CustomWebApplicationFactory factory) : DataAccessIntegrationTestBase(factory)
{
    private IServiceProvider _rootProvider;

    private Task SetUpAsync(bool registerDbContextType = true)
    {
        var waf = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDataAccessConfiguration>();
                services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
                services.RemoveAll<MilvaBulkDbContextFixture>();

                services.ConfigureMilvaDataAccess();

                services.UpdateSingletonInstance<IDataAccessConfiguration>(opt =>
                {
                    opt.DbContext.UseUtcForDateTime = true;
                });

                services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                {
                    x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                    x.UseNpgsql(_factory.GetConnectionString(), o => o.EnableRetryOnFailure());
                });

                services.AddScoped<ITransactionalService, TransactionalService>();

                services.AddMilvaInterception([typeof(ITransactionalService)])
                        .WithTransactionInterceptor(opt =>
                        {
                            if (registerDbContextType)
                                opt.DbContextType = typeof(MilvaBulkDbContextFixture);
                        });
            });
        });

        _rootProvider = waf.Services;
        _serviceProvider = waf.Services.CreateScope().ServiceProvider;

        return _factory.CreateRespawner();
    }

    [Fact]
    public async Task SuccessfulMethod_ShouldCommit()
    {
        await SetUpAsync();

        using (var scope = _rootProvider.CreateScope())
            await scope.ServiceProvider.GetRequiredService<ITransactionalService>().AddAndSucceedAsync(1);

        (await CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task ThrowingMethod_ShouldRollBack()
    {
        await SetUpAsync();

        using (var scope = _rootProvider.CreateScope())
        {
            var act = () => scope.ServiceProvider.GetRequiredService<ITransactionalService>().AddAndThrowAsync(1);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        (await CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task MethodReturningUnsuccessfulResponse_ShouldRollBack()
    {
        await SetUpAsync();

        using (var scope = _rootProvider.CreateScope())
        {
            var response = await scope.ServiceProvider.GetRequiredService<ITransactionalService>().AddAndFailAsync(1);

            response.IsSuccess.Should().BeFalse();
        }

        (await CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task TaskMethodWithoutAsyncKeyword_ShouldCommitAfterItsWorkCompletes()
    {
        await SetUpAsync();

        using (var scope = _rootProvider.CreateScope())
            await scope.ServiceProvider.GetRequiredService<ITransactionalService>().AddWithoutAsyncKeyword(1);

        (await CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task MissingDbContextConfiguration_ShouldThrowBeforeRunningTheMethod()
    {
        await SetUpAsync(registerDbContextType: false);

        using (var scope = _rootProvider.CreateScope())
        {
            var act = () => scope.ServiceProvider.GetRequiredService<ITransactionalService>().AddAndSucceedAsync(1);

            await act.Should().ThrowAsync<MilvaDeveloperException>();
        }

        (await CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CanRetrieveDbContext_ShouldUseTheInterceptedInstancesContext()
    {
        await SetUpAsync();

        using (var scope = _rootProvider.CreateScope())
        {
            var act = () => scope.ServiceProvider.GetRequiredService<ITransactionalService>().AddWithOwnContextAndThrowAsync(1);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        (await CountAsync()).Should().Be(0);
    }

    #region Setup

    public interface ITransactionalService : IInterceptable
    {
        Task AddAndSucceedAsync(int id);
        Task AddAndThrowAsync(int id);
        Task<Response> AddAndFailAsync(int id);
        Task AddWithoutAsyncKeyword(int id);
        Task AddWithOwnContextAndThrowAsync(int id);
    }

    public class TransactionalService(MilvaBulkDbContextFixture dbContext) : ITransactionalService, ICanRetrieveDbContext
    {
        [Transaction]
        public Task AddAndSucceedAsync(int id) => AddAsync(id);

        [Transaction]
        public async Task AddAndThrowAsync(int id)
        {
            await AddAsync(id);
            throw new InvalidOperationException("failed after saving");
        }

        [Transaction]
        public async Task<Response> AddAndFailAsync(int id)
        {
            await AddAsync(id);
            return Response.Error();
        }

        [Transaction]
        public Task AddWithoutAsyncKeyword(int id) => Task.Delay(20).ContinueWith(_ => AddAsync(id), TaskScheduler.Default).Unwrap();

        [Transaction(getDbContextFromServiceProvider: false)]
        public async Task AddWithOwnContextAndThrowAsync(int id)
        {
            await AddAsync(id);
            throw new InvalidOperationException("failed after saving");
        }

        public DbContext GetDbContext(Type contextType) => dbContext;

        private Task AddAsync(int id)
        {
            dbContext.FullAuditableEntities.Add(new SomeFullAuditableEntityFixture { Id = id, SomeDateProp = DateTime.UtcNow, SomeDateTimeOffsetProp = DateTimeOffset.UtcNow });
            return dbContext.SaveChangesAsync();
        }
    }

    private async Task<int> CountAsync()
    {
        using var scope = _rootProvider.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<MilvaBulkDbContextFixture>().FullAuditableEntities.IgnoreQueryFilters().CountAsync(TestContext.Current.CancellationToken);
    }

    #endregion
}
