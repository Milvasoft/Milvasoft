using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Milvasoft.BulkExtensions.Internal;

/// <summary>
/// Saves the changes tracked by a context with bulk operations: one insert, update and delete per table.
/// </summary>
/// <remarks>
/// Tables are processed in foreign key order: inserts and updates from principal to dependent tables, deletes in the reverse order.
/// New rows that refer to other new rows of the same table (such as a tree) are inserted level by level.
/// Database generated keys are passed to dependent rows during the operation and written to the change tracker after it succeeds,
/// after which all changes are accepted, as <c>SaveChanges</c> does.
/// </remarks>
internal static class BulkSaveChangesRunner
{
    public static Task<int> SaveAsync(DbContext context, BulkConfig config, bool async, CancellationToken cancellationToken)
    {
        BulkProviderResolver.Resolve(context);

        if (context.ChangeTracker.AutoDetectChangesEnabled)
            context.ChangeTracker.DetectChanges();

        var plan = BuildPlan(context);

        if (plan.Count == 0)
            return Task.FromResult(0);

        return BulkRunner.RunAsync(context, config, saveChangesMode: true, (executor, isAsync, ct) => ExecuteAsync(plan, executor, isAsync, ct), async, cancellationToken);
    }

    private static async Task<int> ExecuteAsync(List<TablePlan> plan, BulkExecutor executor, bool async, CancellationToken cancellationToken)
    {
        var total = 0;

        foreach (var table in plan)
        {
            var remaining = table.Inserts;

            while (remaining.Count > 0)
            {
                // Rows whose foreign keys point to rows inserted in an earlier round, or to existing rows.
                var probe = new EntryRowSource(table.Model, remaining, executor.TemporaryValues);
                var ready = new List<EntityEntry>();
                var waiting = new List<EntityEntry>();

                for (int row = 0; row < probe.Count; row++)
                    (HasPendingForeignKey(probe, row) ? waiting : ready).Add(remaining[row]);

                if (ready.Count == 0)
                    throw new InvalidOperationException($"New rows of table '{table.Model.Info}' refer to each other in a cycle. Save them with SaveChanges instead.");

                total += await executor.InsertAsync(new EntryRowSource(table.Model, ready, executor.TemporaryValues), async, cancellationToken);

                remaining = waiting;
            }
        }

        foreach (var table in plan)
            if (table.Updates.Count > 0)
                total += await executor.UpdateEntriesAsync(new EntryRowSource(table.Model, table.Updates, executor.TemporaryValues), async, cancellationToken);

        for (int i = plan.Count - 1; i >= 0; i--)
            if (plan[i].Deletes.Count > 0)
                total += await executor.DeleteEntriesAsync(new EntryRowSource(plan[i].Model, plan[i].Deletes, executor.TemporaryValues), async, cancellationToken);

        var tracker = executor.Context.ChangeTracker;

        executor.Pending.Add(tracker.AcceptAllChanges);

        return total;
    }

    /// <summary>
    /// Whether a foreign key of the row still holds a temporary value of a principal that has not been inserted yet.
    /// </summary>
    private static bool HasPendingForeignKey(EntryRowSource source, int row)
    {
        foreach (var binding in source.GetBindings(row))
        {
            if (binding is null || !binding.IsForeignKey)
                continue;

            if (source.Read(row, binding).Kind == ValueKind.Temporary && !(binding.Property.IsPrimaryKey() && binding.IsGeneratedOnAdd))
                return true;
        }

        return false;
    }

    #region Plan

    private sealed class TablePlan(TableModel model)
    {
        public TableModel Model { get; } = model;

        public List<EntityEntry> Inserts { get; } = [];

        public List<EntityEntry> Updates { get; } = [];

        public List<EntityEntry> Deletes { get; } = [];

        public HashSet<EntityEntry> UpdateSet { get; } = [];
    }

    private static List<TablePlan> BuildPlan(DbContext context)
    {
        var plans = new Dictionary<ITable, TablePlan>();
        var order = new List<TablePlan>();
        var changedSharedRows = new List<EntityEntry>();

        TablePlan GetPlan(TableModel model)
        {
            if (!plans.TryGetValue(model.Table, out var plan))
            {
                plans[model.Table] = plan = new TablePlan(model);
                order.Add(plan);
            }

            return plan;
        }

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Unchanged or EntityState.Detached)
                continue;

            // Owned types stored in their owner's row change the owner's row.
            if (TableModel.IsTableSharingDependent(entry.Metadata))
            {
                changedSharedRows.Add(entry);
                continue;
            }

            var plan = GetPlan(TableModel.For(entry.Metadata));

