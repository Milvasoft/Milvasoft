using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.Helpers;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;
using Milvasoft.DataAccess.EfCore.Utils.Enums;
using Milvasoft.DataAccess.EfCore.Utils.IncludeLibrary;
using Milvasoft.Helpers.DataAccess.EfCore.Concrete;
using Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.Persistence;
using System.Linq.Expressions;

namespace Milvasoft.IntegrationTests.DataAccessTests.EfCoreTests.RepositoryBaseTests;

[Collection(nameof(UtcTrueDatabaseTestCollection))]
[Trait("RepositoryBase Async Integration Tests", "Integration tests for Milvasoft.DataAccess.EfCore integration tests.")]
public class BaseRepositoryAsyncTests(CustomWebApplicationFactory factory) : DataAccessIntegrationTestBase(factory)
{
    public override async Task InitializeAsync(Action<IServiceCollection> configureServices = null, Action<IApplicationBuilder> configureApp = null)
    {
        var waf = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDataAccessConfiguration>();
                services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
                services.RemoveAll<MilvaBulkDbContextFixture>();

                configureServices?.Invoke(services);

                services.UpdateSingletonInstance<IDataAccessConfiguration>(opt =>
                {
                    opt.DbContext.UseUtcForDateTime = true;
                });

                services.AddScoped(typeof(ISomeGenericRepository<>), typeof(SomeGenericRepository<>));
            });

            builder.Configure(app =>
            {
                configureApp?.Invoke(app);
            });
        });

        _serviceProvider = waf.Services.CreateScope().ServiceProvider;

        await _factory.CreateRespawner();
    }

    #region Configuration Based Tests

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DataFetch_WithDefaultConfiguration_ShouldNotReturnSoftDeletedData(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act
        var getAllData = await entityRepository.GetAllAsync();
        var getSomeData = await entityRepository.GetSomeAsync(4);
        var firstData = await entityRepository.GetFirstOrDefaultAsync(i => i.Id == 4);
        var singleData = await entityRepository.GetSingleOrDefaultAsync(i => i.Id == 4);
        var getByIdData = await entityRepository.GetByIdAsync(4);

        // Assert
        getAllData.Should().HaveCount(3);
        getSomeData.Should().HaveCount(3);
        firstData.Should().BeNull();
        singleData.Should().BeNull();
        getByIdData.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DataFetch_WithDefaultConfigurationAndNavigationProjection_ShouldNotReturnSoftDeletedData(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act
        var getAllData = await entityRepository.GetAllAsync(projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var getSomeData = await entityRepository.GetSomeAsync(4, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var firstData = await entityRepository.GetFirstOrDefaultAsync(i => i.Id == 1, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var singleData = await entityRepository.GetSingleOrDefaultAsync(i => i.Id == 1, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var getByIdData = await entityRepository.GetByIdAsync(1, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });

        // Assert
        getAllData.Should().HaveCount(3);
        getAllData.Find(i => i.Id == 1).ManyToOneEntities.Count.Should().Be(1);
        getSomeData.Should().HaveCount(3);
        getSomeData.Find(i => i.Id == 1).ManyToOneEntities.Count.Should().Be(1);
        firstData.Should().NotBeNull();
        firstData.ManyToOneEntities.Count.Should().Be(1);
        singleData.Should().NotBeNull();
        singleData.ManyToOneEntities.Count.Should().Be(1);
        getByIdData.Should().NotBeNull();
        getByIdData.ManyToOneEntities.Count.Should().Be(1);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DataFetch_FetchSoftDeletedEntities_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsFalse_ShouldReturnSoftDeletedData(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.Repository = new RepositoryConfiguration
                {
                    DefaultSoftDeletedFetchState = false,
                    ResetSoftDeletedFetchStateAfterEveryOperation = false,
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });
        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);
        entityRepository.FetchSoftDeletedEntities(true);

        // Act
        var getAllData = await entityRepository.GetAllAsync();
        var getSomeData = await entityRepository.GetSomeAsync(4);
        var firstData = await entityRepository.GetFirstOrDefaultAsync(i => i.Id == 4);
        var singleData = await entityRepository.GetSingleOrDefaultAsync(i => i.Id == 4);
        var getByIdData = await entityRepository.GetByIdAsync(4);

        // Assert
        getAllData.Should().HaveCount(4);
        getSomeData.Should().HaveCount(4);
        firstData.Should().NotBeNull();
        singleData.Should().NotBeNull();
        getByIdData.Should().NotBeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DataFetch_FetchSoftDeletedEntities_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsFalseAndNavigationProjection_ShouldReturnSoftDeletedData(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.Repository = new RepositoryConfiguration
                {
                    DefaultSoftDeletedFetchState = false,
                    ResetSoftDeletedFetchStateAfterEveryOperation = false,
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });
        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);
        entityRepository.FetchSoftDeletedEntities(true);

        // Act
        var getAllData = await entityRepository.GetAllAsync(projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var getSomeData = await entityRepository.GetSomeAsync(4, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var firstData = await entityRepository.GetFirstOrDefaultAsync(i => i.Id == 1, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var singleData = await entityRepository.GetSingleOrDefaultAsync(i => i.Id == 1, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var getByIdData = await entityRepository.GetByIdAsync(1, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });

        // Assert
        getAllData.Should().HaveCount(4);
        getAllData.Find(i => i.Id == 1).ManyToOneEntities.Count.Should().Be(2);
        getSomeData.Should().HaveCount(4);
        getSomeData.Find(i => i.Id == 1).ManyToOneEntities.Count.Should().Be(2);
        firstData.Should().NotBeNull();
        firstData.ManyToOneEntities.Count.Should().Be(2);
        singleData.Should().NotBeNull();
        singleData.ManyToOneEntities.Count.Should().Be(2);
        getByIdData.Should().NotBeNull();
        getByIdData.ManyToOneEntities.Count.Should().Be(2);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DataFetch_ResetSoftDeletedEntityFetchState_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsFalse_ShouldNotReturnSoftDeletedDataAfterSecondOperation(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.Repository = new RepositoryConfiguration
                {
                    DefaultSoftDeletedFetchState = false,
                    ResetSoftDeletedFetchStateAfterEveryOperation = true,
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });
        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);
        entityRepository.FetchSoftDeletedEntities(true);
        entityRepository.SoftDeleteFetchStateResetAfterOperation(false);

        // Act
        var getAllData = await entityRepository.GetAllAsync();
        entityRepository.ResetSoftDeletedEntityFetchResetState();
        var getSomeData = await entityRepository.GetSomeAsync(4);
        var firstData = await entityRepository.GetFirstOrDefaultAsync(i => i.Id == 4);
        var singleData = await entityRepository.GetSingleOrDefaultAsync(i => i.Id == 4);
        var getByIdData = await entityRepository.GetByIdAsync(4);

        // Assert
        getAllData.Should().HaveCount(4);
        getSomeData.Should().HaveCount(4);
        firstData.Should().BeNull();
        singleData.Should().BeNull();
        getByIdData.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DataFetch_FetchSoftDeletedEntities_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsTrue_ShouldNotReturnSoftDeletedDataAfterFirstOperation(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.Repository = new RepositoryConfiguration
                {
                    DefaultSoftDeletedFetchState = false,
                    ResetSoftDeletedFetchStateAfterEveryOperation = true,
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });
        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);
        entityRepository.FetchSoftDeletedEntities(true);

        // Act
        var getAllData = await entityRepository.GetAllAsync();
        var getSomeData = await entityRepository.GetSomeAsync(4);
        var firstData = await entityRepository.GetFirstOrDefaultAsync(i => i.Id == 4);
        var singleData = await entityRepository.GetSingleOrDefaultAsync(i => i.Id == 4);
        var getByIdData = await entityRepository.GetByIdAsync(4);

        // Assert
        getAllData.Should().HaveCount(4);
        getSomeData.Should().HaveCount(3);
        firstData.Should().BeNull();
        singleData.Should().BeNull();
        getByIdData.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DataFetch_SoftDeleteFetchStateResetAfterOperation_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsFalse_ShouldNotReturnSoftDeletedDataAfterFirstOperation(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.Repository = new RepositoryConfiguration
                {
                    DefaultSoftDeletedFetchState = false,
                    ResetSoftDeletedFetchStateAfterEveryOperation = false,
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);
        entityRepository.FetchSoftDeletedEntities(true);
        entityRepository.SoftDeleteFetchStateResetAfterOperation(true);

        // Act
        var getAllData = await entityRepository.GetAllAsync();
        var getSomeData = await entityRepository.GetSomeAsync(4);
        var firstData = await entityRepository.GetFirstOrDefaultAsync(i => i.Id == 4);
        var singleData = await entityRepository.GetSingleOrDefaultAsync(i => i.Id == 4);
        var getByIdData = await entityRepository.GetByIdAsync(4);

        // Assert
        getAllData.Should().HaveCount(4);
        getSomeData.Should().HaveCount(3);
        firstData.Should().BeNull();
        singleData.Should().BeNull();
        getByIdData.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DataFetch_ResetSoftDeletedEntityFetchState_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsFalse_ShouldNotReturnSoftDeletedDataAfterFirstOperation(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.Repository = new RepositoryConfiguration
                {
                    DefaultSoftDeletedFetchState = false,
                    ResetSoftDeletedFetchStateAfterEveryOperation = false,
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);
        entityRepository.FetchSoftDeletedEntities(true);
        entityRepository.SoftDeleteFetchStateResetAfterOperation(true);
        entityRepository.ResetSoftDeletedEntityFetchState();

        // Act
        var getAllData = await entityRepository.GetAllAsync();
        var getSomeData = await entityRepository.GetSomeAsync(4);
        var firstData = await entityRepository.GetFirstOrDefaultAsync(i => i.Id == 4);
        var singleData = await entityRepository.GetSingleOrDefaultAsync(i => i.Id == 4);
        var getByIdData = await entityRepository.GetByIdAsync(4);

        // Assert
        getAllData.Should().HaveCount(3);
        getSomeData.Should().HaveCount(3);
        firstData.Should().BeNull();
        singleData.Should().BeNull();
        getByIdData.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DataManipulation_WithDefaultSaveChangeChoise_ShouldSaveChangesAfterEveryOperation(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    DefaultSoftDeletionState = SoftDeletionState.Passive,
                };
                opt.Repository = new RepositoryConfiguration
                {
                    DefaultSaveChangesChoice = SaveChangesChoice.AfterEveryOperation,
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });
        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act & Assert
        var dataBeforeUpdate = await entityRepository.GetByIdAsync(1);

        dataBeforeUpdate.SomeStringProp = "somestring11";
        dataBeforeUpdate.CreationDate.Should().NotBeNull();

        await entityRepository.UpdateAsync(dataBeforeUpdate);

        var dataAfterUpdate = await entityRepository.GetByIdAsync(1);

        dataAfterUpdate.SomeStringProp.Should().Be("somestring11");
        dataAfterUpdate.CreationDate.Should().NotBeNull();
        dataAfterUpdate.LastModificationDate.Should().NotBeNull();

        await entityRepository.DeleteAsync(dataAfterUpdate);

        var dataAfterDelete = await entityRepository.GetByIdAsync(1);

        dataAfterDelete.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DataManipulation_WithManualSaveChangeChoise_ShouldSaveChangesAfterEveryOperation(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    DefaultSoftDeletionState = SoftDeletionState.Passive,
                };
                opt.Repository = new RepositoryConfiguration
                {
                    DefaultSaveChangesChoice = SaveChangesChoice.Manual,
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act & Assert
        var dataBeforeUpdate = await entityRepository.GetByIdAsync(1);

        dataBeforeUpdate.SomeStringProp = "somestring11";
        dataBeforeUpdate.CreationDate.Should().NotBeNull();

        await entityRepository.UpdateAsync(dataBeforeUpdate);

        var dataAfterUpdate = await entityRepository.GetByIdAsync(1);

        dataAfterUpdate.SomeStringProp.Should().Be("somestring1");
        dataAfterUpdate.CreationDate.Should().NotBeNull();
        dataAfterUpdate.LastModificationDate.Should().BeNull();

        await entityRepository.DeleteAsync(dataAfterUpdate);

        var dataAfterDelete = await entityRepository.GetByIdAsync(1);
        dataAfterDelete.Should().NotBeNull();

        await entityRepository.SaveChangesAsync();

        var dataAfterSaveChanges = await entityRepository.GetByIdAsync(1);
        dataAfterSaveChanges.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DataManipulation_ChangeSaveChangesChoiceToManual_WithDefaultSaveChangeChoise_ShouldSaveChangesAfterEveryOperation(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    DefaultSoftDeletionState = SoftDeletionState.Passive,
                };
                opt.Repository = new RepositoryConfiguration
                {
                    DefaultSaveChangesChoice = SaveChangesChoice.AfterEveryOperation,
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);
        entityRepository.ChangeSaveChangesChoice(SaveChangesChoice.Manual);

        // Act & Assert
        var dataBeforeUpdate = await entityRepository.GetByIdAsync(1);

        dataBeforeUpdate.SomeStringProp = "somestring11";
        dataBeforeUpdate.CreationDate.Should().NotBeNull();

        await entityRepository.UpdateAsync(dataBeforeUpdate);

        var dataAfterUpdate = await entityRepository.GetByIdAsync(1);

        dataAfterUpdate.SomeStringProp.Should().Be("somestring1");
        dataAfterUpdate.CreationDate.Should().NotBeNull();
        dataAfterUpdate.LastModificationDate.Should().BeNull();

        await entityRepository.DeleteAsync(dataAfterUpdate);

        var dataAfterDelete = await entityRepository.GetByIdAsync(1);
        dataAfterDelete.Should().NotBeNull();

        await entityRepository.SaveChangesAsync();

        var dataAfterSaveChanges = await entityRepository.GetByIdAsync(1);
        dataAfterSaveChanges.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DataManipulation_ResetSaveChangesChoiceToDefault_WithDefaultSaveChangeChoise_ShouldSaveChangesAfterEveryOperation(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    DefaultSoftDeletionState = SoftDeletionState.Passive,
                };
                opt.Repository = new RepositoryConfiguration
                {
                    DefaultSaveChangesChoice = SaveChangesChoice.AfterEveryOperation,
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);
        entityRepository.ChangeSaveChangesChoice(SaveChangesChoice.Manual);

        // Act & Assert
        var dataBeforeUpdate = await entityRepository.GetByIdAsync(1);

        dataBeforeUpdate.SomeStringProp = "somestring11";
        dataBeforeUpdate.CreationDate.Should().NotBeNull();

        await entityRepository.UpdateAsync(dataBeforeUpdate);

        var dataAfterUpdate = await entityRepository.GetByIdAsync(1);

        dataAfterUpdate.SomeStringProp.Should().Be("somestring1");
        dataAfterUpdate.CreationDate.Should().NotBeNull();
        dataAfterUpdate.LastModificationDate.Should().BeNull();

        entityRepository.ResetSaveChangesChoiceToDefault();

        await entityRepository.DeleteAsync(dataAfterUpdate);

        var dataAfterDelete = await entityRepository.GetByIdAsync(1);
        dataAfterDelete.Should().BeNull();
    }

    #region Soft Deletion

    [Fact]
    public async Task SoftDeletion_WithSoftDeletionIsPassiveAndEntityIsSoftDeletable_ShouldDeleteEntity()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Passive,
                };

            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString());
            });
        });

        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeBaseEntityFixture>>();
        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var entity = new SomeBaseEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop"
        };

        // Act & Assert
        await entityRepository.AddAsync(entity);
        var entityInDb = await entityRepository.GetFirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.Id.Should().Be(1);

        await entityRepository.DeleteAsync(entityInDb);
        var dataCount = await dbContext.BaseEntities.CountAsync();
        dataCount.Should().Be(0);
    }

    [Fact]
    public async Task SoftDeletion_WithSoftDeletionIsActiveAndEntityIsSoftDeletable_ShouldSoftDeleteEntity()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString());
            });
        });

        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeBaseEntityFixture>>();
        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var now = DateTime.UtcNow;
        var entity = new SomeBaseEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop"
        };

        // Act & Assert
        await entityRepository.AddAsync(entity);
        var entityInDb = await entityRepository.GetFirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.Id.Should().Be(1);

        await entityRepository.DeleteAsync(entityInDb);
        var dataCount = await dbContext.BaseEntities.CountAsync();
        dataCount.Should().Be(1);

        entityInDb = await entityRepository.GetFirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.IsDeleted.Should().BeTrue();
        entityInDb.DeletionDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SoftDeletion_WithSoftDeletionIsActiveAndResetStateIsTrue_ShouldApplySoftDeletionSecondProcess()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    ResetSoftDeleteStateAfterEveryOperation = true
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString());
            });
        });

        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeBaseEntityFixture>>();
        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var now = DateTime.UtcNow;
        var entities = new List<SomeBaseEntityFixture>
        {
             new() {
                 Id = 1,
                 SomeStringProp = "stringprop"
             },
             new() {
                 Id = 2,
                 SomeStringProp = "stringprop"
             }
        };

        // Act & Assert
        await entityRepository.AddRangeAsync(entities);
        var countAfterAdd = await dbContext.BaseEntities.CountAsync();
        countAfterAdd.Should().Be(2);

        dbContext.ChangeSoftDeletionState(SoftDeletionState.Passive);

        var firstEntity = await entityRepository.GetByIdAsync(1);
        await entityRepository.DeleteAsync(firstEntity);
        var dataCount = await dbContext.BaseEntities.CountAsync();
        dataCount.Should().Be(1);

        var secondEntity = await entityRepository.GetByIdAsync(2);
        await entityRepository.DeleteAsync(secondEntity);
        dataCount = await dbContext.BaseEntities.CountAsync();
        dataCount.Should().Be(1);

        var secondEntityAfterSoftDeletion = await entityRepository.GetByIdAsync(2);
        secondEntityAfterSoftDeletion.Should().NotBeNull();
        secondEntityAfterSoftDeletion.IsDeleted.Should().BeTrue();
        secondEntityAfterSoftDeletion.DeletionDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SoftDeletion_WithSoftDeletionIsActiveAndResetStateIsFalse_ShouldNotApplySoftDeletionSecondProcess()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    ResetSoftDeleteStateAfterEveryOperation = false
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString());
            });
        });

        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeBaseEntityFixture>>();
        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var entities = new List<SomeBaseEntityFixture>
        {
             new() {
                 Id = 1,
                 SomeStringProp = "stringprop"
             },
             new() {
                 Id = 2,
                 SomeStringProp = "stringprop"
             }
        };

        // Act & Assert
        await entityRepository.AddRangeAsync(entities);
        var countAfterAdd = await dbContext.BaseEntities.CountAsync();
        countAfterAdd.Should().Be(2);

        dbContext.ChangeSoftDeletionState(SoftDeletionState.Passive);

        var firstEntity = await entityRepository.GetByIdAsync(1);
        await entityRepository.DeleteAsync(firstEntity);
        var dataCount = await dbContext.BaseEntities.CountAsync();
        dataCount.Should().Be(1);

        var secondEntity = await entityRepository.GetByIdAsync(2);
        await entityRepository.DeleteAsync(secondEntity);
        dataCount = await dbContext.BaseEntities.CountAsync();
        dataCount.Should().Be(0);
    }

    [Fact]
    public async Task SoftDeletion_WithSoftDeletionIsActiveAndResetStateIsFalseButChangedInRuntime_ShouldApplySoftDeletionSecondProcess()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    ResetSoftDeleteStateAfterEveryOperation = false
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString());
            });
        });

        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeBaseEntityFixture>>();
        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var now = DateTime.UtcNow;
        var entities = new List<SomeBaseEntityFixture>
        {
             new() {
                 Id = 1,
                 SomeStringProp = "stringprop"
             },
             new() {
                 Id = 2,
                 SomeStringProp = "stringprop"
             }
        };

        // Act & Assert
        await entityRepository.AddRangeAsync(entities);
        var countAfterAdd = await dbContext.BaseEntities.CountAsync();
        countAfterAdd.Should().Be(2);

        entityRepository.ChangeSoftDeletionState(SoftDeletionState.Passive);
        entityRepository.SoftDeletionStateResetAfterOperation(true);

        var firstEntity = await entityRepository.GetByIdAsync(1);
        await entityRepository.DeleteAsync(firstEntity);
        var dataCount = await dbContext.BaseEntities.CountAsync();
        dataCount.Should().Be(1);

        var secondEntity = await entityRepository.GetByIdAsync(2);
        await entityRepository.DeleteAsync(secondEntity);
        dataCount = await dbContext.BaseEntities.CountAsync();
        dataCount.Should().Be(1);

        var secondEntityAfterSoftDeletion = await entityRepository.GetByIdAsync(2);
        secondEntityAfterSoftDeletion.Should().NotBeNull();
        secondEntityAfterSoftDeletion.IsDeleted.Should().BeTrue();
        secondEntityAfterSoftDeletion.DeletionDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetCurrentSoftDeletionState_ShouldReturnCorrectValue()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    ResetSoftDeleteStateAfterEveryOperation = false
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString());
            });
        });

        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeBaseEntityFixture>>();

        // Act & Assert
        var currentSoftDeleteState = entityRepository.GetCurrentSoftDeletionState();
        currentSoftDeleteState.Should().Be(SoftDeletionState.Active);
    }

    [Fact]
    public async Task ChangeSoftDeletionState_ShouldUpdateCorrectly()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    ResetSoftDeleteStateAfterEveryOperation = false
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString());
            });
        });

        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeBaseEntityFixture>>();

        // Act & Assert
        var currentSoftDeleteState = entityRepository.GetCurrentSoftDeletionState();
        currentSoftDeleteState.Should().Be(SoftDeletionState.Active);

        entityRepository.ChangeSoftDeletionState(SoftDeletionState.Passive);

        currentSoftDeleteState = entityRepository.GetCurrentSoftDeletionState();
        currentSoftDeleteState.Should().Be(SoftDeletionState.Passive);
    }

    [Fact]
    public async Task SetSoftDeletionStateToDefault_ShouldUpdateCorrectly()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    DefaultSoftDeletionState = SoftDeletionState.Active,
                    ResetSoftDeleteStateAfterEveryOperation = false
                };
            });

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString());
            });
        });

        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeBaseEntityFixture>>();

        // Act & Assert
        var currentSoftDeleteState = entityRepository.GetCurrentSoftDeletionState();
        currentSoftDeleteState.Should().Be(SoftDeletionState.Active);

        entityRepository.ChangeSoftDeletionState(SoftDeletionState.Passive);
        currentSoftDeleteState = entityRepository.GetCurrentSoftDeletionState();
        currentSoftDeleteState.Should().Be(SoftDeletionState.Passive);

        entityRepository.SetSoftDeletionStateToDefault();
        currentSoftDeleteState = entityRepository.GetCurrentSoftDeletionState();
        currentSoftDeleteState.Should().Be(SoftDeletionState.Active);
    }

    #endregion

    #endregion

    #region GetFirstOrDefaultAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetFirstOrDefaultAsync_WithoutParameters_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetFirstOrDefaultAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.SomeStringProp.Should().Be("somestring1");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetFirstOrDefaultAsync_WithCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });
        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetFirstOrDefaultAsync(i => i.SomeDecimalProp > 20M);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(3);
        result.SomeStringProp.Should().Be("somestring3");
        result.SomeDecimalProp.Should().Be(30M);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetFirstOrDefaultAsync_WithProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetFirstOrDefaultAsync(null, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            CreationDate = i.CreationDate,
        });

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.SomeStringProp.Should().BeNull();
        result.SomeDecimalProp.Should().Be(default);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetFirstOrDefaultAsync_WithConditionAndProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetFirstOrDefaultAsync(i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            CreationDate = i.CreationDate,
        });

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(3);
        result.SomeStringProp.Should().BeNull();
        result.SomeDecimalProp.Should().Be(default);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetFirstOrDefaultAsync_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetFirstOrDefaultAsync(i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 30M);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetSingleOrDefaultAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetSingleOrDefaultAsync_WithoutParameters_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        Func<Task> act = async () => await entityRepository.GetSingleOrDefaultAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Sequence contains more than one element.");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetSingleOrDefaultAsync_WithCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetSingleOrDefaultAsync(i => i.SomeDecimalProp > 20M);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(3);
        result.SomeStringProp.Should().Be("somestring3");
        result.SomeDecimalProp.Should().Be(30M);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetSingleOrDefaultAsync_WithConditionAndProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetSingleOrDefaultAsync(i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            CreationDate = i.CreationDate,
        });

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(3);
        result.SomeStringProp.Should().BeNull();
        result.SomeDecimalProp.Should().Be(default);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetSingleOrDefaultAsync_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetSingleOrDefaultAsync(i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 30M);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetByIdAsync_WithoutParameters_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.SomeStringProp.Should().Be("somestring1");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetByIdAsync_WithCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetByIdAsync(1, i => i.SomeDecimalProp > 20M);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetByIdAsync_WithConditionAndProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetByIdAsync(3, i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            CreationDate = i.CreationDate,
        });

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(3);
        result.SomeStringProp.Should().BeNull();
        result.SomeDecimalProp.Should().Be(default);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetByIdAsync_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetByIdAsync(3, condition: i => i.SomeDecimalProp > 20M, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, conditionAfterProjection: i => i.SomeDecimalProp > 30M);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetForDeleteAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetForDeleteAsync_IdOverload_WithoutParameters_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetForDeleteAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.ManyToOneEntities.Should().NotBeEmpty();
        result.ManyToOneEntities[0].SomeEntity.Should().BeNull();
        result.Id.Should().Be(1);
        result.SomeStringProp.Should().Be("somestring1");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetForDeleteAsync_IdOverload_WithCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetForDeleteAsync(1, condition: i => i.SomeDecimalProp > 20M);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetForDeleteAsync_IdOverload_WithInclude_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetForDeleteAsync(1, includes: i => i.Include(m => m.ManyToOneEntities));

        // Assert
        result.Should().NotBeNull();
        result.ManyToOneEntities.Should().NotBeEmpty();
        result.ManyToOneEntities[0].SomeEntity.Should().BeNull();
        result.Id.Should().Be(1);
        result.SomeStringProp.Should().Be("somestring1");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetForDeleteAsync_IdOverload_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetForDeleteAsync(1, condition: i => i.SomeDecimalProp >= 10M, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities,
            CreationDate = i.CreationDate,
        }, conditionAfterProjection: i => i.SomeDecimalProp > 9M);

        // Assert
        result.Should().NotBeNull();
        result.ManyToOneEntities.Should().NotBeEmpty();
        result.ManyToOneEntities[0].SomeEntity.Should().BeNull();
        result.Id.Should().Be(1);
        result.SomeDecimalProp.Should().Be(10M);
        result.SomeStringProp.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetForDeleteAsync_IdOverload_WithConditionAndProjectionAndConditionAfterProjectionAndWithInclude_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetForDeleteAsync(1, condition: i => i.SomeDecimalProp >= 10M, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities,
            CreationDate = i.CreationDate,
        }, conditionAfterProjection: i => i.SomeDecimalProp > 9M, includes: i => i.Include(m => m.ManyToOneEntities));

        // Assert
        result.Should().NotBeNull();
        result.ManyToOneEntities.Should().NotBeEmpty();
        result.ManyToOneEntities[0].SomeEntity.Should().BeNull();
        result.Id.Should().Be(1);
        result.SomeDecimalProp.Should().Be(10M);
        result.SomeStringProp.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetForDeleteAsync_WithoutParameters_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetForDeleteAsync(condition: i => i.Id == 1);

        // Assert
        result[0].Should().NotBeNull();
        result[0].ManyToOneEntities.Should().NotBeEmpty();
        result[0].ManyToOneEntities[0].SomeEntity.Should().BeNull();
        result[0].Id.Should().Be(1);
        result[0].SomeStringProp.Should().Be("somestring1");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetForDeleteAsync_WithCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetForDeleteAsync(condition: i => i.Id == 1 && i.SomeDecimalProp > 20M);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetForDeleteAsync_WithInclude_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetForDeleteAsync(condition: i => i.Id == 1, includes: i => i.Include(m => m.ManyToOneEntities));

        // Assert
        result[0].Should().NotBeNull();
        result[0].ManyToOneEntities.Should().NotBeEmpty();
        result[0].ManyToOneEntities[0].SomeEntity.Should().BeNull();
        result[0].Id.Should().Be(1);
        result[0].SomeStringProp.Should().Be("somestring1");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetForDeleteAsync_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetForDeleteAsync(condition: i => i.Id == 1 && i.SomeDecimalProp >= 10M, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities,
            CreationDate = i.CreationDate,
        }, conditionAfterProjection: i => i.SomeDecimalProp > 9M);

        // Assert
        result[0].Should().NotBeNull();
        result[0].ManyToOneEntities.Should().NotBeEmpty();
        result[0].ManyToOneEntities[0].SomeEntity.Should().BeNull();
        result[0].Id.Should().Be(1);
        result[0].SomeDecimalProp.Should().Be(10M);
        result[0].SomeStringProp.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetForDeleteAsync_WithConditionAndProjectionAndConditionAfterProjectionAndWithInclude_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetForDeleteAsync(condition: i => i.Id == 1 && i.SomeDecimalProp >= 10M, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities,
            CreationDate = i.CreationDate,
        }, conditionAfterProjection: i => i.SomeDecimalProp > 9M, includes: i => i.Include(m => m.ManyToOneEntities));

        // Assert
        result[0].Should().NotBeNull();
        result[0].ManyToOneEntities.Should().NotBeEmpty();
        result[0].ManyToOneEntities[0].SomeEntity.Should().BeNull();
        result[0].Id.Should().Be(1);
        result[0].SomeDecimalProp.Should().Be(10M);
        result[0].SomeStringProp.Should().BeNull();
    }

    #endregion

    #region GetAllAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetAllAsync_WithoutParameters_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetAllAsync_WithCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetAllAsync(i => i.SomeDecimalProp > 10M);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetAllAsync_WithConditionAndProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetAllAsync(i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        });

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].SomeStringProp.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetAllAsync_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetAllAsync(i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 20M);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].SomeStringProp.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetAllAsync_WithListRequest_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Sorting = new SortRequest { SortBy = nameof(SomeFullAuditableEntityFixture.SomeDecimalProp), Type = SortType.Desc },
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(SomeFullAuditableEntityFixture.SomeDecimalProp), Type = AggregationType.Sum }] }
        };

        // Act 
        var result = await entityRepository.GetAllAsync(listRequest);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.Should().BeInDescendingOrder(i => i.SomeDecimalProp);
        result.Data[0].SomeStringProp.Should().NotBeNull();
        result.TotalDataCount.Should().Be(3);
        result.TotalPageCount.Should().Be(2);
        result.CurrentPageNumber.Should().Be(1);
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.AggregationResults.Should().HaveCount(1);
        result.AggregationResults[0].Result.Should().Be(60M);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetAllAsync_WithListRequestAndCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Sorting = new SortRequest { SortBy = nameof(SomeFullAuditableEntityFixture.CreationDate), Type = SortType.Desc },
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(SomeFullAuditableEntityFixture.SomeDecimalProp), Type = AggregationType.Sum }] }
        };

        // Act 
        var result = await entityRepository.GetAllAsync(listRequest, i => i.SomeDecimalProp > 10M);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.Should().BeInDescendingOrder(i => i.CreationDate);
        result.Data[0].SomeStringProp.Should().NotBeNull();
        result.TotalDataCount.Should().Be(2);
        result.TotalPageCount.Should().Be(1);
        result.CurrentPageNumber.Should().Be(1);
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.AggregationResults.Should().HaveCount(1);
        result.AggregationResults[0].Result.Should().Be(50M);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetAllAsync_WithListRequestAndConditionAndProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Sorting = new SortRequest { SortBy = nameof(SomeFullAuditableEntityFixture.CreationDate), Type = SortType.Desc },
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(SomeFullAuditableEntityFixture.SomeDecimalProp), Type = AggregationType.Sum }] }
        };

        // Act 
        var result = await entityRepository.GetAllAsync(listRequest, i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        });

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.Should().BeInDescendingOrder(i => i.CreationDate);
        result.Data[0].SomeStringProp.Should().BeNull();
        result.TotalDataCount.Should().Be(2);
        result.TotalPageCount.Should().Be(1);
        result.CurrentPageNumber.Should().Be(1);
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.AggregationResults.Should().HaveCount(1);
        result.AggregationResults[0].Result.Should().Be(50M);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetAllAsync_WithHasTenantIdEntityListRequestAndConditionAndProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeMultiTenantTestEntityFixture>>();
        var entities = new List<SomeMultiTenantTestEntityFixture>
        {
            new() {
                Id = 1,
                SomeDateProp = DateTime.Now.AddYears(1),
                SomeDecimalProp = 10M,
                TenantId = new Core.EntityBases.MultiTenancy.TenantId("milva_1")
            },
            new() {
                Id = 2,
                SomeDateProp = DateTime.Now.AddYears(2),
                SomeDecimalProp = 20M,
                TenantId = new Core.EntityBases.MultiTenancy.TenantId("milva_1")
            },
            new() {
                Id = 3,
                SomeDateProp = DateTime.Now.AddYears(3),
                SomeDecimalProp = 30M,
                TenantId = new Core.EntityBases.MultiTenancy.TenantId("milva_1")
            },
            new() {
                Id = 4,
                SomeDateProp = DateTime.Now.AddYears(4),
                SomeDecimalProp = 40M,
                DeletionDate = DateTime.Now.AddDays(4).AddYears(4),
                TenantId = new Core.EntityBases.MultiTenancy.TenantId("milva_1"),
                IsDeleted = true,
            }
        };

        await dbContext.SomeMultiTenantEntities.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
        };

        // Act 
        var result = await entityRepository.GetAllAsync(listRequest, i => i.SomeDecimalProp > 10M, i => new SomeMultiTenantTestEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        });

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data[0].TenantId.Should().Be(new Core.EntityBases.MultiTenancy.TenantId("milva_1"));
        result.Data[1].TenantId.Should().Be(new Core.EntityBases.MultiTenancy.TenantId("milva_1"));
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetAllAsync_WithListRequestAndConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Sorting = new SortRequest { SortBy = nameof(SomeFullAuditableEntityFixture.CreationDate), Type = SortType.Desc },
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(SomeFullAuditableEntityFixture.SomeDecimalProp), Type = AggregationType.Sum }] }
        };

        // Act 
        var result = await entityRepository.GetAllAsync(listRequest, i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 20M);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        result.Data.Should().BeInDescendingOrder(i => i.CreationDate);
        result.Data[0].SomeStringProp.Should().BeNull();
        result.TotalDataCount.Should().Be(1);
        result.TotalPageCount.Should().Be(1);
        result.CurrentPageNumber.Should().Be(1);
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.AggregationResults.Should().HaveCount(1);
        result.AggregationResults[0].Result.Should().Be(30M);
    }

    #endregion

    #region GetSomeAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetSomeAsync_WithoutParameters_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetSomeAsync(2);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetSomeAsync_WithCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetSomeAsync(1, i => i.SomeDecimalProp > 10M);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetSomeAsync_WithConditionAndProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetSomeAsync(1, i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        });

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].SomeStringProp.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetSomeAsync_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        var result = await entityRepository.GetSomeAsync(3, i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 20M);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].SomeStringProp.Should().BeNull();
    }

    #endregion

    #region AddAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task AddAsync_WithNullEntity_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();

        SomeFullAuditableEntityFixture entity = null;

        // Act 
        Func<Task> act = async () => await entityRepository.AddAsync(entity);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task AddAsync_WithValidEntity_ShouldAddCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();

        var entity = new SomeFullAuditableEntityFixture()
        {
            Id = 1,
            SomeDateProp = DateTime.Now.AddYears(1),
            SomeDecimalProp = 10M,
            SomeStringProp = "somestring1"
        };

        // Act 
        await entityRepository.AddAsync(entity);
        var addedEntity = await entityRepository.GetByIdAsync(1);

        // Assert
        addedEntity.Should().NotBeNull();
        addedEntity.CreationDate.Should().NotBeNull();
    }

    #endregion

    #region AddRangeAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task AddRangeAsync_WithNullEntityList_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();

        List<SomeFullAuditableEntityFixture> entities = null;

        // Act 
        await entityRepository.AddRangeAsync(entities);
        var count = await dbContext.FullAuditableEntities.CountAsync();

        // Assert
        count.Should().Be(0);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task AddRangeAsync_WithValidEntity_ShouldAddCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();

        var entities = new List<SomeFullAuditableEntityFixture>
        {
            new() {
                Id = 1,
                SomeDateProp = DateTime.Now.AddYears(1),
                SomeDecimalProp = 10M,
                SomeStringProp = "somestring1"
            },
            new() {
                Id = 2,
                SomeDateProp = DateTime.Now.AddYears(2),
                SomeDecimalProp = 20M,
                SomeStringProp = "somestring2"
            }
        };

        // Act 
        await entityRepository.AddRangeAsync(entities);
        var count = await dbContext.FullAuditableEntities.CountAsync();
        var addedEntity = await entityRepository.GetByIdAsync(1);

        // Assert
        count.Should().Be(2);
        addedEntity.Should().NotBeNull();
        addedEntity.CreationDate.Should().NotBeNull();
    }

    #endregion

    #region UpdateAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task UpdateAsync_ForSingleEntity_WithNullEntity_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        SomeFullAuditableEntityFixture entity = null;

        // Act 
        Func<Task> act = async () => await entityRepository.UpdateAsync(entity);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task UpdateAsync_ForSingleEntity_WithNotExistsEntity_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        var entity = new SomeFullAuditableEntityFixture()
        {
            Id = 7,
            SomeDateProp = DateTime.Now.AddYears(1),
            SomeDecimalProp = 10M,
            SomeStringProp = "somestring1"
        };

        // Act 
        Func<Task> act = async () => await entityRepository.UpdateAsync(entity);

        // Assert
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task UpdateAsync_ForSingleEntity_WithValidEntity_ShouldUpdateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        var entity = await entityRepository.GetByIdAsync(1);

        // Act 
        entity.SomeStringProp = "stringpropupdated";
        await entityRepository.UpdateAsync(entity);

        // Assert
        var entityAfterUpdate = await entityRepository.GetByIdAsync(1);
        entityAfterUpdate.SomeStringProp.Should().Be("stringpropupdated");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task UpdateAsync_ForEntityList_WithNullEntityList_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        List<SomeFullAuditableEntityFixture> entities = null;

        // Act 
        await entityRepository.UpdateAsync(entities);
        var allEntities = await dbContext.FullAuditableEntities.ToListAsync();

        // Assert
        allEntities[0].LastModificationDate.Should().BeNull();
        allEntities[1].LastModificationDate.Should().BeNull();
        allEntities[2].LastModificationDate.Should().BeNull();
        allEntities[3].LastModificationDate.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task UpdateAsync_ForEntityList_WithNotExistsEntities_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        List<SomeFullAuditableEntityFixture> entities =
        [
            new SomeFullAuditableEntityFixture()
            {
                Id = 7,
                SomeDateProp = DateTime.Now.AddYears(1),
                SomeDecimalProp = 10M,
                SomeStringProp = "somestring1"
            },
            new SomeFullAuditableEntityFixture()
            {
                Id = 8,
                SomeDateProp = DateTime.Now.AddYears(1),
                SomeDecimalProp = 10M,
                SomeStringProp = "somestring1"
            }
        ];

        // Act 
        Func<Task> act = async () => await entityRepository.UpdateAsync(entities);

        // Assert
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task UpdateAsync_ForEntityList_WithValidEntity_ShouldUpdateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        var entities = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);

        // Act 
        entities[0].SomeStringProp = "stringpropupdated";
        entities[1].SomeStringProp = "stringpropupdated";
        await entityRepository.UpdateAsync(entities);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("stringpropupdated");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task UpdateAsync_ForSingleEntityAndProjectionPropertiesOverload_WithNullEntity_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        SomeFullAuditableEntityFixture entity = null;
        Expression<Func<SomeFullAuditableEntityFixture, object>> projection = i => i.SomeStringProp;
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = [projection];

        // Act 
        Func<Task> act = async () => await entityRepository.UpdateAsync(entity, default, projections);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task UpdateAsync_ForSingleEntityAndProjectionPropertiesOverload_WithNotExistsEntity_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        var entity = new SomeFullAuditableEntityFixture()
        {
            Id = 7,
            SomeDateProp = DateTime.Now.AddYears(1),
            SomeDecimalProp = 10M,
            SomeStringProp = "somestring1"
        };
        Expression<Func<SomeFullAuditableEntityFixture, object>> projection = i => i.SomeStringProp;
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = [projection];

        // Act 
        Func<Task> act = async () => await entityRepository.UpdateAsync(entity, default, projections);

        // Assert
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task UpdateAsync_ForSingleEntityAndProjectionPropertiesOverload_WithValidEntityButPropertySelectorsIsNull_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        var entity = await entityRepository.GetByIdAsync(1);
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = null;

        // Act 
        entity.SomeStringProp = "stringpropupdated";
        entity.SomeDecimalProp = 20M;
        await entityRepository.UpdateAsync(entity, default, projections);

        // Assert
        var entityAfterUpdate = await entityRepository.GetByIdAsync(1);
        entityAfterUpdate.SomeStringProp.Should().Be("somestring1");
        entityAfterUpdate.SomeDecimalProp.Should().Be(10);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task UpdateAsync_ForSingleEntityAndProjectionPropertiesOverload_WithValidEntity_ShouldUpdateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        var entity = await entityRepository.GetByIdAsync(1);
        Expression<Func<SomeFullAuditableEntityFixture, object>> projection = i => i.SomeStringProp;
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = [projection];

        // Act 
        entity.SomeStringProp = "stringpropupdated";
        entity.SomeDecimalProp = 20M;
        await entityRepository.UpdateAsync(entity, default, projections);

        // Assert
        var entityAfterUpdate = await entityRepository.GetByIdAsync(1);
        entityAfterUpdate.SomeStringProp.Should().Be("stringpropupdated");
        entityAfterUpdate.SomeDecimalProp.Should().Be(10);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task UpdateAsync_ForEntityListAndProjectionPropertiesOverload_WithNullEntities_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        List<SomeFullAuditableEntityFixture> entities = null;
        Expression<Func<SomeFullAuditableEntityFixture, object>> projection = i => i.SomeStringProp;
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = [projection];

        // Act 
        var allEntities = await dbContext.FullAuditableEntities.ToListAsync();
        await entityRepository.UpdateAsync(entities, default, projections);

        // Assert
        allEntities[0].LastModificationDate.Should().BeNull();
        allEntities[1].LastModificationDate.Should().BeNull();
        allEntities[2].LastModificationDate.Should().BeNull();
        allEntities[3].LastModificationDate.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task UpdateAsync_ForEntityListAndProjectionPropertiesOverload_WithNotExistsEntities_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        List<SomeFullAuditableEntityFixture> entities =
        [
            new SomeFullAuditableEntityFixture()
                    {
                        Id = 7,
                        SomeDateProp = DateTime.Now.AddYears(1),
                        SomeDecimalProp = 10M,
                        SomeStringProp = "somestring1"
                    },
                    new SomeFullAuditableEntityFixture()
                    {
                        Id = 8,
                        SomeDateProp = DateTime.Now.AddYears(1),
                        SomeDecimalProp = 10M,
                        SomeStringProp = "somestring1"
                    }
        ];
        Expression<Func<SomeFullAuditableEntityFixture, object>> projection = i => i.SomeStringProp;
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = [projection];

        // Act 
        Func<Task> act = async () => await entityRepository.UpdateAsync(entities, default, projections);

        // Assert
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task UpdateAsync_ForEntityListAndProjectionPropertiesOverload_WithValidEntitiesButPropertySelectorsIsNull_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        var entities = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = null;

        // Act 
        entities[0].SomeStringProp = "stringpropupdated";
        entities[0].SomeDecimalProp = 20M;
        entities[1].SomeStringProp = "stringpropupdated";
        entities[1].SomeDecimalProp = 20M;
        await entityRepository.UpdateAsync(entities, default, projections);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("somestring1");
        entitiesAfterUpdate[0].SomeDecimalProp.Should().Be(10M);
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("somestring2");
        entitiesAfterUpdate[1].SomeDecimalProp.Should().Be(20M);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task UpdateAsync_ForEntityListAndProjectionPropertiesOverload_WithValidEntities_ShouldUpdateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        var entities = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        Expression<Func<SomeFullAuditableEntityFixture, object>> projection = i => i.SomeStringProp;
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = [projection];

        // Act 
        entities[0].SomeStringProp = "stringpropupdated";
        entities[0].SomeDecimalProp = 20M;
        entities[1].SomeStringProp = "stringpropupdated";
        entities[1].SomeDecimalProp = 20M;
        await entityRepository.UpdateAsync(entities, default, projections);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[0].SomeDecimalProp.Should().Be(10M);
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[1].SomeDecimalProp.Should().Be(20M);
    }

    #endregion

    #region DeleteAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DeleteAsync_ForSingleEntity_WithNullEntity_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        SomeFullAuditableEntityFixture entity = null;

        // Act 
        Func<Task> act = async () => await entityRepository.DeleteAsync(entity);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DeleteAsync_ForSingleEntity_WithNotExistsEntity_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        var entity = new SomeFullAuditableEntityFixture()
        {
            Id = 7,
            SomeDateProp = DateTime.Now.AddYears(1),
            SomeDecimalProp = 10M,
            SomeStringProp = "somestring1"
        };

        // Act 
        Func<Task> act = async () => await entityRepository.DeleteAsync(entity);

        // Assert
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DeleteAsync_ForSingleEntity_WithValidEntity_ShouldDeleteCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        var entity = await entityRepository.GetByIdAsync(1);

        // Act 
        await entityRepository.DeleteAsync(entity);

        // Assert
        var entityAfterUpdate = await entityRepository.GetByIdAsync(1);
        entityAfterUpdate.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DeleteAsync_ForEntityList_WithNullEntityList_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        List<SomeFullAuditableEntityFixture> entities = null;

        // Act 
        await entityRepository.DeleteAsync(entities);
        var allEntities = await dbContext.FullAuditableEntities.ToListAsync();

        // Assert
        allEntities[0].Should().NotBeNull();
        allEntities[1].Should().NotBeNull();
        allEntities[2].Should().NotBeNull();
        allEntities[3].Should().NotBeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DeleteAsync_ForEntityList_WithNotExistsEntities_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        List<SomeFullAuditableEntityFixture> entities =
        [
            new SomeFullAuditableEntityFixture()
            {
                Id = 7,
                SomeDateProp = DateTime.Now.AddYears(1),
                SomeDecimalProp = 10M,
                SomeStringProp = "somestring1"
            },
            new SomeFullAuditableEntityFixture()
            {
                Id = 8,
                SomeDateProp = DateTime.Now.AddYears(1),
                SomeDecimalProp = 10M,
                SomeStringProp = "somestring1"
            }
        ];

        // Act 
        Func<Task> act = async () => await entityRepository.DeleteAsync(entities);

        // Assert
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task DeleteAsync_ForEntityList_WithValidEntity_ShouldDeleteCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        var entities = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);

        // Act 
        await entityRepository.DeleteAsync(entities);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        entitiesAfterUpdate.Should().BeEmpty();
    }

    #endregion

    #region ReplaceOldsWithNewsAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task ReplaceOldsWithNewsAsync_WithValidParameters_ShouldOperateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        var entities = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        var newEntities = entities.ToList();
        newEntities[0].Id = 5;
        newEntities[0].SomeStringProp = "stringpropupdated";
        newEntities[1].Id = 6;
        newEntities[1].SomeStringProp = "stringpropupdated";

        // Act 
        await entityRepository.ReplaceOldsWithNewsAsync(entities, newEntities);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 5 || i.Id == 6);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("stringpropupdated");
    }

    #endregion

    #region ReplaceOldsWithNewsInSeperateDatabaseProcessAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task ReplaceOldsWithNewsInSeperateDatabaseProcessAsync_WithValidParameters_ShouldOperateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        var entities = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        var newEntities = entities.ToList();
        foreach (var entity in newEntities)
            entity.SomeStringProp = "stringpropupdated";

        // Act 
        await entityRepository.ReplaceOldsWithNewsInSeperateDatabaseProcessAsync(entities, newEntities);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("stringpropupdated");
    }

    #endregion

    #region RemoveAll

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task RemoveAll_EmptyDatabase_ShouldRemoveCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();

        // Act 
        await entityRepository.RemoveAllAsync();

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync();
        entitiesAfterUpdate.Should().BeEmpty();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task RemoveAll_ShouldRemoveCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                        {
                            x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                            x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
                        });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await SeedAsync(dbContext);

        // Act 
        await entityRepository.RemoveAllAsync();

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync();
        entitiesAfterUpdate.Should().BeEmpty();
    }

    #endregion

    #region Setup

    public interface ISomeGenericRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, IMilvaEntity
    {
    }

    public class SomeGenericRepository<TEntity>(MilvaBulkDbContextFixture dbContext) : BaseRepository<TEntity, MilvaBulkDbContextFixture>(dbContext), ISomeGenericRepository<TEntity>
         where TEntity : class, IMilvaEntity
    {
    }

    private static async Task SeedAsync(MilvaBulkDbContextFixture dbContextFixture)
    {
        var entities = new List<SomeFullAuditableEntityFixture>
        {
            new() {
                Id = 1,
                SomeDateProp = DateTime.Now.AddYears(1),
                SomeDecimalProp = 10M,
                SomeStringProp = "somestring1",
                ManyToOneEntities =
                [
                    new() { Id = 1, SomeStringProp = "somestring1", SomeEntity = new SomeEntityFixture{ SomeDecimalProp = 10 } },
                    new() { Id = 2, SomeStringProp = "somestring2", IsDeleted = true, SomeEntity = new SomeEntityFixture{ SomeDecimalProp = 20 } },
                ]
            },
            new() {
                Id = 2,
                SomeDateProp = DateTime.Now.AddYears(2),
                SomeDecimalProp = 20M,
                SomeStringProp = "somestring2"
            },
            new() {
                Id = 3,
                SomeDateProp = DateTime.Now.AddYears(3),
                SomeDecimalProp = 30M,
                SomeStringProp = "somestring3"
            },
            new() {
                Id = 4,
                SomeDateProp = DateTime.Now.AddYears(4),
                SomeDecimalProp = 40M,
                SomeStringProp = "somestring4",
                DeletionDate = DateTime.Now.AddDays(4).AddYears(4),
                IsDeleted = true,
            }
        };

        await dbContextFixture.FullAuditableEntities.AddRangeAsync(entities);
        await dbContextFixture.SaveChangesAsync();
    }

    #endregion
}
