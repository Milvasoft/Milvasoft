using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Milvasoft.BulkExtensions.Infrastructure;

namespace Milvasoft.BulkExtensions.Internal;

/// <summary>
/// Runs a bulk operation with an open connection inside a transaction.
/// </summary>
/// <remarks>
/// When the context has no transaction, the operation starts one, commits it at the end, and runs inside the execution strategy of the context,
/// so retrying strategies such as <c>EnableRetryOnFailure</c> work. When a transaction already exists (from <c>BeginTransaction</c> or a
/// <see cref="System.Transactions.TransactionScope"/>), the operation joins it and the caller decides when to commit.
/// Values returned by the database are written to entities only after the operation succeeds.
/// </remarks>
internal static class BulkRunner
{
    public const string LoggerCategory = "Milvasoft.BulkExtensions";

    public static async Task<TResult> RunAsync<TResult>(DbContext context,
                                                        BulkConfig config,
                                                        bool saveChangesMode,
                                                        Func<BulkExecutor, bool, CancellationToken, Task<TResult>> work,
                                                        bool async,
                                                        CancellationToken cancellationToken)
    {
        var provider = BulkProviderResolver.Resolve(context);
        var database = context.Database;
        var timeout = config?.CommandTimeout ?? database.GetCommandTimeout();
        var logger = CreateLogger(context);

        if (database.CurrentTransaction is not null || System.Transactions.Transaction.Current is not null)
        {
            var (result, executor) = await RunAttemptAsync(context, provider, config, saveChangesMode, timeout, logger, ownTransaction: false, work, async, cancellationToken);

            executor.Pending.Apply();

            return result;
        }

        var strategy = database.CreateExecutionStrategy();
        BulkExecutor succeeded = null;
        TResult value;

        if (async)
        {
            value = await strategy.ExecuteAsync(async ct =>
            {
                var (result, executor) = await RunAttemptAsync(context, provider, config, saveChangesMode, timeout, logger, ownTransaction: true, work, true, ct);
                succeeded = executor;
                return result;
            }, cancellationToken);
        }
        else
        {
            value = strategy.Execute(() =>
            {
                var (result, executor) = RunAttemptAsync(context, provider, config, saveChangesMode, timeout, logger, ownTransaction: true, work, false, default).GetAwaiter().GetResult();
                succeeded = executor;
                return result;
            });
        }

        succeeded.Pending.Apply();

        return value;
    }

    private static async Task<(TResult, BulkExecutor)> RunAttemptAsync<TResult>(DbContext context,
                                                                               BulkProvider provider,
                                                                               BulkConfig config,
                                                                               bool saveChangesMode,
                                                                               int? timeout,
                                                                               ILogger logger,
                                                                               bool ownTransaction,
                                                                               Func<BulkExecutor, bool, CancellationToken, Task<TResult>> work,
                                                                               bool async,
                                                                               CancellationToken cancellationToken)
    {
        var database = context.Database;

        if (async)
            await database.OpenConnectionAsync(cancellationToken);
        else
            database.OpenConnection();

        try
        {
            if (!ownTransaction)
            {
                var command = new BulkCommandContext(context, database.GetDbConnection(), database.CurrentTransaction?.GetDbTransaction(), false, timeout, logger);
                var executor = new BulkExecutor(provider, command, config, saveChangesMode);

                return (await work(executor, async, cancellationToken), executor);
            }

            var transaction = async ? await database.BeginTransactionAsync(cancellationToken) : database.BeginTransaction();

            try
            {
                var command = new BulkCommandContext(context, database.GetDbConnection(), transaction.GetDbTransaction(), true, timeout, logger);
                var executor = new BulkExecutor(provider, command, config, saveChangesMode);
                var result = await work(executor, async, cancellationToken);

                if (async)
                    await transaction.CommitAsync(cancellationToken);
                else
                    transaction.Commit();

                return (result, executor);
            }
            finally
            {
                if (async)
                    await transaction.DisposeAsync();
                else
                    transaction.Dispose();
            }
        }
        finally
        {
            if (async)
                await database.CloseConnectionAsync();
            else
                database.CloseConnection();
        }
    }

    private static ILogger CreateLogger(DbContext context)
    {
        var services = ((IInfrastructure<IServiceProvider>)context).Instance;
        var loggerFactory = services.GetService(typeof(ILoggerFactory)) as ILoggerFactory ?? NullLoggerFactory.Instance;

        return loggerFactory.CreateLogger(LoggerCategory);
    }
}