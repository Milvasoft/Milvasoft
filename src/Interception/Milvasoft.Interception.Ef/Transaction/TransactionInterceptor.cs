using Fody;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Ef.Transaction;

/// <summary>
/// Interceptor that starts a database transaction for methods marked with <see cref="TransactionAttribute"/>.
/// </summary>
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

        DbContext context = null;

        if (!transactionAttribute.GetDbContextFromServiceProvider)
        {
            if (call.MethodImplementation.DeclaringType.IsAssignableFrom(typeof(ICanRetrieveDbContext)))
            {
                var canRetrieveDbContext = call.MethodImplementation.DeclaringType as ICanRetrieveDbContext;

                context = canRetrieveDbContext.GetDbContext(_transactionInterceptionOptions.GetDbContextType());
            }
        }
        else
            context = _serviceProvider.GetService(_transactionInterceptionOptions.GetDbContextType()) as DbContext;

        if (context == null)
            await call.NextAsync();

        var executionStrategy = context.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            // In nested uses, transactions are shared. In such cases, it determines which transaction will be completed after the commit process is completed.
            bool isTransactionStarter = false;

            IDbContextTransaction transaction = null;

            if (context.Database.CurrentTransaction == null)
            {
                transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
                isTransactionStarter = true;
            }
            else
            {
                transaction = context.Database.CurrentTransaction;
            }

            try
            {
                await call.NextAsync();

                if (transaction != null && isTransactionStarter)
                {
                    await transaction.CommitAsync().ConfigureAwait(false);
                    await transaction.DisposeAsync();
                }
            }
            catch (Exception)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    await transaction.DisposeAsync();
                }

                throw;
            }
        });
    }
}