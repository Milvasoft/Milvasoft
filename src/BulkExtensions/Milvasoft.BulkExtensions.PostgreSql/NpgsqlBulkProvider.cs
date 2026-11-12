using Milvasoft.BulkExtensions.Infrastructure;
using Npgsql;
using NpgsqlTypes;
using System.Data.Common;
using System.Globalization;

namespace Milvasoft.BulkExtensions.PostgreSql;

/// <summary>
/// PostgreSQL implementation of bulk operations.
/// </summary>
/// <remarks>
/// <para>
/// Plain inserts stream rows into the target table with binary <c>COPY</c>. Other operations copy the rows into a temporary staging table
/// (<c>ON COMMIT DROP</c>, dropped right after use) and run one set based statement against it:
/// <c>INSERT ... SELECT ... RETURNING</c>, <c>UPDATE ... FROM</c>, <c>INSERT ... ON CONFLICT</c> or <c>DELETE ... USING</c>.
/// </para>
/// <para>
/// Staging columns are named by position (<c>c0</c>, <c>c1</c>, ...), so the same table column can be staged twice (a current and an
/// original value) and system columns such as <c>xmin</c> can be staged. A row number column matches returned rows to input rows:
/// inserts select staged rows in input order, and PostgreSQL returns inserted rows in the order they were inserted.
/// </para>
/// </remarks>
public sealed class NpgsqlBulkProvider : BulkProvider
{
    private const string _ordinalColumn = "__milva_bulk_ordinal";

    /// <inheritdoc/>
    public override async Task<int> InsertAsync(BulkCommandContext context, BulkTableInfo table, BulkRowSet rows, BulkReturning returning, bool async, CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return 0;

        var target = QuoteTable(table);

        // Every column takes its default value; COPY needs at least one column, so generate the rows instead.
        if (rows.Columns.Count == 0)
        {
            var sql = $"INSERT INTO {target} AS t SELECT FROM generate_series(1, {rows.Count.ToString(CultureInfo.InvariantCulture)})";

            if (returning is null)
                return await context.ExecuteNonQueryAsync(sql, async, cancellationToken);

            var position = 0;

            return await ExecuteReaderAsync(context, $"{sql} RETURNING {TargetColumns(returning.Columns)}", reader => returning.ConsumeRow(reader, position++), async, cancellationToken);
        }

        if (returning is null)
            return checked((int)await CopyAsync(context, target, rows, [.. rows.Columns.Select(c => Quote(c.Name))], withOrdinal: false, async, cancellationToken));

        var staging = await StageAsync(context, table, rows, withOrdinal: true, async, cancellationToken);

        var index = 0;
        var inserted = await ExecuteReaderAsync(context,
                                                $"INSERT INTO {target} AS t ({string.Join(", ", rows.Columns.Select(c => Quote(c.Name)))}) "
                                                + $"SELECT {staging.AllColumns()} FROM {staging.Name} AS s ORDER BY s.{Quote(_ordinalColumn)} "
                                                + $"RETURNING {TargetColumns(returning.Columns)}",
                                                reader => returning.ConsumeRow(reader, index++),
                                                async,
                                                cancellationToken);

        await staging.DropAsync(context, async, cancellationToken);

        return inserted;
    }

