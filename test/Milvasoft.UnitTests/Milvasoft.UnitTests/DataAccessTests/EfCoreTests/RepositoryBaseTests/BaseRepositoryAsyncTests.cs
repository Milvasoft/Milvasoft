using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;
using Milvasoft.DataAccess.EfCore.Utils.Enums;
using Milvasoft.Helpers.DataAccess.EfCore.Concrete;
using Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;
using Milvasoft.UnitTests.CoreTests.HelperTests.CommonTests.Fixtures;
using Milvasoft.UnitTests.DataAccessTests.EfCoreTests.Fixtures;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.DataAccessTests.EfCoreTests.RepositoryBaseTests;

[Trait("EF Core Data Access Unit Tests", "Unit tests for Milvasoft.DataAccess.EfCore unit testable parts.")]
public class BaseRepositoryAsyncTests
{
    #region Configuration Based Tests

    [Fact]
    public async Task DataFetch_WithDefaultConfiguration_ShouldNotReturnSoftDeletedData()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var getAllData = await entityRepository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);
        var getSomeData = await entityRepository.GetSomeAsync(4, cancellationToken: TestContext.Current.CancellationToken);
        var firstData = await entityRepository.GetFirstOrDefaultAsync(i => i.Id == 4, cancellationToken: TestContext.Current.CancellationToken);
        var singleData = await entityRepository.GetSingleOrDefaultAsync(i => i.Id == 4, cancellationToken: TestContext.Current.CancellationToken);
        var getByIdData = await entityRepository.GetByIdAsync(4, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        getAllData.Should().HaveCount(3);
        getSomeData.Should().HaveCount(3);
        firstData.Should().BeNull();
        singleData.Should().BeNull();
        getByIdData.Should().BeNull();
    }

    [Fact]
    public async Task DataFetch_FetchSoftDeletedEntities_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsFalse_ShouldReturnSoftDeletedData()
    {
        // Arrange
        var configuration = new DataAccessConfiguration
        {
            Repository = new RepositoryConfiguration
            {
                DefaultSoftDeletedFetchState = false,
                ResetSoftDeletedFetchStateAfterEveryOperation = false,
            }
        };
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);
        entityRepository.FetchSoftDeletedEntities(true);

        // Act
        var getAllData = await entityRepository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);
        var getSomeData = await entityRepository.GetSomeAsync(4, cancellationToken: TestContext.Current.CancellationToken);
        var firstData = await entityRepository.GetFirstOrDefaultAsync(i => i.Id == 4, cancellationToken: TestContext.Current.CancellationToken);
        var singleData = await entityRepository.GetSingleOrDefaultAsync(i => i.Id == 4, cancellationToken: TestContext.Current.CancellationToken);
        var getByIdData = await entityRepository.GetByIdAsync(4, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        getAllData.Should().HaveCount(4);
        getSomeData.Should().HaveCount(4);
        firstData.Should().NotBeNull();
        singleData.Should().NotBeNull();
        getByIdData.Should().NotBeNull();
    }

    [Fact]
    public async Task DataFetch_ResetSoftDeletedEntityFetchState_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsFalse_ShouldNotReturnSoftDeletedDataAfterSecondOperation()
    {
        // Arrange
        var configuration = new DataAccessConfiguration
        {
            Repository = new RepositoryConfiguration
            {
                DefaultSoftDeletedFetchState = false,
                ResetSoftDeletedFetchStateAfterEveryOperation = true,
            }
        };
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);
        entityRepository.FetchSoftDeletedEntities(true);
        entityRepository.SoftDeleteFetchStateResetAfterOperation(false);

        // Act
        var getAllData = await entityRepository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);
        entityRepository.ResetSoftDeletedEntityFetchResetState();
        var getSomeData = await entityRepository.GetSomeAsync(4, cancellationToken: TestContext.Current.CancellationToken);
        var firstData = await entityRepository.GetFirstOrDefaultAsync(i => i.Id == 4, cancellationToken: TestContext.Current.CancellationToken);
        var singleData = await entityRepository.GetSingleOrDefaultAsync(i => i.Id == 4, cancellationToken: TestContext.Current.CancellationToken);
        var getByIdData = await entityRepository.GetByIdAsync(4, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        getAllData.Should().HaveCount(4);
        getSomeData.Should().HaveCount(4);
        firstData.Should().BeNull();
        singleData.Should().BeNull();
        getByIdData.Should().BeNull();
    }

    [Fact]
    public async Task DataFetch_FetchSoftDeletedEntities_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsTrue_ShouldNotReturnSoftDeletedDataAfterFirstOperation()
    {
        // Arrange
        var configuration = new DataAccessConfiguration
        {
            Repository = new RepositoryConfiguration
            {
                DefaultSoftDeletedFetchState = false,
                ResetSoftDeletedFetchStateAfterEveryOperation = true,
            }
        };
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);
        entityRepository.FetchSoftDeletedEntities(true);

        // Act
        var getAllData = await entityRepository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);
        var getSomeData = await entityRepository.GetSomeAsync(4, cancellationToken: TestContext.Current.CancellationToken);
        var firstData = await entityRepository.GetFirstOrDefaultAsync(i => i.Id == 4, cancellationToken: TestContext.Current.CancellationToken);
        var singleData = await entityRepository.GetSingleOrDefaultAsync(i => i.Id == 4, cancellationToken: TestContext.Current.CancellationToken);
        var getByIdData = await entityRepository.GetByIdAsync(4, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        getAllData.Should().HaveCount(4);
        getSomeData.Should().HaveCount(3);
        firstData.Should().BeNull();
        singleData.Should().BeNull();
        getByIdData.Should().BeNull();
    }

    [Fact]
    public async Task DataFetch_SoftDeleteFetchStateResetAfterOperation_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsFalse_ShouldNotReturnSoftDeletedDataAfterFirstOperation()
    {
        // Arrange
        var configuration = new DataAccessConfiguration
        {
            Repository = new RepositoryConfiguration
            {
                DefaultSoftDeletedFetchState = false,
                ResetSoftDeletedFetchStateAfterEveryOperation = false,
            }
        };
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);
        entityRepository.FetchSoftDeletedEntities(true);
        entityRepository.SoftDeleteFetchStateResetAfterOperation(true);

        // Act
        var getAllData = await entityRepository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);
        var getSomeData = await entityRepository.GetSomeAsync(4, cancellationToken: TestContext.Current.CancellationToken);
        var firstData = await entityRepository.GetFirstOrDefaultAsync(i => i.Id == 4, cancellationToken: TestContext.Current.CancellationToken);
        var singleData = await entityRepository.GetSingleOrDefaultAsync(i => i.Id == 4, cancellationToken: TestContext.Current.CancellationToken);
        var getByIdData = await entityRepository.GetByIdAsync(4, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        getAllData.Should().HaveCount(4);
        getSomeData.Should().HaveCount(3);
        firstData.Should().BeNull();
        singleData.Should().BeNull();
        getByIdData.Should().BeNull();
    }

    [Fact]
    public async Task DataFetch_ResetSoftDeletedEntityFetchState_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsFalse_ShouldNotReturnSoftDeletedDataAfterFirstOperation()
    {
        // Arrange
        var configuration = new DataAccessConfiguration
        {
            Repository = new RepositoryConfiguration
            {
                DefaultSoftDeletedFetchState = false,
                ResetSoftDeletedFetchStateAfterEveryOperation = false,
            }
        };
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);
        entityRepository.FetchSoftDeletedEntities(true);
        entityRepository.SoftDeleteFetchStateResetAfterOperation(true);
        entityRepository.ResetSoftDeletedEntityFetchState();

        // Act
        var getAllData = await entityRepository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);
        var getSomeData = await entityRepository.GetSomeAsync(4, cancellationToken: TestContext.Current.CancellationToken);
        var firstData = await entityRepository.GetFirstOrDefaultAsync(i => i.Id == 4, cancellationToken: TestContext.Current.CancellationToken);
        var singleData = await entityRepository.GetSingleOrDefaultAsync(i => i.Id == 4, cancellationToken: TestContext.Current.CancellationToken);
        var getByIdData = await entityRepository.GetByIdAsync(4, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        getAllData.Should().HaveCount(3);
        getSomeData.Should().HaveCount(3);
        firstData.Should().BeNull();
        singleData.Should().BeNull();
        getByIdData.Should().BeNull();
    }

    [Fact]
    public async Task DataManipulation_WithDefaultSaveChangeChoise_ShouldSaveChangesAfterEveryOperation()
    {
        // Arrange
        var configuration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Passive,
            },
            Repository = new RepositoryConfiguration
            {
                DefaultSaveChangesChoice = SaveChangesChoice.AfterEveryOperation,
            }
        };
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act & Assert
        var dataBeforeUpdate = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        dataBeforeUpdate.SomeStringProp = "somestring11";
        dataBeforeUpdate.CreationDate.Should().NotBeNull();

        await entityRepository.UpdateAsync(dataBeforeUpdate, cancellationToken: TestContext.Current.CancellationToken);

        var dataAfterUpdate = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        dataAfterUpdate.SomeStringProp.Should().Be("somestring11");
        dataAfterUpdate.CreationDate.Should().NotBeNull();
        dataAfterUpdate.LastModificationDate.Should().NotBeNull();

        await entityRepository.DeleteAsync(dataAfterUpdate, cancellationToken: TestContext.Current.CancellationToken);

        var dataAfterDelete = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        dataAfterDelete.Should().BeNull();
    }

    [Fact]
    public async Task DataManipulation_WithManualSaveChangeChoise_ShouldSaveChangesAfterEveryOperation()
    {
        // Arrange
        var configuration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Passive,
            },
            Repository = new RepositoryConfiguration
            {
                DefaultSaveChangesChoice = SaveChangesChoice.Manual,
            }
        };
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act & Assert
        var dataBeforeUpdate = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        dataBeforeUpdate.SomeStringProp = "somestring11";
        dataBeforeUpdate.CreationDate.Should().NotBeNull();

        await entityRepository.UpdateAsync(dataBeforeUpdate, cancellationToken: TestContext.Current.CancellationToken);

        var dataAfterUpdate = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        dataAfterUpdate.SomeStringProp.Should().Be("somestring1");
        dataAfterUpdate.CreationDate.Should().NotBeNull();
        dataAfterUpdate.LastModificationDate.Should().BeNull();

        await entityRepository.DeleteAsync(dataAfterUpdate, cancellationToken: TestContext.Current.CancellationToken);

        var dataAfterDelete = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);
        dataAfterDelete.Should().NotBeNull();

        await entityRepository.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var dataAfterSaveChanges = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);
        dataAfterSaveChanges.Should().BeNull();
    }

    [Fact]
    public async Task DataManipulation_ChangeSaveChangesChoiceToManual_WithDefaultSaveChangeChoise_ShouldSaveChangesAfterEveryOperation()
    {
        // Arrange
        var configuration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Passive,
            },
            Repository = new RepositoryConfiguration
            {
                DefaultSaveChangesChoice = SaveChangesChoice.AfterEveryOperation,
            }
        };
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);
        entityRepository.ChangeSaveChangesChoice(SaveChangesChoice.Manual);

        // Act & Assert
        var dataBeforeUpdate = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        dataBeforeUpdate.SomeStringProp = "somestring11";
        dataBeforeUpdate.CreationDate.Should().NotBeNull();

        await entityRepository.UpdateAsync(dataBeforeUpdate, cancellationToken: TestContext.Current.CancellationToken);

        var dataAfterUpdate = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        dataAfterUpdate.SomeStringProp.Should().Be("somestring1");
        dataAfterUpdate.CreationDate.Should().NotBeNull();
        dataAfterUpdate.LastModificationDate.Should().BeNull();

        await entityRepository.DeleteAsync(dataAfterUpdate, cancellationToken: TestContext.Current.CancellationToken);

        var dataAfterDelete = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);
        dataAfterDelete.Should().NotBeNull();

        await entityRepository.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);

        var dataAfterSaveChanges = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);
        dataAfterSaveChanges.Should().BeNull();
    }

    [Fact]
    public async Task DataManipulation_ResetSaveChangesChoiceToDefault_WithDefaultSaveChangeChoise_ShouldSaveChangesAfterEveryOperation()
    {
        // Arrange
        var configuration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                DefaultSoftDeletionState = SoftDeletionState.Passive,
            },
            Repository = new RepositoryConfiguration
            {
                DefaultSaveChangesChoice = SaveChangesChoice.AfterEveryOperation,
            }
        };
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);
        entityRepository.ChangeSaveChangesChoice(SaveChangesChoice.Manual);

        // Act & Assert
        var dataBeforeUpdate = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        dataBeforeUpdate.SomeStringProp = "somestring11";
        dataBeforeUpdate.CreationDate.Should().NotBeNull();

        await entityRepository.UpdateAsync(dataBeforeUpdate, cancellationToken: TestContext.Current.CancellationToken);

        var dataAfterUpdate = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        dataAfterUpdate.SomeStringProp.Should().Be("somestring1");
        dataAfterUpdate.CreationDate.Should().NotBeNull();
        dataAfterUpdate.LastModificationDate.Should().BeNull();

        entityRepository.ResetSaveChangesChoiceToDefault();

        await entityRepository.DeleteAsync(dataAfterUpdate, cancellationToken: TestContext.Current.CancellationToken);

        var dataAfterDelete = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);
        dataAfterDelete.Should().BeNull();
    }

    #endregion

    #region GetFirstOrDefaultAsync

    [Fact]
    public async Task GetFirstOrDefaultAsync_WithoutParameters_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetFirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.SomeStringProp.Should().Be("somestring1");
    }

    [Fact]
    public async Task GetFirstOrDefaultAsync_WithCondition_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetFirstOrDefaultAsync(i => i.SomeDecimalProp > 20M, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(3);
        result.SomeStringProp.Should().Be("somestring3");
        result.SomeDecimalProp.Should().Be(30M);
    }

    [Fact]
    public async Task GetFirstOrDefaultAsync_WithProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetFirstOrDefaultAsync(null, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            CreationDate = i.CreationDate,
        }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.SomeStringProp.Should().BeNull();
        result.SomeDecimalProp.Should().Be(default);
    }

    [Fact]
    public async Task GetFirstOrDefaultAsync_WithConditionAndProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetFirstOrDefaultAsync(i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            CreationDate = i.CreationDate,
        }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(3);
        result.SomeStringProp.Should().BeNull();
        result.SomeDecimalProp.Should().Be(default);
    }

    [Fact]
    public async Task GetFirstOrDefaultAsync_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetFirstOrDefaultAsync(i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 1, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetSingleOrDefaultAsync

    [Fact]
    public async Task GetSingleOrDefaultAsync_WithoutParameters_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        Func<Task> act = async () => await entityRepository.GetSingleOrDefaultAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Sequence contains more than one element");
    }

    [Fact]
    public async Task GetSingleOrDefaultAsync_WithCondition_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetSingleOrDefaultAsync(i => i.SomeDecimalProp > 20M, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(3);
        result.SomeStringProp.Should().Be("somestring3");
        result.SomeDecimalProp.Should().Be(30M);
    }

    [Fact]
    public async Task GetSingleOrDefaultAsync_WithConditionAndProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetSingleOrDefaultAsync(i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            CreationDate = i.CreationDate,
        }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(3);
        result.SomeStringProp.Should().BeNull();
        result.SomeDecimalProp.Should().Be(default);
    }

    [Fact]
    public async Task GetSingleOrDefaultAsync_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetSingleOrDefaultAsync(i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 1, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WithoutParameters_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.SomeStringProp.Should().Be("somestring1");
    }

    [Fact]
    public async Task GetByIdAsync_WithCondition_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetByIdAsync(1, i => i.SomeDecimalProp > 20M, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithConditionAndProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetByIdAsync(3, i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            CreationDate = i.CreationDate,
        }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(3);
        result.SomeStringProp.Should().BeNull();
        result.SomeDecimalProp.Should().Be(default);
    }

    [Fact]
    public async Task GetByIdAsync_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetByIdAsync(3, i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 1, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_WithoutParameters_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithCondition_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetAllAsync(i => i.SomeDecimalProp > 10M, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithConditionAndProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetAllAsync(i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].SomeStringProp.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetAllAsync(i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 20M, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].SomeStringProp.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithListRequest_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Sorting = new SortRequest { SortBy = nameof(SomeFullAuditableEntityFixture.CreationDate), Type = Components.Rest.Enums.SortType.Desc },
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(SomeFullAuditableEntityFixture.SomeDecimalProp), Type = Components.Rest.Enums.AggregationType.Sum }] }
        };

        // Act
        var result = await entityRepository.GetAllAsync(listRequest, cancellationToken: TestContext.Current.CancellationToken);

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

    [Fact]
    public async Task GetAllAsync_WithListRequestAndCondition_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Sorting = new SortRequest { SortBy = nameof(SomeFullAuditableEntityFixture.CreationDate), Type = Components.Rest.Enums.SortType.Desc },
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(SomeFullAuditableEntityFixture.SomeDecimalProp), Type = Components.Rest.Enums.AggregationType.Sum }] }
        };

        // Act
        var result = await entityRepository.GetAllAsync(listRequest, i => i.SomeDecimalProp > 10M, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.Should().BeInDescendingOrder(i => i.SomeDecimalProp);
        result.Data[0].SomeStringProp.Should().NotBeNull();
        result.TotalDataCount.Should().Be(2);
        result.TotalPageCount.Should().Be(1);
        result.CurrentPageNumber.Should().Be(1);
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.AggregationResults.Should().HaveCount(1);
        result.AggregationResults[0].Result.Should().Be(50M);
    }

    [Fact]
    public async Task GetAllAsync_WithListRequestAndConditionAndProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Sorting = new SortRequest { SortBy = nameof(SomeFullAuditableEntityFixture.CreationDate), Type = Components.Rest.Enums.SortType.Desc },
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(SomeFullAuditableEntityFixture.SomeDecimalProp), Type = Components.Rest.Enums.AggregationType.Sum }] }
        };

        // Act
        var result = await entityRepository.GetAllAsync(listRequest, i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.Should().BeInDescendingOrder(i => i.SomeDecimalProp);
        result.Data[0].SomeStringProp.Should().BeNull();
        result.TotalDataCount.Should().Be(2);
        result.TotalPageCount.Should().Be(1);
        result.CurrentPageNumber.Should().Be(1);
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.AggregationResults.Should().HaveCount(1);
        result.AggregationResults[0].Result.Should().Be(50M);
    }

    [Fact]
    public async Task GetAllAsync_WithListRequestAndConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Sorting = new SortRequest { SortBy = nameof(SomeFullAuditableEntityFixture.CreationDate), Type = Components.Rest.Enums.SortType.Desc },
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(SomeFullAuditableEntityFixture.SomeDecimalProp), Type = Components.Rest.Enums.AggregationType.Sum }] }
        };

        // Act
        var result = await entityRepository.GetAllAsync(listRequest, i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 20M, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        result.Data.Should().BeInDescendingOrder(i => i.SomeDecimalProp);
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

    [Fact]
    public async Task GetSomeAsync_WithoutParameters_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetSomeAsync(2, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSomeAsync_WithCondition_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetSomeAsync(1, i => i.SomeDecimalProp > 10M, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSomeAsync_WithConditionAndProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetSomeAsync(1, i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].SomeStringProp.Should().BeNull();
    }

    [Fact]
    public async Task GetSomeAsync_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        var result = await entityRepository.GetSomeAsync(3, i => i.SomeDecimalProp > 10M, i => new SomeBaseEntityFixture
        {
            Id = i.Id,
            SomeDecimalProp = i.SomeDecimalProp,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 20M, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].SomeStringProp.Should().BeNull();
    }

    #endregion

    #region AddAsync

    [Fact]
    public async Task AddAsync_WithNullEntity_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAsync(dbContext);

        SomeFullAuditableEntityFixture entity = null;

        // Act
        Func<Task> act = async () => await entityRepository.AddAsync(entity);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AddAsync_WithValidEntity_ShouldAddCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAsync(dbContext);

        var entity = new SomeFullAuditableEntityFixture()
        {
            Id = 1,
            SomeDateProp = DateTime.Now.AddYears(1),
            SomeDecimalProp = 10M,
            SomeStringProp = "somestring1"
        };

        // Act
        await entityRepository.AddAsync(entity, cancellationToken: TestContext.Current.CancellationToken);
        var addedEntity = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        addedEntity.Should().NotBeNull();
        addedEntity.CreationDate.Should().NotBeNull();
    }

    #endregion

    #region AddRangeAsync

    [Fact]
    public async Task AddRangeAsync_WithNullEntityList_ShouldDoNothing()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAsync(dbContext);

        List<SomeFullAuditableEntityFixture> entities = null;

        // Act
        await entityRepository.AddRangeAsync(entities, cancellationToken: TestContext.Current.CancellationToken);
        var count = await dbContext.FullAuditableEntities.CountAsync(TestContext.Current.CancellationToken);

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task AddRangeAsync_WithValidEntity_ShouldAddCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAsync(dbContext);

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
        await entityRepository.AddRangeAsync(entities, cancellationToken: TestContext.Current.CancellationToken);
        var count = await dbContext.FullAuditableEntities.CountAsync(TestContext.Current.CancellationToken);
        var addedEntity = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        count.Should().Be(2);
        addedEntity.Should().NotBeNull();
        addedEntity.CreationDate.Should().NotBeNull();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ForSingleEntity_WithNullEntity_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        SomeFullAuditableEntityFixture entity = null;

        // Act
        Func<Task> act = async () => await entityRepository.UpdateAsync(entity);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_ForSingleEntity_WithNotExistsEntity_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

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

    [Fact]
    public async Task UpdateAsync_ForSingleEntity_WithValidEntity_ShouldUpdateCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        var entity = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        entity.SomeStringProp = "stringpropupdated";
        await entityRepository.UpdateAsync(entity, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        var entityAfterUpdate = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);
        entityAfterUpdate.SomeStringProp.Should().Be("stringpropupdated");
    }

    [Fact]
    public async Task UpdateAsync_ForEntityList_WithNullEntityList_ShouldDoNothing()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        List<SomeFullAuditableEntityFixture> entities = null;

        // Act
        await entityRepository.UpdateAsync(entities, cancellationToken: TestContext.Current.CancellationToken);
        var allEntities = await dbContext.FullAuditableEntities.ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        allEntities[0].LastModificationDate.Should().BeNull();
        allEntities[1].LastModificationDate.Should().BeNull();
        allEntities[2].LastModificationDate.Should().BeNull();
        allEntities[3].LastModificationDate.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ForEntityList_WithNotExistsEntities_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

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

    [Fact]
    public async Task UpdateAsync_ForEntityList_WithValidEntity_ShouldUpdateCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        var entities = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        entities[0].SomeStringProp = "stringpropupdated";
        entities[1].SomeStringProp = "stringpropupdated";
        await entityRepository.UpdateAsync(entities, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2, cancellationToken: TestContext.Current.CancellationToken);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("stringpropupdated");
    }

    [Fact]
    public async Task UpdateAsync_ForSingleEntityAndProjectionPropertiesOverload_WithNullEntity_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        SomeFullAuditableEntityFixture entity = null;
        Expression<Func<SomeFullAuditableEntityFixture, object>> projection = i => i.SomeStringProp;
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = [projection];

        // Act
        Func<Task> act = async () => await entityRepository.UpdateAsync(entity, default, projections);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_ForSingleEntityAndProjectionPropertiesOverload_WithNotExistsEntity_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

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

    [Fact]
    public async Task UpdateAsync_ForSingleEntityAndProjectionPropertiesOverload_WithValidEntityButPropertySelectorsIsNull_ShouldDoNothing()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        var entity = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = null;

        // Act
        entity.SomeStringProp = "stringpropupdated";
        entity.SomeDecimalProp = 20M;
        await entityRepository.UpdateAsync(entity, cancellationToken: TestContext.Current.CancellationToken, propertySelectors: projections);

        // Assert
        var entityAfterUpdate = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);
        entityAfterUpdate.SomeStringProp.Should().Be("somestring1");
        entityAfterUpdate.SomeDecimalProp.Should().Be(10);
    }

    [Fact]
    public async Task UpdateAsync_ForSingleEntityAndProjectionPropertiesOverload_WithValidEntity_ShouldUpdateCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        var entity = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);
        Expression<Func<SomeFullAuditableEntityFixture, object>> projection = i => i.SomeStringProp;
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = [projection];

        // Act
        entity.SomeStringProp = "stringpropupdated";
        entity.SomeDecimalProp = 20M;
        await entityRepository.UpdateAsync(entity, cancellationToken: TestContext.Current.CancellationToken, propertySelectors: projections);

        // Assert
        var entityAfterUpdate = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);
        entityAfterUpdate.SomeStringProp.Should().Be("stringpropupdated");
        entityAfterUpdate.SomeDecimalProp.Should().Be(10);
    }

    [Fact]
    public async Task UpdateAsync_ForEntityListAndProjectionPropertiesOverload_WithNullEntities_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        List<SomeFullAuditableEntityFixture> entities = null;
        Expression<Func<SomeFullAuditableEntityFixture, object>> projection = i => i.SomeStringProp;
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = [projection];

        // Act
        var allEntities = await dbContext.FullAuditableEntities.ToListAsync(TestContext.Current.CancellationToken);
        await entityRepository.UpdateAsync(entities, cancellationToken: TestContext.Current.CancellationToken, propertySelectors: projections);

        // Assert
        allEntities[0].LastModificationDate.Should().BeNull();
        allEntities[1].LastModificationDate.Should().BeNull();
        allEntities[2].LastModificationDate.Should().BeNull();
        allEntities[3].LastModificationDate.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ForEntityListAndProjectionPropertiesOverload_WithNotExistsEntities_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

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

    [Fact]
    public async Task UpdateAsync_ForEntityListAndProjectionPropertiesOverload_WithValidEntitiesButPropertySelectorsIsNull_ShouldDoNothing()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        var entities = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2, cancellationToken: TestContext.Current.CancellationToken);
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = null;

        // Act
        entities[0].SomeStringProp = "stringpropupdated";
        entities[0].SomeDecimalProp = 20M;
        entities[1].SomeStringProp = "stringpropupdated";
        entities[1].SomeDecimalProp = 20M;
        await entityRepository.UpdateAsync(entities, cancellationToken: TestContext.Current.CancellationToken, propertySelectors: projections);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2, cancellationToken: TestContext.Current.CancellationToken);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("somestring1");
        entitiesAfterUpdate[0].SomeDecimalProp.Should().Be(10M);
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("somestring2");
        entitiesAfterUpdate[1].SomeDecimalProp.Should().Be(20M);
    }

    [Fact]
    public async Task UpdateAsync_ForEntityListAndProjectionPropertiesOverload_WithValidEntities_ShouldUpdateCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        var entities = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2, cancellationToken: TestContext.Current.CancellationToken);
        Expression<Func<SomeFullAuditableEntityFixture, object>> projection = i => i.SomeStringProp;
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = [projection];

        // Act
        entities[0].SomeStringProp = "stringpropupdated";
        entities[0].SomeDecimalProp = 20M;
        entities[1].SomeStringProp = "stringpropupdated";
        entities[1].SomeDecimalProp = 20M;
        await entityRepository.UpdateAsync(entities, cancellationToken: TestContext.Current.CancellationToken, propertySelectors: projections);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2, cancellationToken: TestContext.Current.CancellationToken);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[0].SomeDecimalProp.Should().Be(10M);
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[1].SomeDecimalProp.Should().Be(20M);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ForSingleEntity_WithNullEntity_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        SomeFullAuditableEntityFixture entity = null;

        // Act
        Func<Task> act = async () => await entityRepository.DeleteAsync(entity);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeleteAsync_ForSingleEntity_WithNotExistsEntity_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

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

    [Fact]
    public async Task DeleteAsync_ForSingleEntity_WithValidEntity_ShouldDeleteCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        var entity = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        await entityRepository.DeleteAsync(entity, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var entityAfterUpdate = await entityRepository.GetByIdAsync(1, cancellationToken: TestContext.Current.CancellationToken);
        entityAfterUpdate.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ForEntityList_WithNullEntityList_ShouldDoNothing()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        List<SomeFullAuditableEntityFixture> entities = null;

        // Act
        await entityRepository.DeleteAsync(entities, cancellationToken: TestContext.Current.CancellationToken);
        var allEntities = await dbContext.FullAuditableEntities.ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        allEntities[0].Should().NotBeNull();
        allEntities[1].Should().NotBeNull();
        allEntities[2].Should().NotBeNull();
        allEntities[3].Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ForEntityList_WithNotExistsEntities_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

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

    [Fact]
    public async Task DeleteAsync_ForEntityList_WithValidEntity_ShouldDeleteCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        var entities = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        await entityRepository.DeleteAsync(entities, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2, cancellationToken: TestContext.Current.CancellationToken);
        entitiesAfterUpdate.Should().BeEmpty();
    }

    #endregion

    #region ReplaceOldsWithNewsAsync

    [Fact]
    public async Task ReplaceOldsWithNewsAsync_WithValidParameters_ShouldOperateCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        var entities = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2, cancellationToken: TestContext.Current.CancellationToken);
        var newEntities = entities.ToList();
        newEntities[0].Id = 5;
        newEntities[0].SomeStringProp = "stringpropupdated";
        newEntities[1].Id = 6;
        newEntities[1].SomeStringProp = "stringpropupdated";

        // Act
        await entityRepository.ReplaceOldsWithNewsAsync(entities, newEntities, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 5 || i.Id == 6, cancellationToken: TestContext.Current.CancellationToken);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("stringpropupdated");
    }

    #endregion

    #region ReplaceOldsWithNewsInSeperateDatabaseProcessAsync

    [Fact]
    public async Task ReplaceOldsWithNewsInSeperateDatabaseProcessAsync_WithValidParameters_ShouldOperateCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        var entities = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2, cancellationToken: TestContext.Current.CancellationToken);
        var newEntities = entities.ToList();
        foreach (var entity in newEntities)
            entity.SomeStringProp = "stringpropupdated";

        // Act
        await entityRepository.ReplaceOldsWithNewsInSeperateDatabaseProcessAsync(entities, newEntities, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(i => i.Id == 1 || i.Id == 2, cancellationToken: TestContext.Current.CancellationToken);
        entitiesAfterUpdate[0].SomeStringProp.Should().Be("stringpropupdated");
        entitiesAfterUpdate[1].SomeStringProp.Should().Be("stringpropupdated");
    }

    #endregion

    #region RemoveAll

    [Fact]
    public async Task RemoveAll_EmptyDatabase_ShouldRemoveCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAsync(dbContext);

        // Act
        await entityRepository.RemoveAllAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);
        entitiesAfterUpdate.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveAll_ShouldRemoveCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        await ResetDatabaseAndSeedAsync(dbContext);

        // Act
        await entityRepository.RemoveAllAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var entitiesAfterUpdate = await entityRepository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);
        entitiesAfterUpdate.Should().BeEmpty();
    }

    #endregion

    #region GetUpdatablePropertiesBuilder

    [Fact]
    public async Task GetUpdatablePropertiesBuilder_WithInvalidDto_ShouldReturnNull()
    {
        // Arrange
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            Auditing = new AuditConfiguration
            {
                AuditModificationDate = false,
                AuditModifier = false
            }
        };
        var services = GetServices(dataAccessConfiguration);
        var entityRepository = services.GetService<ISomeGenericRepository<RestTestEntityFixture>>();
        UpdatedPropsTestInvalidDto dto = new UpdatedPropsTestInvalidDto
        {
            Id = 1,
            Name = "john",
            Price = 10M
        };

        // Act
        var result = entityRepository.GetUpdatablePropertiesBuilder(dto);

        // Assert
        result.Should().NotBeNull();
        result.SetPropertyCallsLog.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUpdatablePropertiesBuilder_WithValidDtoButNull_ShouldReturnNull()
    {
        // Arrange
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            Auditing = new AuditConfiguration
            {
                AuditModificationDate = false,
                AuditModifier = false
            }
        };
        var services = GetServices(dataAccessConfiguration);
        var entityRepository = services.GetService<ISomeGenericRepository<RestTestEntityFixture>>();
        UpdatedPropsTestDto dto = null;

        // Act
        var result = entityRepository.GetUpdatablePropertiesBuilder(dto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUpdatablePropertiesBuilder_WithAuditModificationDateAndModifierUserFalseInDataAccessConfiguration_ShouldReturnCorrectResult()
    {
        // Arrange
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            Auditing = new AuditConfiguration
            {
                AuditModificationDate = false,
                AuditModifier = false
            }
        };
        var services = GetServices(dataAccessConfiguration);
        var entityRepository = services.GetService<ISomeGenericRepository<RestTestEntityFixture>>();
        var now = DateTime.Now;
        UpdatedPropsTestDto dto = new()
        {
            Id = 1,
            Name = "john",
            UpdateDate = now
        };

        // Act
        var result = entityRepository.GetUpdatablePropertiesBuilder(dto);

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
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            Auditing = new AuditConfiguration
            {
                AuditModificationDate = true,
            }
        };
        var services = GetServices(dataAccessConfiguration);
        var entityRepository = services.GetService<ISomeGenericRepository<RestTestEntityFixture>>();
        var now = DateTime.Now;
        UpdatedPropsTestDto dto = new()
        {
            Id = 1,
            Name = "john",
            UpdateDate = now
        };

        // Act
        var result = entityRepository.GetUpdatablePropertiesBuilder(dto);

        // Assert
        result.Should().NotBeNull();
        result.SetPropertyCallsLog.Should().NotBeEmpty();
        result.SetPropertyCallsLog.Should().Contain(EntityPropertyNames.LastModificationDate);
    }

    [Fact]
    public async Task GetUpdatablePropertiesBuilder_WithAuditModifierUserTrueAndGetCurrentUsernameMethodIsNotNullInDataAccessConfiguration_ShouldReturnCorrectResult()
    {
        // Arrange
        var dataAccessConfiguration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                GetCurrentUserNameMethod = (sp) => "testuser"
            },
            Auditing = new AuditConfiguration
            {
                AuditModificationDate = false,
                AuditModifier = true
            }
        };
        var services = GetServices(dataAccessConfiguration);
        var entityRepository = services.GetService<ISomeGenericRepository<RestTestEntityFixture>>();
        var now = DateTime.Now;
        UpdatedPropsTestDto dto = new()
        {
            Id = 1,
            Name = "john",
            UpdateDate = now
        };

        // Act
        var result = entityRepository.GetUpdatablePropertiesBuilder(dto);

        // Assert
        result.Should().NotBeNull();
        result.SetPropertyCallsLog.Should().NotBeEmpty();
        result.SetPropertyCallsLog.Should().NotContain(EntityPropertyNames.LastModificationDate);
        result.SetPropertyCallsLog.Should().Contain(EntityPropertyNames.LastModifierUserName);
    }

    #endregion

    #region Setup

    public interface ISomeGenericRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, IMilvaEntity
    {
    }

    public class SomeGenericRepository<TEntity>(SomeMilvaDbContextFixture dbContext) : BaseRepository<TEntity, SomeMilvaDbContextFixture>(dbContext), ISomeGenericRepository<TEntity>
         where TEntity : class, IMilvaEntity
    {
    }

    private static ServiceProvider GetServices(DataAccessConfiguration configuration = null)
    {
        var services = new ServiceCollection();

        services.ConfigureMilvaDataAccess(opt =>
        {
            opt.DbContext = configuration != null ? configuration.DbContext : opt.DbContext;
            opt.Repository = configuration != null ? configuration.Repository : opt.Repository;
            opt.Auditing = configuration != null ? configuration.Auditing : opt.Auditing;
            opt.DbContext.GetCurrentUserNameMethod = (sp) => "testuser";
        });

        services.AddScoped(typeof(ISomeGenericRepository<>), typeof(SomeGenericRepository<>));

        services.AddDbContext<SomeMilvaDbContextFixture>(opt =>
        {
            opt.UseInMemoryDatabase(databaseName: $"BaseRepositoryAsyncTestDbInMemory_{Guid.NewGuid}_{DateTime.Now.Nanosecond}")
               .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        });

        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider;
    }

    private static async Task ResetDatabaseAsync(SomeMilvaDbContextFixture dbContextFixture)
    {
        await dbContextFixture.Database.EnsureDeletedAsync();
        await dbContextFixture.Database.EnsureCreatedAsync();
    }

    private static async Task ResetDatabaseAndSeedAsync(SomeMilvaDbContextFixture dbContextFixture)
    {
        await ResetDatabaseAsync(dbContextFixture);
        await SeedAsync(dbContextFixture);
    }

    private static async Task SeedAsync(SomeMilvaDbContextFixture dbContextFixture)
    {
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
