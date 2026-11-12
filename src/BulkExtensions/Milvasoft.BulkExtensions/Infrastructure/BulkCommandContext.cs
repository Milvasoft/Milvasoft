using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Milvasoft.BulkExtensions.Infrastructure;

/// <summary>
/// Connection, transaction and settings a provider uses to run one bulk operation.
/// The connection is open and a transaction is active for the whole operation.
/// </summary>
public sealed class BulkCommandContext
{
    internal BulkCommandContext(DbContext context, DbConnection connection, DbTransaction transaction, bool ownsTransaction, int? commandTimeout, ILogger logger)
    {
        Context = context;
        Connection = connection;
        Transaction = transaction;
        OwnsTransaction = ownsTransaction;
        CommandTimeout = commandTimeout;
        Logger = logger;
    }

    /// <summary>
    /// The context the operation runs for.
    /// </summary>
    public DbContext Context { get; }

    /// <summary>
    /// The open connection of <see cref="Context"/>.
    /// </summary>
    public DbConnection Connection { get; }

    /// <summary>
    /// The active transaction. <see langword="null"/> when the connection is enlisted in an ambient <see cref="System.Transactions.TransactionScope"/>.
    /// </summary>
    public DbTransaction Transaction { get; }

    /// <summary>
    /// <see langword="true"/> when the transaction was started for this operation and is committed when it ends.
    /// </summary>
    public bool OwnsTransaction { get; }

    /// <summary>
    /// Command timeout in seconds, or <see langword="null"/> for the provider default.
    /// </summary>
    public int? CommandTimeout { get; }

    /// <summary>
    /// Logger for the SQL the provider runs.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Creates a command on <see cref="Connection"/> with the transaction and timeout set.
    /// </summary>
    /// <param name="sql">Command text.</param>
    public DbCommand CreateCommand(string sql)
    {
        var command = Connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = Transaction;

        if (CommandTimeout.HasValue)
            command.CommandTimeout = CommandTimeout.Value;

        LogCommand(sql);

        return command;
    }

    /// <summary>
    /// Runs <paramref name="sql"/> and returns the number of affected rows.
    /// </summary>
    /// <param name="sql">Command text.</param>
    /// <param name="async">Whether to run asynchronously.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<int> ExecuteNonQueryAsync(string sql, bool async, CancellationToken cancellationToken)
    {
        var command = CreateCommand(sql);

        try
        {
            return async ? await command.ExecuteNonQueryAsync(cancellationToken) : command.ExecuteNonQuery();
        }
        finally
        {
            if (async)
                await command.DisposeAsync();
            else
                command.Dispose();
        }
    }

    /// <summary>
    /// Writes <paramref name="sql"/> to <see cref="Logger"/> at debug level.
    /// </summary>
    /// <param name="sql">Command text.</param>
    public void LogCommand(string sql)
    {
        if (Logger.IsEnabled(LogLevel.Debug))
            Logger.LogDebug("Executing bulk command: {Sql}", sql);
    }
}