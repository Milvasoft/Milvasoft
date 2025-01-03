using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Helpers;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.Utils.Enums;
using Milvasoft.DataAccess.EfCore.Utils.LookupModels;
using Milvasoft.IntegrationTests.Client.Fixtures.DtoFixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.Persistence;
using System.Linq.Expressions;

namespace Milvasoft.IntegrationTests.DataAccessTests.EfCoreTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
[Collection(nameof(UtcTrueDatabaseTestCollection))]
[Trait("EF Core Data Access Integration Tests", "Integration tests for Milvasoft.DataAccess.EfCore integration tests.")]
public class DbContextTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
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

                services.AddDbContext<MilvaBulkDbContextFixture>(x =>
                {
                    x.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });
                    x.UseNpgsql(_factory.GetConnectionString());
                });

            });

            builder.Configure(app =>
            {
                configureApp?.Invoke(app);
            });
        });

        _serviceProvider = waf.Services.CreateScope().ServiceProvider;

        await _factory.CreateRespawner();
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
            SomeDecimalProp = 10M
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
                    RequestedPropertyNames = [nameof(SomeBaseEntityFixture.SomeDecimalProp)],
                }
            ]
        };
        object expectedData = new { SomeDecimalProp = 10M, Id = 1 };

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
        var entity2 = new SomeBaseEntityFixture
        {
            Id = 2,
            SomeStringProp = "stringprop2",
            SomeDecimalProp = 20M
        };
        var entity3 = new SomeBaseEntityFixture
        {
            Id = 3,
            SomeStringProp = "stringprop2",
            SomeDecimalProp = 30M
        };
        await dbContext.BaseEntities.AddAsync(entity);
        await dbContext.BaseEntities.AddAsync(entity2);
        await dbContext.BaseEntities.AddAsync(entity3);
        await dbContext.SaveChangesAsync();

        var request = new EntityPropertyValuesRequest
        {
            EntityName = nameof(SomeBaseEntityFixture),
            PropertyName = nameof(SomeBaseEntityFixture.SomeStringProp)
        };
        List<object> expectedData = ["stringprop", "stringprop2"];

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
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> expectedExpression = i => i;

        // Act
        var result = dbContext.GetUpdatablePropertiesBuilder<RestTestEntityFixture, UpdatedPropsTestInvalidDto>(dto);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expectedExpression, result.SetPropertyCalls);

        equality.Should().BeTrue();
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
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> notExpectedExpression = i => i;

        // Act
        var result = dbContext.GetUpdatablePropertiesBuilder<RestTestEntityFixture, UpdatedPropsTestDto>(dto);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(notExpectedExpression, result.SetPropertyCalls);
        equality.Should().BeFalse();
        result.SetPropertyCalls.Body.ToString().Should().NotContain(EntityPropertyNames.LastModificationDate);
        result.SetPropertyCalls.Body.ToString().Should().NotContain(EntityPropertyNames.LastModifierUserName);
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
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> notExpectedExpression = i => i;

        // Act
        var result = dbContext.GetUpdatablePropertiesBuilder<RestTestEntityFixture, UpdatedPropsTestDto>(dto);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(notExpectedExpression, result.SetPropertyCalls);
        equality.Should().BeFalse();
        result.SetPropertyCalls.Body.ToString().Should().Contain(EntityPropertyNames.LastModificationDate);
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
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> notExpectedExpression = i => i;

        // Act
        var result = dbContext.GetUpdatablePropertiesBuilder<RestTestEntityFixture, UpdatedPropsTestDto>(dto);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(notExpectedExpression, result.SetPropertyCalls);
        equality.Should().BeFalse();
        result.SetPropertyCalls.Body.ToString().Should().NotContain(EntityPropertyNames.LastModificationDate);
        result.SetPropertyCalls.Body.ToString().Should().NotContain(EntityPropertyNames.LastModifierUserName);
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
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> notExpectedExpression = i => i;

        // Act
        var result = dbContext.GetUpdatablePropertiesBuilder<RestTestEntityFixture, UpdatedPropsTestDto>(dto);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(notExpectedExpression, result.SetPropertyCalls);
        equality.Should().BeFalse();
        result.SetPropertyCalls.Body.ToString().Should().NotContain(EntityPropertyNames.LastModificationDate);
        result.SetPropertyCalls.Body.ToString().Should().Contain(EntityPropertyNames.LastModifierUserName);
    }

    #endregion
}