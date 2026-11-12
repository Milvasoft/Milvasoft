using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Milvasoft.BulkExtensions.Infrastructure;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Milvasoft.BulkExtensions.Internal;

/// <summary>
/// Bulk view of one database table: its columns and, for every entity type stored in it, how each column is read from an entity.
/// Built once per table of a model and cached.
/// </summary>
internal sealed class TableModel
{
    private static readonly ConditionalWeakTable<ITable, TableModel> _cache = [];

    private readonly ConcurrentDictionary<IEntityType, ColumnBinding[]> _bindings = new();
    private readonly Lazy<Dictionary<string, HashSet<ColumnModel>>> _paths;

    private TableModel(ITable table)
    {
        Table = table;
        Info = new BulkTableInfo(table.Name, table.Schema);

        var columns = new List<ColumnModel>();
        var byName = new Dictionary<string, ColumnModel>(StringComparer.Ordinal);

        foreach (var column in table.Columns)
        {
            var model = new ColumnModel(columns.Count, column);
            columns.Add(model);
            byName[column.Name] = model;
        }

        Columns = columns;
        PrimaryKey = table.PrimaryKey is null ? [] : [.. table.PrimaryKey.Columns.Select(c => byName[c.Name])];

        foreach (var key in PrimaryKey)
            key.IsPrimaryKey = true;

        _paths = new Lazy<Dictionary<string, HashSet<ColumnModel>>>(BuildPaths);
    }

    public ITable Table { get; }

    public BulkTableInfo Info { get; }

    public IReadOnlyList<ColumnModel> Columns { get; }

    /// <summary>
    /// Primary key columns in key order. Empty for keyless tables.
    /// </summary>
    public IReadOnlyList<ColumnModel> PrimaryKey { get; }

    /// <summary>
    /// Gets the table model of the table <paramref name="entityType"/> is stored in.
    /// </summary>
    /// <param name="entityType">An entity type whose rows are stored in exactly one table.</param>
    public static TableModel For(IEntityType entityType)
    {
        var table = GetSingleTable(entityType);

        return _cache.GetValue(table, static t => new TableModel(t));
    }

    /// <summary>
    /// Whether <paramref name="entityType"/> is an owned type stored in the table of its owner (table splitting).
    /// Rows of such types are part of the owner's rows.
    /// </summary>
    public static bool IsTableSharingDependent(IEntityType entityType)
    {
        if (!entityType.IsOwned() || entityType.FindOwnership() is not { } ownership)
            return false;

        var ownTables = entityType.GetTableMappings().Select(m => m.Table).ToHashSet();

        return ownership.PrincipalEntityType.GetTableMappings().Any(m => ownTables.Contains(m.Table));
    }

    /// <summary>
    /// Gets how each column is read for entities of <paramref name="entityType"/>. The array is aligned with <see cref="Columns"/>;
    /// an element is <see langword="null"/> when the column does not belong to the entity type (for example a column of a sibling type in TPH).
    /// </summary>
    public ColumnBinding[] GetBindings(IEntityType entityType) => _bindings.GetOrAdd(entityType, CreateBindings);

    /// <summary>
    /// Resolves property names or paths given in <see cref="BulkConfig"/> to columns.
    /// </summary>
    /// <param name="names">Property names or dotted paths.</param>
    /// <param name="settingName">Name of the setting, used in error messages.</param>
    public HashSet<ColumnModel> ResolveColumns(IEnumerable<string> names, string settingName)
    {
        var result = new HashSet<ColumnModel>();
        var paths = _paths.Value;

        foreach (var name in names)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"{settingName} contains an empty property name.");

            if (!paths.TryGetValue(name.Trim(), out var columns))
                throw new ArgumentException($"{settingName} contains '{name}', which is not a mapped property of a type stored in table '{Info}'.");

