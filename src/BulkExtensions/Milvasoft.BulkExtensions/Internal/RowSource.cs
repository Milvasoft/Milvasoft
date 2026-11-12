using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Milvasoft.BulkExtensions.Internal;

/// <summary>
/// How a column value was obtained.
/// </summary>
internal enum ValueKind : byte
{
    /// <summary>
    /// The value is known.
    /// </summary>
    Known,

    /// <summary>
    /// EF Core holds a temporary placeholder for a value the database will generate (or that comes from a principal not saved yet).
    /// </summary>
    Temporary,

    /// <summary>
    /// The value of a shadow property of an entity that is not tracked, and cannot be found through a navigation.
    /// </summary>
    Unknown,
}

/// <summary>
/// A model (not yet converted) column value read from a row.
/// </summary>
/// <param name="Value">Model value.</param>
/// <param name="IsSentinel">Whether the value means "not set", so a database default or generated value may be used instead.</param>
/// <param name="Kind">How the value was obtained.</param>
internal readonly record struct ValueRead(object Value, bool IsSentinel, ValueKind Kind);

/// <summary>
/// The rows of one table taking part in an operation.
/// </summary>
internal abstract class RowSource
{
    private readonly ColumnBinding[][] _rowBindings;
    private readonly IEntityType[] _rowTypes;

    protected RowSource(TableModel table, IEntityType[] rowTypes)
    {
        Table = table;
        _rowTypes = rowTypes;
        _rowBindings = new ColumnBinding[rowTypes.Length][];

        var byType = new Dictionary<IEntityType, ColumnBinding[]>();

        for (int i = 0; i < rowTypes.Length; i++)
        {
            if (!byType.TryGetValue(rowTypes[i], out var bindings))
                byType[rowTypes[i]] = bindings = table.GetBindings(rowTypes[i]);

            _rowBindings[i] = bindings;
        }

        TypeBindings = [.. byType.Values];
    }

    public TableModel Table { get; }

    public int Count => _rowTypes.Length;

    /// <summary>
    /// Bindings of each distinct entity type in the rows.
    /// </summary>
    public IReadOnlyList<ColumnBinding[]> TypeBindings { get; }

    public IEntityType GetEntityType(int row) => _rowTypes[row];

    public ColumnBinding[] GetBindings(int row) => _rowBindings[row];

    /// <summary>
    /// Bindings of <paramref name="column"/> across the entity types in the rows, without nulls.
    /// </summary>
    public IEnumerable<ColumnBinding> BindingsOf(ColumnModel column)
    {
        foreach (var bindings in TypeBindings)
            if (bindings[column.Index] is { } binding)
                yield return binding;
    }

    public abstract ValueRead Read(int row, ColumnBinding binding);

    /// <summary>
    /// Reads the value the row had when it was loaded from the database. Only change tracker rows have one; other rows return the current value.
    /// </summary>
    public virtual ValueRead ReadOriginal(int row, ColumnBinding binding) => Read(row, binding);

    /// <summary>
    /// Writes a value read back from the database to the row.
    /// </summary>
    public abstract void Write(int row, ColumnBinding binding, object modelValue);

    /// <summary>
    /// Gets the provider value of <paramref name="column"/> for <paramref name="row"/>.
    /// </summary>
    /// <param name="row">Row index.</param>
    /// <param name="column">Column.</param>
    /// <param name="original">Whether to read the original value, used for concurrency checks.</param>
    public object GetProviderValue(int row, ColumnModel column, bool original = false)
    {
        var binding = _rowBindings[row][column.Index];

        if (binding is null)
            return null;

        var read = original ? ReadOriginal(row, binding) : Read(row, binding);

        if (read.Kind == ValueKind.Temporary)
            throw new InvalidOperationException($"'{binding.Path}' of an entity of type '{binding.Property.DeclaringType.DisplayName()}' has a temporary value. "
                                                + "The entity it refers to has not been saved.");

        return binding.ToProvider(read.Kind == ValueKind.Unknown ? binding.Property.Sentinel : read.Value);
    }

    /// <summary>
    /// Gets the provider values of key columns for <paramref name="row"/>. Throws when a value is not known.
    /// </summary>
    public object[] GetKey(int row, IReadOnlyList<ColumnModel> columns)
    {
        var key = new object[columns.Count];

        for (int i = 0; i < key.Length; i++)
            key[i] = GetKeyValue(row, columns[i]);

        return key;
    }

    /// <summary>
    /// Gets the provider value of one key column for <paramref name="row"/>. Throws when the value is not known.
    /// </summary>
    public object GetKeyValue(int row, ColumnModel column)
    {
        var binding = _rowBindings[row][column.Index]
            ?? throw new InvalidOperationException($"Column '{column.Name}' is used to match rows but entity type '{_rowTypes[row].DisplayName()}' has no property mapped to it.");

        var read = Read(row, binding);

