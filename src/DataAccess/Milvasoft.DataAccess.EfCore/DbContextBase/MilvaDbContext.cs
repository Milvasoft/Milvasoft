using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Milvasoft.Components.Rest.Request;
using Milvasoft.DataAccess.EfCore.Utils.LookupModels;
using Milvasoft.Types.Structs;
using System.Collections;
using System.Linq.Expressions;

namespace Milvasoft.DataAccess.EfCore.DbContextBase;

/// <summary>
/// Handles all database operations with new features like soft deletion.
/// </summary>
/// <param name="options"></param>
public abstract class MilvaDbContext(DbContextOptions options) : DbContext(options), IMilvaDbContextBase
{
    /// <summary>
    /// Service provider for access DI contaniner in DbContext.
    /// </summary>
    public IServiceProvider ServiceProvider { get; set; }

    /// <summary>
    /// Milva specific db context configuration.
    /// </summary>
    protected IDataAccessConfiguration _dbContextConfiguration;

    /// <summary>
    /// Default prop for soft delete.
    /// </summary>
    protected SoftDeletionState _currentSoftDeleteState;

    /// <summary>
    /// Default prop for datetime configuration.
    /// </summary>
    protected bool _useUtcForDateTimes = false;

    /// <summary>
    /// It updates the state that determines whether soft delete state reset to default occurs after any operation.
    /// </summary>
    protected bool _resetSoftDeleteStateAfterEveryOperation = true;

    /// <summary>
    /// Initializes new instance.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="dbContextConfiguration"></param>
    protected MilvaDbContext(DbContextOptions options, IDataAccessConfiguration dbContextConfiguration) : this(options)
    {
        SetDataAccessConfiguration(dbContextConfiguration);
    }

    /// <summary>
    /// Initializes new instance.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="dbContextConfiguration"></param>
    /// <param name="serviceProvider"></param>
    protected MilvaDbContext(DbContextOptions options, IDataAccessConfiguration dbContextConfiguration, IServiceProvider serviceProvider) : this(options)
    {
        SetDataAccessConfiguration(dbContextConfiguration);
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Overrided the OnModelCreating for custom configurations to database.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseTenantId();

        if (_useUtcForDateTimes)
            modelBuilder.UseUtcDateTime();

        base.OnModelCreating(modelBuilder);
    }

    #region Configuration

    /// <summary>
    /// Sets milva db context specific configuration.
    /// </summary>
    /// <param name="dbContextConfiguration"></param>
    public void SetDataAccessConfiguration(IDataAccessConfiguration dbContextConfiguration)
    {
        _dbContextConfiguration = dbContextConfiguration;
        _currentSoftDeleteState = dbContextConfiguration.DbContext.DefaultSoftDeletionState;
        _resetSoftDeleteStateAfterEveryOperation = dbContextConfiguration.DbContext.ResetSoftDeleteStateAfterEveryOperation;
        _useUtcForDateTimes = dbContextConfiguration.DbContext.UseUtcForDateTime;
    }

    /// <summary>
    /// Gets milva db context configuration object.
    /// </summary>
    /// <returns></returns>
    public IDataAccessConfiguration GetDataAccessConfiguration() => _dbContextConfiguration;

    /// <summary>
    /// Changes soft deletion state.
    /// </summary>
    public void ChangeSoftDeletionState(SoftDeletionState state) => _currentSoftDeleteState = state;

    /// <summary>
    /// Sets soft deletion state to default state in <see cref="DataAccessConfiguration"/>.
    /// </summary>
    public void SetSoftDeletionStateToDefault() => _currentSoftDeleteState = _dbContextConfiguration.DbContext.DefaultSoftDeletionState;

    /// <summary>
    /// It updates the state that determines whether soft delete state reset to default occurs after any operation.
    /// </summary>
    /// <param name="state">Soft delete reset state.</param>
    public void SoftDeletionStateResetAfterOperation(bool state = true) => _resetSoftDeleteStateAfterEveryOperation = state;

