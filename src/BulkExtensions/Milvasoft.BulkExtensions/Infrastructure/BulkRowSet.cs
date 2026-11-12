using System.Data.Common;

namespace Milvasoft.BulkExtensions.Infrastructure;

/// <summary>
/// The rows a provider writes to the database. Values are already converted to the provider types of the columns
/// (value converters applied). A <see langword="null"/> value means database NULL.
/// </summary>
public abstract class BulkRowSet
{
    /// <summary>
    /// Columns of each row, in the order the provider must write them.
    /// </summary>
    public abstract IReadOnlyList<BulkColumnInfo> Columns { get; }

    /// <summary>
    /// Number of rows.
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// Gets the value of <paramref name="column"/> for <paramref name="row"/>.
    /// </summary>
    /// <param name="row">Row index, from 0 to <see cref="Count"/> - 1.</param>
    /// <param name="column">Index into <see cref="Columns"/>.</param>
    public abstract object GetValue(int row, int column);
}

/// <summary>
/// Describes the values a provider must return from the database after an insert, update or upsert,
/// and receives them.
/// </summary>
/// <remarks>
/// Insert and update match returned rows to input rows by position: the provider passes the index of the input row with
/// <see cref="ConsumeRow"/>. Upsert cannot know the input row of a returned row, so it returns the match columns first
/// (<see cref="KeyColumnCount"/>) and passes rows with <see cref="ConsumeKeyedRow"/>.
/// </remarks>
public abstract class BulkReturning
{
    /// <summary>
    /// Columns to return, in this order, starting at ordinal 0.
    /// </summary>
    public abstract IReadOnlyList<BulkColumnInfo> Columns { get; }

    /// <summary>
    /// For upsert: the first <see cref="KeyColumnCount"/> entries of <see cref="Columns"/> are the match columns, read from the
    /// inserted or updated row. Zero for insert and update.
    /// </summary>
    public abstract int KeyColumnCount { get; }

    /// <summary>
    /// Passes one returned row of an insert or update.
    /// </summary>
    /// <param name="reader">Reader positioned on the returned row.</param>
    /// <param name="rowIndex">Index of the input row in the <see cref="BulkRowSet"/> that produced this row.</param>
    public abstract void ConsumeRow(DbDataReader reader, int rowIndex);

    /// <summary>
    /// Passes one returned row of an upsert.
    /// </summary>
    /// <param name="reader">Reader positioned on the returned row.</param>
    public abstract void ConsumeKeyedRow(DbDataReader reader);
}