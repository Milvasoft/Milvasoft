using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Milvasoft.BulkExtensions.Infrastructure;
using System.Data.Common;

namespace Milvasoft.BulkExtensions.Internal;

/// <summary>
/// Runs bulk operations for the rows of one table: chooses columns, splits rows, calls the provider and collects the values the database returns.
/// One instance serves one attempt of one public operation.
/// </summary>
internal sealed class BulkExecutor(BulkProvider provider, BulkCommandContext command, BulkConfig config, bool saveChangesMode)
{
    private readonly DbContext _context = command.Context;
    private readonly BulkProvider _provider = provider;
    private readonly BulkCommandContext _command = command;
    private readonly BulkConfig _config = config ?? new BulkConfig();
    private readonly bool _saveChangesMode = saveChangesMode;
    private readonly Dictionary<IProperty, ValueGenerator> _clientGenerators = [];
    private IValueGeneratorSelector _generatorSelector;

    public DbContext Context => _context;

    /// <summary>
    /// Changes to entities, applied after the operation succeeds.
    /// </summary>
    public PendingWrites Pending { get; } = new();

    /// <summary>
    /// Database values of temporary keys. Only used by <c>BulkSaveChanges</c>.
    /// </summary>
    public TemporaryValueMap TemporaryValues { get; } = saveChangesMode ? new TemporaryValueMap() : null;

    /// <summary>
    /// Property filters of <see cref="BulkConfig"/> apply to the public operations, not to <c>BulkSaveChanges</c>.
    /// </summary>
    private bool UseConfigFilters => !_saveChangesMode;

    /// <summary>
    /// <c>BulkSaveChanges</c> always reads generated values back, like <c>SaveChanges</c>.
    /// </summary>
    private bool ReadBack => _saveChangesMode || _config.SetOutputIdentity;

    #region Operations

    public async Task<int> InsertAsync(RowSource source, bool async, CancellationToken cancellationToken)
    {
        var columns = GetInsertColumns(source, []);

        if (source is EntityRowSource entityRows)
            await GenerateClientValuesAsync(entityRows, columns, async, cancellationToken);

        var total = 0;

        foreach (var partition in Partition(source, columns))
            total += await InsertPartitionAsync(source, partition, async, cancellationToken);

        return total;
    }

    public Task<int> UpdateAsync(RowSource source, bool async, CancellationToken cancellationToken)
    {
        EnsureWritable(source.Table);

        var match = GetMatchColumns(source.Table);
        var set = GetUpdateColumns(source, source.Table.Columns.Where(c => source.BindingsOf(c).Any()), match, applyOnUpdateFilters: false);

        if (set.Count == 0)
            throw new InvalidOperationException($"There are no columns to update in table '{source.Table.Info}'. Check {nameof(BulkConfig.PropertiesToInclude)} and {nameof(BulkConfig.PropertiesToExclude)}.");

        var rows = Enumerable.Range(0, source.Count).ToArray();

        BuildKeyIndex(source, rows, match, "updated");

        var rowSet = new RowSetView(source, rows, [.. match, .. set]);

        PositionReturning returning = null;

        if (ReadBack && GetUpdateReadColumns(source, match, set) is { Count: > 0 } readColumns)
            returning = new PositionReturning(this, source, rows, readColumns);

        return _provider.UpdateAsync(_command, source.Table.Info, rowSet, Infos(match), Infos(set), returning, async, cancellationToken);
    }