    /// <inheritdoc/>
    public override async Task<int> UpdateAsync(BulkCommandContext context,
                                                BulkTableInfo table,
                                                BulkRowSet rows,
                                                IReadOnlyList<BulkColumnInfo> matchColumns,
                                                IReadOnlyList<BulkColumnInfo> setColumns,
                                                BulkReturning returning,
                                                bool async,
                                                CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return 0;

        var staging = await StageAsync(context, table, rows, withOrdinal: returning is not null, async, cancellationToken);

        var sql = $"UPDATE {QuoteTable(table)} AS t SET {string.Join(", ", setColumns.Select(c => $"{Quote(c.Name)} = {staging.Column(c)}"))} "
                  + $"FROM {staging.Name} AS s WHERE {staging.MatchCondition(matchColumns)}";

        int updated;

        if (returning is null)
        {
            updated = await context.ExecuteNonQueryAsync(sql, async, cancellationToken);
        }
        else
        {
            var returned = returning.Columns.Select(c => $"t.{Quote(c.Name)}").Append($"s.{Quote(_ordinalColumn)}");
            var ordinal = returning.Columns.Count;

            updated = await ExecuteReaderAsync(context, $"{sql} RETURNING {string.Join(", ", returned)}", reader => returning.ConsumeRow(reader, reader.GetInt32(ordinal)), async, cancellationToken);
        }

        await staging.DropAsync(context, async, cancellationToken);

        return updated;
    }

    /// <inheritdoc/>
    public override async Task<int> UpsertAsync(BulkCommandContext context,
                                                BulkTableInfo table,
                                                BulkRowSet rows,
                                                IReadOnlyList<BulkColumnInfo> matchColumns,
                                                IReadOnlyList<BulkColumnInfo> setColumns,
                                                BulkReturning returning,
                                                bool async,
                                                CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return 0;

        var staging = await StageAsync(context, table, rows, withOrdinal: false, async, cancellationToken);

        var onConflict = setColumns.Count == 0
            ? "DO NOTHING"
            : $"DO UPDATE SET {string.Join(", ", setColumns.Select(c => $"{Quote(c.Name)} = EXCLUDED.{Quote(c.Name)}"))}";

        var sql = $"INSERT INTO {QuoteTable(table)} AS t ({string.Join(", ", rows.Columns.Select(c => Quote(c.Name)))}) SELECT {staging.AllColumns()} FROM {staging.Name} AS s "
                  + $"ON CONFLICT ({string.Join(", ", matchColumns.Select(c => Quote(c.Name)))}) {onConflict}";

        var affected = returning is null
            ? await context.ExecuteNonQueryAsync(sql, async, cancellationToken)
            : await ExecuteReaderAsync(context, $"{sql} RETURNING {TargetColumns(returning.Columns)}", returning.ConsumeKeyedRow, async, cancellationToken);

        // DO NOTHING returns only inserted rows; read the existing rows that matched the input as well.
        if (returning is not null && setColumns.Count == 0)
        {
            await ExecuteReaderAsync(context,
                                     $"SELECT {TargetColumns(returning.Columns)} FROM {QuoteTable(table)} AS t JOIN {staging.Name} AS s ON {staging.MatchCondition(matchColumns)}",
                                     returning.ConsumeKeyedRow,
                                     async,
                                     cancellationToken);
        }

        await staging.DropAsync(context, async, cancellationToken);

        return affected;
    }

    /// <inheritdoc/>
    public override async Task<int> DeleteAsync(BulkCommandContext context,
                                                BulkTableInfo table,
                                                BulkRowSet rows,
                                                IReadOnlyList<BulkColumnInfo> matchColumns,
                                                bool async,
                                                CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return 0;

        var staging = await StageAsync(context, table, rows, withOrdinal: false, async, cancellationToken);

        var deleted = await context.ExecuteNonQueryAsync($"DELETE FROM {QuoteTable(table)} AS t USING {staging.Name} AS s WHERE {staging.MatchCondition(matchColumns)}", async, cancellationToken);

        await staging.DropAsync(context, async, cancellationToken);

        return deleted;
    }

    /// <inheritdoc/>
    public override async Task<List<T>> ReadAsync<T>(BulkCommandContext context,
                                                     BulkTableInfo table,
                                                     BulkRowSet rows,
                                                     IReadOnlyList<BulkColumnInfo> matchColumns,
                                                     IReadOnlyList<BulkColumnInfo> selectColumns,
                                                     Func<string, bool, CancellationToken, Task<List<T>>> query,
                                                     bool async,
                                                     CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return [];

        var staging = await StageAsync(context, table, rows, withOrdinal: false, async, cancellationToken);

        var sql = $"SELECT {TargetColumns(selectColumns)} FROM {QuoteTable(table)} AS t WHERE EXISTS (SELECT 1 FROM {staging.Name} AS s WHERE {staging.MatchCondition(matchColumns)})";

        context.LogCommand(sql);

        var result = await query(sql, async, cancellationToken);

        await staging.DropAsync(context, async, cancellationToken);

        return result;
    }

