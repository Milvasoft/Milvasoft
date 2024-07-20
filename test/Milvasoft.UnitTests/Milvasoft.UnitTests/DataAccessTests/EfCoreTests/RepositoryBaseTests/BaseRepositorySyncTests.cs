using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
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
public class BaseRepositorySyncTests
{
    #region Configuration Based Tests

    [Fact]
    public void DataFetch_WithDefaultConfiguration_ShouldNotReturnSoftDeletedData()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void DataFetch_FetchSoftDeletedEntities_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsFalse_ShouldReturnSoftDeletedData()
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
        ResetDatabaseAndSeed(dbContext);
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

    [Fact]
    public void DataFetch_ResetSoftDeletedEntityFetchState_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsFalse_ShouldNotReturnSoftDeletedDataAfterSecondOperation()
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
        ResetDatabaseAndSeed(dbContext);
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

    [Fact]
    public void DataFetch_FetchSoftDeletedEntities_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsTrue_ShouldNotReturnSoftDeletedDataAfterFirstOperation()
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
        ResetDatabaseAndSeed(dbContext);
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

    [Fact]
    public void DataFetch_SoftDeleteFetchStateResetAfterOperation_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsFalse_ShouldNotReturnSoftDeletedDataAfterFirstOperation()
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
        ResetDatabaseAndSeed(dbContext);
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

    [Fact]
    public void DataFetch_ResetSoftDeletedEntityFetchState_WithDefaultSoftDeleteFetchIsFalseAndResetStateIsFalse_ShouldNotReturnSoftDeletedDataAfterFirstOperation()
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
        ResetDatabaseAndSeed(dbContext);
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

    [Fact]
    public void DataManipulation_WithDefaultSaveChangeChoise_ShouldSaveChangesAfterEveryOperation()
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
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void DataManipulation_WithManualSaveChangeChoise_ShouldSaveChangesAfterEveryOperation()
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
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void DataManipulation_ChangeSaveChangesChoiceToManual_WithDefaultSaveChangeChoise_ShouldSaveChangesAfterEveryOperation()
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
        ResetDatabaseAndSeed(dbContext);
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

    [Fact]
    public void DataManipulation_ResetSaveChangesChoiceToDefault_WithDefaultSaveChangeChoise_ShouldSaveChangesAfterEveryOperation()
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
        ResetDatabaseAndSeed(dbContext);
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