    /// <summary>
    /// Updates the changed columns of modified change tracker entries, as <c>SaveChanges</c> does. Entries are grouped by the set of
    /// changed columns, concurrency tokens are compared with their original values, and an entry whose row is not found fails with
    /// <see cref="DbUpdateConcurrencyException"/>.
    /// </summary>
    public async Task<int> UpdateEntriesAsync(EntryRowSource source, bool async, CancellationToken cancellationToken)
    {
        EnsureWritable(source.Table);

        var match = GetMatchColumns(source.Table);
        var tokens = GetConcurrencyTokenColumns(source, match);
        var candidates = GetUpdateColumns(source, source.Table.Columns.Where(c => source.BindingsOf(c).Any()), match, applyOnUpdateFilters: false);
        var groups = new Dictionary<string, List<int>>(StringComparer.Ordinal);
        var order = new List<string>();

        for (int row = 0; row < source.Count; row++)
        {
            var bindings = source.GetBindings(row);
            var changed = new char[candidates.Count];

            for (int i = 0; i < candidates.Count; i++)
                changed[i] = bindings[candidates[i].Index] is { } binding && source.IsModified(row, binding) ? '1' : '0';

            var key = new string(changed);

            if (!groups.TryGetValue(key, out var rows))
            {
                groups[key] = rows = [];
                order.Add(key);
            }

            rows.Add(row);
        }

        var total = 0;

        foreach (var key in order)
        {
            var set = candidates.Where((_, i) => key[i] == '1').ToList();

            // Only properties that are never written after insert changed.
            if (set.Count == 0)
                continue;

            var rows = groups[key].ToArray();
            var rowSet = new RowSetView(source, rows, [.. match, .. set], tokens);
            var returning = new PositionReturning(this, source, rows, GetUpdateReadColumns(source, match, set));

            var updated = await _provider.UpdateAsync(_command, source.Table.Info, rowSet, [.. Infos(match), .. rowSet.OriginalColumns], Infos(set), returning, async, cancellationToken);

            if (updated != rows.Length || returning.Received != rows.Length)
            {
                var missing = rows.Where((_, i) => !returning.WasReceived(i)).Select(source.GetEntry).ToList();

                throw new BulkConcurrencyException($"{missing.Count} of {rows.Length} updated rows of table '{source.Table.Info}' were not found. "
                                                       + "They were deleted, or their concurrency token changed, since they were loaded.", missing);
            }

            total += updated;
        }

        return total;
    }

    public async Task<int> UpsertAsync(RowSource source, bool async, CancellationToken cancellationToken)
    {
        var match = GetMatchColumns(source.Table);
        var columns = GetInsertColumns(source, match);

        foreach (var column in match)
            if (!columns.Contains(column))
                throw new InvalidOperationException($"Column '{column.Name}' is used to match rows but its value cannot be inserted (it is computed or not mapped for the given entities).");

        var generated = source is EntityRowSource entityRows ? await GenerateClientValuesAsync(entityRows, columns, async, cancellationToken) : [];

        // Rows whose match values are left to the database, or are NULL, cannot match an existing row; they are plain inserts.
        var partitions = new List<RowPartition>();

        foreach (var partition in Partition(source, columns))
        {
            if (!match.All(partition.Columns.Contains))
            {
                partitions.Add(partition with { InsertOnly = true });
                continue;
            }

            var withNull = partition.Rows.Where(row => source.GetKey(row, match).Any(v => v is null)).ToArray();

            if (withNull.Length == 0)
            {
                partitions.Add(partition);
                continue;
            }

            partitions.Add(partition with { Rows = [.. partition.Rows.Except(withNull)] });
            partitions.Add(partition with { Rows = withNull, InsertOnly = true });
        }

        var keys = BuildKeyIndex(source, [.. partitions.Where(p => !p.InsertOnly).SelectMany(p => p.Rows)], match, "inserted or updated");
        var total = 0;

        foreach (var partition in partitions)
        {
            if (partition.Rows.Length == 0)
                continue;

            if (partition.InsertOnly)
            {
                total += await InsertPartitionAsync(source, partition, async, cancellationToken);
                continue;
            }

            var set = GetUpdateColumns(source, partition.Columns, match, applyOnUpdateFilters: true);
            var rowSet = new RowSetView(source, partition.Rows, partition.Columns);

            // Existing rows keep their key and their unchanged columns. Keys and generated values are read back, and so are values
            // generated on the client, which an existing row does not take.
            var readColumns = source.Table.Columns.Where(c => c.IsSupported
                                                              && !match.Contains(c)
                                                              && !set.Contains(c)
                                                              && (generated.Contains(c)
                                                                  || (ReadBack && (c.IsPrimaryKey
                                                                                   || source.BindingsOf(c).Any(b => (IsStoreGenerated(b) && !partition.Columns.Contains(c)) || b.IsGeneratedOnUpdate)))))
                                                  .ToList();

            var returning = readColumns.Count > 0 ? new KeyedReturning(this, source, match, readColumns, keys) : null;

            total += await _provider.UpsertAsync(_command, source.Table.Info, rowSet, Infos(match), Infos(set), returning, async, cancellationToken);
        }

        return total;
    }