    /// <inheritdoc/>
    public override Task TruncateAsync(BulkCommandContext context, BulkTableInfo table, bool async, CancellationToken cancellationToken)
        => context.ExecuteNonQueryAsync($"TRUNCATE TABLE {QuoteTable(table)}", async, cancellationToken);

    #region Helpers

    private static NpgsqlConnection GetConnection(BulkCommandContext context)
        => context.Connection as NpgsqlConnection
           ?? throw new InvalidOperationException($"{nameof(NpgsqlBulkProvider)} needs an {nameof(NpgsqlConnection)}, but the context uses '{context.Connection.GetType().Name}'.");

    /// <summary>
    /// Creates an empty temporary table with one column per column of <paramref name="rows"/>, using the exact database types of
    /// <paramref name="table"/>, and copies the rows into it.
    /// </summary>
    private static async Task<StagingTable> StageAsync(BulkCommandContext context, BulkTableInfo table, BulkRowSet rows, bool withOrdinal, bool async, CancellationToken cancellationToken)
    {
        var staging = new StagingTable(Quote($"__milva_bulk_{Guid.NewGuid():N}"), rows);
        var selected = rows.Columns.Select((c, i) => $"t.{Quote(c.Name)} AS {StagingTable.Alias(i)}");

        if (withOrdinal)
            selected = selected.Append($"0::integer AS {Quote(_ordinalColumn)}");

        await context.ExecuteNonQueryAsync($"CREATE TEMPORARY TABLE {staging.Name} ON COMMIT DROP AS SELECT {string.Join(", ", selected)} FROM {QuoteTable(table)} AS t WITH NO DATA",
                                           async,
                                           cancellationToken);

        await CopyAsync(context, staging.Name, rows, [.. Enumerable.Range(0, rows.Columns.Count).Select(StagingTable.Alias)], withOrdinal, async, cancellationToken);

        return staging;
    }

    /// <summary>
    /// Streams <paramref name="rows"/> into <paramref name="target"/> with binary COPY. With <paramref name="withOrdinal"/>, the row position is written to an extra column.
    /// </summary>
    private static async Task<ulong> CopyAsync(BulkCommandContext context, string target, BulkRowSet rows, string[] columnNames, bool withOrdinal, bool async, CancellationToken cancellationToken)
    {
        var connection = GetConnection(context);
        var names = withOrdinal ? columnNames.Append(Quote(_ordinalColumn)) : columnNames;
        var sql = $"COPY {target} ({string.Join(", ", names)}) FROM STDIN (FORMAT BINARY)";
        var dataTypeNames = rows.Columns.Select(c => NpgsqlValueWriter.GetDataTypeName(c.StoreType)).ToArray();
        var writers = new NpgsqlValueWriter.Writer[dataTypeNames.Length];

        context.LogCommand(sql);

        var importer = async ? await connection.BeginBinaryImportAsync(sql, cancellationToken) : connection.BeginBinaryImport(sql);

        try
        {
            if (context.CommandTimeout is int timeout && timeout > 0)
                importer.Timeout = TimeSpan.FromSeconds(timeout);

            for (int row = 0; row < rows.Count; row++)
            {
                if (async)
                    await importer.StartRowAsync(cancellationToken);
                else
                    importer.StartRow();

                for (int column = 0; column < dataTypeNames.Length; column++)
                {
                    var value = rows.GetValue(row, column);

                    try
                    {
                        if (value is null or DBNull)
                        {
                            if (async)
                                await importer.WriteNullAsync(cancellationToken);
                            else
                                importer.WriteNull();
                        }
                        else
                        {
                            var writer = writers[column];

                            if (writer is null || writer.Type != value.GetType())
                                writers[column] = writer = NpgsqlValueWriter.GetWriter(value.GetType());

                            await writer.WriteAsync(importer, value, dataTypeNames[column], async, cancellationToken);
                        }
                    }
                    catch (Exception ex) when (ex is InvalidCastException or ArgumentException or NotSupportedException)
                    {
                        throw new InvalidOperationException($"Value of type '{value?.GetType().Name}' cannot be written to column '{rows.Columns[column].Name}' "
                                                            + $"of PostgreSQL type '{dataTypeNames[column]}': {ex.Message}", ex);
                    }
                }

                if (withOrdinal)
                {
                    if (async)
                        await importer.WriteAsync(row, NpgsqlDbType.Integer, cancellationToken);
                    else
                        importer.Write(row, NpgsqlDbType.Integer);
                }
            }

            return async ? await importer.CompleteAsync(cancellationToken) : importer.Complete();
        }
        finally
        {
            if (async)
                await importer.DisposeAsync();
            else
                importer.Dispose();
        }
    }