    /// <summary>
    /// Gets current soft deletion state.
    /// </summary>
    /// <returns></returns>
    public SoftDeletionState GetCurrentSoftDeletionState() => _currentSoftDeleteState;

    #endregion

    #region SaveChanges Overrides

    /// <summary>
    /// Overrided the SaveChanges method for soft deleting.
    /// </summary>
    /// <returns></returns>
    public override int SaveChanges()
    {
        AuditEntites();

        return base.SaveChanges();
    }

    /// <summary>
    /// Overrided the SaveChangesAsync method for soft deleting.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        AuditEntites();

        return await base.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Auditing

    /// <summary>
    /// Provides auditing entities by AuditConfiguration.
    /// If deletion process happens then sets the IgnoreSoftDelete variable to true at the end of process.
    /// </summary>
    protected internal virtual void AuditEntites()
    {
        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            switch (entry.State)
            {
                case EntityState.Added:

                    AuditDate(entry, EntityPropertyNames.CreationDate, _dbContextConfiguration.Auditing.AuditCreationDate);

                    AuditPerformerUser(entry, EntityPropertyNames.CreatorUserName, _dbContextConfiguration.Auditing.AuditCreator);

                    break;
                case EntityState.Modified:

                    AuditDate(entry, EntityPropertyNames.LastModificationDate, _dbContextConfiguration.Auditing.AuditModificationDate);

                    AuditPerformerUser(entry, EntityPropertyNames.LastModifierUserName, _dbContextConfiguration.Auditing.AuditModifier);

                    break;
                case EntityState.Deleted:

                    SoftDelete(entry);

                    break;
                default:
                    break;
            }
        }