    public Task<int> DeleteAsync(RowSource source, bool async, CancellationToken cancellationToken)
    {
        var match = GetMatchColumns(source.Table);

        return _provider.DeleteAsync(_command, source.Table.Info, new KeyRowSet(source, match), Infos(match), async, cancellationToken);
    }

    /// <summary>
    /// Deletes the rows of deleted change tracker entries, comparing concurrency tokens with their original values.
    /// Fails with <see cref="DbUpdateConcurrencyException"/> when a row is not found.
    /// </summary>
    public async Task<int> DeleteEntriesAsync(EntryRowSource source, bool async, CancellationToken cancellationToken)
    {
        var match = GetMatchColumns(source.Table);
        var tokens = GetConcurrencyTokenColumns(source, match);
        var rows = Enumerable.Range(0, source.Count).ToArray();
        var rowSet = new RowSetView(source, rows, match, tokens);

        var deleted = await _provider.DeleteAsync(_command, source.Table.Info, rowSet, [.. Infos(match), .. rowSet.OriginalColumns], async, cancellationToken);

        if (deleted != rows.Length)
            throw new BulkConcurrencyException($"{rows.Length - deleted} of {rows.Length} deleted rows of table '{source.Table.Info}' were not found. "
                                                   + "They were deleted, or their concurrency token changed, since they were loaded.",
                                                   [.. rows.Select(source.GetEntry)]);

        return deleted;
    }

    public Task<List<T>> ReadAsync<T>(RowSource source, bool async, CancellationToken cancellationToken) where T : class
    {
        var match = GetMatchColumns(source.Table);

        return _provider.ReadAsync(_command, source.Table.Info, new KeyRowSet(source, match), Infos(match), Infos(source.Table.Columns), QueryAsync<T>, async, cancellationToken);
    }

    public Task TruncateAsync(TableModel table, bool async, CancellationToken cancellationToken)
        => _provider.TruncateAsync(_command, table.Info, async, cancellationToken);

    #endregion

    #region Insert helpers

    private async Task<int> InsertPartitionAsync(RowSource source, RowPartition partition, bool async, CancellationToken cancellationToken)
    {
        var rowSet = new RowSetView(source, partition.Rows, partition.Columns);

        PositionReturning returning = null;

        if (ReadBack)
        {
            var readColumns = source.Table.Columns.Where(c => c.IsSupported
                                                              && !partition.Columns.Contains(c)
                                                              && source.BindingsOf(c).Any(IsStoreGenerated))
                                                  .ToList();

            if (readColumns.Count > 0)
                returning = new PositionReturning(this, source, partition.Rows, readColumns);
        }

        var count = await _provider.InsertAsync(_command, source.Table.Info, rowSet, returning, async, cancellationToken);

        if (returning is not null && returning.Received != partition.Rows.Length)
            throw new InvalidOperationException($"The database returned {returning.Received} rows for {partition.Rows.Length} inserted rows in table '{source.Table.Info}'.");

        return count;
    }

    /// <summary>
    /// Columns written by an insert: every column with a property of the given entities, except columns the database always computes.
    /// The discriminator and <paramref name="required"/> columns are kept even when the configuration filters them out.
    /// </summary>
    private List<ColumnModel> GetInsertColumns(RowSource source, IReadOnlyCollection<ColumnModel> required)
    {
        EnsureWritable(source.Table);

        var include = UseConfigFilters && _config.PropertiesToInclude is { Count: > 0 } includeNames
            ? source.Table.ResolveColumns(includeNames, nameof(BulkConfig.PropertiesToInclude))
            : null;

        var exclude = UseConfigFilters && _config.PropertiesToExclude is { Count: > 0 } excludeNames
            ? source.Table.ResolveColumns(excludeNames, nameof(BulkConfig.PropertiesToExclude))
            : null;

        var columns = new List<ColumnModel>();

        foreach (var column in source.Table.Columns)
        {
            var bindings = source.BindingsOf(column).ToList();

            if (bindings.Count == 0 || bindings.All(b => b.BeforeSave == PropertySaveBehavior.Ignore))
                continue;

            if (!bindings.Any(b => b.IsDiscriminator) && !required.Contains(column))
            {
                if (include is not null && !include.Contains(column))
                    continue;

                if (exclude is not null && exclude.Contains(column))
                    continue;
            }

            columns.Add(column);
        }

        return columns;
    }

