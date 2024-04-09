using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Interception.Decorator;
using System.Transactions;

namespace Milvasoft.Interception.Ef.WithNoLock;

/// <summary>
/// Interceptor that adds "WITH(NOLOCK)" hint to select queries made in methods marked with <see cref="WithNoLockAttribute"/>.
/// </summary>
public partial class WithNoLockInterceptor(IServiceProvider serviceProvider) : IMilvaInterceptor
{
    public int InterceptionOrder { get; set; } = -997;

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IWithNoLockInterceptionOptions _withNoLockInterceptionOptions = serviceProvider.GetService<IWithNoLockInterceptionOptions>();
    private static readonly TransactionOptions _transactionOptions = new()
    {
        IsolationLevel = IsolationLevel.ReadUncommitted,
    };

    public async Task OnInvoke(Call call)
    {
        var withNoLockAttribute = call.GetInterceptorAttribute<WithNoLockAttribute>();

        DbContext context = null;

        if (!withNoLockAttribute.GetDbContextFromServiceProvider)
        {
            if (call.MethodImplementation.DeclaringType.IsAssignableFrom(typeof(ICanRetrieveDbContext)))
            {
                var canRetrieveDbContext = call.MethodImplementation.DeclaringType as ICanRetrieveDbContext;

                context = canRetrieveDbContext.GetDbContext(_withNoLockInterceptionOptions.GetDbContextType());
            }
        }
        else
            context = _serviceProvider.GetService(_withNoLockInterceptionOptions.GetDbContextType()) as DbContext;

        if (context != null)
        {
            var executionStrategy = context.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transactionScope = new TransactionScope(TransactionScopeOption.Required, _transactionOptions, TransactionScopeAsyncFlowOption.Enabled);

                await call.NextAsync();

                transactionScope.Complete();
            });
        }
        else
            await call.NextAsync();
    }
}