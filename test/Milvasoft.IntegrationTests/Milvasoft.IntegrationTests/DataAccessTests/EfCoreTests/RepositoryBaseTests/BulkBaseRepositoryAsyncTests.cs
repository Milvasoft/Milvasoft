using FluentAssertions;
using Microsoft.AspNetCore.Builder;
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
using Milvasoft.DataAccess.EfCore.Utils;
using Milvasoft.Helpers.DataAccess.EfCore.Concrete;
using Milvasoft.IntegrationTests.Client.Fixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.Persistence;
using Milvasoft.MultiTenancy.EfCore;
using Milvasoft.MultiTenancy.EfCore.RepositoryBase.Concrete;
using Milvasoft.MultiTenancy.Extensions;

namespace Milvasoft.IntegrationTests.DataAccessTests.EfCoreTests.RepositoryBaseTests;

[Collection(nameof(UtcTrueDatabaseTestCollection))]
[Trait("BulkRepositoryBase Async Integration Tests", "Integration tests for Milvasoft.DataAccess.EfCore integration tests.")]
public class BulkBaseRepositoryAsyncTests(CustomWebApplicationFactory factory) : DataAccessIntegrationTestBase(factory)
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

    #region BulkAddAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task BulkAddAsync_WithNullEntityList_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
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
        await entityRepository.BulkAddAsync(entities);
        var count = await dbContext.FullAuditableEntities.CountAsync();

        // Assert
        count.Should().Be(0);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task BulkAddAsync_WithValidEntity_ShouldAddCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange

        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };

                opt.Auditing = new AuditConfiguration
                {
                    AuditCreationDate = true,
                    AuditCreator = true,
                    AuditModificationDate = true,
                    AuditModifier = true,
                    AuditDeleter = true,
                    AuditDeletionDate = true
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
        await entityRepository.BulkAddAsync(entities);
        var count = await dbContext.FullAuditableEntities.CountAsync();
        var addedEntity = await entityRepository.GetByIdAsync(1);

        // Assert
        count.Should().Be(2);
        addedEntity.Should().NotBeNull();
        addedEntity.CreationDate.Should().NotBeNull();
        addedEntity.CreatorUserName.Should().Be("testuser");
    }

    [Theory(Skip = "Need migration.")]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task BulkAddAsync_WithMultiTenantDbContextAndValidEntity_ShouldAddCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.AddMultiTenancy<SomeTenantEntity, TenantId>()
                     .WithResolutionStrategy<TestTenantIdResolutionStrategy>()
                     .WithStore<TestTenantStore<SomeTenantEntity, TenantId>>();

            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.Repository = new RepositoryConfiguration
                {
                    DefaultSaveChangesChoice = DataAccess.EfCore.Utils.Enums.SaveChangesChoice.Manual
                };
            });

            services.AddDbContextFactory<MultiTenantDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });

            services.AddScoped<MilvaMultiTenancyDbContextFactory<MultiTenantDbContextFixture>>();
            services.AddScoped(sp => sp.GetRequiredService<MilvaMultiTenancyDbContextFactory<MultiTenantDbContextFixture>>().CreateDbContext());
            services.AddScoped(typeof(ISomeMultiTenantGenericRepository<>), typeof(SomeMultiTenantGenericRepository<>));
        });

        var dbContext = _serviceProvider.GetService<MultiTenantDbContextFixture>();
        var entityRepository = _serviceProvider.GetService<ISomeMultiTenantGenericRepository<SomeMultiTenantTestEntityFixture>>();

        var entities = new List<SomeMultiTenantTestEntityFixture>
        {
            new() {
                Id = 1,
                SomeDateProp = DateTime.Now.AddYears(1),
                SomeDecimalProp = 10M,
            },
            new() {
                Id = 2,
                SomeDateProp = DateTime.Now.AddYears(2),
                SomeDecimalProp = 20M,
            }
        };

        // Act 
        await entityRepository.BulkAddAsync(entities);
        var count = await dbContext.Entities.CountAsync();
        var addedEntity = await entityRepository.GetByIdAsync(1);

        // Assert
        count.Should().Be(2);
        addedEntity.Should().NotBeNull();
        addedEntity.CreationDate.Should().NotBeNull();
        addedEntity.CreatorUserName.Should().Be("testuser");
        addedEntity.TenantId = new TenantId("test_1");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task BulkAddWithSaveChangesAsync_WithNullEntityList_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
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
        await entityRepository.BulkAddWithSaveChangesAsync(entities);
        var count = await dbContext.FullAuditableEntities.CountAsync();

        // Assert
        count.Should().Be(0);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task BulkAddWithSaveChangesAsync_WithValidEntity_ShouldAddCorrectly(QueryTrackingBehavior queryTrackingBehavior)
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
        await entityRepository.BulkAddWithSaveChangesAsync(entities);
        var count = await dbContext.FullAuditableEntities.CountAsync();
        var addedEntity = await entityRepository.GetByIdAsync(1);

        // Assert
        count.Should().Be(2);
        addedEntity.Should().NotBeNull();
        addedEntity.CreationDate.Should().NotBeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task BulkAddWithSaveChangesAsync_WithValidEntityAndSaveChangesManuallyAndNoSaveChangesCall_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange

        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.Repository = new RepositoryConfiguration
                {
                    DefaultSaveChangesChoice = DataAccess.EfCore.Utils.Enums.SaveChangesChoice.Manual
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
        await entityRepository.BulkAddWithSaveChangesAsync(entities);
        var count = await dbContext.FullAuditableEntities.CountAsync();

        // Assert
        count.Should().Be(0);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task BulkAddWithSaveChangesAsync_WithValidEntityAndSaveChangesManuallyAndSaveChangesCall_ShouldAddCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange

        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.Repository = new RepositoryConfiguration
                {
                    DefaultSaveChangesChoice = DataAccess.EfCore.Utils.Enums.SaveChangesChoice.Manual
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
        await entityRepository.BulkAddWithSaveChangesAsync(entities);
        await entityRepository.SaveChangesBulkAsync();
        var count = await dbContext.FullAuditableEntities.CountAsync();
        var addedEntity = await entityRepository.GetByIdAsync(1);

        // Assert
        count.Should().Be(2);
        addedEntity.Should().NotBeNull();
        addedEntity.CreationDate.Should().NotBeNull();
    }

    #endregion

    #region BulkUpdateAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task BulkUpdateAsync_ForEntityList_WithNullEntityList_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
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
        await entityRepository.BulkUpdateAsync(entities);
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
    public async Task BulkUpdateAsync_ForEntityList_WithValidEntity_ShouldUpdateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };

                opt.Auditing = new AuditConfiguration
                {
                    AuditCreationDate = true,
                    AuditCreator = true,
                    AuditModificationDate = true,
                    AuditModifier = true,
                    AuditDeleter = true,
                    AuditDeletionDate = true
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

        var entities = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);

        // Act 
        entities[0].SomeStringProp = "stringpropupdated";
        entities[1].SomeStringProp = "stringpropupdated";
        await entityRepository.BulkUpdateAsync(entities);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[0].LastModificationDate.Should().NotBeNull();
        entitiesAfterUpdate[1].LastModificationDate.Should().NotBeNull();
        entitiesAfterUpdate[0].LastModifierUserName.Should().Be("testuser");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task BulkUpdateWithSaveChangesAsync_ForEntityList_WithNullEntityList_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
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
        await entityRepository.BulkUpdateWithSaveChangesAsync(entities);
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
    public async Task BulkUpdateWithSaveChangesAsync_ForEntityList_WithValidEntity_ShouldUpdateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
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
        await entityRepository.BulkUpdateWithSaveChangesAsync(entities);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[0].LastModificationDate.Should().NotBeNull();
        entitiesAfterUpdate[1].LastModificationDate.Should().NotBeNull();
    }

    #endregion

    #region BulkDeleteAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task BulkDeleteAsync_ForEntityList_WithNullEntityList_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
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
        await entityRepository.BulkDeleteAsync(entities);
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
    public async Task BulkDeleteAsync_ForEntityList_WithValidEntityAndSoftDeletePassive_ShouldDeleteCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt => opt.DbContext.DefaultSoftDeletionState = DataAccess.EfCore.Utils.Enums.SoftDeletionState.Passive);

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
        await entityRepository.BulkDeleteAsync(entities);

        // Assert
        entityRepository.FetchSoftDeletedEntities(true);
        var entitiesAfterDelete = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        entitiesAfterDelete.Should().BeEmpty();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task BulkDeleteAsync_ForEntityList_WithValidEntityAndSoftDeleteActive_ShouldDeleteCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    GetCurrentUserNameMethod = (sp) => "testuser",
                    DefaultSoftDeletionState = DataAccess.EfCore.Utils.Enums.SoftDeletionState.Active
                };

                opt.Auditing = new AuditConfiguration
                {
                    AuditCreationDate = true,
                    AuditCreator = true,
                    AuditModificationDate = true,
                    AuditModifier = true,
                    AuditDeleter = true,
                    AuditDeletionDate = true
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

        var entities = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);

        // Act 
        await entityRepository.BulkDeleteAsync(entities);

        // Assert
        entityRepository.FetchSoftDeletedEntities(true);
        var entitiesAfterDelete = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        entitiesAfterDelete.Should().NotBeEmpty();
        entitiesAfterDelete[0].DeletionDate.Should().NotBeNull();
        entitiesAfterDelete[0].DeleterUserName.Should().Be("testuser");
        entitiesAfterDelete[1].DeletionDate.Should().NotBeNull();
        entitiesAfterDelete[0].IsDeleted.Should().Be(true);
        entitiesAfterDelete[1].IsDeleted.Should().Be(true);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task BulkDeleteWithSaveChangesAsync_ForEntityList_WithNullEntityList_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
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
        await entityRepository.BulkDeleteWithSaveChangesAsync(entities);
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
    public async Task BulkDeleteWithSaveChangesAsync_ForEntityList_WithValidEntityAndSoftDeletePassive_ShouldDeleteCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt => opt.DbContext.DefaultSoftDeletionState = DataAccess.EfCore.Utils.Enums.SoftDeletionState.Passive);

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
        await entityRepository.BulkDeleteWithSaveChangesAsync(entities);

        // Assert
        entityRepository.FetchSoftDeletedEntities(true);
        var entitiesAfterDelete = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        entitiesAfterDelete.Should().BeEmpty();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task BulkDeleteWithSaveChangesAsync_ForEntityList_WithValidEntityAndSoftDeleteActive_ShouldDeleteCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt => opt.DbContext.DefaultSoftDeletionState = DataAccess.EfCore.Utils.Enums.SoftDeletionState.Active);

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
        await entityRepository.BulkDeleteWithSaveChangesAsync(entities);

        // Assert
        entityRepository.FetchSoftDeletedEntities(true);
        var entitiesAfterDelete = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        entitiesAfterDelete.Should().NotBeEmpty();
        entitiesAfterDelete[0].DeletionDate.Should().NotBeNull();
        entitiesAfterDelete[1].DeletionDate.Should().NotBeNull();
        entitiesAfterDelete[0].IsDeleted.Should().Be(true);
        entitiesAfterDelete[1].IsDeleted.Should().Be(true);
    }

    #endregion

    #region ExecuteUpdateAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task ExecuteUpdateAsync_IdOverload_ForEntityList_WithNullEntityList_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
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

        var propBuilder = new SetPropertyBuilder<SomeFullAuditableEntityFixture>();
        propBuilder.SetPropertyValue(i => i.SomeStringProp, "stringpropupdated");

        // Act 
        await entityRepository.ExecuteUpdateAsync(id: 5, propBuilder);
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
    public async Task ExecuteUpdateAsyncIdOverload_ForEntityList_WithValidEntity_ShouldUpdateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };

                opt.Auditing = new AuditConfiguration
                {
                    AuditCreationDate = true,
                    AuditCreator = true,
                    AuditModificationDate = true,
                    AuditModifier = true,
                    AuditDeleter = true,
                    AuditDeletionDate = true
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
        var propBuilder = new SetPropertyBuilder<SomeFullAuditableEntityFixture>();
        propBuilder.SetPropertyValue(i => i.SomeStringProp, "stringpropupdated");

        // Act 
        await entityRepository.ExecuteUpdateAsync(1, propBuilder);

        // Assert
        var entityAfterUpdate = await entityRepository.GetByIdAsync(1);
        entityAfterUpdate.SomeStringProp.Should().Be("stringpropupdated");
        entityAfterUpdate.SomeDecimalProp.Should().Be(10M);
        entityAfterUpdate.LastModificationDate.Should().NotBeNull();
        entityAfterUpdate.LastModifierUserName.Should().Be("testuser");
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task ExecuteUpdateAsync_ForEntityList_WithValidEntity_ShouldUpdateCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };

                opt.Auditing = new AuditConfiguration
                {
                    AuditCreationDate = true,
                    AuditCreator = true,
                    AuditModificationDate = true,
                    AuditModifier = true,
                    AuditDeleter = true,
                    AuditDeletionDate = true
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
        var propBuilder = new SetPropertyBuilder<SomeFullAuditableEntityFixture>();
        propBuilder.SetPropertyValue(i => i.SomeStringProp, "stringpropupdated");

        // Act 
        await entityRepository.ExecuteUpdateAsync(i => i.Id == 1 || i.Id == 2, propBuilder);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[0].SomeDecimalProp.Should().Be(10M);
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[1].SomeDecimalProp.Should().Be(20M);
        entitiesAfterUpdate[0].LastModificationDate.Should().NotBeNull();
        entitiesAfterUpdate[1].LastModificationDate.Should().NotBeNull();
        entitiesAfterUpdate[0].LastModifierUserName.Should().Be("testuser");
        entitiesAfterUpdate[1].LastModifierUserName.Should().Be("testuser");
    }

    #endregion

    #region ExecuteDeleteAsync

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task ExecuteDeleteAsync_IdOverload_ForEntityList_WithNullEntityList_ShouldDoNothing(QueryTrackingBehavior queryTrackingBehavior)
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
        await entityRepository.ExecuteDeleteAsync(5);
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
    public async Task ExecuteDeleteAsync_IdOverload_ForEntityList_WithValidEntityAndSoftDeletePassive_ShouldDeleteCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt => opt.DbContext.DefaultSoftDeletionState = DataAccess.EfCore.Utils.Enums.SoftDeletionState.Passive);

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
        await entityRepository.ExecuteDeleteAsync(1);

        // Assert
        entityRepository.FetchSoftDeletedEntities(true);
        var entitiesAfterDelete = await entityRepository.GetByIdAsync(1);
        entitiesAfterDelete.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task ExecuteDeleteAsync_IdOverload_ForEntityList_WithValidEntityAndSoftDeleteActive_ShouldDeleteCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    GetCurrentUserNameMethod = (sp) => "testuser",
                    DefaultSoftDeletionState = DataAccess.EfCore.Utils.Enums.SoftDeletionState.Active
                };

                opt.Auditing = new AuditConfiguration
                {
                    AuditCreationDate = true,
                    AuditCreator = true,
                    AuditModificationDate = true,
                    AuditModifier = true,
                    AuditDeleter = true,
                    AuditDeletionDate = true
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

        // Act 
        await entityRepository.ExecuteDeleteAsync(1);

        // Assert
        entityRepository.FetchSoftDeletedEntities(true);
        var entityAfterDelete = await entityRepository.GetByIdAsync(1);
        entityAfterDelete.Should().NotBeNull();
        entityAfterDelete.DeletionDate.Should().NotBeNull();
        entityAfterDelete.DeleterUserName.Should().Be("testuser");
        entityAfterDelete.IsDeleted.Should().Be(true);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task ExecuteDeleteAsync_ForEntityList_WithValidEntityAndSoftDeletePassive_ShouldDeleteCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt => opt.DbContext.DefaultSoftDeletionState = DataAccess.EfCore.Utils.Enums.SoftDeletionState.Passive);

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
        await entityRepository.ExecuteDeleteAsync(i => i.Id == 1 || i.Id == 2);

        // Assert
        entityRepository.FetchSoftDeletedEntities(true);
        var entitiesAfterDelete = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        entitiesAfterDelete.Should().BeEmpty();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task ExecuteDeleteAsync_ForEntityList_WithValidEntityAndSoftDeleteActive_ShouldDeleteCorrectly(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    GetCurrentUserNameMethod = (sp) => "testuser",
                    DefaultSoftDeletionState = DataAccess.EfCore.Utils.Enums.SoftDeletionState.Active
                };

                opt.Auditing = new AuditConfiguration
                {
                    AuditCreationDate = true,
                    AuditCreator = true,
                    AuditModificationDate = true,
                    AuditModifier = true,
                    AuditDeleter = true,
                    AuditDeletionDate = true
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

        var propBuilder = new SetPropertyBuilder<SomeFullAuditableEntityFixture>();
        propBuilder.SetPropertyValue(i => i.SomeStringProp, "stringpropupdated");

        // Act 
        await entityRepository.ExecuteDeleteAsync(i => i.Id == 1 || i.Id == 2, propBuilder);

        // Assert
        entityRepository.FetchSoftDeletedEntities(true);
        var entitiesAfterDelete = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2);
        entitiesAfterDelete.Should().NotBeEmpty();
        entitiesAfterDelete[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterDelete[1].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterDelete[0].DeletionDate.Should().NotBeNull();
        entitiesAfterDelete[0].DeleterUserName.Should().Be("testuser");
        entitiesAfterDelete[1].DeletionDate.Should().NotBeNull();
        entitiesAfterDelete[0].IsDeleted.Should().Be(true);
        entitiesAfterDelete[1].IsDeleted.Should().Be(true);
    }

    #endregion

    #region Setup

    public interface ISomeGenericRepository<TEntity> : IBulkBaseRepository<TEntity> where TEntity : class, IMilvaEntity
    {
    }

    public class SomeGenericRepository<TEntity>(MilvaBulkDbContextFixture dbContext) : BulkBaseRepository<TEntity, MilvaBulkDbContextFixture>(dbContext), ISomeGenericRepository<TEntity>
         where TEntity : class, IMilvaEntity
    {
    }

    public interface ISomeMultiTenantGenericRepository<TEntity> : IBulkBaseRepository<TEntity> where TEntity : class, IMilvaEntity
    {
    }

    public class SomeMultiTenantGenericRepository<TEntity>(MultiTenantDbContextFixture dbContext) : MultiTenantBaseRepository<TEntity, MultiTenantDbContextFixture>(dbContext), ISomeMultiTenantGenericRepository<TEntity>
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

        foreach (var entity in entities)
            dbContextFixture.Entry(entity).State = EntityState.Detached;
    }

    #endregion
}