    /// <summary>
    /// Splits rows by which database generated columns they leave unset. A column that a row leaves at its default value is omitted
    /// for that row, so the database generates it (this is how EF Core treats identity keys and default values).
    /// Usually all rows end up in one partition.
    /// </summary>
    private List<RowPartition> Partition(RowSource source, List<ColumnModel> columns)
    {
        var generated = columns.Where(c => source.BindingsOf(c).Any(IsStoreGeneratedOnAdd)).ToList();

        if (generated.Count > 64)
            throw new NotSupportedException($"Table '{source.Table.Info}' has more than 64 columns generated by the database, which bulk operations do not support.");

        var rowsByMask = new Dictionary<ulong, List<int>>();
        var masks = new List<ulong>();

        for (int row = 0; row < source.Count; row++)
        {
            var bindings = source.GetBindings(row);
            ulong mask = 0;

            for (int i = 0; i < generated.Count; i++)
            {
                var binding = bindings[generated[i].Index];

                // The column belongs to another type of the hierarchy; let the database fill it.
                if (binding is null)
                {
                    mask |= 1UL << i;
                    continue;
                }

                if (!IsStoreGeneratedOnAdd(binding))
                    continue;

                if (source.Read(row, binding).IsSentinel)
                    mask |= 1UL << i;
                else if (binding.BeforeSave == PropertySaveBehavior.Throw)
                    throw new InvalidOperationException($"'{binding.Path}' of entity type '{source.GetEntityType(row).DisplayName()}' is generated by the database and cannot be set. Leave it at its default value.");
            }

            if (!rowsByMask.TryGetValue(mask, out var rows))
            {
                rowsByMask[mask] = rows = [];
                masks.Add(mask);
            }

            rows.Add(row);
        }

        var partitions = new List<RowPartition>(masks.Count);

        foreach (var mask in masks)
        {
            var omitted = new HashSet<ColumnModel>();

            for (int i = 0; i < generated.Count; i++)
                if ((mask & (1UL << i)) != 0)
                    omitted.Add(generated[i]);

            partitions.Add(new RowPartition([.. rowsByMask[mask]], [.. columns.Where(c => !omitted.Contains(c))]));
        }

        return partitions;
    }

    /// <summary>
    /// Runs client side value generators (for example for <see cref="Guid"/> keys) for entities whose value is not set, as <c>DbContext.Add</c> would.
    /// </summary>
    /// <returns>The columns for which values were generated.</returns>
    private async Task<HashSet<ColumnModel>> GenerateClientValuesAsync(EntityRowSource source, List<ColumnModel> columns, bool async, CancellationToken cancellationToken)
    {
        var generatedColumns = new HashSet<ColumnModel>();

        foreach (var column in columns)
        {
            if (!source.BindingsOf(column).Any(b => GetClientGenerator(b) is not null))
                continue;

            for (int row = 0; row < source.Count; row++)
            {
                var binding = source.GetBindings(row)[column.Index];

                if (binding is null || binding.Hops.Length > 0 || binding.IsShadow || binding.Setter is null)
                    continue;

                var generator = GetClientGenerator(binding);

                if (generator is null || !source.Read(row, binding).IsSentinel)
                    continue;

                var entity = source.GetEntity(row);
                var entry = _context.Entry(entity);
                var value = async ? await generator.NextAsync(entry, cancellationToken) : generator.Next(entry);

                // The entity gets the value only when the operation succeeds; until then the row source supplies it.
                source.SetGeneratedValue(row, binding, value);
                Pending.Add(() => binding.Setter(entity, value));
                generatedColumns.Add(column);
            }
        }

        return generatedColumns;
    }

    #endregion

    #region Column helpers

    private static void EnsureWritable(TableModel table)
    {
        if (table.Columns.FirstOrDefault(c => !c.IsSupported) is { } column)
            throw new NotSupportedException($"Table '{table.Info}' has column '{column.Name}' that is not mapped to a scalar property, such as the JSON column of an owned or complex type. "
                                            + "Bulk insert and update do not support such columns.");
    }

    private IReadOnlyList<ColumnModel> GetMatchColumns(TableModel table)
    {
        IReadOnlyList<ColumnModel> match = UseConfigFilters && _config.UpdateByProperties is { Count: > 0 } names
            ? [.. table.ResolveColumns(names, nameof(BulkConfig.UpdateByProperties)).OrderBy(c => c.Index)]
            : table.PrimaryKey;

        if (match.Count == 0)
            throw new InvalidOperationException($"Table '{table.Info}' has no primary key. Set {nameof(BulkConfig.UpdateByProperties)} to choose the properties that identify rows.");

        return match;
    }

