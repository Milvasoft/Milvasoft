using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.Exceptions;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;
using Milvasoft.DataAccess.EfCore.Utils;
using Milvasoft.Helpers.DataAccess.EfCore.Concrete;
using Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.Persistence;

namespace Milvasoft.IntegrationTests.DataAccessTests.EfCoreTests.RepositoryBaseTests;

/// <summary>
/// ExecuteUpdate must not touch soft deleted rows (consistent with reads) and soft deleted rows can be restored explicitly.
/// </summary>
[Collection(nameof(UtcTrueDatabaseTestCollection))]
[Trait("RepositoryBase Async Integration Tests", "Integration tests for Milvasoft.DataAccess.EfCore integration tests.")]
public class SoftDeleteBulkUpdateTests(CustomWebApplicationFactory factory) : DataAccessIntegrationTestBase(factory)
{
    private const int _activeId = 1;
    private const int _softDeletedId = 2;

    public override Task InitializeAsync(Action<IServiceCollection> configureServices = null, Action<IApplicationBuilder> configureApp = null)
    {
        var waf = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDataAccessConfiguration>();
                services.RemoveAll<DbContextOptions<MilvaBulkDbContextFixture>>();
                services.RemoveAll<MilvaBulkDbContextFixture>();

                services.ConfigureMilvaDataAccess(opt =>
                {
                    opt.DbContext = new DbContextConfiguration { GetCurrentUserNameMethod = sp => "testuser" };

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
                    x.UseNpgsql(_factory.GetConnectionString());
                });

                services.AddScoped(typeof(ISomeRepository<>), typeof(SomeRepository<>));
            });

            builder.Configure(app => { });
        });

        _serviceProvider = waf.Services.CreateScope().ServiceProvider;

        return _factory.CreateRespawner();
    }

    [Fact]
    public async Task ExecuteUpdateAsync_WithPredicate_ShouldNotUpdateSoftDeletedRows()
    {
        // Arrange
        var (repository, dbContext) = await SetUpAsync();

        // Act
        var affectedRows = await repository.ExecuteUpdateAsync(e => true, UpdateString("updated"), TestContext.Current.CancellationToken);

        // Assert
        affectedRows.Should().Be(1);
        (await GetAsync(dbContext, _activeId)).SomeStringProp.Should().Be("updated");

        var softDeleted = await GetAsync(dbContext, _softDeletedId);
        softDeleted.SomeStringProp.Should().Be("deleted");
        softDeleted.LastModificationDate.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteUpdateAsync_WithIdOfSoftDeletedRow_ShouldNotUpdateIt()
    {
        // Arrange
        var (repository, dbContext) = await SetUpAsync();

        // Act
        var affectedRows = await repository.ExecuteUpdateAsync(_softDeletedId, UpdateString("updated"), TestContext.Current.CancellationToken);

        // Assert
        affectedRows.Should().Be(0);
        (await GetAsync(dbContext, _softDeletedId)).SomeStringProp.Should().Be("deleted");
    }

    [Fact]
    public async Task ExecuteUpdateAsync_WhenSoftDeletedEntitiesAreFetched_ShouldUpdateSoftDeletedRows()
    {
        // Arrange
        var (repository, dbContext) = await SetUpAsync();

        repository.FetchSoftDeletedEntities(true);

        // Act
        var affectedRows = await repository.ExecuteUpdateAsync(_softDeletedId, UpdateString("updated"), TestContext.Current.CancellationToken);

        // Assert
        affectedRows.Should().Be(1);
        (await GetAsync(dbContext, _softDeletedId)).SomeStringProp.Should().Be("updated");
    }

    [Fact]
    public async Task ExecuteUpdate_Sync_ShouldNotUpdateSoftDeletedRows()
    {
        // Arrange
        var (repository, dbContext) = await SetUpAsync();

        // Act
        var affectedById = repository.ExecuteUpdate(_softDeletedId, UpdateString("updated"));
        var affectedByPredicate = repository.ExecuteUpdate(e => true, UpdateString("updated"));

        // Assert
        affectedById.Should().Be(0);
        affectedByPredicate.Should().Be(1);
        (await GetAsync(dbContext, _softDeletedId)).SomeStringProp.Should().Be("deleted");
        (await GetAsync(dbContext, _activeId)).SomeStringProp.Should().Be("updated");
    }

    [Fact]
    public async Task RestoreAsync_WithIdOfSoftDeletedRow_ShouldRestoreItAndClearDeletionAuditValues()
    {
        // Arrange
        var (repository, dbContext) = await SetUpAsync();

        // Act
        var restoredRows = await repository.RestoreAsync(_softDeletedId, TestContext.Current.CancellationToken);

        // Assert
        restoredRows.Should().Be(1);

        var restored = await GetAsync(dbContext, _softDeletedId);
        restored.IsDeleted.Should().BeFalse();
        restored.DeletionDate.Should().BeNull();
        restored.DeleterUserName.Should().BeNull();
        restored.LastModificationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
        restored.LastModifierUserName.Should().Be("testuser");

        (await repository.GetByIdAsync(_softDeletedId, cancellationToken: TestContext.Current.CancellationToken)).Should().NotBeNull();
    }

    [Fact]
    public async Task RestoreAsync_WithPredicate_ShouldOnlyTouchSoftDeletedRows()
    {
        // Arrange
        var (repository, dbContext) = await SetUpAsync();

        // Act
        var restoredRows = await repository.RestoreAsync(e => true, TestContext.Current.CancellationToken);

        // Assert
        restoredRows.Should().Be(1);
        (await GetAsync(dbContext, _softDeletedId)).IsDeleted.Should().BeFalse();
        (await GetAsync(dbContext, _activeId)).LastModificationDate.Should().BeNull();
    }

    [Fact]
    public async Task Restore_Sync_ShouldRestoreSoftDeletedRow()
    {
        // Arrange
        var (repository, dbContext) = await SetUpAsync();

        // Act
        var restoredById = repository.Restore(_softDeletedId);
        var restoredAgain = repository.Restore(e => e.Id == _softDeletedId);

        // Assert
        restoredById.Should().Be(1);
        restoredAgain.Should().Be(0);

        var restored = await GetAsync(dbContext, _softDeletedId);
        restored.IsDeleted.Should().BeFalse();
        restored.DeletionDate.Should().BeNull();
        restored.DeleterUserName.Should().BeNull();
    }

    [Fact]
    public async Task RestoreAsync_WithNotSoftDeletableEntity_ShouldThrow()
    {
        // Arrange
        await InitializeAsync(configureServices: null);

        var repository = _serviceProvider.GetRequiredService<ISomeRepository<SomeLogEntity>>();

        // Act
        var act = () => repository.RestoreAsync(1, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<MilvaDeveloperException>();
    }

    #region Setup

    public interface ISomeRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, IMilvaEntity
    {
    }

    public class SomeRepository<TEntity>(MilvaBulkDbContextFixture dbContext) : BaseRepository<TEntity, MilvaBulkDbContextFixture>(dbContext), ISomeRepository<TEntity>
         where TEntity : class, IMilvaEntity
    {
    }

    private async Task<(ISomeRepository<SomeFullAuditableEntityFixture> Repository, MilvaBulkDbContextFixture DbContext)> SetUpAsync()
    {
        await InitializeAsync(configureServices: null);

        var dbContext = _serviceProvider.GetRequiredService<MilvaBulkDbContextFixture>();

        dbContext.FullAuditableEntities.AddRange(
            new SomeFullAuditableEntityFixture { Id = _activeId, SomeStringProp = "active" },
            new SomeFullAuditableEntityFixture
            {
                Id = _softDeletedId,
                SomeStringProp = "deleted",
                IsDeleted = true,
                DeletionDate = DateTime.UtcNow.AddDays(-1),
                DeleterUserName = "deleter"
            });

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        dbContext.ChangeTracker.Clear();

        return (_serviceProvider.GetRequiredService<ISomeRepository<SomeFullAuditableEntityFixture>>(), dbContext);
    }

    private static SetPropertyBuilder<SomeFullAuditableEntityFixture> UpdateString(string value)
        => new SetPropertyBuilder<SomeFullAuditableEntityFixture>().SetPropertyValue(e => e.SomeStringProp, value);

    private static Task<SomeFullAuditableEntityFixture> GetAsync(MilvaBulkDbContextFixture dbContext, int id)
        => dbContext.FullAuditableEntities.AsNoTracking().SingleAsync(e => e.Id == id, TestContext.Current.CancellationToken);

    #endregion
}
