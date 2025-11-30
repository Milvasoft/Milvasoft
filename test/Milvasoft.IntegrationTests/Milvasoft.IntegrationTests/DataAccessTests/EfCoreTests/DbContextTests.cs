using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Helpers;
using Milvasoft.Core.MultiLanguage.Builder;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.Utils.Enums;
using Milvasoft.DataAccess.EfCore.Utils.LookupModels;
using Milvasoft.IntegrationTests.Client.Fixtures.DtoFixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.Persistence;
using Milvasoft.Localization;
using Npgsql;
using System.Linq.Expressions;

namespace Milvasoft.IntegrationTests.DataAccessTests.EfCoreTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
[Collection(nameof(UtcTrueDatabaseTestCollection))]
[Trait("MilvaDbContext Integration Tests", "Integration tests for Milvasoft.DataAccess.EfCore integration tests.")]
public class DbContextTests(CustomWebApplicationFactory factory) : DataAccessIntegrationTestBase(factory)
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

                services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                {
                    x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                    x.UseNpgsql(_factory.GetConnectionString());
                });

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

    #region Auditing

    [Fact]
    public async Task Auditing_WithAuditCreatorButCurrentUsernameDelegateIsNull_ShouldNotAudit()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    GetCurrentUserNameMethod = null
                };

                opt.Auditing = new AuditConfiguration
                {
                    AuditCreator = true,
                    AuditCreationDate = false,
                };
            });
        });

        var dbContext = _serviceProvider.GetService<MilvaBulkDbContextFixture>();
        var entity = new SomeBaseEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop"
        };

        // Act & Assert
        await dbContext.BaseEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        var entityInDb = await dbContext.BaseEntities.FirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.Id.Should().Be(1);
        entityInDb.CreationDate.Should().BeNull();
        entityInDb.CreatorUserName.Should().BeNull();
    }

    [Fact]
    public async Task Auditing_WithAuditPerformTimeAndPerformerUser_ShouldAudit()
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
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
                opt.Auditing = new AuditConfiguration
                {
                    AuditCreationDate = true,
                    AuditCreator = true,
                    AuditModificationDate = true,
                    AuditModifier = true,
                    AuditDeletionDate = true,
                    AuditDeleter = true
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var now = DateTime.UtcNow;
        var entity = new SomeBaseEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop"
        };

        // Act & Assert

        //Add
        await dbContext.BaseEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        var entityInDb = await dbContext.BaseEntities.FirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.Id.Should().Be(1);
        entityInDb.CreationDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
        entityInDb.CreatorUserName.Should().Be("testuser");

        // Update
        dbContext.BaseEntities.Update(entityInDb);
        await dbContext.SaveChangesAsync();
        var entityAfterUpdate = await dbContext.BaseEntities.FirstOrDefaultAsync();
        entityAfterUpdate.Id.Should().Be(1);
        entityAfterUpdate.LastModificationDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
        entityAfterUpdate.LastModifierUserName.Should().Be("testuser");

        // Delete 
        dbContext.BaseEntities.Remove(entityAfterUpdate);
        await dbContext.SaveChangesAsync();
        var entityAfterDelete = await dbContext.BaseEntities.FirstOrDefaultAsync();
        entityAfterDelete.Id.Should().Be(1);
        entityAfterDelete.DeletionDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(2));
        entityAfterDelete.IsDeleted.Should().BeTrue();
        entityAfterDelete.DeleterUserName.Should().Be("testuser");
    }

    #endregion

    #region Soft Deletion

    [Fact]
    public async Task SoftDeletion_WithSoftDeletionIsActiveAndEntityIsNotSoftDeletable_ShouldDeleteEntity()
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
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var entity = new SomeEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop"
        };

        // Act & Assert
        await dbContext.Entities.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        var entityInDb = await dbContext.Entities.FirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.Id.Should().Be(1);

        dbContext.Entities.Remove(entityInDb);
        await dbContext.SaveChangesAsync();
        var dataCount = await dbContext.Entities.CountAsync();
        dataCount.Should().Be(0);
    }

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
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var entity = new SomeBaseEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop"
        };

        // Act & Assert
        await dbContext.BaseEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        var entityInDb = await dbContext.BaseEntities.FirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.Id.Should().Be(1);

        dbContext.BaseEntities.Remove(entityInDb);
        await dbContext.SaveChangesAsync();
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
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var now = DateTime.UtcNow;
        var entity = new SomeBaseEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop"
        };

        // Act & Assert
        await dbContext.BaseEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        var entityInDb = await dbContext.BaseEntities.FirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.Id.Should().Be(1);

        dbContext.BaseEntities.Remove(entityInDb);
        await dbContext.SaveChangesAsync();
        var dataCount = await dbContext.BaseEntities.CountAsync();
        dataCount.Should().Be(1);

        entityInDb = await dbContext.BaseEntities.FirstOrDefaultAsync();
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
        });

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
        await dbContext.BaseEntities.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
        var countAfterAdd = await dbContext.BaseEntities.CountAsync();
        countAfterAdd.Should().Be(2);

        dbContext.ChangeSoftDeletionState(SoftDeletionState.Passive);

        var firstEntity = await dbContext.BaseEntities.FindAsync(1);
        dbContext.BaseEntities.Remove(firstEntity);
        await dbContext.SaveChangesAsync();
        var dataCount = await dbContext.BaseEntities.CountAsync();
        dataCount.Should().Be(1);

        var secondEntity = await dbContext.BaseEntities.FindAsync(2);
        dbContext.BaseEntities.Remove(secondEntity);
        await dbContext.SaveChangesAsync();
        dataCount = await dbContext.BaseEntities.CountAsync();
        dataCount.Should().Be(1);

        var secondEntityAfterSoftDeletion = await dbContext.BaseEntities.FindAsync(2);
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
        });

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
        await dbContext.BaseEntities.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
        var countAfterAdd = await dbContext.BaseEntities.CountAsync();
        countAfterAdd.Should().Be(2);

        dbContext.ChangeSoftDeletionState(SoftDeletionState.Passive);

        var firstEntity = await dbContext.BaseEntities.FindAsync(1);
        dbContext.BaseEntities.Remove(firstEntity);
        await dbContext.SaveChangesAsync();
        var dataCount = await dbContext.BaseEntities.CountAsync();
        dataCount.Should().Be(1);

        var secondEntity = await dbContext.BaseEntities.FindAsync(2);
        dbContext.BaseEntities.Remove(secondEntity);
        await dbContext.SaveChangesAsync();
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
        });

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
        await dbContext.BaseEntities.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
        var countAfterAdd = await dbContext.BaseEntities.CountAsync();
        countAfterAdd.Should().Be(2);

        dbContext.ChangeSoftDeletionState(SoftDeletionState.Passive);
        dbContext.SoftDeletionStateResetAfterOperation(true);

        var firstEntity = await dbContext.BaseEntities.FindAsync(1);
        dbContext.BaseEntities.Remove(firstEntity);
        await dbContext.SaveChangesAsync();
        var dataCount = await dbContext.BaseEntities.CountAsync();
        dataCount.Should().Be(1);

        var secondEntity = await dbContext.BaseEntities.FindAsync(2);
        dbContext.BaseEntities.Remove(secondEntity);
        await dbContext.SaveChangesAsync();
        dataCount = await dbContext.BaseEntities.CountAsync();
        dataCount.Should().Be(1);

        var secondEntityAfterSoftDeletion = await dbContext.BaseEntities.FindAsync(2);
        secondEntityAfterSoftDeletion.Should().NotBeNull();
        secondEntityAfterSoftDeletion.IsDeleted.Should().BeTrue();
        secondEntityAfterSoftDeletion.DeletionDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetDataAccessConfiguration_ShouldReturnCorrectValue()
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
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        // Act & Assert
        var conf = dbContext.GetDataAccessConfiguration();
        conf.DbContext.ResetSoftDeleteStateAfterEveryOperation.Should().BeFalse();
    }

    [Fact]
    public async Task SetDataAccessConfiguration_ShouldUpdateCorrectly()
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
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        // Act & Assert
        var conf = dbContext.GetDataAccessConfiguration();
        conf.DbContext.ResetSoftDeleteStateAfterEveryOperation.Should().BeFalse();

        conf.DbContext.GetCurrentUserNameMethod = (sp) => "user";

        dbContext.SetDataAccessConfiguration(conf);

        conf = dbContext.GetDataAccessConfiguration();
        conf.DbContext.GetCurrentUserNameMethod.Should().NotBeNull();
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
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        // Act & Assert
        var currentSoftDeleteState = dbContext.GetCurrentSoftDeletionState();
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
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        // Act & Assert
        var currentSoftDeleteState = dbContext.GetCurrentSoftDeletionState();
        currentSoftDeleteState.Should().Be(SoftDeletionState.Active);

        dbContext.ChangeSoftDeletionState(SoftDeletionState.Passive);

        currentSoftDeleteState = dbContext.GetCurrentSoftDeletionState();
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
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        // Act & Assert
        var currentSoftDeleteState = dbContext.GetCurrentSoftDeletionState();
        currentSoftDeleteState.Should().Be(SoftDeletionState.Active);

        dbContext.ChangeSoftDeletionState(SoftDeletionState.Passive);
        currentSoftDeleteState = dbContext.GetCurrentSoftDeletionState();
        currentSoftDeleteState.Should().Be(SoftDeletionState.Passive);

        dbContext.SetSoftDeletionStateToDefault();
        currentSoftDeleteState = dbContext.GetCurrentSoftDeletionState();
        currentSoftDeleteState.Should().Be(SoftDeletionState.Active);
    }

    [Fact]
    public async Task SoftDeletion_WithSoftDeletionIsActiveAndEntityAndRelatedEntityIsNotSoftDeletableAndTrackingActive_ShouldDeleteRelatedEntity()
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
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var entity = new SomeEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop",
            RelatedEntities =
            [
                new() {
                    Id = 1,
                    SomeStringProp = "relatedstringprop",
                    SomeDateProp = DateTime.UtcNow,
                }
            ]
        };

        // Act & Assert
        await dbContext.Entities.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        var entityInDb = await dbContext.Entities.FirstOrDefaultAsync();
        var relatedEntityInDb = await dbContext.RelatedEntities.FirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.Id.Should().Be(1);
        relatedEntityInDb.Should().NotBeNull();
        relatedEntityInDb.Id.Should().Be(1);

        dbContext.Entities.Remove(entityInDb);
        await dbContext.SaveChangesAsync();
        var dataCount = await dbContext.FullAuditableEntities.CountAsync();
        dataCount.Should().Be(0);

        entityInDb = await dbContext.Entities.FirstOrDefaultAsync();
        relatedEntityInDb = await dbContext.RelatedEntities.FirstOrDefaultAsync();
        entityInDb.Should().BeNull();
        relatedEntityInDb.Should().BeNull();
    }

    [Fact]
    public async Task SoftDeletion_WithSoftDeletionIsActiveAndEntityIsNotSoftDeletableAndRelatedEntityIsSoftDeletableAndTrackingActive_ShouldRemoveBothEntities()
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
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var entity = new SomeEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop",
            ManyToOneEntities =
            [
                new() {
                    Id = 1,
                    SomeStringProp = "relatedstringprop",
                    SomeDateProp = DateTime.UtcNow,
                    SomeFullAuditableEntity = new SomeFullAuditableEntityFixture
                    {
                         SomeStringProp = "dummy"
                    }
                }
            ]
        };

        // Act & Assert
        await dbContext.Entities.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        dbContext.Entry(entity).State = EntityState.Detached;
        var entityInDb = await dbContext.Entities.Select(i => new SomeEntityFixture
        {
            Id = i.Id,
            ManyToOneEntities = i.ManyToOneEntities
        }).FirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.Id.Should().Be(1);

        dbContext.Entities.Remove(entityInDb);
        await dbContext.SaveChangesAsync();

        entityInDb = await dbContext.Entities.FirstOrDefaultAsync();
        var relatedEntityInDb = await dbContext.SomeManyToOneEntities.FirstOrDefaultAsync();
        entityInDb.Should().BeNull();
        relatedEntityInDb.Should().BeNull();
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(QueryTrackingBehavior.NoTracking, false)]
    [InlineData(QueryTrackingBehavior.TrackAll, true)]
    public async Task SoftDeletion_WithSoftDeletionIsActiveAndBothSideSoftDeletableAndAsNoTrackingAndNotIncludedRelatedEntities_ShouldDoNothingOnRelatedEntities(QueryTrackingBehavior queryTrackingBehavior, bool expectedRelatedIsDeleted)
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

            services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
            services.RemoveAll<MilvaBulkDbContextFixture>();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var now = DateTime.UtcNow;
        var entity = new SomeFullAuditableEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop",
            ManyToOneEntities =
            [
                new() {
                    Id = 1,
                    SomeStringProp = "relatedstringprop",
                    SomeDateProp = DateTime.UtcNow,
                    SomeEntity = new SomeEntityFixture{
                        Id = 1,
                        SomeStringProp = "dummy"
                    }
                }
            ]
        };

        // Act & Assert
        await dbContext.FullAuditableEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        dbContext.Entry(entity).State = EntityState.Detached;

        var entityInDb = await dbContext.FullAuditableEntities.FirstOrDefaultAsync();

        var relatedEntityInDb = await dbContext.SomeManyToOneEntities.FirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.Id.Should().Be(1);
        relatedEntityInDb.Should().NotBeNull();
        relatedEntityInDb.Id.Should().Be(1);

        dbContext.FullAuditableEntities.Remove(entityInDb);
        await dbContext.SaveChangesAsync();
        var dataCount = await dbContext.FullAuditableEntities.CountAsync();
        dataCount.Should().Be(1);

        entityInDb = await dbContext.FullAuditableEntities.FirstOrDefaultAsync();
        relatedEntityInDb = await dbContext.SomeManyToOneEntities.FirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.IsDeleted.Should().BeTrue();
        entityInDb.DeletionDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
        relatedEntityInDb.Should().NotBeNull();
        relatedEntityInDb.IsDeleted.Should().Be(expectedRelatedIsDeleted);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(QueryTrackingBehavior.NoTracking, false)]
    [InlineData(QueryTrackingBehavior.TrackAll, true)]
    public async Task SoftDeletion_WithSoftDeletionIsActiveAndBothSideSoftDeletableAndAsNoTrackingAndNotIncludedRelatedEntity_ShouldDoNothingOnRelatedEntity(QueryTrackingBehavior queryTrackingBehavior, bool expectedRelatedIsDeleted)
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

            services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
            services.RemoveAll<MilvaBulkDbContextFixture>();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var now = DateTime.UtcNow;
        var entity = new SomeFullAuditableEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop",
            RelatedFullAuditableEntity = new AnotherFullAuditableEntityFixture
            {
                Id = 1,
                SomeStringProp = "relatedstringprop",
                SomeDateProp = DateTime.UtcNow,
            }
        };

        // Act & Assert
        await dbContext.FullAuditableEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        dbContext.Entry(entity).State = EntityState.Detached;
        dbContext.Entry(entity.RelatedFullAuditableEntity).State = EntityState.Detached;

        var entityInDb = await dbContext.FullAuditableEntities.FirstOrDefaultAsync();

        var relatedEntityInDb = await dbContext.AnotherFullAuditableEntities.FirstOrDefaultAsync();
        dbContext.Entry(relatedEntityInDb).State = EntityState.Detached;
        entityInDb.Should().NotBeNull();
        entityInDb.Id.Should().Be(1);
        relatedEntityInDb.Should().NotBeNull();
        relatedEntityInDb.Id.Should().Be(1);

        dbContext.FullAuditableEntities.Remove(entityInDb);
        await dbContext.SaveChangesAsync();
        var dataCount = await dbContext.FullAuditableEntities.CountAsync();
        dataCount.Should().Be(1);

        entityInDb = await dbContext.FullAuditableEntities.FirstOrDefaultAsync();
        relatedEntityInDb = await dbContext.AnotherFullAuditableEntities.FirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.IsDeleted.Should().BeTrue();
        entityInDb.DeletionDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
        relatedEntityInDb.Should().NotBeNull();
        relatedEntityInDb.IsDeleted.Should().Be(expectedRelatedIsDeleted);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task SoftDeletion_WithSoftDeletionIsActiveAndEntityIsSoftDeletableAndAsNoTrackingAndIncludedRelatedEntities_ShouldSoftDeleteRelatedEntities(QueryTrackingBehavior queryTrackingBehavior)
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

            services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
            services.RemoveAll<MilvaBulkDbContextFixture>();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var now = DateTime.UtcNow;
        var entity = new SomeFullAuditableEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop",
            ManyToOneEntities =
            [
                new() {
                    Id = 1,
                    SomeStringProp = "relatedstringprop",
                    SomeDateProp = DateTime.UtcNow,
                    SomeEntity = new SomeEntityFixture{
                        Id = 1,
                        SomeStringProp = "dummy"
                    }
                }
            ]
        };

        // Act & Assert
        await dbContext.FullAuditableEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        dbContext.Entry(entity).State = EntityState.Detached;

        var entityInDb = await dbContext.FullAuditableEntities.Select(i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            ManyToOneEntities = i.ManyToOneEntities
        }).FirstOrDefaultAsync();

        var relatedEntityInDb = await dbContext.SomeManyToOneEntities.FirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.Id.Should().Be(1);
        relatedEntityInDb.Should().NotBeNull();
        relatedEntityInDb.Id.Should().Be(1);

        dbContext.FullAuditableEntities.Remove(entityInDb);
        await dbContext.SaveChangesAsync();
        var dataCount = await dbContext.FullAuditableEntities.CountAsync();
        dataCount.Should().Be(1);

        entityInDb = await dbContext.FullAuditableEntities.FirstOrDefaultAsync();
        relatedEntityInDb = await dbContext.SomeManyToOneEntities.FirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.IsDeleted.Should().BeTrue();
        entityInDb.DeletionDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
        relatedEntityInDb.Should().NotBeNull();
        relatedEntityInDb.IsDeleted.Should().BeTrue();
        relatedEntityInDb.DeletionDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    public async Task SoftDeletion_WithSoftDeletionIsActiveAndEntityIsSoftDeletableAndAsNoTrackingAndIncludedRelatedEntity_ShouldSoftDeleteRelatedEntity(QueryTrackingBehavior queryTrackingBehavior)
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

            services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
            services.RemoveAll<MilvaBulkDbContextFixture>();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString()).UseQueryTrackingBehavior(queryTrackingBehavior);
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var now = DateTime.UtcNow;
        var entity = new SomeFullAuditableEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop",
            RelatedFullAuditableEntity = new AnotherFullAuditableEntityFixture
            {
                Id = 1,
                SomeStringProp = "relatedstringprop",
                SomeDateProp = DateTime.UtcNow,
            }
        };

        // Act & Assert
        await dbContext.FullAuditableEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        dbContext.Entry(entity).State = EntityState.Detached;

        var entityInDb = await dbContext.FullAuditableEntities.Select(i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            ManyToOneEntities = i.ManyToOneEntities
        }).FirstOrDefaultAsync();

        var relatedEntityInDb = await dbContext.AnotherFullAuditableEntities.FirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.Id.Should().Be(1);
        relatedEntityInDb.Should().NotBeNull();
        relatedEntityInDb.Id.Should().Be(1);

        dbContext.FullAuditableEntities.Remove(entityInDb);
        await dbContext.SaveChangesAsync();
        var dataCount = await dbContext.FullAuditableEntities.CountAsync();
        dataCount.Should().Be(1);

        entityInDb = await dbContext.FullAuditableEntities.FirstOrDefaultAsync();
        relatedEntityInDb = await dbContext.AnotherFullAuditableEntities.FirstOrDefaultAsync();
        entityInDb.Should().NotBeNull();
        entityInDb.IsDeleted.Should().BeTrue();
        entityInDb.DeletionDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
        relatedEntityInDb.Should().NotBeNull();
        relatedEntityInDb.IsDeleted.Should().BeTrue();
        relatedEntityInDb.DeletionDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Dynamic Fetch

    /// <summary>
    /// lookup request
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> InvalidLookupRequestForGetLookupsAsyncMethod()
    {
        yield return new object[]
        {
            new LookupRequest { Parameters = [ new (){ EntityName = null }]}
        };

        yield return new object[]
        {
            new LookupRequest { Parameters = [ new (){ EntityName = "" }]}
        };

        yield return new object[]
        {
            new LookupRequest { Parameters = [ new (){ EntityName = " " }]}
        };

        yield return new object[]
        {
            new LookupRequest { Parameters = [ new() { EntityName = nameof(SomeBaseEntityFixture), RequestedPropertyNames = null } ]}
        };

        yield return new object[]
        {
            new LookupRequest { Parameters = [ new() { EntityName = nameof(SomeBaseEntityFixture), RequestedPropertyNames = [] } ]}
        };

        // Duplicate property
        yield return new object[]
        {
            new LookupRequest
            {
                Parameters =
                [
                    new()
                    {
                        EntityName = nameof(SomeBaseEntityFixture),
                        RequestedPropertyNames =
                        [
                            nameof(SomeBaseEntityFixture.Id),
                            nameof(SomeBaseEntityFixture.Id),
                        ]
                    }
                ]
            }
        };

        // Restricted entity name
        yield return new object[]
        {
            new LookupRequest
            {
                Parameters =
                [
                    new()
                    {
                        EntityName = nameof(SomeEntityFixture),
                        RequestedPropertyNames =
                        [
                            nameof(SomeEntityFixture.Id),
                        ]
                    }
                ]
            }
        };

        // Max property count 
        yield return new object[]
        {
            new LookupRequest
            {
                Parameters =
                [
                    new()
                    {
                        EntityName = nameof(SomeBaseEntityFixture),
                        RequestedPropertyNames =
                        [
                            nameof(SomeBaseEntityFixture.Id),
                            nameof(SomeBaseEntityFixture.CreatorUserName),
                            nameof(SomeBaseEntityFixture.CreationDate),
                            nameof(SomeBaseEntityFixture.LastModifierUserName),
                        ]
                    }
                ]
            }
        };
    }

    [Fact]
    public async Task GetLookupsAsync_WithNullLookupRequest_ShouldReturnEmptyCollection()
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
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var request = new LookupRequest
        {
            Parameters = null
        };

        // Act
        var result = await dbContext.GetLookupsAsync(request);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(InvalidLookupRequestForGetLookupsAsyncMethod))]
    public async Task GetLookupsAsync_WithInvalidLookupRequest_ShouldThrowException(LookupRequest lookupRequest)
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
                    DynamicFetch = new DynamicFetchConfiguration
                    {
                        AllowedEntityNamesForLookup = [nameof(SomeBaseEntityFixture)],
                        MaxAllowedPropertyCountForLookup = 3
                    }
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        // Act
        Func<Task> act = async () => await dbContext.GetLookupsAsync(lookupRequest);

        // Assert
        await act.Should().ThrowAsync<MilvaUserFriendlyException>();
    }

    [Fact]
    public async Task GetLookupsAsync_WithValidLookupRequest_ShouldReturnCorrectResult()
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
                    DynamicFetch = new DynamicFetchConfiguration
                    {
                        EntityAssemblyName = "Milvasoft.IntegrationTests.Client",
                        AllowedEntityNamesForLookup = [nameof(SomeBaseEntityFixture)],
                        MaxAllowedPropertyCountForLookup = 2
                    }
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var entity = new SomeBaseEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop",
            SomeDecimalProp = 10M,
            SomeNullableProp = 100
        };
        await dbContext.BaseEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        var lookupRequest = new LookupRequest
        {
            Parameters =
            [
                new()
                {
                    EntityName = nameof(SomeBaseEntityFixture),
                    RequestedPropertyNames = [nameof(SomeBaseEntityFixture.SomeDecimalProp), nameof(SomeBaseEntityFixture.SomeNullableProp)],
                }
            ]
        };
        object expectedData = new { SomeDecimalProp = 10M, SomeNullableProp = 100, Id = 1 };

        // Act
        var result = await dbContext.GetLookupsAsync(lookupRequest);

        // Assert
        result.Should().HaveCount(1);
        var lookupResult = result[0];
        lookupResult.Should().BeOfType<LookupResult>();
        ((LookupResult)lookupResult).EntityName.Should().Be(nameof(SomeBaseEntityFixture));
        ((LookupResult)lookupResult).Data.Should().HaveCount(1);
        ((LookupResult)lookupResult).Data[0].ToJson().Should().BeEquivalentTo(expectedData.ToJson());
    }

    [Fact]
    public async Task GetLookupsAsync_WithValidLookupRequestAndHasTranslations_ShouldReturnCorrectResult()
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
                    DynamicFetch = new DynamicFetchConfiguration
                    {
                        EntityAssemblyName = "Milvasoft.IntegrationTests.Client",
                        AllowedEntityNamesForLookup = [nameof(HasTranslationEntityFixture)],
                        MaxAllowedPropertyCountForLookup = 2
                    }
                };
            });

            services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
            services.RemoveAll<MilvaBulkDbContextFixture>();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(_factory.GetConnectionString())
                 .ReplaceService<IModelCustomizer, TranslationRelationsModelCustomizer>();
            });

            services.AddMilvaMultiLanguage().WithDefaultMultiLanguageManager();
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var entity = new HasTranslationEntityFixture
        {
            Id = 1,
            Priority = 1,
            Translations =
            [
                new()
                {
                    Id = 1,
                    LanguageId = 1,
                    Name = "türkçe",
                    EntityId = 1,
                },
                new()
                {
                    Id = 2,
                    LanguageId = 2,
                    Name = "English",
                    EntityId = 1,
                }
            ]
        };
        await dbContext.HasTranslationEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        var lookupRequest = new LookupRequest
        {
            Parameters =
            [
                new()
                {
                    EntityName = nameof(HasTranslationEntityFixture),
                    RequestedPropertyNames = [nameof(HasTranslationEntityFixture.Priority), "Name"],
                }
            ]
        };
        object expectedData = new
        {
            Priority = 1,
            Name = "türkçe",
            Id = 1,
        };

        using var _ = new CultureSwitcher("tr-TR");

        // Act
        var result = await dbContext.GetLookupsAsync(lookupRequest);

        // Assert
        result.Should().HaveCount(1);
        var lookupResult = result[0];
        lookupResult.Should().BeOfType<LookupResult>();
        ((LookupResult)lookupResult).EntityName.Should().Be(nameof(HasTranslationEntityFixture));
        ((LookupResult)lookupResult).Data.Should().HaveCount(1);
        ((LookupResult)lookupResult).Data[0].ToJson().Should().BeEquivalentTo(expectedData.ToJson());
    }

    [Fact]
    public async Task GetLookupsAsync_WithValidLookupRequestAndHasJsonTranslations_ShouldReturnCorrectResult()
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
                    DynamicFetch = new DynamicFetchConfiguration
                    {
                        EntityAssemblyName = "Milvasoft.IntegrationTests.Client",
                        AllowedEntityNamesForLookup = [nameof(HasJsonTranslationEntityFixture)],
                        MaxAllowedPropertyCountForLookup = 2
                    }
                };
            });

            services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
            services.RemoveAll<MilvaBulkDbContextFixture>();

            var dataSource = new NpgsqlDataSourceBuilder(_factory.GetConnectionString()).EnableDynamicJson().Build();

            services.AddDbContext<MilvaBulkDbContextFixture>(x =>
            {
                x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                x.UseNpgsql(dataSource)
                 .ReplaceService<IModelCustomizer, TranslationRelationsModelCustomizer>();
            });

            services.AddMilvaMultiLanguage().WithDefaultMultiLanguageManager();
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var entity = new HasJsonTranslationEntityFixture
        {
            Id = 1,
            Priority = 1,
            Translations =
            [
                new()
                {
                    Id = 1,
                    LanguageId = 1,
                    Name = "türkçe",
                    EntityId = 1,
                },
                new()
                {
                    Id = 2,
                    LanguageId = 2,
                    Name = "English",
                    EntityId = 1,
                }
            ]
        };
        await dbContext.HasJsonTranslationEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        var lookupRequest = new LookupRequest
        {
            Parameters =
            [
                new()
                {
                    EntityName = nameof(HasJsonTranslationEntityFixture),
                    RequestedPropertyNames = [nameof(HasJsonTranslationEntityFixture.Priority), "Name"],
                }
            ]
        };
        object expectedData = new
        {
            Priority = 1,
            Name = "türkçe",
            Id = 1,
        };

        using var _ = new CultureSwitcher("tr-TR");

        // Act
        var result = await dbContext.GetLookupsAsync(lookupRequest);

        // Assert
        result.Should().HaveCount(1);
        var lookupResult = result[0];
        lookupResult.Should().BeOfType<LookupResult>();
        ((LookupResult)lookupResult).EntityName.Should().Be(nameof(HasJsonTranslationEntityFixture));
        ((LookupResult)lookupResult).Data.Should().HaveCount(1);
        ((LookupResult)lookupResult).Data[0].ToJson().Should().BeEquivalentTo(expectedData.ToJson());
    }

    [Fact]
    public async Task GetLookupsAsync_WithValidLookupRequestAndMilvaTenant_ShouldReturnCorrectResult()
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
                    DynamicFetch = new DynamicFetchConfiguration
                    {
                        EntityAssemblyName = "Milvasoft.IntegrationTests.Client",
                        AllowedEntityNamesForLookup = [nameof(SomeTenantEntity)],
                        MaxAllowedPropertyCountForLookup = 2
                    }
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var entity = new SomeTenantEntity("milvasoft", 1)
        {
            SomeStringProp = "stringprop",
        };
        await dbContext.SomeTenantEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        var lookupRequest = new LookupRequest
        {
            Parameters =
            [
                new()
                {
                    EntityName = nameof(SomeTenantEntity),
                    RequestedPropertyNames = [nameof(SomeTenantEntity.SomeStringProp)],
                }
            ]
        };
        object expectedData = new { SomeStringProp = "stringprop", Id = "milvasoft_1" };

        // Act
        var result = await dbContext.GetLookupsAsync(lookupRequest);

        // Assert
        result.Should().HaveCount(1);
        var lookupResult = result[0];
        lookupResult.Should().BeOfType<LookupResult>();
        ((LookupResult)lookupResult).EntityName.Should().Be(nameof(SomeTenantEntity));
        ((LookupResult)lookupResult).Data.Should().HaveCount(1);
        ((LookupResult)lookupResult).Data[0].ToJson().Should().BeEquivalentTo(expectedData.ToJson());
    }

    /// <summary>
    /// Entity property values request
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> InvalidRequestForGetPropertyValuesAsyncMethod()
    {
        yield return new object[]
        {
            new EntityPropertyValuesRequest { EntityName = null }
        };

        yield return new object[]
        {
            new EntityPropertyValuesRequest { EntityName = "" }
        };

        yield return new object[]
        {
            new EntityPropertyValuesRequest { EntityName = " " }
        };

        yield return new object[]
        {
            new EntityPropertyValuesRequest { EntityName = nameof(SomeBaseEntityFixture), PropertyName = null }
        };

        yield return new object[]
        {
            new EntityPropertyValuesRequest { EntityName = nameof(SomeBaseEntityFixture), PropertyName = "" }
        };

        // Restricted entity name
        yield return new object[]
        {
            new EntityPropertyValuesRequest
            {
                EntityName = nameof(SomeEntityFixture),
                PropertyName = nameof(SomeEntityFixture.Id)
            }
        };
    }

    [Theory]
    [MemberData(nameof(InvalidRequestForGetPropertyValuesAsyncMethod))]
    public async Task GetPropertyValuesAsync_WithInvalidRequest_ShouldThrowException(EntityPropertyValuesRequest request)
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
                    DynamicFetch = new DynamicFetchConfiguration
                    {
                        AllowedEntityNamesForLookup = [nameof(SomeBaseEntityFixture)],
                        MaxAllowedPropertyCountForLookup = 3
                    }
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        // Act
        Func<Task> act = async () => await dbContext.GetPropertyValuesAsync(request);

        // Assert
        await act.Should().ThrowAsync<MilvaUserFriendlyException>();
    }

    [Fact]
    public async Task GetPropertyValuesAsync_WithValidRequest_ShouldReturnCorrectResult()
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
                    DynamicFetch = new DynamicFetchConfiguration
                    {
                        EntityAssemblyName = "Milvasoft.IntegrationTests.Client",
                        AllowedEntityNamesForLookup = [nameof(SomeFullAuditableEntityFixture)],
                        MaxAllowedPropertyCountForLookup = 2
                    }
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var entities = new List<SomeFullAuditableEntityFixture>
        {
            new() {
                Id = 1,
                SomeStringProp = "stringprop",
                SomeDecimalProp = 10M
            },
            new() {
                Id = 2,
                SomeStringProp = "stringprop2",
                SomeDecimalProp = 20M
            },
            new() {
                Id = 3,
                SomeStringProp = "stringprop2",
                SomeDecimalProp = 30M
            },
            new() {
                Id = 4,
                SomeStringProp = "deletedstringprop2",
                SomeDecimalProp = 30M,
                IsDeleted = true
            }
        };

        // Act & Assert
        await dbContext.FullAuditableEntities.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();

        var request = new EntityPropertyValuesRequest
        {
            EntityName = nameof(SomeFullAuditableEntityFixture),
            PropertyName = nameof(SomeFullAuditableEntityFixture.SomeStringProp)
        };

        // Act
        var result = await dbContext.GetPropertyValuesAsync(request);

        // Assert
        result.Should().HaveCount(2);
        var lookupResult = result[0];
        lookupResult.Should().BeOfType<string>();
        result.Should().Contain("stringprop");
        result.Should().Contain("stringprop2");
    }

    [Fact]
    public async Task GetRequiredContentsAsync_WithValidParameters_ShouldReturnCorrectResult()
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
                    DynamicFetch = new DynamicFetchConfiguration
                    {
                        EntityAssemblyName = "Milvasoft.UnitTests",
                        AllowedEntityNamesForLookup = [nameof(SomeBaseEntityFixture)],
                        MaxAllowedPropertyCountForLookup = 2
                    }
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var entity = new SomeBaseEntityFixture
        {
            Id = 1,
            SomeStringProp = "stringprop",
            SomeDecimalProp = 10M
        };
        await dbContext.BaseEntities.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.GetContentsAsync<SomeBaseEntityFixture>(null, null, null);

        // Assert
        result.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetEntityPropertyValuesAsync_WithInvalidRequest_ShouldThrowException(string propertyName)
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
                    DynamicFetch = new DynamicFetchConfiguration
                    {
                        AllowedEntityNamesForLookup = [nameof(SomeBaseEntityFixture)],
                        MaxAllowedPropertyCountForLookup = 3
                    }
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        // Act
        Func<Task> act = async () => await dbContext.GetEntityPropertyValuesAsync<SomeEntityFixture, int>(propertyName, null, null);

        // Assert
        await act.Should().ThrowAsync<MilvaDeveloperException>();
    }

    [Fact]
    public async Task GetEntityPropertyValuesAsync_WithValidRequest_ShouldReturnCorrectResult()
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
                    DynamicFetch = new DynamicFetchConfiguration
                    {
                        EntityAssemblyName = "Milvasoft.IntegrationTests.Client",
                        AllowedEntityNamesForLookup = [nameof(SomeFullAuditableEntityFixture)],
                        MaxAllowedPropertyCountForLookup = 2
                    }
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        var entities = new List<SomeFullAuditableEntityFixture>
        {
            new() {
                Id = 1,
                SomeStringProp = "stringprop",
                SomeDecimalProp = 10M
            },
            new() {
                Id = 2,
                SomeStringProp = "stringprop2",
                SomeDecimalProp = 20M
            },
            new() {
                Id = 3,
                SomeStringProp = "stringprop2",
                SomeDecimalProp = 30M
            },
            new() {
                Id = 4,
                SomeStringProp = "deletedstringprop2",
                SomeDecimalProp = 30M,
                IsDeleted = true
            }
        };

        // Act & Assert
        await dbContext.FullAuditableEntities.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.GetEntityPropertyValuesAsync<SomeFullAuditableEntityFixture, string>(nameof(SomeFullAuditableEntityFixture.SomeStringProp), null, null);

        // Assert
        result.Should().HaveCount(2);
        var lookupResult = result[0];
        lookupResult.Should().BeOfType<string>();
        result.Should().Contain("stringprop");
        result.Should().Contain("stringprop2");
    }

    #endregion

    #region GetUpdatablePropertiesBuilder

    [Fact]
    public async Task GetUpdatablePropertiesBuilder_WithInvalidDto_ShouldReturnNull()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext.UseUtcForDateTime = true;
                opt.Auditing = new AuditConfiguration
                {
                    AuditModificationDate = false,
                    AuditModifier = false
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        UpdatedPropsTestInvalidDto dto = new UpdatedPropsTestInvalidDto
        {
            Id = 1,
            Name = "john",
            Price = 10M
        };

        // Act
        var result = dbContext.GetUpdatablePropertiesBuilder<RestTestEntityFixture, UpdatedPropsTestInvalidDto>(dto);

        // Assert
        result.Should().NotBeNull();
        result.SetPropertyCallsLog.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUpdatablePropertiesBuilder_WithValidDtoButNull_ShouldReturnNull()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext.UseUtcForDateTime = true;
                opt.Auditing = new AuditConfiguration
                {
                    AuditModificationDate = false,
                    AuditModifier = false
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        UpdatedPropsTestDto dto = null;

        // Act
        var result = dbContext.GetUpdatablePropertiesBuilder<RestTestEntityFixture, UpdatedPropsTestDto>(dto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUpdatablePropertiesBuilder_WithAuditModificationDateAndModifierUserFalseInDataAccessConfiguration_ShouldReturnCorrectResult()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext.UseUtcForDateTime = true;
                opt.Auditing = new AuditConfiguration
                {
                    AuditModificationDate = false,
                    AuditModifier = false
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        var now = DateTime.Now;
        UpdatedPropsTestDto dto = new()
        {
            Id = 1,
            Name = "john",
            UpdateDate = now
        };

        // Act
        var result = dbContext.GetUpdatablePropertiesBuilder<RestTestEntityFixture, UpdatedPropsTestDto>(dto);

        // Assert
        result.Should().NotBeNull();
        result.SetPropertyCallsLog.Should().NotBeEmpty();
        result.SetPropertyCallsLog.Should().NotContain(EntityPropertyNames.LastModificationDate);
        result.SetPropertyCallsLog.Should().NotContain(EntityPropertyNames.LastModifierUserName);
    }

    [Fact]
    public async Task GetUpdatablePropertiesBuilder_WithAuditModificationDateTrueInDataAccessConfiguration_ShouldReturnCorrectResult()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext.UseUtcForDateTime = true;
                opt.Auditing = new AuditConfiguration
                {
                    AuditModificationDate = true,
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        var now = DateTime.Now;
        UpdatedPropsTestDto dto = new()
        {
            Id = 1,
            Name = "john",
            UpdateDate = now
        };

        // Act
        var result = dbContext.GetUpdatablePropertiesBuilder<RestTestEntityFixture, UpdatedPropsTestDto>(dto);

        // Assert
        result.Should().NotBeNull();
        result.SetPropertyCallsLog.Should().NotBeEmpty();
        result.SetPropertyCallsLog.Should().Contain(EntityPropertyNames.LastModificationDate);
    }

    [Fact]
    public async Task GetUpdatablePropertiesBuilder_WithAuditModifierUserTrueButGetCurrentUsernameMethodIsNullInDataAccessConfiguration_ShouldReturnCorrectResult()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext.UseUtcForDateTime = true;
                opt.Auditing = new AuditConfiguration
                {
                    AuditModificationDate = false,
                    AuditModifier = true
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        var now = DateTime.Now;
        UpdatedPropsTestDto dto = new()
        {
            Id = 1,
            Name = "john",
            UpdateDate = now
        };

        // Act
        var result = dbContext.GetUpdatablePropertiesBuilder<RestTestEntityFixture, UpdatedPropsTestDto>(dto);

        // Assert
        result.Should().NotBeNull();
        result.SetPropertyCallsLog.Should().NotBeEmpty();
        result.SetPropertyCallsLog.Should().NotContain(EntityPropertyNames.LastModificationDate);
        result.SetPropertyCallsLog.Should().NotContain(EntityPropertyNames.LastModifierUserName);
    }

    [Fact]
    public async Task GetUpdatablePropertiesBuilder_WithAuditModifierUserTrueAndGetCurrentUsernameMethodIsNotNullInDataAccessConfiguration_ShouldReturnCorrectResult()
    {
        // Arrange
        await InitializeAsync(services =>
        {
            services.ConfigureMilvaDataAccess(opt =>
            {
                opt.DbContext = new DbContextConfiguration
                {
                    UseUtcForDateTime = true,
                    GetCurrentUserNameMethod = (sp) => "testuser"
                };
                opt.Auditing = new AuditConfiguration
                {
                    AuditModificationDate = false,
                    AuditModifier = true
                };
            });
        });

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();
        var now = DateTime.Now;
        UpdatedPropsTestDto dto = new()
        {
            Id = 1,
            Name = "john",
            UpdateDate = now
        };

        // Act
        var result = dbContext.GetUpdatablePropertiesBuilder<RestTestEntityFixture, UpdatedPropsTestDto>(dto);

        // Assert
        result.Should().NotBeNull();
        result.SetPropertyCallsLog.Should().NotBeEmpty();
        result.SetPropertyCallsLog.Should().NotContain(EntityPropertyNames.LastModificationDate);
        result.SetPropertyCallsLog.Should().Contain(EntityPropertyNames.LastModifierUserName);
    }

    #endregion
}