    /// <summary>
    /// Columns an update writes, chosen from <paramref name="candidates"/>. Keys, match columns, the discriminator and columns
    /// EF Core does not allow to change after insert are never updated.
    /// </summary>
    private List<ColumnModel> GetUpdateColumns(RowSource source, IEnumerable<ColumnModel> candidates, IReadOnlyList<ColumnModel> match, bool applyOnUpdateFilters)
    {
        HashSet<ColumnModel> include = null, exclude = null, includeOnUpdate = null, excludeOnUpdate = null;

        if (UseConfigFilters)
        {
            if (_config.PropertiesToInclude is { Count: > 0 } includeNames)
                include = source.Table.ResolveColumns(includeNames, nameof(BulkConfig.PropertiesToInclude));

            if (_config.PropertiesToExclude is { Count: > 0 } excludeNames)
                exclude = source.Table.ResolveColumns(excludeNames, nameof(BulkConfig.PropertiesToExclude));

            if (applyOnUpdateFilters && _config.PropertiesToIncludeOnUpdate is { } includeOnUpdateNames)
                includeOnUpdate = source.Table.ResolveColumns(includeOnUpdateNames, nameof(BulkConfig.PropertiesToIncludeOnUpdate));

            if (applyOnUpdateFilters && _config.PropertiesToExcludeOnUpdate is { Count: > 0 } excludeOnUpdateNames)
                excludeOnUpdate = source.Table.ResolveColumns(excludeOnUpdateNames, nameof(BulkConfig.PropertiesToExcludeOnUpdate));
        }

        var columns = new List<ColumnModel>();

        foreach (var column in candidates)
        {
            if (column.IsPrimaryKey || !column.IsSupported || match.Contains(column))
                continue;

            var bindings = source.BindingsOf(column).ToList();

            if (bindings.Count == 0 || bindings.Any(b => b.IsDiscriminator) || !bindings.Any(b => b.AfterSave == PropertySaveBehavior.Save))
                continue;

            if ((include is not null && !include.Contains(column))
                || (exclude is not null && exclude.Contains(column))
                || (includeOnUpdate is not null && !includeOnUpdate.Contains(column))
                || (excludeOnUpdate is not null && excludeOnUpdate.Contains(column)))
                continue;

            // The value of a shadow property is not known for every entity; updating it would overwrite data with defaults.
            if (bindings.Any(b => b.IsShadow) && HasUnknownValue(source, column))
                continue;

            columns.Add(column);
        }

        return columns;
    }

    private static List<ColumnModel> GetConcurrencyTokenColumns(RowSource source, IReadOnlyList<ColumnModel> match)
        => [.. source.Table.Columns.Where(c => c.IsSupported && !match.Contains(c) && source.BindingsOf(c).Any(b => b.Property.IsConcurrencyToken))];

    /// <summary>
    /// Columns read back after an update: values the database generates on update, such as computed columns and <c>xmin</c>.
    /// </summary>
    private static List<ColumnModel> GetUpdateReadColumns(RowSource source, IReadOnlyList<ColumnModel> match, List<ColumnModel> set)
        => [.. source.Table.Columns.Where(c => c.IsSupported && !match.Contains(c) && !set.Contains(c) && source.BindingsOf(c).Any(b => b.IsGeneratedOnUpdate))];

    private static bool HasUnknownValue(RowSource source, ColumnModel column)
    {
        for (int row = 0; row < source.Count; row++)
            if (source.GetBindings(row)[column.Index] is { } binding && source.Read(row, binding).Kind == ValueKind.Unknown)
                return true;

        return false;
    }

    private ValueGenerator GetClientGenerator(ColumnBinding binding)
    {
        if (!binding.RequiresValueGenerator)
            return null;

        if (!_clientGenerators.TryGetValue(binding.Property, out var generator))
        {
            _generatorSelector ??= _context.GetService<IValueGeneratorSelector>();

            if (!_generatorSelector.TrySelect(binding.Property, binding.Property.DeclaringType, out generator) || generator.GeneratesTemporaryValues)
                generator = null;

            _clientGenerators[binding.Property] = generator;
        }

        return generator;
    }