    private static async Task<int> ExecuteReaderAsync(BulkCommandContext context, string sql, Action<DbDataReader> consume, bool async, CancellationToken cancellationToken)
    {
        var command = context.CreateCommand(sql);

        try
        {
            var reader = async ? await command.ExecuteReaderAsync(cancellationToken) : command.ExecuteReader();

            try
            {
                var count = 0;

                // The synchronous call only runs for the synchronous API (async == false).
#pragma warning disable AsyncFixer02
                while (async ? await reader.ReadAsync(cancellationToken) : reader.Read())
#pragma warning restore AsyncFixer02
                {
                    consume(reader);
                    count++;
                }

                return count;
            }
            finally
            {
                if (async)
                    await reader.DisposeAsync();
                else
                    reader.Dispose();
            }
        }
        finally
        {
            if (async)
                await command.DisposeAsync();
            else
                command.Dispose();
        }
    }

    private static string TargetColumns(IEnumerable<BulkColumnInfo> columns) => string.Join(", ", columns.Select(c => $"t.{Quote(c.Name)}"));

    private static string QuoteTable(BulkTableInfo table) => table.Schema is null ? Quote(table.Name) : $"{Quote(table.Schema)}.{Quote(table.Name)}";

    private static string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";

    /// <summary>
    /// A temporary table holding the rows of an operation. Columns are named by their position in the row set.
    /// </summary>
    private sealed class StagingTable(string name, BulkRowSet rows)
    {
        public string Name { get; } = name;

        public static string Alias(int index) => $"c{index.ToString(CultureInfo.InvariantCulture)}";

        /// <summary>
        /// The staging column holding <paramref name="column"/>, which must be one of the row set's column entries.
        /// </summary>
        public string Column(BulkColumnInfo column)
        {
            for (int i = 0; i < rows.Columns.Count; i++)
                if (ReferenceEquals(rows.Columns[i], column))
                    return $"s.{Alias(i)}";

            throw new ArgumentException($"Column '{column.Name}' is not part of the staged rows.", nameof(column));
        }

        public string AllColumns() => string.Join(", ", Enumerable.Range(0, rows.Columns.Count).Select(i => $"s.{Alias(i)}"));

        /// <summary>
        /// Matches target rows to staged rows with <c>=</c>; a NULL match value matches no row.
        /// </summary>
        public string MatchCondition(IReadOnlyList<BulkColumnInfo> columns) => string.Join(" AND ", columns.Select(c => $"t.{Quote(c.Name)} = {Column(c)}"));

        public Task<int> DropAsync(BulkCommandContext context, bool async, CancellationToken cancellationToken)
            => context.ExecuteNonQueryAsync($"DROP TABLE IF EXISTS {Name}", async, cancellationToken);
    }

    #endregion
}