using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
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
    /// Sets soft deletion state to default state if soft deletion state reset after every operation is enabled.
    /// Called after every save and execute delete operation.
    /// </summary>
    public void ResetSoftDeletionStateAfterOperation()
    {
        if (_resetSoftDeleteStateAfterEveryOperation)
            SetSoftDeletionStateToDefault();
    }

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
    /// Applies auditing and soft deletion (see <see cref="AuditEntites"/>), then saves all changes made in this context to the database.
    /// </summary>
    /// <remarks>
    /// <see cref="DbContext.SaveChanges()"/> delegates to this overload, so every synchronous save path is audited.
    /// Derived contexts that need to run logic on save must override this overload (and <see cref="SaveChangesAsync(bool, CancellationToken)"/>).
    /// </remarks>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <returns></returns>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AuditEntites();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// Applies auditing and soft deletion (see <see cref="AuditEntites"/>), then saves all changes made in this context to the database.
    /// </summary>
    /// <remarks>
    /// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> delegates to this overload, so every asynchronous save path is audited.
    /// Derived contexts that need to run logic on save must override this overload (and <see cref="SaveChanges(bool)"/>).
    /// </remarks>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        AuditEntites();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    #endregion

    #region Auditing

    /// <summary>
    /// Applies soft deletion (when <see cref="SoftDeletionState.Active"/>) and auditing to the tracked entries according to <see cref="AuditConfiguration"/>.
    /// It is called right before the changes are saved. Resets the soft deletion state at the end if it is configured.
    /// </summary>
    /// <remarks>
    /// <para> Added entries get creation audit values (only when they are not already set). </para>
    /// <para> Modified entries get modification audit values on every save. Creation audit values of modified entries are never written
    ///        (for example when an entity is updated with <c>DbSet.Update</c>) while the related creation auditing is enabled. </para>
    /// <para> Deleted soft deletable entries are soft deleted. See <see cref="ApplySoftDeletion"/>. </para>
    /// </remarks>
    protected internal virtual void AuditEntites()
    {
        var autoDetectChanges = ChangeTracker.AutoDetectChangesEnabled;

        if (autoDetectChanges)
            ChangeTracker.DetectChanges();

        // Changes are already detected. Prevent Entries()/Entry() calls below from running change detection again and again.
        ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            var softDeletedEntities = _currentSoftDeleteState == SoftDeletionState.Active
                ? ApplySoftDeletion()
                : new HashSet<object>(ReferenceEqualityComparer.Instance);

            foreach (var entry in ChangeTracker.Entries().ToList())
            {
                if (entry.State == EntityState.Added)
                    AuditCreation(entry);
                else if (entry.State == EntityState.Modified && !softDeletedEntities.Contains(entry.Entity))
                    AuditModification(entry);
            }
        }
        finally
        {
            ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;
        }

        ResetSoftDeletionStateAfterOperation();
    }

    /// <summary>
    /// Converts deleted soft deletable entries to soft deletions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A soft deletion only writes <see cref="EntityPropertyNames.IsDeleted"/>, <see cref="EntityPropertyNames.DeletionDate"/> and
    /// <see cref="EntityPropertyNames.DeleterUserName"/>; no other column of the soft deleted entity is written.
    /// </para>
    /// <para>
    /// Soft deletion only propagates from a principal to its dependents through relationships whose delete behavior is
    /// <see cref="DeleteBehavior.Cascade"/> or <see cref="DeleteBehavior.ClientCascade"/>. It never propagates to principals
    /// (e.g. the parent of a deleted child) and never through many-to-many skip navigations (e.g. tags of a deleted post).
    /// Soft deletable dependents are soft deleted; dependents that are not soft deletable, join entities and owned entities are kept as they are,
    /// because the soft deleted row they depend on still exists.
    /// </para>
    /// <para>
    /// When an entity is removed, EF Core (with the default <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.CascadeTiming.Immediate"/> cascade timing)
    /// immediately applies the cascade side effects of a hard delete to the tracked dependents: cascade dependents are marked as
    /// <see cref="EntityState.Deleted"/> and the foreign keys of the other dependents are set to null. Those side effects can not be intercepted
    /// before they happen with public APIs, so they are undone here, before saving, for every entity that turns out to be soft deleted:
    /// <list type="bullet">
    /// <item> Deleted entries are classified by walking the cascade relationships between them: an entry that was deleted together with
    ///        a hard deleted principal stays deleted (its row must go); an entry deleted only because of a soft deleted principal is soft deleted
    ///        (if soft deletable) or restored (if not). </item>
    /// <item> Foreign keys that EF set to null because their principal was removed are restored to their original values when that principal is
    ///        soft deleted or restored, so no dependent of a soft deleted entity is detached from it in the database. </item>
    /// </list>
    /// A foreign key that was set to null manually in the same unit of work as the soft deletion of its principal is indistinguishable from
    /// the EF side effect, so it is restored too.
    /// </para>
    /// </remarks>
    /// <returns> Soft deleted entities. </returns>
    protected internal virtual HashSet<object> ApplySoftDeletion()
    {
        var softDeletedEntities = new HashSet<object>(ReferenceEqualityComparer.Instance);

        var deletedEntries = ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).ToList();

        if (deletedEntries.Count == 0)
            return softDeletedEntities;

        var fates = ResolveDeletionFates(deletedEntries);

        var restoredEntries = new List<EntityEntry>();
        var propagationQueue = new Queue<EntityEntry>();

        for (int i = 0; i < deletedEntries.Count; i++)
        {
            var entry = deletedEntries[i];

            switch (fates[i])
            {
                case DeletionFate.Soft:

                    // Deleted -> Unchanged keeps current values and clears all modified flags, so only soft deletion columns are written below.
                    entry.State = EntityState.Unchanged;

                    AuditDeletion(entry);

                    softDeletedEntities.Add(entry.Entity);
                    propagationQueue.Enqueue(entry);
                    restoredEntries.Add(entry);

                    break;
                case DeletionFate.Keep:

                    entry.State = EntityState.Unchanged;

                    RestoreModifiedProperties(entry);

                    restoredEntries.Add(entry);

                    break;
                default:
                    break;
            }
        }

        RestoreForeignKeysNulledByCascade(restoredEntries);

        // Dependents which are not marked as deleted by EF (e.g. when the cascade delete timing is not immediate) are soft deleted here.
        while (propagationQueue.TryDequeue(out var principalEntry))
        {
            foreach (var dependentEntry in GetTrackedCascadeDependents(principalEntry))
            {
                if (dependentEntry.State is EntityState.Unchanged or EntityState.Modified
                    && IsSoftDeletable(dependentEntry)
                    && softDeletedEntities.Add(dependentEntry.Entity))
                {
                    AuditDeletion(dependentEntry);
                    propagationQueue.Enqueue(dependentEntry);
                }
            }
        }

        return softDeletedEntities;
    }

    /// <summary>
    /// Marks entry as soft deleted. Only <see cref="EntityPropertyNames.IsDeleted"/>, <see cref="EntityPropertyNames.DeletionDate"/> and
    /// <see cref="EntityPropertyNames.DeleterUserName"/> properties are set and marked as modified. Does nothing if the entity is not soft deletable.
    /// </summary>
    /// <param name="entry"></param>
    protected internal virtual void AuditDeletion(EntityEntry entry)
    {
        if (!IsSoftDeletable(entry))
            return;

        var isDeletedProperty = entry.Property(EntityPropertyNames.IsDeleted);

        isDeletedProperty.CurrentValue = true;
        isDeletedProperty.IsModified = true;

        AuditDate(entry, EntityPropertyNames.DeletionDate, _dbContextConfiguration.Auditing.AuditDeletionDate);

        AuditPerformerUser(entry, EntityPropertyNames.DeleterUserName, _dbContextConfiguration.Auditing.AuditDeleter);
    }

    /// <summary>
    /// Sets creation audit values of an added entry if they are not already set.
    /// </summary>
    /// <param name="entry"></param>
    protected internal virtual void AuditCreation(EntityEntry entry)
    {
        if (entry.HasProperty(EntityPropertyNames.CreationDate) && entry.Property(EntityPropertyNames.CreationDate).CurrentValue == null)
            AuditDate(entry, EntityPropertyNames.CreationDate, _dbContextConfiguration.Auditing.AuditCreationDate);

        if (entry.HasProperty(EntityPropertyNames.CreatorUserName) && entry.Property(EntityPropertyNames.CreatorUserName).CurrentValue == null)
            AuditPerformerUser(entry, EntityPropertyNames.CreatorUserName, _dbContextConfiguration.Auditing.AuditCreator);
    }

    /// <summary>
    /// Sets modification audit values of a modified entry and prevents creation audit values from being overwritten.
    /// </summary>
    /// <param name="entry"></param>
    protected internal virtual void AuditModification(EntityEntry entry)
    {
        ProtectCreationAuditProperty(entry, EntityPropertyNames.CreationDate, _dbContextConfiguration.Auditing.AuditCreationDate);

        ProtectCreationAuditProperty(entry, EntityPropertyNames.CreatorUserName, _dbContextConfiguration.Auditing.AuditCreator);

        // Entry may become unchanged if only creation audit values were modified.
        if (entry.State != EntityState.Modified)
            return;

        AuditDate(entry, EntityPropertyNames.LastModificationDate, _dbContextConfiguration.Auditing.AuditModificationDate);

        AuditPerformerUser(entry, EntityPropertyNames.LastModifierUserName, _dbContextConfiguration.Auditing.AuditModifier);
    }

    /// <summary>
    /// Sets current date to <paramref name="propertyName"/> property and marks it as modified, if <paramref name="audit"/> is true and the entity has the property.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="propertyName"></param>
    /// <param name="audit"></param>
    protected internal virtual void AuditDate(EntityEntry entry, string propertyName, bool audit)
    {
        if (!audit || !entry.HasProperty(propertyName))
            return;

        var property = entry.Property(propertyName);

        property.CurrentValue = CommonHelper.GetNow(_useUtcForDateTimes);
        property.IsModified = true;
    }

    /// <summary>
    /// Sets current user name to <paramref name="propertyName"/> property and marks it as modified,
    /// if <paramref name="audit"/> is true, the entity has the property and the current user name is available.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="propertyName"></param>
    /// <param name="audit"></param>
    protected internal virtual void AuditPerformerUser(EntityEntry entry, string propertyName, bool audit)
    {
        if (!audit || !entry.HasProperty(propertyName))
            return;

        var currentUserName = _dbContextConfiguration.DbContext.GetCurrentUserNameMethod?.Invoke(ServiceProvider);

        if (string.IsNullOrWhiteSpace(currentUserName))
            return;

        var property = entry.Property(propertyName);

        property.CurrentValue = currentUserName;
        property.IsModified = true;
    }

    /// <summary>
    /// Prevents a creation audit property of a modified entry from being written to the database.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="propertyName"></param>
    /// <param name="audit"></param>
    private static void ProtectCreationAuditProperty(EntityEntry entry, string propertyName, bool audit)
    {
        if (!audit || !entry.HasProperty(propertyName))
            return;

        var property = entry.Property(propertyName);

        if (!property.IsModified)
            return;

        property.CurrentValue = property.OriginalValue;
        property.IsModified = false;
    }

    private static bool IsSoftDeletable(EntityEntry entry) => entry.Metadata.FindProperty(EntityPropertyNames.IsDeleted)?.ClrType == typeof(bool);

    private static bool IsCascade(IReadOnlyForeignKey foreignKey) => foreignKey.DeleteBehavior is DeleteBehavior.Cascade or DeleteBehavior.ClientCascade;

    /// <summary>
    /// Decides what happens to each deleted entry when soft deletion is active.
    /// </summary>
    /// <remarks>
    /// The "parents" of a deleted entry are the deleted entries it depends on through a cascade (or ownership) relationship,
    /// i.e. the principals whose removal made EF mark it as deleted.
    /// <list type="bullet">
    /// <item> Entry without deleted parents (deleted explicitly): soft deleted if soft deletable (and not owned), otherwise hard deleted. </item>
    /// <item> Entry with a hard deleted parent: hard deleted, because its principal row will not exist anymore. </item>
    /// <item> Owned entry of a soft deleted/kept owner: kept, it is a part of its owner. </item>
    /// <item> Soft deletable entry with a soft deleted parent: soft deleted. </item>
    /// <item> Other entries with soft deleted/kept parents: kept. </item>
    /// </list>
    /// </remarks>
    private static DeletionFate[] ResolveDeletionFates(List<EntityEntry> deletedEntries)
    {
        var keyIndex = new EntryKeyIndex(deletedEntries);

        var parents = new List<int>[deletedEntries.Count];

        for (int i = 0; i < deletedEntries.Count; i++)
        {
            var entry = deletedEntries[i];

            parents[i] = [];

            foreach (var foreignKey in entry.Metadata.GetForeignKeys())
            {
                if (!foreignKey.IsOwnership && !IsCascade(foreignKey))
                    continue;

                if (keyIndex.TryFind(foreignKey, GetCurrentValues(entry, foreignKey.Properties), out var parentIndex) && parentIndex != i)
                    parents[i].Add(parentIndex);
            }
        }

        var fates = new DeletionFate[deletedEntries.Count];

        bool resolvedAny;

        do
        {
            resolvedAny = false;

            for (int i = 0; i < deletedEntries.Count; i++)
            {
                if (fates[i] != DeletionFate.Undetermined || parents[i].Exists(p => fates[p] == DeletionFate.Undetermined))
                    continue;

                fates[i] = parents[i].Count == 0 ? ResolveRootFate(deletedEntries[i]) : ResolveDependentFate(deletedEntries[i], parents[i], fates);
                resolvedAny = true;
            }
        } while (resolvedAny);

        // Remaining entries are in cascade cycles.
        for (int i = 0; i < deletedEntries.Count; i++)
            if (fates[i] == DeletionFate.Undetermined)
                fates[i] = ResolveRootFate(deletedEntries[i]);

        return fates;

        static DeletionFate ResolveRootFate(EntityEntry entry) => IsSoftDeletable(entry) && !entry.Metadata.IsOwned() ? DeletionFate.Soft : DeletionFate.Hard;

        static DeletionFate ResolveDependentFate(EntityEntry entry, List<int> parentIndexes, DeletionFate[] fates)
        {
            if (parentIndexes.Exists(p => fates[p] == DeletionFate.Hard))
                return DeletionFate.Hard;

            if (!entry.Metadata.IsOwned() && IsSoftDeletable(entry) && parentIndexes.Exists(p => fates[p] == DeletionFate.Soft))
                return DeletionFate.Soft;

            return DeletionFate.Keep;
        }
    }

    /// <summary>
    /// Marks properties whose current value differs from the original value as modified. Used to restore the modified state of an entry
    /// that EF marked as deleted only because of a cascade.
    /// </summary>
    /// <param name="entry"></param>
    private static void RestoreModifiedProperties(EntityEntry entry)
    {
        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsPrimaryKey())
                continue;

            if (!property.Metadata.GetValueComparer().Equals(property.OriginalValue, property.CurrentValue))
                property.IsModified = true;
        }
    }

    /// <summary>
    /// Restores foreign keys that EF set to null when their principal was removed, for principals that are not hard deleted anymore.
    /// </summary>
    /// <param name="restoredPrincipalEntries"></param>
    private void RestoreForeignKeysNulledByCascade(List<EntityEntry> restoredPrincipalEntries)
    {
        if (restoredPrincipalEntries.Count == 0)
            return;

        var keyIndex = new EntryKeyIndex(restoredPrincipalEntries);

        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList())
        {
            foreach (var foreignKey in entry.Metadata.GetForeignKeys())
            {
                // EF sets foreign keys of these relationships to null when the principal is removed.
                if (foreignKey.IsOwnership || IsCascade(foreignKey) || foreignKey.DeleteBehavior == DeleteBehavior.ClientNoAction)
                    continue;

                var foreignKeyProperties = foreignKey.Properties.Select(entry.Property).ToList();

                if (!foreignKeyProperties.TrueForAll(p => p.IsModified && p.CurrentValue == null && p.OriginalValue != null))
                    continue;

                var originalValues = foreignKeyProperties.Select(p => p.OriginalValue).ToArray();

                if (!keyIndex.TryFind(foreignKey, originalValues, out _))
                    continue;

                foreach (var foreignKeyProperty in foreignKeyProperties)
                {
                    foreignKeyProperty.CurrentValue = foreignKeyProperty.OriginalValue;
                    foreignKeyProperty.IsModified = false;
                }
            }
        }
    }

    /// <summary>
    /// Gets tracked dependents of <paramref name="principalEntry"/> which are reachable through its loaded cascade (non-ownership) navigations.
    /// </summary>
    /// <param name="principalEntry"></param>
    /// <returns></returns>
    private List<EntityEntry> GetTrackedCascadeDependents(EntityEntry principalEntry)
    {
        var dependents = new List<EntityEntry>();

        foreach (var navigation in principalEntry.Metadata.GetNavigations())
        {
            if (navigation.IsOnDependent || navigation.ForeignKey.IsOwnership || !IsCascade(navigation.ForeignKey))
                continue;

            var navigationValue = principalEntry.Navigation(navigation.Name).CurrentValue;

            if (navigationValue is null)
                continue;

            if (navigation.IsCollection)
            {
                foreach (var dependent in (IEnumerable)navigationValue)
                    AddIfTracked(dependent);
            }
            else
                AddIfTracked(navigationValue);
        }

        return dependents;

        void AddIfTracked(object dependent)
        {
            if (dependent is null)
                return;

            var dependentEntry = Entry(dependent);

            if (dependentEntry.State != EntityState.Detached)
                dependents.Add(dependentEntry);
        }
    }

    private static object[] GetCurrentValues(EntityEntry entry, IReadOnlyList<IProperty> properties)
    {
        var values = new object[properties.Count];

        for (int i = 0; i < properties.Count; i++)
            values[i] = entry.Property(properties[i]).CurrentValue;

        return values;
    }

    private enum DeletionFate
    {
        Undetermined,
        Hard,
        Soft,
        Keep
    }

    /// <summary>
    /// Finds entries by key values. Used for matching foreign key values to principal entries.
    /// </summary>
    private sealed class EntryKeyIndex
    {
        private readonly Dictionary<(IKey Key, KeyValues Values), int> _index = [];

        public EntryKeyIndex(List<EntityEntry> entries)
        {
            for (int i = 0; i < entries.Count; i++)
                foreach (var key in entries[i].Metadata.GetKeys())
                    _index.TryAdd((key, new KeyValues(GetCurrentValues(entries[i], key.Properties))), i);
        }

        /// <summary>
        /// Finds principal entry of <paramref name="foreignKey"/> whose principal key values equal to <paramref name="foreignKeyValues"/>.
        /// </summary>
        public bool TryFind(IForeignKey foreignKey, object[] foreignKeyValues, out int entryIndex)
        {
            entryIndex = -1;

            if (Array.Exists(foreignKeyValues, v => v is null))
                return false;

            return _index.TryGetValue((foreignKey.PrincipalKey, new KeyValues(foreignKeyValues)), out entryIndex);
        }
    }

    private readonly struct KeyValues(object[] values) : IEquatable<KeyValues>
    {
        private readonly object[] _values = values;

        public bool Equals(KeyValues other)
        {
            if (_values.Length != other._values.Length)
                return false;

            for (int i = 0; i < _values.Length; i++)
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(_values[i], other._values[i]))
                    return false;

            return true;
        }

        public override bool Equals(object obj) => obj is KeyValues other && Equals(other);

        public override int GetHashCode()
        {
            var hash = new HashCode();

            foreach (var value in _values)
                hash.Add(value is null ? 0 : StructuralComparisons.StructuralEqualityComparer.GetHashCode(value));

            return hash.ToHashCode();
        }
    }

    #endregion

    #region Dynamic Fetch

    /// <summary>
    /// Gets requested contents by <typeparamref name="TEntity"/> DbSet.
    /// If this method gets soft deleted entities please override <see cref="CommonHelper.CreateIsDeletedFalseExpression{TEntity}"/> method your own condition.
    /// </summary>
    /// <returns></returns>
    public Task<List<TEntity>> GetContentsAsync<TEntity>(FilterRequest filterRequest,
                                                         SortRequest sortRequest,
                                                         Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                         bool fetchSoftDeletedEntities = false) where TEntity : class
        => Set<TEntity>().Where(fetchSoftDeletedEntities ? e => true : MilvaDbContextCache.GetIsDeletedFilter<TEntity>())
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

        var assemblyTypes = _dbContextConfiguration.DbContext.DynamicFetch.GetEntityAssemblyTypes();

        var entityName = request.EntityName;

        Type entityType = Array.Find(assemblyTypes, i => i.Name == entityName) ?? throw new MilvaUserFriendlyException(MilvaException.InvalidParameter);

        var propType = (entityType.GetProperty(request.PropertyName)?.PropertyType) ?? throw new MilvaUserFriendlyException(MilvaException.InvalidParameter);

        dynamic taskResult = this.GetType()
                                   .GetMethod(nameof(GetEntityPropertyValuesAsync))
                                   .MakeGenericMethod(entityType, propType)
                                   .Invoke(this,
                                   [
                                       request.PropertyName,
                                       request.Filtering,
                                       request.Sorting,
                                   ]);

        var lookupList = (IList)await taskResult;

        List<object> lookups = [];

        if (lookupList.Count > 0)
            lookups.AddRange(lookupList.Cast<object>());

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
    public Task<List<object>> GetLookupsAsync(LookupRequest lookupRequest)
        => new LookupManager(this, _dbContextConfiguration).GetLookupsAsync(lookupRequest);

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
        var builder = dto.GetUpdatablePropertiesBuilder<TEntity, TDto>(_useUtcForDateTimes);

        SetAuditProperties(this, builder);

        return builder;

        static void SetAuditProperties(MilvaDbContext @this, SetPropertyBuilder<TEntity> builder)
        {
            if (typeof(TEntity).CanAssignableTo(typeof(IHasModificationDate)) && @this._dbContextConfiguration.Auditing.AuditModificationDate)
            {
                var lastModificationDateSelector = CommonHelper.CreatePropertySelector<TEntity, DateTime>(EntityPropertyNames.LastModificationDate);

                builder.AuditCallsAdded = true;
                builder = builder.SetPropertyValue(lastModificationDateSelector, CommonHelper.GetNow(@this._useUtcForDateTimes));
            }

            if (typeof(TEntity).CanAssignableTo(typeof(IHasModifier)) && @this._dbContextConfiguration.Auditing.AuditModifier)
            {
                var currentUserName = @this._dbContextConfiguration.DbContext.InvokeGetCurrentUserMethod(@this.ServiceProvider);

                if (!string.IsNullOrWhiteSpace(currentUserName))
                {
                    var lastModifierUsernameSelector = CommonHelper.CreatePropertySelector<TEntity, string>(EntityPropertyNames.LastModifierUserName);

                    builder.AuditCallsAdded = true;
                    builder.SetPropertyValue(lastModifierUsernameSelector, currentUserName);
                }
            }
        }
    }
}