using Fody;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Ef.Builder;

namespace Milvasoft.Interception.Ef.Transaction;

/// <summary>
/// Interceptor that starts a database transaction for methods marked with <see cref="TransactionAttribute"/>.
/// </summary>
/// <remarks>
/// <para>The transaction is committed after the method's work completes. It is rolled back when the method throws,
/// and, when <see cref="ITransactionInterceptionOptions.RollbackOnUnsuccessfulResponse"/> is enabled (off by default), when it returns an unsuccessful <see cref="IResponse"/>.</para>
/// <para>When a transaction is already active on the context (nested use), the call participates in it and the outermost starter completes it.</para>
/// <para>If no DbContext can be found a <see cref="Core.Exceptions.MilvaDeveloperException"/> is thrown and the method is not run.</para>
/// </remarks>
[ConfigureAwait(false)]
public partial class TransactionInterceptor(IServiceProvider serviceProvider) : IMilvaInterceptor
{
    /// <inheritdoc/>
    public int InterceptionOrder { get; set; } = -998;

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ITransactionInterceptionOptions _transactionInterceptionOptions = serviceProvider.GetService<ITransactionInterceptionOptions>();

    /// <inheritdoc/>
    public async Task OnInvoke(Call call)
    {
        var transactionAttribute = call.GetInterceptorAttribute<TransactionAttribute>();

        var context = InterceptedDbContextResolver.Resolve(call,
                                                           transactionAttribute?.GetDbContextFromServiceProvider ?? true,
                                                           _transactionInterceptionOptions,
                                                           _serviceProvider,
                                                           nameof(TransactionInterceptor));

        // Nested use: the outer starter owns the transaction.
        if (context.Database.CurrentTransaction != null)
        {
            await call.NextAsync();
            return;
        }

        var executionStrategy = context.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            // Disposing a transaction that was neither committed nor rolled back rolls it back.
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                await call.NextAsync();
            }
            catch (Exception)
            {
                await TryRollbackAsync(transaction);

                throw;
            }

            if (ShouldRollback(call.ReturnValue))
            {
                await transaction.RollbackAsync();
                return;
            }

            // A commit failure propagates to the caller unchanged.
            await transaction.CommitAsync();
        });
    }

    private bool ShouldRollback(object returnValue)
        => (_transactionInterceptionOptions?.RollbackOnUnsuccessfulResponse ?? false) && returnValue is IResponse { IsSuccess: false };

    private static async Task TryRollbackAsync(IDbContextTransaction transaction)
    {
        try
        {
            await transaction.RollbackAsync();
        }
        catch (Exception)
        {
            // The original exception is more important; the transaction is rolled back on dispose anyway.
        }
    }
}