        if (read.Kind != ValueKind.Known)
            throw new InvalidOperationException($"The value of '{binding.Path}' of an entity of type '{_rowTypes[row].DisplayName()}' is not known, so the entity cannot be matched to a row. "
                                                + (read.Kind == ValueKind.Unknown
                                                    ? "It is a shadow property; attach the entity to the context or set its navigation."
                                                    : "The entity it refers to has not been saved."));

        return binding.ToProvider(read.Value);
    }

    /// <summary>
    /// Reads a column through EF Core change tracking entries, starting from <paramref name="entry"/>.
    /// </summary>
    protected static ValueRead ReadFromEntry(EntityEntry entry, ColumnBinding binding, TemporaryValueMap temporaryValues, bool original = false)
    {
        var current = entry;
        object instance = null;

        for (int i = 0; i < binding.Hops.Length; i++)
        {
            if (instance is null && binding.Hops[i] is INavigation navigation)
            {
                current = current.Reference(navigation).TargetEntry;

                if (current is null)
                    return new ValueRead(null, false, ValueKind.Known);
            }
            else
            {
                instance = binding.HopGetters[i].GetClrValue(instance ?? current.Entity);

                if (instance is null)
                    return new ValueRead(null, false, ValueKind.Known);
            }
        }

        if (instance is not null)
        {
            var clrValue = binding.Getter.GetClrValue(instance);

            return new ValueRead(clrValue, binding.IsSentinel(clrValue), ValueKind.Known);
        }

        var propertyEntry = current.Property(binding.Property);
        var value = original && current.State != EntityState.Added ? propertyEntry.OriginalValue : propertyEntry.CurrentValue;

        if (propertyEntry.IsTemporary)
        {
            if (temporaryValues is not null && temporaryValues.TryResolve(binding.Property, value, out var resolved))
                return new ValueRead(resolved, binding.IsSentinel(resolved), ValueKind.Known);

            return new ValueRead(value, true, ValueKind.Temporary);
        }

        return new ValueRead(value, binding.IsSentinel(value), ValueKind.Known);
    }

    /// <summary>
    /// Whether the change tracker marks the column of the entry as changed. Columns of owned entities that were added, deleted
    /// or replaced count as changed.
    /// </summary>
    protected static bool IsModifiedInEntry(EntityEntry entry, ColumnBinding binding)
    {
        var current = entry;
        ComplexPropertyEntry complex = null;

        foreach (var hop in binding.Hops)
        {
            switch (hop)
            {
                case INavigation navigation when complex is null:
                    current = current.Reference(navigation).TargetEntry;

                    if (current is null || current.State is EntityState.Added or EntityState.Deleted)
                        return true;

                    break;

                case IComplexProperty complexProperty:
                    complex = complex is null ? current.ComplexProperty(complexProperty) : complex.ComplexProperty(complexProperty);
                    break;

                default:
                    return true;
            }
        }

        return complex is null ? current.Property(binding.Property).IsModified : complex.Property(binding.Property).IsModified;
    }

    /// <summary>
    /// Writes a column through EF Core change tracking entries, starting from <paramref name="entry"/>. Properties of complex types are skipped.
    /// </summary>
    protected static void WriteToEntry(EntityEntry entry, ColumnBinding binding, object modelValue)
    {
        var current = entry;

        foreach (var hop in binding.Hops)
        {
            if (hop is not INavigation navigation)
                return;

            current = current.Reference(navigation).TargetEntry;

            if (current is null)
                return;
        }

        current.Property(binding.Property).CurrentValue = modelValue;
    }
}

/// <summary>
/// Rows given as entity instances, as in <c>BulkInsert(entities)</c>.
/// </summary>
internal sealed class EntityRowSource(DbContext context, TableModel table, IReadOnlyList<object> entities, IEntityType[] rowTypes) : RowSource(table, rowTypes)
{
    private readonly DbContext _context = context;
    private readonly IReadOnlyList<object> _entities = entities;
    private EntityEntry[] _entries;
    private Dictionary<(int, ColumnBinding), object> _generatedValues;

    public object GetEntity(int row) => _entities[row];

    /// <summary>
    /// Uses <paramref name="value"/>, made by a client side value generator, instead of the entity's value. The entity itself is
    /// changed only after the operation succeeds. The value is kept, so a retried operation uses the same value.
    /// </summary>
    public void SetGeneratedValue(int row, ColumnBinding binding, object value) => (_generatedValues ??= [])[(row, binding)] = value;

    public override ValueRead Read(int row, ColumnBinding binding)
    {
        if (binding.IsDiscriminator)
            return new ValueRead(binding.DiscriminatorValue, false, ValueKind.Known);

        if (_generatedValues is not null && _generatedValues.TryGetValue((row, binding), out var generated))
            return new ValueRead(generated, false, ValueKind.Known);

        var entity = _entities[row];

        if (binding.IsShadow)
            return ReadShadow(row, entity, binding);

        var instance = entity;

        for (int i = 0; i < binding.HopGetters.Length; i++)
        {
            instance = binding.HopGetters[i].GetClrValue(instance);

            if (instance is null)
                return new ValueRead(null, false, ValueKind.Known);
        }

        // One getter call; the sentinel check reuses the value.
        var value = binding.Getter.GetClrValue(instance);

        return new ValueRead(value, binding.IsSentinel(value), ValueKind.Known);
    }