    /// <summary>
    /// Whether the database generates the value when the entity leaves it at its default, so the column is omitted for such rows.
    /// </summary>
    private bool IsStoreGeneratedOnAdd(ColumnBinding binding)
        => binding.IsGeneratedOnAdd && binding.BeforeSave != PropertySaveBehavior.Ignore && GetClientGenerator(binding) is null;

    /// <summary>
    /// Whether the database may generate the value, so it is read back.
    /// </summary>
    private bool IsStoreGenerated(ColumnBinding binding)
        => binding.Property.ValueGenerated != ValueGenerated.Never && GetClientGenerator(binding) is null;

    private static BulkColumnInfo[] Infos(IEnumerable<ColumnModel> columns) => [.. columns.Select(c => c.Info)];

    #endregion

    #region Keys and returned values

    private static Dictionary<object[], int> BuildKeyIndex(RowSource source, int[] rows, IReadOnlyList<ColumnModel> match, string verb)
    {
        var index = new Dictionary<object[], int>(rows.Length, KeyComparer.Instance);

        foreach (var row in rows)
        {
            var key = source.GetKey(row, match);

            // A NULL match value matches no row, so it cannot collide.
            if (key.Any(v => v is null))
                continue;

            if (!index.TryAdd(key, row))
                throw new InvalidOperationException($"The input contains more than one entity with the same values for {string.Join(", ", match.Select(c => c.Name))} in table '{source.Table.Info}'. "
                                                    + $"A row can be {verb} only once in a bulk operation.");
        }

        return index;
    }

    /// <summary>
    /// Records a value the database returned for <paramref name="row"/>. It is written to the entity after the operation succeeds.
    /// </summary>
    private void Record(RowSource source, int row, ColumnBinding binding, object modelValue)
    {
        if (TemporaryValues is not null && source.Read(row, binding) is { Kind: ValueKind.Temporary } before)
            TemporaryValues.Add(binding.Property, before.Value, modelValue);

        Pending.Add(source, row, binding, modelValue);
    }

    private async Task<List<T>> QueryAsync<T>(string sql, bool async, CancellationToken cancellationToken) where T : class
    {
        var query = _context.Set<T>().FromSqlRaw(sql);

        return async ? await query.ToListAsync(cancellationToken) : [.. query];
    }

    /// <param name="Rows">Row indexes in the row source.</param>
    /// <param name="Columns">Columns written for these rows.</param>
    /// <param name="InsertOnly">For upsert: rows that cannot match an existing row.</param>
    private sealed record RowPartition(int[] Rows, List<ColumnModel> Columns, bool InsertOnly = false);

    /// <summary>
    /// Returned rows of an insert or update, matched to input rows by position.
    /// </summary>
    private sealed class PositionReturning(BulkExecutor executor, RowSource source, int[] rows, List<ColumnModel> columns) : BulkReturning
    {
        private readonly bool[] _received = new bool[rows.Length];

        public override IReadOnlyList<BulkColumnInfo> Columns { get; } = Infos(columns);

        public override int KeyColumnCount => 0;

        /// <summary>
        /// Number of distinct input rows that got a returned row.
        /// </summary>
        public int Received { get; private set; }

        public bool WasReceived(int rowIndex) => _received[rowIndex];

        public override void ConsumeRow(DbDataReader reader, int rowIndex)
        {
            if ((uint)rowIndex >= (uint)rows.Length)
                throw new ArgumentOutOfRangeException(nameof(rowIndex), rowIndex, "The returned row index is outside the input rows.");

            var row = rows[rowIndex];
            var bindings = source.GetBindings(row);

            for (int i = 0; i < columns.Count; i++)
            {
                if (bindings[columns[i].Index] is not { } binding)
                    continue;

                executor.Record(source, row, binding, binding.FromProvider(binding.ReadProviderValue(reader, i)));
            }

            if (!_received[rowIndex])
            {
                _received[rowIndex] = true;
                Received++;
            }
        }

        public override void ConsumeKeyedRow(DbDataReader reader) => throw new NotSupportedException("Inserted and updated rows are matched by position.");
    }

