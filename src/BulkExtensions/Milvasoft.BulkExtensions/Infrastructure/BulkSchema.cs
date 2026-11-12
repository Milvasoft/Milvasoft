namespace Milvasoft.BulkExtensions.Infrastructure;

/// <summary>
/// A database table targeted by a bulk operation.
/// </summary>
public sealed class BulkTableInfo
{
    internal BulkTableInfo(string name, string schema)
    {
        Name = name;
        Schema = schema;
    }

    /// <summary>
    /// Table name, not quoted.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Schema name, not quoted. <see langword="null"/> when the model does not set a schema.
    /// </summary>
    public string Schema { get; }

    /// <inheritdoc/>
    public override string ToString() => Schema is null ? Name : $"{Schema}.{Name}";
}

/// <summary>
/// A column that takes part in a bulk operation.
/// </summary>
public sealed class BulkColumnInfo
{
    internal BulkColumnInfo(string name, string storeType, bool isNullable)
    {
        Name = name;
        StoreType = storeType;
        IsNullable = isNullable;
    }

    /// <summary>
    /// Column name, not quoted.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Database type of the column as configured in the model, for example <c>character varying(100)</c>.
    /// </summary>
    public string StoreType { get; }

    /// <summary>
    /// Whether the column accepts null.
    /// </summary>
    public bool IsNullable { get; }

    /// <inheritdoc/>
    public override string ToString() => $"{Name} ({StoreType})";
}