using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Ef.Transaction;

public partial class TransactionInterceptor(IServiceProvider serviceProvider) : IMilvaInterceptor
{
    public static int InterceptionOrder { get; set; } = -998;

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ITransactionInterceptionOptions _transactionInterceptionOptions = serviceProvider.GetService<ITransactionInterceptionOptions>();

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
            using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                await call.NextAsync();

                if (transaction != null)
                    await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                if (transaction != null)
                    await transaction.RollbackAsync().ConfigureAwait(false);

                throw;
            }
        });
    }
}