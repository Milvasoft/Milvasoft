using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Helpers;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.Utils.Enums;
using Milvasoft.DataAccess.EfCore.Utils.LookupModels;
using Milvasoft.Interception.Interceptors.ActivityScope;
using Milvasoft.UnitTests.DataAccessTests.EfCoreTests.Fixtures;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests.DbContextBaseTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
[Trait("EF Core Data Access Unit Tests", "Unit tests for Milvasoft.DataAccess.EfCore unit testable parts.")]
public class MilvaDbContextTests
{
    #region Auditing

    [Fact]
    public async Task Auditing_WithAuditCreatorButCurrentUsernameDelegateIsNull_ShouldNotAudit()
    {
        // Arrange
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                GetCurrentUserNameMethod = null
            },
            Auditing = new AuditConfiguration
            {
                AuditCreator = true,
                AuditCreationDate = false,
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        ResetDatabase(dbContext);
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
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Active,
                GetCurrentUserNameMethod = (sp) => "testuser"
            },
            Auditing = new AuditConfiguration
            {
                AuditCreationDate = true,
                AuditCreator = true,
                AuditModificationDate = true,
                AuditModifier = true,
                AuditDeletionDate = true,
                AuditDeleter = true
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        ResetDatabase(dbContext);
        var now = DateTime.Now;
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
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Active,
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        ResetDatabase(dbContext);
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
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Passive,
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        ResetDatabase(dbContext);
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
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Active,
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        ResetDatabase(dbContext);
        var now = DateTime.Now;
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
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Active,
                ResetSoftDeleteStateAfterEveryOperation = true
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        ResetDatabase(dbContext);
        var now = DateTime.Now;
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
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Active,
                ResetSoftDeleteStateAfterEveryOperation = false
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        ResetDatabase(dbContext);
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
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Active,
                ResetSoftDeleteStateAfterEveryOperation = false
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        ResetDatabase(dbContext);
        var now = DateTime.Now;
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
    public void GetDataAccessConfiguration_ShouldReturnCorrectValue()
    {
        // Arrange
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Active,
                ResetSoftDeleteStateAfterEveryOperation = false
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();

        // Act & Assert
        var conf = dbContext.GetDataAccessConfiguration();
        conf.DbContext.ResetSoftDeleteStateAfterEveryOperation.Should().BeFalse();
    }

    [Fact]
    public void SetDataAccessConfiguration_ShouldUpdateCorrectly()
    {
        // Arrange
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Active,
                ResetSoftDeleteStateAfterEveryOperation = false
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();

        // Act & Assert
        var conf = dbContext.GetDataAccessConfiguration();
        conf.DbContext.ResetSoftDeleteStateAfterEveryOperation.Should().BeFalse();

        conf.DbContext.GetCurrentUserNameMethod = (sp) => "user";

        dbContext.SetDataAccessConfiguration(conf);

        conf = dbContext.GetDataAccessConfiguration();
        conf.DbContext.GetCurrentUserNameMethod.Should().NotBeNull();
    }

    [Fact]
    public void GetCurrentSoftDeletionState_ShouldReturnCorrectValue()
    {
        // Arrange
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Active,
                ResetSoftDeleteStateAfterEveryOperation = false
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();

        // Act & Assert
        var currentSoftDeleteState = dbContext.GetCurrentSoftDeletionState();
        currentSoftDeleteState.Should().Be(SoftDeletionState.Active);
    }

    [Fact]
    public void ChangeSoftDeletionState_ShouldUpdateCorrectly()
    {
        // Arrange
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Active,
                ResetSoftDeleteStateAfterEveryOperation = false
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();

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
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Active,
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        ResetDatabase(dbContext);
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
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Active,
                DynamicFetch = new DynamicFetchConfiguration
                {
                    AllowedEntityNamesForLookup = [nameof(SomeBaseEntityFixture)],
                    MaxAllowedPropertyCountForLookup = 3
                }
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        ResetDatabase(dbContext);

        // Act
        Func<Task> act = async () => await dbContext.GetLookupsAsync(lookupRequest);

        // Assert
        await act.Should().ThrowAsync<MilvaUserFriendlyException>();
    }

    [Fact]
    public async Task GetLookupsAsync_WithValidLookupRequest_ShouldReturnCorrectResult()
    {
        // Arrange
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Active,
                DynamicFetch = new DynamicFetchConfiguration
                {
                    EntityAssemblyName = "Milvasoft.UnitTests",
                    AllowedEntityNamesForLookup = [nameof(SomeBaseEntityFixture)],
                    MaxAllowedPropertyCountForLookup = 2
                }
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        ResetDatabase(dbContext);
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

    [Fact]
    public async Task GetRequiredContentsAsync_WithValidParameters_ShouldReturnCorrectResult()
    {
        // Arrange
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Active,
                DynamicFetch = new DynamicFetchConfiguration
                {
                    EntityAssemblyName = "Milvasoft.UnitTests",
                    AllowedEntityNamesForLookup = [nameof(SomeBaseEntityFixture)],
                    MaxAllowedPropertyCountForLookup = 2
                }
            }
        };

        var services = GetServices(dataAccessConfiguration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        ResetDatabase(dbContext);
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

    #region Setup

    public class SomeClass : IInterceptable
    {
        [ActivityStarter("TestActivity")]
        public virtual string MethodWithActivity() => ActivityHelper.OperationName;

        public virtual string MethodWithNoActivity() => ActivityHelper.OperationName;
    }

    private static ServiceProvider GetServices(DataAccessConfiguration configuration)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDataAccessConfiguration>(configuration);

        services.AddDbContext<SomeMilvaDbContextFixture>(opt =>
        {
            opt.UseInMemoryDatabase(databaseName: $"MilvaDbContextTestDbInMemory_{Guid.NewGuid}_{DateTime.Now.Nanosecond}");
        });

        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider;
    }

    private static void ResetDatabase(SomeMilvaDbContextFixture dbContextFixture)
    {
        dbContextFixture.Database.EnsureDeleted();
        dbContextFixture.Database.EnsureCreated();
    }

    #endregion
}