        if (_resetSoftDeleteStateAfterEveryOperation)
            _currentSoftDeleteState = _dbContextConfiguration.DbContext.DefaultSoftDeletionState;
    }

    /// <summary>
    /// Soft delete operation.
    /// </summary>
    /// <param name="entry"></param>
    protected internal virtual void SoftDelete(EntityEntry entry)
    {
        if (_currentSoftDeleteState == SoftDeletionState.Passive)
            return;

        //Apply soft delete to entry.
        AuditDeletion(entry);

        //If entry is a many side on one to many or many to many relation, skip this entry's navigations for soft delete.
        if (entry.Collections.IsNullOrEmpty())
            return;

        //For changing includes.
        //If navigation entry is a collection entry and included apply soft delete.
        //If navigation entry included apply soft delete.
        foreach (var navigationEntry in entry.Navigations)
        {
            if (navigationEntry is CollectionEntry collectionEntry && collectionEntry.CurrentValue != null)
            {
                foreach (var dependentEntry in collectionEntry.CurrentValue)
                    AuditDeletion(Entry(dependentEntry));
            }
            else if (navigationEntry?.CurrentValue != null)
                AuditDeletion(Entry(navigationEntry.CurrentValue));
        }
    }

    /// <summary>
    /// Entity auditing for delete. 
    /// </summary>
    /// <param name="entry"></param>
    protected internal virtual void AuditDeletion(EntityEntry entry)
    {
        entry.State = EntityState.Deleted;

        if (!entry.Metadata.GetProperties().Any(x => x.Name == EntityPropertyNames.IsDeleted))
            return;

        entry.State = EntityState.Modified;

        //Change "IsDeleted" property value.
        entry.Property(EntityPropertyNames.IsDeleted).CurrentValue = true;
        entry.Property(EntityPropertyNames.IsDeleted).IsModified = true;

        AuditDate(entry, EntityPropertyNames.DeletionDate, _dbContextConfiguration.Auditing.AuditDeletionDate);

        AuditPerformerUser(entry, EntityPropertyNames.DeleterUserName, _dbContextConfiguration.Auditing.AuditDeleter);
    }

    /// <summary>
    /// Entity auditing by date. 
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="propertyName"></param>
    /// <param name="audit"></param>
    protected internal virtual void AuditDate(EntityEntry entry, string propertyName, bool audit)
    {
        if (!audit)
            return;

        if (entry.Metadata.GetProperties().Any(prop => prop.Name == propertyName))
        {
            entry.Property(propertyName).CurrentValue = CommonHelper.GetNow(_useUtcForDateTimes);
            entry.Property(propertyName).IsModified = true;
        }
    }

    /// <summary>
    /// Entity auditing by performer user.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="propertyName"></param>
    /// <param name="audit"></param>
    protected internal virtual void AuditPerformerUser(EntityEntry entry, string propertyName, bool audit)
    {
        if (!audit)
            return;

        var currentUserName = _dbContextConfiguration.DbContext.GetCurrentUserNameMethod?.Invoke(ServiceProvider);

        if (!string.IsNullOrWhiteSpace(currentUserName) && entry.Metadata.GetProperties().Any(prop => prop.Name == propertyName))
        {
            entry.Property(propertyName).CurrentValue = currentUserName;
            entry.Property(propertyName).IsModified = true;
        }
    }

    #endregion

    #region Dynamic Fetch

    /// <summary>
    /// Gets requested contents by <typeparamref name="TEntity"/> DbSet.
    /// If this method gets soft deleted entities please override <see cref="CommonHelper.CreateIsDeletedFalseExpression{TEntity}"/> method your own condition.
    /// </summary>
    /// <returns></returns>
    public async Task<List<TEntity>> GetContentsAsync<TEntity>(FilterRequest filterRequest,
                                                               SortRequest sortRequest,
                                                               Expression<Func<TEntity, TEntity>> projectionExpression = null) where TEntity : class
        => await Set<TEntity>().Where(CommonHelper.CreateIsDeletedFalseExpression<TEntity>() ?? (entity => true))
                               .IncludeTranslations(this)
                               .WithFiltering(filterRequest)
                               .WithSorting(sortRequest)
                               .Select(projectionExpression ?? (entity => entity))
                               .ToListAsync();

    /// <summary>
    /// Gets 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TPropertyType"></typeparam>
    /// <param name="propertyName"></param>
    /// <param name="filterRequest"></param>
    /// <param name="sortRequest"></param>
    /// <returns></returns>
    public async Task<List<TPropertyType>> GetEntityPropertyValuesAsync<TEntity, TPropertyType>(string propertyName, FilterRequest filterRequest, SortRequest sortRequest) where TEntity : class
    {
        var keySelector = (Expression<Func<TEntity, TPropertyType>>)CommonHelper.DynamicInvokeCreatePropertySelector(nameof(CommonHelper.CreateRequiredPropertySelector),
                                                                                                                     typeof(TEntity),
                                                                                                                     typeof(TPropertyType),
                                                                                                                     propertyName);

        var result = await Set<TEntity>().AsNoTrackingWithIdentityResolution()
                                         .Where(CommonHelper.CreateIsDeletedFalseExpression<TEntity>() ?? (entity => true))
                                         .WithFiltering(filterRequest)
                                         .WithSorting(sortRequest)
                                         .GroupBy(keySelector)
                                         .Select(x => x.Key)
                                         .ToListAsync();

        return result;
    }

    /// <summary>
    /// Get values for some entity's property.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<List<object>> GetPropertyValuesAsync(EntityPropertyValuesRequest request)
    {
        ValidateRequestParameters(request);

        var assemblyTypes = _dbContextConfiguration.DbContext.DynamicFetch.GetEntityAssembly().GetTypes();

        var entityName = request.EntityName;

        Type entityType = Array.Find(assemblyTypes, i => i.Name == entityName) ?? throw new MilvaUserFriendlyException(MilvaException.InvalidParameter);

        var propType = (entityType.GetProperty(request.PropertyName)?.PropertyType) ?? throw new MilvaUserFriendlyException(MilvaException.InvalidParameter);

        var taskResult = (Task)this.GetType()
                                   .GetMethod(nameof(GetEntityPropertyValuesAsync))
                                   .MakeGenericMethod(entityType, propType)
                                   .Invoke(this,
                                   [
                                       request.PropertyName,
                                       request.Filtering,
                                       request.Sorting,
                                   ]);

        await taskResult;

        var resultProperty = taskResult.GetType().GetProperty("Result");

        var lookupList = (IList)resultProperty.GetValue(taskResult);

        List<object> lookups = [];

        if (lookupList.Count > 0)
            foreach (var lookup in lookupList)
                lookups.Add(lookup);

        return lookups;

        void ValidateRequestParameters(EntityPropertyValuesRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EntityName) ||
                string.IsNullOrWhiteSpace(request.PropertyName) ||
                !_dbContextConfiguration.DbContext.DynamicFetch.AllowedEntityNamesForLookup.Contains(request.EntityName))
                throw new MilvaUserFriendlyException(MilvaException.InvalidParameter);
        }
    }

    /// <summary>
    /// Gets the lookup data for the specified <paramref name="lookupRequest"/>.
    /// </summary>
    /// <param name="lookupRequest">The lookup request containing the parameters for the lookup.</param>
    /// <returns>A list of lookup results containing the requested data.</returns>
    public async Task<List<object>> GetLookupsAsync(LookupRequest lookupRequest)
        => await new LookupManager(this, _dbContextConfiguration).GetLookupsAsync(lookupRequest);

    #endregion

    /// <summary>
    /// Gets <see cref="SetPropertyBuilder{TSource}"/> for entity's matching properties with <paramref name="dto"/>'s updatable properties.
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="dto"></param>
    /// <remarks>
    /// 
    /// This method is used to update the entity object with the values of the updatable properties in the DTO object.
    /// It iterates over the updatable properties in the DTO object and finds the matching property in the entity class.
    /// If a matching property is found and the property value is an instance of <see cref="IUpdateProperty"/> and IsUpdated property is true,
    /// the specified action is performed on the matching property in the entity object.
    /// 
    /// <para></para>
    /// 
    /// If entity implements <see cref="IHasModificationDate"/>, <see cref="EntityPropertyNames.LastModificationDate"/> property call will be added automatically.
    /// If entity implements <see cref="IHasModifier"/>, <see cref="EntityPropertyNames.LastModifierUserName"/> property call will be added automatically.
    /// If utc conversion requested in <see cref="DbContextConfiguration.UseUtcForDateTime"/>, <see cref="DateTime"/> typed property call will be added after converted to utc.
    /// 
    /// </remarks>
    public SetPropertyBuilder<TEntity> GetUpdatablePropertiesBuilder<TEntity, TDto>(TDto dto) where TEntity : class, IMilvaEntity where TDto : DtoBase
    {
        var builder = dto.GetUpdatablePropertiesBuilder<TEntity, TDto>();

        builder = SetAuditProperties(builder);

        return builder;

        SetPropertyBuilder<TEntity> SetAuditProperties(SetPropertyBuilder<TEntity> builder)
        {
            if (typeof(TEntity).CanAssignableTo(typeof(IHasModificationDate)) && _dbContextConfiguration.Auditing.AuditModificationDate)
            {
                var lastModificationDateSelector = CommonHelper.CreatePropertySelector<TEntity, DateTime>(EntityPropertyNames.LastModificationDate);

                builder.AuditCallsAdded = true;
                builder = builder.SetPropertyValue(lastModificationDateSelector, CommonHelper.GetNow(_useUtcForDateTimes));
            }

            if (typeof(TEntity).CanAssignableTo(typeof(IHasModifier)) && _dbContextConfiguration.Auditing.AuditModifier)
            {
                var currentUserName = _dbContextConfiguration.DbContext.InvokeGetCurrentUserMethod(ServiceProvider);

                if (!string.IsNullOrWhiteSpace(currentUserName))
                {
                    var lastModifierUsernameSelector = CommonHelper.CreatePropertySelector<TEntity, string>(EntityPropertyNames.LastModifierUserName);

                    builder.AuditCallsAdded = true;
                    builder = builder.SetPropertyValue(lastModifierUsernameSelector, currentUserName);
                }
            }

            return builder;
        }
    }
}