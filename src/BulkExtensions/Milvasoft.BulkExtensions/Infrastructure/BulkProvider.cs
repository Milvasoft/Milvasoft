namespace Milvasoft.BulkExtensions.Infrastructure;

/// <summary>
/// Database specific part of the bulk operations. Each database provider package implements this class.
/// </summary>
/// <remarks>
/// <para>
/// The core library resolves the model, chooses columns, converts values, handles transactions and writes returned values back to entities.
/// A provider only moves rows to the database and runs the statements. Every method receives an open connection with an active transaction.
/// </para>
/// <para>
/// Methods take an <c>async</c> flag. When it is <see langword="false"/> the provider must use only synchronous database APIs,
/// so that the returned task is already completed.
/// </para>
/// <para>
/// Instances are shared between contexts and threads, so implementations must be stateless.
/// </para>
/// </remarks>
public abstract class BulkProvider
{
    /// <summary>
    /// Inserts <paramref name="rows"/> into <paramref name="table"/>.
    /// </summary>
    /// <param name="context">Connection and settings.</param>
    /// <param name="table">Target table.</param>
    /// <param name="rows">Rows to insert. May have no columns, in which case every column takes its default value.</param>
    /// <param name="returning">When not <see langword="null"/>, the provider returns <see cref="BulkReturning.Columns"/> of each inserted row
    /// through <see cref="BulkReturning.ConsumeRow"/>.</param>
    /// <param name="async">Whether to run asynchronously.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of inserted rows.</returns>
    public abstract Task<int> InsertAsync(BulkCommandContext context, BulkTableInfo table, BulkRowSet rows, BulkReturning returning, bool async, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the rows of <paramref name="table"/> that match <paramref name="rows"/> on <paramref name="matchColumns"/>.
    /// </summary>
    /// <param name="context">Connection and settings.</param>
    /// <param name="table">Target table.</param>
    /// <param name="rows">Input rows. Contains the match columns and the columns to set.</param>
    /// <param name="matchColumns">Columns used to find the rows, compared with <c>=</c> (a NULL value matches no row).
    /// The same database column may appear in <paramref name="matchColumns"/> and <paramref name="setColumns"/> as two different
    /// entries of <paramref name="rows"/>, for example a concurrency token matched by its original value and set to its new value.</param>
    /// <param name="setColumns">Columns to update. Never empty.</param>
    /// <param name="returning">When not <see langword="null"/>, the provider returns <see cref="BulkReturning.Columns"/> of each updated row
    /// through <see cref="BulkReturning.ConsumeRow"/>. <see cref="BulkReturning.Columns"/> may be empty, in which case only the row positions are reported.</param>
    /// <param name="async">Whether to run asynchronously.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of updated rows.</returns>
    public abstract Task<int> UpdateAsync(BulkCommandContext context,
                                          BulkTableInfo table,
                                          BulkRowSet rows,
                                          IReadOnlyList<BulkColumnInfo> matchColumns,
                                          IReadOnlyList<BulkColumnInfo> setColumns,
                                          BulkReturning returning,
                                          bool async,
                                          CancellationToken cancellationToken);

    /// <summary>
    /// Inserts <paramref name="rows"/>, and updates the existing rows that conflict on <paramref name="matchColumns"/>.
    /// </summary>
    /// <param name="context">Connection and settings.</param>
    /// <param name="table">Target table.</param>
    /// <param name="rows">Input rows. Contains every column to insert, including the match columns.</param>
    /// <param name="matchColumns">Columns of the unique constraint that decides whether a row exists. Input rows never have NULL in these columns.</param>
    /// <param name="setColumns">Columns to update on existing rows. When empty, existing rows are left unchanged.</param>
    /// <param name="returning">When not <see langword="null"/>, the provider returns <see cref="BulkReturning.Columns"/> of each inserted or updated row
    /// through <see cref="BulkReturning.ConsumeKeyedRow"/>.</param>
    /// <param name="async">Whether to run asynchronously.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of inserted and updated rows.</returns>
    public abstract Task<int> UpsertAsync(BulkCommandContext context,
                                          BulkTableInfo table,
                                          BulkRowSet rows,
                                          IReadOnlyList<BulkColumnInfo> matchColumns,
                                          IReadOnlyList<BulkColumnInfo> setColumns,
                                          BulkReturning returning,
                                          bool async,
                                          CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the rows of <paramref name="table"/> that match <paramref name="rows"/> on <paramref name="matchColumns"/>.
    /// </summary>
    /// <param name="context">Connection and settings.</param>
    /// <param name="table">Target table.</param>
    /// <param name="rows">Input rows. Contains only the match columns.</param>
    /// <param name="matchColumns">Columns used to find the rows. Compared with <c>=</c>, so a NULL value matches no row.</param>
    /// <param name="async">Whether to run asynchronously.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of deleted rows.</returns>
    public abstract Task<int> DeleteAsync(BulkCommandContext context,
                                          BulkTableInfo table,
                                          BulkRowSet rows,
                                          IReadOnlyList<BulkColumnInfo> matchColumns,
                                          bool async,
                                          CancellationToken cancellationToken);

    /// <summary>
    /// Makes <paramref name="rows"/> available to a query and runs <paramref name="query"/> with SQL that selects <paramref name="selectColumns"/> of
    /// the rows of <paramref name="table"/> that match <paramref name="rows"/> on <paramref name="matchColumns"/>.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="context">Connection and settings.</param>
    /// <param name="table">Target table.</param>
    /// <param name="rows">Input rows. Contains only the match columns.</param>
    /// <param name="matchColumns">Columns used to find the rows. Compared with <c>=</c>, so a NULL value matches no row.</param>
    /// <param name="selectColumns">Columns to select. Every column of the table that the model maps, including system columns such as <c>xmin</c>.</param>
    /// <param name="query">Runs the given SQL through Entity Framework and returns the entities.</param>
    /// <param name="async">Whether to run asynchronously.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entities that <paramref name="query"/> returned.</returns>
    public abstract Task<List<T>> ReadAsync<T>(BulkCommandContext context,
                                               BulkTableInfo table,
                                               BulkRowSet rows,
                                               IReadOnlyList<BulkColumnInfo> matchColumns,
                                               IReadOnlyList<BulkColumnInfo> selectColumns,
                                               Func<string, bool, CancellationToken, Task<List<T>>> query,
                                               bool async,
                                               CancellationToken cancellationToken) where T : class;

    /// <summary>
    /// Removes every row of <paramref name="table"/>.
    /// </summary>
    /// <param name="context">Connection and settings.</param>
    /// <param name="table">Target table.</param>
    /// <param name="async">Whether to run asynchronously.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public abstract Task TruncateAsync(BulkCommandContext context, BulkTableInfo table, bool async, CancellationToken cancellationToken);
}