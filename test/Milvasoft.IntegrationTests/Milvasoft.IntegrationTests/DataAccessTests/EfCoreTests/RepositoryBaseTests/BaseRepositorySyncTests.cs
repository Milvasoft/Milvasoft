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
[Trait("RepositoryBase Sync Integration Tests", "Integration tests for Milvasoft.DataAccess.EfCore integration tests.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S6966:Awaitable method should be used", Justification = "<Pending>")]
#pragma warning disable AsyncFixer02 // Long-running or blocking operations inside an async method
public class BaseRepositorySyncTests(CustomWebApplicationFactory factory) : DataAccessIntegrationTestBase(factory)
{
    public override Task InitializeAsync(Action<IServiceCollection> configureServices = null, Action<IApplicationBuilder> configureApp = null)
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

                services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                {
                    x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                    x.UseNpgsql(_factory.GetConnectionString());
                });

                services.AddScoped(typeof(ISomeGenericRepository<>), typeof(SomeGenericRepository<>));
            });

            builder.Configure(app =>
            {
                configureApp?.Invoke(app);
            });
        });

        _serviceProvider = waf.Services.CreateScope().ServiceProvider;

        return _factory.CreateRespawner();
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
        var getAllData = entityRepository.GetAll();
        var getSomeData = entityRepository.GetSome(4);
        var firstData = entityRepository.GetFirstOrDefault(i => i.Id == 4);
        var singleData = entityRepository.GetSingleOrDefault(i => i.Id == 4);
        var getByIdData = entityRepository.GetById(4);

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
        var getAllData = entityRepository.GetAll(projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var getSomeData = entityRepository.GetSome(4, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var firstData = entityRepository.GetFirstOrDefault(i => i.Id == 1, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var singleData = entityRepository.GetSingleOrDefault(i => i.Id == 1, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var getByIdData = entityRepository.GetById(1, projection: i => new SomeFullAuditableEntityFixture
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
        var getAllData = entityRepository.GetAll(projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var getSomeData = entityRepository.GetSome(4, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var firstData = entityRepository.GetFirstOrDefault(i => i.Id == 1, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var singleData = entityRepository.GetSingleOrDefault(i => i.Id == 1, projection: i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeStringProp = i.SomeStringProp,
            SomeDecimalProp = i.SomeDecimalProp,
            ManyToOneEntities = i.ManyToOneEntities
        });
        var getByIdData = entityRepository.GetById(1, projection: i => new SomeFullAuditableEntityFixture
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
        var getAllData = entityRepository.GetAll();
        var getSomeData = entityRepository.GetSome(4);
        var firstData = entityRepository.GetFirstOrDefault(i => i.Id == 4);
        var singleData = entityRepository.GetSingleOrDefault(i => i.Id == 4);
        var getByIdData = entityRepository.GetById(4);

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
        var getAllData = entityRepository.GetAll();
        entityRepository.ResetSoftDeletedEntityFetchResetState();
        var getSomeData = entityRepository.GetSome(4);
        var firstData = entityRepository.GetFirstOrDefault(i => i.Id == 4);
        var singleData = entityRepository.GetSingleOrDefault(i => i.Id == 4);
        var getByIdData = entityRepository.GetById(4);

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
        var getAllData = entityRepository.GetAll();
        var getSomeData = entityRepository.GetSome(4);
        var firstData = entityRepository.GetFirstOrDefault(i => i.Id == 4);
        var singleData = entityRepository.GetSingleOrDefault(i => i.Id == 4);
        var getByIdData = entityRepository.GetById(4);

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
        var getAllData = entityRepository.GetAll();
        var getSomeData = entityRepository.GetSome(4);
        var firstData = entityRepository.GetFirstOrDefault(i => i.Id == 4);
        var singleData = entityRepository.GetSingleOrDefault(i => i.Id == 4);
        var getByIdData = entityRepository.GetById(4);

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
        var getAllData = entityRepository.GetAll();
        var getSomeData = entityRepository.GetSome(4);
        var firstData = entityRepository.GetFirstOrDefault(i => i.Id == 4);
        var singleData = entityRepository.GetSingleOrDefault(i => i.Id == 4);
        var getByIdData = entityRepository.GetById(4);

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
        var dataBeforeUpdate = entityRepository.GetById(1);

        dataBeforeUpdate.SomeStringProp = "somestring11";
        dataBeforeUpdate.CreationDate.Should().NotBeNull();

        entityRepository.Update(dataBeforeUpdate);

        var dataAfterUpdate = entityRepository.GetById(1);

        dataAfterUpdate.SomeStringProp.Should().Be("somestring11");
        dataAfterUpdate.CreationDate.Should().NotBeNull();
        dataAfterUpdate.LastModificationDate.Should().NotBeNull();

        entityRepository.Delete(dataAfterUpdate);

        var dataAfterDelete = entityRepository.GetById(1);

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
        var dataBeforeUpdate = entityRepository.GetById(1);

        dataBeforeUpdate.SomeStringProp = "somestring11";
        dataBeforeUpdate.CreationDate.Should().NotBeNull();

        entityRepository.Update(dataBeforeUpdate);

        var dataAfterUpdate = entityRepository.GetById(1);

        dataAfterUpdate.SomeStringProp.Should().Be("somestring1");
        dataAfterUpdate.CreationDate.Should().NotBeNull();
        dataAfterUpdate.LastModificationDate.Should().BeNull();

        entityRepository.Delete(dataAfterUpdate);

        var dataAfterDelete = entityRepository.GetById(1);
        dataAfterDelete.Should().NotBeNull();

        entityRepository.SaveChanges();

        var dataAfterSaveChanges = entityRepository.GetById(1);
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
        var dataBeforeUpdate = entityRepository.GetById(1);

        dataBeforeUpdate.SomeStringProp = "somestring11";
        dataBeforeUpdate.CreationDate.Should().NotBeNull();

        entityRepository.Update(dataBeforeUpdate);

        var dataAfterUpdate = entityRepository.GetById(1);

        dataAfterUpdate.SomeStringProp.Should().Be("somestring1");
        dataAfterUpdate.CreationDate.Should().NotBeNull();
        dataAfterUpdate.LastModificationDate.Should().BeNull();

        entityRepository.Delete(dataAfterUpdate);

        var dataAfterDelete = entityRepository.GetById(1);
        dataAfterDelete.Should().NotBeNull();

        entityRepository.SaveChanges();

        var dataAfterSaveChanges = entityRepository.GetById(1);
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
        var dataBeforeUpdate = entityRepository.GetById(1);

        dataBeforeUpdate.SomeStringProp = "somestring11";
        dataBeforeUpdate.CreationDate.Should().NotBeNull();

        entityRepository.Update(dataBeforeUpdate);

        var dataAfterUpdate = entityRepository.GetById(1);

        dataAfterUpdate.SomeStringProp.Should().Be("somestring1");
        dataAfterUpdate.CreationDate.Should().NotBeNull();
        dataAfterUpdate.LastModificationDate.Should().BeNull();

        entityRepository.ResetSaveChangesChoiceToDefault();

        entityRepository.Delete(dataAfterUpdate);

        var dataAfterDelete = entityRepository.GetById(1);
        dataAfterDelete.Should().BeNull();
    }

    #endregion

    #region GetFirstOrDefault

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetFirstOrDefault_WithoutParameters_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetFirstOrDefault();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.SomeStringProp.Should().Be("somestring1");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetFirstOrDefault_WithCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetFirstOrDefault(i => i.SomeDecimalProp > 20M);

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
    public async Task GetFirstOrDefault_WithProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetFirstOrDefault(null, i => new SomeFullAuditableEntityFixture
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
    public async Task GetFirstOrDefault_WithConditionAndProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetFirstOrDefault(i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
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
    public async Task GetFirstOrDefault_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetFirstOrDefault(i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp < 20M);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetSingleOrDefault

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetSingleOrDefault_WithoutParameters_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        Action act = () => entityRepository.GetSingleOrDefault();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Sequence contains more than one element");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetSingleOrDefault_WithCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetSingleOrDefault(i => i.SomeDecimalProp > 20M);

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
    public async Task GetSingleOrDefault_WithConditionAndProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetSingleOrDefault(i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
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
    public async Task GetSingleOrDefault_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetSingleOrDefault(i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp < 30M);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetById

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetById_WithoutParameters_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetById(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.SomeStringProp.Should().Be("somestring1");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetById_WithCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetById(1, i => i.SomeDecimalProp > 20M);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetById_WithConditionAndProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetById(3, i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
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
    public async Task GetById_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetById(3, i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 30M);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetForDelete

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetForDelete_IdOverload_WithoutParameters_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetForDelete(1);

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
    public async Task GetForDelete_IdOverload_WithCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetForDelete(1, condition: i => i.SomeDecimalProp > 20M);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetForDelete_IdOverload_WithInclude_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetForDelete(1, includes: i => i.Include(m => m.ManyToOneEntities));

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
    public async Task GetForDelete_IdOverload_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetForDelete(1, condition: i => i.SomeDecimalProp >= 10M, projection: i => new SomeFullAuditableEntityFixture
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
        var result = entityRepository.GetForDelete(1, condition: i => i.SomeDecimalProp >= 10M, projection: i => new SomeFullAuditableEntityFixture
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
    public async Task GetForDelete_WithoutParameters_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetForDelete(condition: i => i.Id == 1);

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
    public async Task GetForDelete_WithCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetForDelete(condition: i => i.Id == 1 && i.SomeDecimalProp > 20M);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetForDelete_WithInclude_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetForDelete(condition: i => i.Id == 1, includes: i => i.Include(m => m.ManyToOneEntities));

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
    public async Task GetForDelete_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetForDelete(condition: i => i.Id == 1 && i.SomeDecimalProp >= 10M, projection: i => new SomeFullAuditableEntityFixture
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
        var result = entityRepository.GetForDelete(condition: i => i.Id == 1 && i.SomeDecimalProp >= 10M, projection: i => new SomeFullAuditableEntityFixture
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

    #region GetAll

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetAll_WithoutParameters_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetAll();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetAll_WithCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetAll(i => i.SomeDecimalProp > 10M);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetAll_WithConditionAndProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetAll(i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
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
    public async Task GetAll_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetAll(i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
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
    public async Task GetAll_WithListRequest_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetAll(listRequest);

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
    public async Task GetAll_WithListRequestAndCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetAll(listRequest, i => i.SomeDecimalProp > 10M);

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
    public async Task GetAll_WithListRequestAndConditionAndProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetAll(listRequest, i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
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
    public async Task GetAll_WithListRequestAndConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetAll(listRequest, i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
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

    #region GetSome

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetSome_WithoutParameters_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetSome(2);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetSome_WithCondition_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetSome(1, i => i.SomeDecimalProp > 10M);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task GetSome_WithConditionAndProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetSome(1, i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
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
    public async Task GetSome_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult(QueryTrackingBehavior queryTrackingBehavior)
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
        var result = entityRepository.GetSome(3, i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
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

    #region Add

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Add_WithNullEntity_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
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
        Action act = () => entityRepository.Add(entity);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Add_WithValidEntity_ShouldAddCorrectly(QueryTrackingBehavior queryTrackingBehavior)
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
        entityRepository.Add(entity);
        var addedEntity = entityRepository.GetById(1);

        // Assert
        addedEntity.Should().NotBeNull();
        addedEntity.CreationDate.Should().NotBeNull();
    }

    #endregion

    #region AddRange

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task AddRange_WithNullEntityList_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
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
        entityRepository.AddRange(entities);
        var count = dbContext.FullAuditableEntities.Count();

        // Assert
        count.Should().Be(0);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task AddRange_WithValidEntity_ShouldAddCorrectly(QueryTrackingBehavior queryTrackingBehavior)
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
        entityRepository.AddRange(entities);
        var count = dbContext.FullAuditableEntities.Count();
        var addedEntity = entityRepository.GetById(1);

        // Assert
        count.Should().Be(2);
        addedEntity.Should().NotBeNull();
        addedEntity.CreationDate.Should().NotBeNull();
    }

    #endregion

    #region Update

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Update_ForSingleEntity_WithNullEntity_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
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
        Action act = () => entityRepository.Update(entity);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Update_ForSingleEntity_WithNotExistsEntity_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
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
        Action act = () => entityRepository.Update(entity);

        // Assert
        act.Should().Throw<DbUpdateConcurrencyException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Update_ForSingleEntity_WithValidEntity_ShouldUpdateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
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

        var entity = entityRepository.GetById(1);

        // Act 
        entity.SomeStringProp = "stringpropupdated";
        entityRepository.Update(entity);

        // Assert
        var entityAfterUpdate = entityRepository.GetById(1);
        entityAfterUpdate.SomeStringProp.Should().Be("stringpropupdated");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Update_ForEntityList_WithNullEntityList_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
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
        entityRepository.Update(entities);
        var allEntities = dbContext.FullAuditableEntities.ToList();

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
    public async Task Update_ForEntityList_WithNotExistsEntities_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
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
        Action act = () => entityRepository.Update(entities);

        // Assert
        act.Should().Throw<DbUpdateConcurrencyException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Update_ForEntityList_WithValidEntity_ShouldUpdateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
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

        var entities = entityRepository.GetAll(i => i.Id == 1 || i.Id == 2);

        // Act 
        entities[0].SomeStringProp = "stringpropupdated";
        entities[1].SomeStringProp = "stringpropupdated";
        entityRepository.Update(entities);

        // Assert
        var entitiesAfterUpdate = entityRepository.GetAll(i => i.Id == 1 || i.Id == 2);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("stringpropupdated");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Update_ForSingleEntityAndProjectionPropertiesOverload_WithNullEntity_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
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
        Action act = () => entityRepository.Update(entity, projections);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Update_ForSingleEntityAndProjectionPropertiesOverload_WithNotExistsEntity_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
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
        Action act = () => entityRepository.Update(entity, projections);

        // Assert
        act.Should().Throw<DbUpdateConcurrencyException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Update_ForSingleEntityAndProjectionPropertiesOverload_WithValidEntityButPropertySelectorsIsNull_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
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

        var entity = entityRepository.GetById(1);
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = null;

        // Act 
        entity.SomeStringProp = "stringpropupdated";
        entity.SomeDecimalProp = 20M;
        entityRepository.Update(entity, projections);

        // Assert
        var entityAfterUpdate = entityRepository.GetById(1);
        entityAfterUpdate.SomeStringProp.Should().Be("somestring1");
        entityAfterUpdate.SomeDecimalProp.Should().Be(10);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Update_ForSingleEntityAndProjectionPropertiesOverload_WithValidEntity_ShouldUpdateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
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

        var entity = entityRepository.GetById(1);
        Expression<Func<SomeFullAuditableEntityFixture, object>> projection = i => i.SomeStringProp;
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = [projection];

        // Act 
        entity.SomeStringProp = "stringpropupdated";
        entity.SomeDecimalProp = 20M;
        entityRepository.Update(entity, projections);

        // Assert
        var entityAfterUpdate = entityRepository.GetById(1);
        entityAfterUpdate.SomeStringProp.Should().Be("stringpropupdated");
        entityAfterUpdate.SomeDecimalProp.Should().Be(10);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Update_ForEntityListAndProjectionPropertiesOverload_WithNullEntities_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
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
        var allEntities = dbContext.FullAuditableEntities.ToList();
        entityRepository.Update(entities, projections);

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
    public async Task Update_ForEntityListAndProjectionPropertiesOverload_WithNotExistsEntities_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
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
        Action act = () => entityRepository.Update(entities, projections);

        // Assert
        act.Should().Throw<DbUpdateConcurrencyException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Update_ForEntityListAndProjectionPropertiesOverload_WithValidEntitiesButPropertySelectorsIsNull_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
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

        var entities = entityRepository.GetAll(i => i.Id == 1 || i.Id == 2);
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = null;

        // Act 
        entities[0].SomeStringProp = "stringpropupdated";
        entities[0].SomeDecimalProp = 20M;
        entities[1].SomeStringProp = "stringpropupdated";
        entities[1].SomeDecimalProp = 20M;
        entityRepository.Update(entities, projections);

        // Assert
        var entitiesAfterUpdate = entityRepository.GetAll(i => i.Id == 1 || i.Id == 2);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("somestring1");
        entitiesAfterUpdate[0].SomeDecimalProp.Should().Be(10M);
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("somestring2");
        entitiesAfterUpdate[1].SomeDecimalProp.Should().Be(20M);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Update_ForEntityListAndProjectionPropertiesOverload_WithValidEntities_ShouldUpdateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
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

        var entities = entityRepository.GetAll(i => i.Id == 1 || i.Id == 2);
        Expression<Func<SomeFullAuditableEntityFixture, object>> projection = i => i.SomeStringProp;
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = [projection];

        // Act 
        entities[0].SomeStringProp = "stringpropupdated";
        entities[0].SomeDecimalProp = 20M;
        entities[1].SomeStringProp = "stringpropupdated";
        entities[1].SomeDecimalProp = 20M;
        entityRepository.Update(entities, projections);

        // Assert
        var entitiesAfterUpdate = entityRepository.GetAll(i => i.Id == 1 || i.Id == 2);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[0].SomeDecimalProp.Should().Be(10M);
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[1].SomeDecimalProp.Should().Be(20M);
    }

    #endregion

    #region Delete

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Delete_ForSingleEntity_WithNullEntity_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
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
        Action act = () => entityRepository.Delete(entity);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Delete_ForSingleEntity_WithNotExistsEntity_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
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
        Action act = () => entityRepository.Delete(entity);

        // Assert
        act.Should().Throw<DbUpdateConcurrencyException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Delete_ForSingleEntity_WithValidEntity_ShouldDeleteCorrectly(QueryTrackingBehavior queryTrackingBehavior)
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

        var entity = entityRepository.GetById(1);

        // Act 
        entityRepository.Delete(entity);

        // Assert
        var entityAfterUpdate = entityRepository.GetById(1);
        entityAfterUpdate.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Delete_ForEntityList_WithNullEntityList_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
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
        entityRepository.Delete(entities);
        var allEntities = dbContext.FullAuditableEntities.ToList();

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
    public async Task Delete_ForEntityList_WithNotExistsEntities_ShouldThrowException(QueryTrackingBehavior queryTrackingBehavior)
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
        Action act = () => entityRepository.Delete(entities);

        // Assert
        act.Should().Throw<DbUpdateConcurrencyException>();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task Delete_ForEntityList_WithValidEntity_ShouldDeleteCorrectly(QueryTrackingBehavior queryTrackingBehavior)
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

        var entities = entityRepository.GetAll(i => i.Id == 1 || i.Id == 2);

        // Act 
        entityRepository.Delete(entities);

        // Assert
        var entitiesAfterUpdate = entityRepository.GetAll(i => i.Id == 1 || i.Id == 2);
        entitiesAfterUpdate.Should().BeEmpty();
    }

    #endregion

    #region ReplaceOldsWithNews

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task ReplaceOldsWithNews_WithValidParameters_ShouldOperateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
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

        var entities = entityRepository.GetAll(i => i.Id == 1 || i.Id == 2);
        var newEntities = entities.ToList();
        newEntities[0].Id = 5;
        newEntities[0].SomeStringProp = "stringpropupdated";
        newEntities[1].Id = 6;
        newEntities[1].SomeStringProp = "stringpropupdated";

        // Act 
        entityRepository.ReplaceOldsWithNews(entities, newEntities);

        // Assert
        var entitiesAfterUpdate = entityRepository.GetAll(i => i.Id == 5 || i.Id == 6);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("stringpropupdated");
    }

    #endregion

    #region ReplaceOldsWithNewsInSeperateDatabaseProcess

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task ReplaceOldsWithNewsInSeperateDatabaseProcess_WithValidParameters_ShouldOperateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
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

        var entities = entityRepository.GetAll(i => i.Id == 1 || i.Id == 2);
        var newEntities = entities.ToList();
        foreach (var entity in newEntities)
            entity.SomeStringProp = "stringpropupdated";

        // Act 
        entityRepository.ReplaceOldsWithNewsInSeperateDatabaseProcess(entities, newEntities);

        // Assert
        var entitiesAfterUpdate = entityRepository.GetAll(i => i.Id == 1 || i.Id == 2);
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
        entityRepository.RemoveAll();

        // Assert
        var entitiesAfterUpdate = entityRepository.GetAll();
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
        entityRepository.RemoveAll();

        // Assert
        var entitiesAfterUpdate = entityRepository.GetAll();
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
#pragma warning restore AsyncFixer02 // Long-running or blocking operations inside an async method