    public override void Write(int row, ColumnBinding binding, object modelValue)
    {
        if (binding.IsShadow || binding.HasComplexHop || binding.Setter is null)
            return;

        var instance = _entities[row];

        for (int i = 0; i < binding.HopGetters.Length; i++)
        {
            instance = binding.HopGetters[i].GetClrValue(instance);

            if (instance is null)
                return;
        }

        binding.Setter(instance, modelValue);
    }

    /// <summary>
    /// Shadow properties have no CLR member. Their value is taken from the change tracker when the entity is tracked,
    /// or from the principal entity when the property is a foreign key and its navigation is set.
    /// </summary>
    private ValueRead ReadShadow(int row, object entity, ColumnBinding binding)
    {
        _entries ??= new EntityEntry[Count];

        var entry = _entries[row] ??= _context.Entry(entity);

        // A temporary value stays temporary: a database generated column is then left to the database, any other column
        // (a foreign key to an entity that is not saved yet) fails instead of being written as a default.
        if (entry.State != EntityState.Detached)
            return ReadFromEntry(entry, binding, null);

        if (binding.Hops.Length == 0 && TryReadFromPrincipal(entity, GetEntityType(row), binding.Property, out var value))
            return new ValueRead(value, binding.IsSentinel(value), ValueKind.Known);

        return new ValueRead(binding.Property.Sentinel, true, ValueKind.Unknown);
    }

    private static bool TryReadFromPrincipal(object entity, IEntityType entityType, IProperty property, out object value)
    {
        foreach (var foreignKey in property.GetContainingForeignKeys())
        {
            var navigation = foreignKey.DependentToPrincipal;

            if (navigation is null || navigation.IsShadowProperty() || !foreignKey.DeclaringEntityType.IsAssignableFrom(entityType))
                continue;

            var principal = navigation.GetGetter().GetClrValue(entity);

            if (principal is null)
                continue;

            var index = IndexOf(foreignKey.Properties, property);
            var principalProperty = foreignKey.PrincipalKey.Properties[index];

            if (principalProperty.IsShadowProperty())
                continue;

            var getter = principalProperty.GetGetter();

            // A principal whose key is not set yet has not been saved; its key cannot be used.
            if (getter.HasSentinelValue(principal))
                continue;

            value = getter.GetClrValue(principal);

            return true;
        }

        value = null;

        return false;
    }

    private static int IndexOf(IReadOnlyList<IProperty> properties, IProperty property)
    {
        for (int i = 0; i < properties.Count; i++)
            if (properties[i] == property)
                return i;

        return -1;
    }
}

/// <summary>
/// Rows given as change tracker entries, as in <c>BulkSaveChanges()</c>.
/// </summary>
internal sealed class EntryRowSource(TableModel table, IReadOnlyList<EntityEntry> entries, TemporaryValueMap temporaryValues)
    : RowSource(table, [.. entries.Select(e => e.Metadata)])
{
    public EntityEntry GetEntry(int row) => entries[row];

    public override ValueRead Read(int row, ColumnBinding binding)
    {
        if (binding.IsDiscriminator)
            return new ValueRead(binding.DiscriminatorValue, false, ValueKind.Known);

        return ReadFromEntry(entries[row], binding, temporaryValues);
    }

    public override ValueRead ReadOriginal(int row, ColumnBinding binding)
    {
        if (binding.IsDiscriminator)
            return new ValueRead(binding.DiscriminatorValue, false, ValueKind.Known);

        return ReadFromEntry(entries[row], binding, temporaryValues, original: true);
    }

    public bool IsModified(int row, ColumnBinding binding) => IsModifiedInEntry(entries[row], binding);

    public override void Write(int row, ColumnBinding binding, object modelValue) => WriteToEntry(entries[row], binding, modelValue);
}

/// <summary>
/// Real values generated by the database for temporary key values, collected while a <c>BulkSaveChanges</c> runs.
/// Foreign keys that hold a temporary value of their principal are resolved through it. The change tracker itself is
/// only updated after the transaction commits.
/// </summary>
internal sealed class TemporaryValueMap
{
    private readonly Dictionary<(IProperty, object), object> _values = [];

    public void Add(IProperty property, object temporaryValue, object value) => _values[(property, temporaryValue)] = value;

    public bool TryResolve(IProperty property, object temporaryValue, out object value)
    {
        if (_values.TryGetValue((property, temporaryValue), out value))
            return true;

        foreach (var foreignKey in property.GetContainingForeignKeys())
        {
            for (int i = 0; i < foreignKey.Properties.Count; i++)
            {
                if (foreignKey.Properties[i] == property && _values.TryGetValue((foreignKey.PrincipalKey.Properties[i], temporaryValue), out value))
                    return true;
            }
        }

        value = null;

        return false;
    }
}