            result.UnionWith(columns);
        }

        return result;
    }

    private static ITable GetSingleTable(IEntityType entityType)
    {
        ITable table = null;
        var count = 0;

        foreach (var mapping in entityType.GetTableMappings())
        {
            table = mapping.Table;
            count++;
        }

        return count switch
        {
            1 => table,
            0 => throw new NotSupportedException($"Entity type '{entityType.DisplayName()}' is not mapped to a table. Bulk operations do not support views, functions, SQL queries or keyless types without a table."),
            _ => throw new NotSupportedException($"Entity type '{entityType.DisplayName()}' is mapped to more than one table (TPT inheritance or entity splitting). Bulk operations do not support this mapping."),
        };
    }

    private ColumnBinding[] CreateBindings(IEntityType entityType)
    {
        if (GetSingleTable(entityType) != Table)
            throw new InvalidOperationException($"Entity type '{entityType.DisplayName()}' is not stored in table '{Info}'.");

        var discriminator = entityType.FindDiscriminatorProperty();
        var bindings = new ColumnBinding[Columns.Count];

        foreach (var column in Columns)
        {
            ColumnBinding best = null;

            foreach (var mapping in column.Column.PropertyMappings)
            {
                var hops = new List<IPropertyBase>();

                if (!TryGetPath(mapping.Property.DeclaringType, entityType, hops))
                    continue;

                if (best is null || hops.Count < best.Hops.Length)
                {
                    var isDiscriminator = discriminator is not null && mapping.Property == discriminator;

                    best = new ColumnBinding(column, mapping.Property, [.. hops], mapping.TypeMapping, isDiscriminator ? entityType.GetDiscriminatorValue() : null, isDiscriminator);
                }
            }

            bindings[column.Index] = best;
        }

        return bindings;
    }

    /// <summary>
    /// Finds the navigations and complex properties to follow from an entity of <paramref name="entityType"/> to an object of <paramref name="declaringType"/>.
    /// </summary>
    private static bool TryGetPath(ITypeBase declaringType, IEntityType entityType, List<IPropertyBase> hops)
    {
        switch (declaringType)
        {
            case IEntityType declaringEntity when declaringEntity.IsAssignableFrom(entityType):
                return true;

            case IEntityType declaringEntity when declaringEntity.IsOwned():
                {
                    var ownership = declaringEntity.FindOwnership();
                    var navigation = ownership?.PrincipalToDependent;

                    if (navigation is null || navigation.IsCollection || !TryGetPath(ownership.PrincipalEntityType, entityType, hops))
                        return false;

                    hops.Add(navigation);

                    return true;
                }

            case IComplexType complexType:
                {
                    var complexProperty = complexType.ComplexProperty;

                    if (complexProperty.IsCollection || !TryGetPath(complexProperty.DeclaringType, entityType, hops))
                        return false;

                    hops.Add(complexProperty);

                    return true;
                }

            default:
                return false;
        }
    }

    private Dictionary<string, HashSet<ColumnModel>> BuildPaths()
    {
        var paths = new Dictionary<string, HashSet<ColumnModel>>(StringComparer.Ordinal);

        void Add(string path, ColumnModel column)
        {
            if (!paths.TryGetValue(path, out var set))
                paths[path] = set = [];

            set.Add(column);
        }

        foreach (var entityType in GetRowEntityTypes())
        {
            foreach (var binding in GetBindings(entityType))
            {
                if (binding is null)
                    continue;

                Add(binding.Path, binding.Column);

                // A navigation or complex property name selects all of its columns.
                for (int i = 1; i <= binding.Hops.Length; i++)
                    Add(string.Join('.', binding.Hops.Take(i).Select(h => h.Name)), binding.Column);
            }
        }

        return paths;
    }

    /// <summary>
    /// Entity types whose rows are stored in this table, excluding owned types that share the row of their owner.
    /// </summary>
    private IEnumerable<IEntityType> GetRowEntityTypes()
        => Table.EntityTypeMappings.Select(m => m.TypeBase)
                                   .OfType<IEntityType>()
                                   .Where(t => !IsTableSharingDependent(t) && t.GetTableMappings().Count() == 1)
                                   .Distinct();
}

/// <summary>
/// A column of a <see cref="TableModel"/>.
/// </summary>
internal sealed class ColumnModel(int index, IColumn column)
{
    public int Index { get; } = index;

    public IColumn Column { get; } = column;

    public string Name => Column.Name;

    public BulkColumnInfo Info { get; } = new BulkColumnInfo(column.Name, column.StoreType, column.IsNullable);

    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Columns without scalar property mappings, such as the container column of a JSON mapped owned or complex type.
    /// </summary>
    public bool IsSupported { get; } = column.PropertyMappings.Count > 0;

    public override string ToString() => Name;
}