    [Fact]
    public void GetFirstOrDefault_WithoutParameters_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        // Act 
        var result = entityRepository.GetFirstOrDefault();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.SomeStringProp.Should().Be("somestring1");
    }

    [Fact]
    public void GetFirstOrDefault_WithCondition_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        // Act 
        var result = entityRepository.GetFirstOrDefault(i => i.SomeDecimalProp > 20M);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(3);
        result.SomeStringProp.Should().Be("somestring3");
        result.SomeDecimalProp.Should().Be(30M);
    }

    [Fact]
    public void GetFirstOrDefault_WithProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void GetFirstOrDefault_WithConditionAndProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void GetFirstOrDefault_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        // Act 
        var result = entityRepository.GetFirstOrDefault(i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 1);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetSingleOrDefault

    [Fact]
    public void GetSingleOrDefault_WithoutParameters_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        // Act 
        Action act = () => entityRepository.GetSingleOrDefault();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Sequence contains more than one element");
    }

    [Fact]
    public void GetSingleOrDefault_WithCondition_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        // Act 
        var result = entityRepository.GetSingleOrDefault(i => i.SomeDecimalProp > 20M);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(3);
        result.SomeStringProp.Should().Be("somestring3");
        result.SomeDecimalProp.Should().Be(30M);
    }

    [Fact]
    public void GetSingleOrDefault_WithConditionAndProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void GetSingleOrDefault_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        // Act 
        var result = entityRepository.GetSingleOrDefault(i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 1);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetById

    [Fact]
    public void GetById_WithoutParameters_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        // Act 
        var result = entityRepository.GetById(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.SomeStringProp.Should().Be("somestring1");
    }

    [Fact]
    public void GetById_WithCondition_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        // Act 
        var result = entityRepository.GetById(1, i => i.SomeDecimalProp > 20M);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetById_WithConditionAndProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void GetById_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        // Act 
        var result = entityRepository.GetById(3, i => i.SomeDecimalProp > 20M, i => new SomeFullAuditableEntityFixture
        {
            Id = i.Id,
            CreationDate = i.CreationDate,
        }, i => i.SomeDecimalProp > 1);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAll

    [Fact]
    public void GetAll_WithoutParameters_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        // Act 
        var result = entityRepository.GetAll();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Fact]
    public void GetAll_WithCondition_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        // Act 
        var result = entityRepository.GetAll(i => i.SomeDecimalProp > 10M);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Fact]
    public void GetAll_WithConditionAndProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void GetAll_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void GetAll_WithListRequest_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Sorting = new SortRequest { SortBy = nameof(SomeFullAuditableEntityFixture.CreationDate), Type = Components.Rest.Enums.SortType.Desc },
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(SomeFullAuditableEntityFixture.SomeDecimalProp), Type = Components.Rest.Enums.AggregationType.Sum }] }
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

    [Fact]
    public void GetAll_WithListRequestAndCondition_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Sorting = new SortRequest { SortBy = nameof(SomeFullAuditableEntityFixture.CreationDate), Type = Components.Rest.Enums.SortType.Desc },
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(SomeFullAuditableEntityFixture.SomeDecimalProp), Type = Components.Rest.Enums.AggregationType.Sum }] }
        };

        // Act 
        var result = entityRepository.GetAll(listRequest, i => i.SomeDecimalProp > 10M);

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
    public void GetAll_WithListRequestAndConditionAndProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Sorting = new SortRequest { SortBy = nameof(SomeFullAuditableEntityFixture.CreationDate), Type = Components.Rest.Enums.SortType.Desc },
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(SomeFullAuditableEntityFixture.SomeDecimalProp), Type = Components.Rest.Enums.AggregationType.Sum }] }
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
    public void GetAll_WithListRequestAndConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);
        var listRequest = new ListRequest
        {
            PageNumber = 1,
            RowCount = 2,
            Sorting = new SortRequest { SortBy = nameof(SomeFullAuditableEntityFixture.CreationDate), Type = Components.Rest.Enums.SortType.Desc },
            Aggregation = new AggregationRequest { Criterias = [new AggregationCriteria { AggregateBy = nameof(SomeFullAuditableEntityFixture.SomeDecimalProp), Type = Components.Rest.Enums.AggregationType.Sum }] }
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

    #region GetSome

    [Fact]
    public void GetSome_WithoutParameters_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        // Act 
        var result = entityRepository.GetSome(2);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Fact]
    public void GetSome_WithCondition_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        // Act 
        var result = entityRepository.GetSome(1, i => i.SomeDecimalProp > 10M);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].SomeStringProp.Should().NotBeNull();
    }

    [Fact]
    public void GetSome_WithConditionAndProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void GetSome_WithConditionAndProjectionAndConditionAfterProjection_ShouldReturnCorrectResult()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Add_WithNullEntity_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabase(dbContext);

        SomeFullAuditableEntityFixture entity = null;

        // Act 
        Action act = () => entityRepository.Add(entity);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Add_WithValidEntity_ShouldAddCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabase(dbContext);

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

    [Fact]
    public void AddRange_WithNullEntityList_ShouldDoNothing()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabase(dbContext);

        List<SomeFullAuditableEntityFixture> entities = null;

        // Act 
        entityRepository.AddRange(entities);
        var count = dbContext.FullAuditableEntities.Count();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void AddRange_WithValidEntity_ShouldAddCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabase(dbContext);

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

    [Fact]
    public void Update_ForSingleEntity_WithNullEntity_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        SomeFullAuditableEntityFixture entity = null;

        // Act 
        Action act = () => entityRepository.Update(entity);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Update_ForSingleEntity_WithNotExistsEntity_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Update_ForSingleEntity_WithValidEntity_ShouldUpdateCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        var entity = entityRepository.GetById(1);

        // Act 
        entity.SomeStringProp = "stringpropupdated";
        entityRepository.Update(entity);

        // Assert
        var entityAfterUpdate = entityRepository.GetById(1);
        entityAfterUpdate.SomeStringProp.Should().Be("stringpropupdated");
    }

    [Fact]
    public void Update_ForEntityList_WithNullEntityList_ShouldDoNothing()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Update_ForEntityList_WithNotExistsEntities_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Update_ForEntityList_WithValidEntity_ShouldUpdateCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Update_ForSingleEntityAndProjectionPropertiesOverload_WithNullEntity_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        SomeFullAuditableEntityFixture entity = null;
        Expression<Func<SomeFullAuditableEntityFixture, object>> projection = i => i.SomeStringProp;
        Expression<Func<SomeFullAuditableEntityFixture, object>>[] projections = [projection];

        // Act 
        Action act = () => entityRepository.Update(entity, projections);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Update_ForSingleEntityAndProjectionPropertiesOverload_WithNotExistsEntity_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Update_ForSingleEntityAndProjectionPropertiesOverload_WithValidEntityButPropertySelectorsIsNull_ShouldDoNothing()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Update_ForSingleEntityAndProjectionPropertiesOverload_WithValidEntity_ShouldUpdateCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Update_ForEntityListAndProjectionPropertiesOverload_WithNullEntities_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Update_ForEntityListAndProjectionPropertiesOverload_WithNotExistsEntities_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Update_ForEntityListAndProjectionPropertiesOverload_WithValidEntitiesButPropertySelectorsIsNull_ShouldDoNothing()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Update_ForEntityListAndProjectionPropertiesOverload_WithValidEntities_ShouldUpdateCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Delete_ForSingleEntity_WithNullEntity_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        SomeFullAuditableEntityFixture entity = null;

        // Act 
        Action act = () => entityRepository.Delete(entity);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Delete_ForSingleEntity_WithNotExistsEntity_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Delete_ForSingleEntity_WithValidEntity_ShouldDeleteCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        var entity = entityRepository.GetById(1);

        // Act 
        entityRepository.Delete(entity);

        // Assert
        var entityAfterUpdate = entityRepository.GetById(1);
        entityAfterUpdate.Should().BeNull();
    }

    [Fact]
    public void Delete_ForEntityList_WithNullEntityList_ShouldDoNothing()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Delete_ForEntityList_WithNotExistsEntities_ShouldThrowException()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void Delete_ForEntityList_WithValidEntity_ShouldDeleteCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        var entities = entityRepository.GetAll(i => i.Id == 1 || i.Id == 2);

        // Act 
        entityRepository.Delete(entities);

        // Assert
        var entitiesAfterUpdate = entityRepository.GetAll(i => i.Id == 1 || i.Id == 2);
        entitiesAfterUpdate.Should().BeEmpty();
    }

    #endregion

    #region ReplaceOldsWithNews

    [Fact]
    public void ReplaceOldsWithNews_WithValidParameters_ShouldOperateCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void ReplaceOldsWithNewsInSeperateDatabaseProcess_WithValidParameters_ShouldOperateCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

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

    [Fact]
    public void RemoveAll_EmptyDatabase_ShouldRemoveCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabase(dbContext);

        // Act 
        entityRepository.RemoveAll();

        // Assert
        var entitiesAfterUpdate = entityRepository.GetAll();
        entitiesAfterUpdate.Should().BeEmpty();
    }

    [Fact]
    public void RemoveAll_ShouldRemoveCorrectly()
    {
        // Arrange
        var configuration = new DataAccessConfiguration();
        var services = GetServices(configuration);
        var dbContext = services.GetService<SomeMilvaDbContextFixture>();
        var entityRepository = services.GetService<ISomeGenericRepository<SomeFullAuditableEntityFixture>>();
        ResetDatabaseAndSeed(dbContext);

        // Act 
        entityRepository.RemoveAll();

        // Assert
        var entitiesAfterUpdate = entityRepository.GetAll();
        entitiesAfterUpdate.Should().BeEmpty();
    }

    #endregion

    #region GetUpdatablePropertiesBuilder

    [Fact]
    public void GetUpdatablePropertiesBuilder_WithInvalidDto_ShouldReturnNull()
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
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> expectedExpression = i => i;

        // Act
        var result = entityRepository.GetUpdatablePropertiesBuilder(dto);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expectedExpression, result.SetPropertyCalls);

        equality.Should().BeTrue();
    }

    [Fact]
    public void GetUpdatablePropertiesBuilder_WithValidDtoButNull_ShouldReturnNull()
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
    public void GetUpdatablePropertiesBuilder_WithAuditModificationDateAndModifierUserFalseInDataAccessConfiguration_ShouldReturnCorrectResult()
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
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> notExpectedExpression = i => i;

        // Act
        var result = entityRepository.GetUpdatablePropertiesBuilder(dto);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(notExpectedExpression, result.SetPropertyCalls);
        equality.Should().BeFalse();
        result.SetPropertyCalls.Body.ToString().Should().NotContain(EntityPropertyNames.LastModificationDate);
        result.SetPropertyCalls.Body.ToString().Should().NotContain(EntityPropertyNames.LastModifierUserName);
    }

    [Fact]
    public void GetUpdatablePropertiesBuilder_WithAuditModificationDateTrueInDataAccessConfiguration_ShouldReturnCorrectResult()
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
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> notExpectedExpression = i => i;

        // Act
        var result = entityRepository.GetUpdatablePropertiesBuilder(dto);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(notExpectedExpression, result.SetPropertyCalls);
        equality.Should().BeFalse();
        result.SetPropertyCalls.Body.ToString().Should().Contain(EntityPropertyNames.LastModificationDate);
    }

    [Fact]
    public void GetUpdatablePropertiesBuilder_WithAuditModifierUserTrueAndGetCurrentUsernameMethodIsNotNullInDataAccessConfiguration_ShouldReturnCorrectResult()
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
        Expression<Func<SetPropertyCalls<RestTestEntityFixture>, SetPropertyCalls<RestTestEntityFixture>>> notExpectedExpression = i => i;

        // Act
        var result = entityRepository.GetUpdatablePropertiesBuilder(dto);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(notExpectedExpression, result.SetPropertyCalls);
        equality.Should().BeFalse();
        result.SetPropertyCalls.Body.ToString().Should().NotContain(EntityPropertyNames.LastModificationDate);
        result.SetPropertyCalls.Body.ToString().Should().Contain(EntityPropertyNames.LastModifierUserName);
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
            opt.UseInMemoryDatabase(databaseName: $"BaseRepositorySyncTestDbInMemory_{Guid.NewGuid}_{DateTime.Now.Nanosecond}")
               .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        });

        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider;
    }

    private static void ResetDatabase(SomeMilvaDbContextFixture dbContextFixture)
    {
        dbContextFixture.Database.EnsureDeleted();
        dbContextFixture.Database.EnsureCreated();
    }

    private static void ResetDatabaseAndSeed(SomeMilvaDbContextFixture dbContextFixture)
    {
        ResetDatabase(dbContextFixture);
        Seed(dbContextFixture);
    }

    private static void Seed(SomeMilvaDbContextFixture dbContextFixture)
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

        dbContextFixture.FullAuditableEntities.AddRange(entities);
        dbContextFixture.SaveChanges();
    }

    #endregion
}
