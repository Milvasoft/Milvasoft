using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.Core.Helpers;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.DataAccess.EfCore.Bulk.RepositoryBase.Abstract;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.IntegrationTests.Client.Fixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.Persistence;
using Milvasoft.MultiTenancy.Context;
using Milvasoft.MultiTenancy.EfCore;
using Milvasoft.MultiTenancy.EfCore.RepositoryBase.Concrete;
using Milvasoft.MultiTenancy.Extensions;

namespace Milvasoft.IntegrationTests.DataAccessTests.EfCoreTests;

/// <summary>
/// Tenant isolation of <see cref="MilvaMultiTenancyDbContext"/>, its factory and the multi tenant bulk repository.
/// </summary>
[Collection(nameof(UtcTrueDatabaseTestCollection))]
[Trait("MultiTenancy Integration Tests", "Integration tests for Milvasoft.MultiTenancy.EfCore.")]
public class MultiTenancyTests(CustomWebApplicationFactory factory) : DataAccessIntegrationTestBase(factory)
{
    private static readonly TenantId _tenantA = new("tenanta", 1);
    private static readonly TenantId _tenantB = new("tenantb", 1);

    private IServiceProvider _rootProvider;

    private Task SetUpAsync()
    {
        var waf = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDataAccessConfiguration>();
                services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
                services.RemoveAll<MilvaBulkDbContextFixture>();

                // Used by the test host to migrate the database.
                services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                {
                    x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                    x.UseNpgsql(_factory.GetConnectionString());
                });

                // The strategy always reports "test_1"; the factory must ignore it and read the validated tenant context only.
                services.AddMultiTenancy<SomeTenantEntity, TenantId>()
                        .WithResolutionStrategy<TestTenantIdResolutionStrategy>()
                        .WithStore<TestTenantStore<SomeTenantEntity, TenantId>>();

                services.ConfigureMilvaDataAccess();

                services.UpdateSingletonInstance<IDataAccessConfiguration>(opt =>
                {
                    opt.DbContext.UseUtcForDateTime = true;
                });

                services.AddDbContextFactory<MultiTenantDbContextFixture>(x =>
                {
                    x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                    x.UseNpgsql(_factory.GetConnectionString());
                });