    private sealed class KeyedReturning(BulkExecutor executor, RowSource source, IReadOnlyList<ColumnModel> match, List<ColumnModel> values, Dictionary<object[], int> keys) : BulkReturning
    {
        private readonly BulkExecutor _executor = executor;
        private readonly RowSource _source = source;
        private readonly IReadOnlyList<ColumnModel> _match = match;
        private readonly List<ColumnModel> _values = values;
        private readonly Dictionary<object[], int> _keys = keys;
        private readonly ColumnBinding[] _keyBindings = [.. match.Select(c => source.BindingsOf(c).First())];

        public override IReadOnlyList<BulkColumnInfo> Columns { get; } = Infos(match.Concat(values));

        public override int KeyColumnCount => _match.Count;

        public override void ConsumeRow(DbDataReader reader, int rowIndex) => throw new NotSupportedException("Upserted rows are matched by key.");

        public override void ConsumeKeyedRow(DbDataReader reader)
        {
            var key = new object[_match.Count];

            for (int i = 0; i < key.Length; i++)
                key[i] = _keyBindings[i].ReadProviderValue(reader, i);

            if (!_keys.TryGetValue(key, out var row))
                throw new InvalidOperationException($"A row returned by the database for table '{_source.Table.Info}' could not be matched to an input entity by {string.Join(", ", _match.Select(c => c.Name))}. "
                                                    + "This happens when the database compares or stores these values differently from .NET, for example with case insensitive collations, "
                                                    + "citext or blank padded character(n) columns. Turn off SetOutputIdentity or match by other properties.");

            var bindings = _source.GetBindings(row);

            for (int i = 0; i < _values.Count; i++)
            {
                if (bindings[_values[i].Index] is not { } binding)
                    continue;

                _executor.Record(_source, row, binding, binding.FromProvider(binding.ReadProviderValue(reader, _match.Count + i)));
            }
        }
    }

    #endregion
}

/// <summary>
/// A <see cref="BulkRowSet"/> over selected rows and columns of a <see cref="RowSource"/>. Values are read when the provider asks for them.
/// Original columns are added after the current columns with their original values, as separate column entries.
/// </summary>
internal sealed class RowSetView : BulkRowSet
{
    private readonly RowSource _source;
    private readonly int[] _rows;
    private readonly IReadOnlyList<ColumnModel> _columns;
    private readonly IReadOnlyList<ColumnModel> _originalColumns;

    public RowSetView(RowSource source, int[] rows, IReadOnlyList<ColumnModel> columns, IReadOnlyList<ColumnModel> originalColumns = null)
    {
        _source = source;
        _rows = rows;
        _columns = columns;
        _originalColumns = originalColumns ?? [];
        OriginalColumns = [.. _originalColumns.Select(c => new BulkColumnInfo(c.Info.Name, c.Info.StoreType, c.Info.IsNullable))];
        Columns = [.. columns.Select(c => c.Info), .. OriginalColumns];
    }

    public override IReadOnlyList<BulkColumnInfo> Columns { get; }

    /// <summary>
    /// Entries of <see cref="Columns"/> that hold original values.
    /// </summary>
    public BulkColumnInfo[] OriginalColumns { get; }

    public override int Count => _rows.Length;

    public override object GetValue(int row, int column)
        => column < _columns.Count
            ? _source.GetProviderValue(_rows[row], _columns[column])
            : _source.GetProviderValue(_rows[row], _originalColumns[column - _columns.Count], original: true);
}

/// <summary>
/// A <see cref="BulkRowSet"/> of the key (match) values of every row of a <see cref="RowSource"/>, for delete and read.
/// Values are read when the provider asks for them; a value that is not known throws. Duplicate keys are kept, they do no harm in a match.
/// </summary>
internal sealed class KeyRowSet(RowSource source, IReadOnlyList<ColumnModel> match) : BulkRowSet
{
    public override IReadOnlyList<BulkColumnInfo> Columns { get; } = [.. match.Select(c => c.Info)];

    public override int Count => source.Count;

    public override object GetValue(int row, int column) => source.GetKeyValue(row, match[column]);
}

/// <summary>
/// A <see cref="DbUpdateConcurrencyException"/> raised by <c>BulkSaveChanges</c>. <see cref="Entries"/> lists the entries whose rows were not found
/// (for deletes, the entries of the failed statement).
/// </summary>
internal sealed class BulkConcurrencyException(string message, IReadOnlyList<EntityEntry> entries) : DbUpdateConcurrencyException(message)
{
    public override IReadOnlyList<EntityEntry> Entries { get; } = entries;
}