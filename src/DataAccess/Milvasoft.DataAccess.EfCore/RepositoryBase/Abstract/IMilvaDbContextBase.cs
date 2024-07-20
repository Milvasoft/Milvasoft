using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;

/// <summary>
/// Interface for base repository.
/// </summary>
public interface IMilvaDbContextBase
{
    /// <summary>
    /// Service provider for access DI contaniner in DbContext.
    /// </summary>
    public IServiceProvider ServiceProvider { get; set; }

    /// <summary>
    /// Gets a DbSet for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>A DbSet for the specified entity type.</returns>
    public DbSet<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public int SaveChanges();

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether AcceptAllChanges should be called after the changes have been sent successfully to the database.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public int SaveChanges(bool acceptAllChangesOnSuccess);

    /// <summary>
    /// Asynchronously saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously saves all changes made in this context to the database.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether AcceptAllChanges should be called after the changes have been sent successfully to the database.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);

    /// <summary>
    /// Overrided the BulkSaveChanges method for soft deleting.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    void SaveChangesBulk(BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken());

    /// <summary>
    /// Overrided the BulkSaveChangesAsync method for soft deleting.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken());

    /// <summary>
    /// Gets milva db context configuration object.
    /// </summary>
    /// <returns></returns>
    IDataAccessConfiguration GetDataAccessConfiguration();

    /// <summary>
    /// Changes soft deletion state.
    /// </summary>
    /// <param name="state">The new soft deletion state.</param>
    void ChangeSoftDeletionState(SoftDeletionState state);

    /// <summary>
    /// Sets soft deletion state to default state in <see cref="DataAccessConfiguration"/>.
    /// </summary>
    void SetSoftDeletionStateToDefault();

    /// <summary>
    /// It updates the state that determines whether soft delete state reset to default occurs after any operation.
    /// </summary>
    /// <param name="state">Soft delete reset state.</param>
    void SoftDeletionStateResetAfterOperation(bool state = true);

    /// <summary>
    /// Gets current soft deletion state.
    /// </summary>
    /// <returns>The current soft deletion state.</returns>
    SoftDeletionState GetCurrentSoftDeletionState();

    /// <summary>
    /// Gets <see cref="SetPropertyBuilder{TSource}"/> for entity's matching properties with <paramref name="dto"/>'s not null properties.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="dto">The DTO object.</param>
    /// <returns>The SetPropertyBuilder instance.</returns>
    SetPropertyBuilder<TEntity> GetUpdatablePropertiesBuilder<TEntity, TDto>(TDto dto) where TEntity : class, IMilvaEntity where TDto : DtoBase;
}