                services.AddScoped<MilvaMultiTenancyDbContextFactory<MultiTenantDbContextFixture>>();
                services.AddScoped(sp => sp.GetRequiredService<MilvaMultiTenancyDbContextFactory<MultiTenantDbContextFixture>>().CreateDbContext());
                services.AddScoped(typeof(IMultiTenantRepository<>), typeof(MultiTenantRepository<>));
            });

            builder.Configure(app => { });
        });

        _rootProvider = waf.Services;
        _serviceProvider = waf.Services.CreateScope().ServiceProvider;

        return _factory.CreateRespawner();
    }

    [Fact]
    public async Task Factory_WithoutResolvedTenant_ShouldNotUseTheRawResolutionStrategy()
    {
        await SetUpAsync();

        using var scope = _rootProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MultiTenantDbContextFixture>();

        string.IsNullOrEmpty(dbContext.CurrentTenantId.TenancyName).Should().BeTrue();
    }

    [Fact]
    public async Task Add_ShouldStampCurrentTenant_AndQueriesShouldOnlySeeCurrentTenantRows()
    {
        await SetUpAsync();

        await SeedAsync(_tenantA, id: 1);
        await SeedAsync(_tenantB, id: 2);

        using var scope = CreateTenantScope(_tenantA);
        var dbContext = scope.ServiceProvider.GetRequiredService<MultiTenantDbContextFixture>();

        var rows = await dbContext.Entities.ToListAsync(TestContext.Current.CancellationToken);

        rows.Should().ContainSingle().Which.Id.Should().Be(1);
        rows[0].TenantId.Should().Be(_tenantA);
    }

    [Fact]
    public async Task Add_WithoutTenant_ShouldThrowAndNotSave()
    {
        await SetUpAsync();

        using var scope = _rootProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MultiTenantDbContextFixture>();

        dbContext.Entities.Add(NewEntity(1));

        var act = () => dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<TenantMismatchException>();
        (await CountAllAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Update_OfAnotherTenantsRow_ShouldThrowAndNotMoveTheRow()
    {
        await SetUpAsync();

        await SeedAsync(_tenantA, id: 1);

        using var scope = CreateTenantScope(_tenantB);
        var dbContext = scope.ServiceProvider.GetRequiredService<MultiTenantDbContextFixture>();

        var otherTenantsRow = await dbContext.Entities.IgnoreQueryFilters().AsTracking().SingleAsync(e => e.Id == 1, TestContext.Current.CancellationToken);
        otherTenantsRow.SomeDecimalProp = 999M;

        var act = () => dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<TenantMismatchException>();

        var stored = await GetAllIgnoringTenantAsync();
        stored.Single().TenantId.Should().Be(_tenantA);
        stored.Single().SomeDecimalProp.Should().Be(10M);
    }

    [Fact]
    public async Task Update_OfOwnRow_ShouldSucceed()
    {
        await SetUpAsync();

        await SeedAsync(_tenantA, id: 1);

        using var scope = CreateTenantScope(_tenantA);
        var dbContext = scope.ServiceProvider.GetRequiredService<MultiTenantDbContextFixture>();

        var row = await dbContext.Entities.AsTracking().SingleAsync(e => e.Id == 1, TestContext.Current.CancellationToken);
        row.SomeDecimalProp = 20M;

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        (await GetAllIgnoringTenantAsync()).Single().SomeDecimalProp.Should().Be(20M);
    }

    [Fact]
    public async Task Delete_OfAnotherTenantsRow_ShouldThrowAndKeepTheRow()
    {
        await SetUpAsync();

        await SeedAsync(_tenantA, id: 1);

        using var scope = CreateTenantScope(_tenantB);
        var dbContext = scope.ServiceProvider.GetRequiredService<MultiTenantDbContextFixture>();

        var otherTenantsRow = await dbContext.Entities.IgnoreQueryFilters().AsTracking().SingleAsync(e => e.Id == 1, TestContext.Current.CancellationToken);
        dbContext.Entities.Remove(otherTenantsRow);

        var act = () => dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<TenantMismatchException>();

        var stored = await GetAllIgnoringTenantAsync();
        stored.Should().ContainSingle();
        stored.Single().IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task BulkUpdate_WithForgedEntityOfAnotherTenantsRow_ShouldNotChangeThatRow()
    {
        await SetUpAsync();

        await SeedAsync(_tenantA, id: 1);

        using var scope = CreateTenantScope(_tenantB);
        var repository = scope.ServiceProvider.GetRequiredService<IMultiTenantRepository<SomeMultiTenantTestEntityFixture>>();

        var forged = NewEntity(1);
        forged.TenantId = _tenantB;
        forged.SomeDecimalProp = 999M;

        await repository.BulkUpdateAsync([forged], cancellationToken: TestContext.Current.CancellationToken);

        var stored = await GetAllIgnoringTenantAsync();
        stored.Single().TenantId.Should().Be(_tenantA);
        stored.Single().SomeDecimalProp.Should().Be(10M);
    }

    [Fact]
    public async Task BulkUpdate_WithEntityCarryingAnotherTenant_ShouldThrow()
    {
        await SetUpAsync();

        await SeedAsync(_tenantA, id: 1);

        using var scope = CreateTenantScope(_tenantB);
        var repository = scope.ServiceProvider.GetRequiredService<IMultiTenantRepository<SomeMultiTenantTestEntityFixture>>();

        var entity = NewEntity(1);
        entity.TenantId = _tenantA;

        var act = () => repository.BulkUpdateAsync([entity], cancellationToken: TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<TenantMismatchException>();
    }

    [Fact]
    public async Task BulkDelete_WithForgedEntityOfAnotherTenantsRow_ShouldKeepThatRow()
    {
        await SetUpAsync();

        await SeedAsync(_tenantA, id: 1);

        using var scope = CreateTenantScope(_tenantB);
        var repository = scope.ServiceProvider.GetRequiredService<IMultiTenantRepository<SomeMultiTenantTestEntityFixture>>();

        var forged = NewEntity(1);
        forged.TenantId = _tenantB;

        await repository.BulkDeleteAsync([forged], cancellationToken: TestContext.Current.CancellationToken);

        var stored = await GetAllIgnoringTenantAsync();
        stored.Should().ContainSingle();
        stored.Single().IsDeleted.Should().BeFalse();
    }

    #region Setup

    public interface IMultiTenantRepository<TEntity> : IBulkBaseRepository<TEntity> where TEntity : class, IMilvaEntity
    {
    }

    public class MultiTenantRepository<TEntity>(MultiTenantDbContextFixture dbContext) : MultiTenantBaseRepository<TEntity, MultiTenantDbContextFixture>(dbContext), IMultiTenantRepository<TEntity>
         where TEntity : class, IMilvaEntity
    {
    }

    private IServiceScope CreateTenantScope(TenantId tenantId)
    {
        var scope = _rootProvider.CreateScope();

        scope.ServiceProvider.GetRequiredService<ITenantContext<SomeTenantEntity, TenantId>>().SetTenant(new SomeTenantEntity(tenantId.TenancyName, tenantId.BranchNo));

        return scope;
    }

    private static SomeMultiTenantTestEntityFixture NewEntity(int id) => new()
    {
        Id = id,
        SomeDateProp = DateTime.UtcNow,
        SomeDecimalProp = 10M,
    };

    private async Task SeedAsync(TenantId tenantId, int id)
    {
        using var scope = CreateTenantScope(tenantId);
        var dbContext = scope.ServiceProvider.GetRequiredService<MultiTenantDbContextFixture>();

        dbContext.Entities.Add(NewEntity(id));

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<List<SomeMultiTenantTestEntityFixture>> GetAllIgnoringTenantAsync()
    {
        using var scope = _rootProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MultiTenantDbContextFixture>();

        return await dbContext.Entities.IgnoreQueryFilters().AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);
    }

    private async Task<int> CountAllAsync() => (await GetAllIgnoringTenantAsync()).Count;

    #endregion
}