            switch (entry.State)
            {
                case EntityState.Added:
                    plan.Inserts.Add(entry);
                    break;
                case EntityState.Modified:
                    plan.Updates.Add(entry);
                    plan.UpdateSet.Add(entry);
                    break;
                case EntityState.Deleted:
                    plan.Deletes.Add(entry);
                    break;
            }
        }

        if (changedSharedRows.Count > 0)
        {
            var owners = new RowOwnerIndex(context);

            foreach (var entry in changedSharedRows)
            {
                var (model, owner) = owners.Find(entry);

                // Added and deleted owners are written as a whole row anyway.
                if (owner.State != EntityState.Unchanged && owner.State != EntityState.Modified)
                    continue;

                var plan = GetPlan(model);

                if (plan.UpdateSet.Add(owner))
                    plan.Updates.Add(owner);
            }
        }

        return SortByDependencies(order);
    }

    /// <summary>
    /// Orders tables so that principal tables come before the tables that refer to them.
    /// </summary>
    private static List<TablePlan> SortByDependencies(List<TablePlan> plans)
    {
        var byTable = plans.ToDictionary(p => p.Model.Table);
        var dependencies = plans.ToDictionary(p => p, _ => new HashSet<TablePlan>());

        foreach (var plan in plans)
        {
            var entityTypes = plan.Model.Table.EntityTypeMappings.Select(m => m.TypeBase).OfType<IEntityType>().Distinct();

            foreach (var entityType in entityTypes)
            {
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    // With TPC the principal rows live in the tables of the concrete types, not of the base type.
                    foreach (var principalMapping in foreignKey.PrincipalEntityType.GetDerivedTypesInclusive().SelectMany(t => t.GetTableMappings()))
                    {
                        if (principalMapping.Table != plan.Model.Table && byTable.TryGetValue(principalMapping.Table, out var principal))
                            dependencies[plan].Add(principal);
                    }
                }
            }
        }

        var sorted = new List<TablePlan>(plans.Count);
        var done = new HashSet<TablePlan>();

        while (sorted.Count < plans.Count)
        {
            var next = plans.FirstOrDefault(p => !done.Contains(p) && dependencies[p].All(done.Contains))
                ?? throw new NotSupportedException("The changed tables refer to each other in a cycle ("
                                                   + string.Join(", ", plans.Where(p => !done.Contains(p)).Select(p => p.Model.Info))
                                                   + "). Save these changes with SaveChanges instead.");

            sorted.Add(next);
            done.Add(next);
        }

        return sorted;
    }

    /// <summary>
    /// Finds the entry that owns the table row of an owned entity stored in its owner's table. Owned and owner rows share the primary key.
    /// </summary>
    private sealed class RowOwnerIndex(DbContext context)
    {
        private readonly Dictionary<ITable, (TableModel Model, Dictionary<object[], EntityEntry> Rows)> _tables = [];

        public (TableModel Model, EntityEntry Owner) Find(EntityEntry ownedEntry)
        {
            var table = ownedEntry.Metadata.GetTableMappings().Single().Table;

            if (!_tables.TryGetValue(table, out var index))
                _tables[table] = index = Build(table);

            var key = GetKey(ownedEntry, index.Model.PrimaryKey);

            if (key is null || !index.Rows.TryGetValue(key, out var owner))
                throw new InvalidOperationException($"The owner of the changed '{ownedEntry.Metadata.DisplayName()}' entity is not tracked by the context.");

            return (index.Model, owner);
        }

        private (TableModel, Dictionary<object[], EntityEntry>) Build(ITable table)
        {
            TableModel model = null;
            var rows = new Dictionary<object[], EntityEntry>(KeyComparer.Instance);

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (TableModel.IsTableSharingDependent(entry.Metadata) || !entry.Metadata.GetTableMappings().Any(m => m.Table == table))
                    continue;

                model ??= TableModel.For(entry.Metadata);

                if (GetKey(entry, model.PrimaryKey) is { } key)
                    rows.TryAdd(key, entry);
            }

            return (model ?? throw new InvalidOperationException($"No owner of table '{table.Name}' is tracked by the context."), rows);
        }

        /// <summary>
        /// Values of the primary key columns for the entry, read through the properties of its own type mapped to these columns.
        /// </summary>
        private static object[] GetKey(EntityEntry entry, IReadOnlyList<ColumnModel> primaryKey)
        {
            var key = new object[primaryKey.Count];

            for (int i = 0; i < key.Length; i++)
            {
                var property = primaryKey[i].Column.PropertyMappings.Select(m => m.Property)
                                                                    .FirstOrDefault(p => p.DeclaringType is IEntityType declaring && declaring.IsAssignableFrom(entry.Metadata));

                if (property is null)
                    return null;

                key[i] = entry.Property(property).CurrentValue;
            }

            return key;
        }
    }

    #endregion
}