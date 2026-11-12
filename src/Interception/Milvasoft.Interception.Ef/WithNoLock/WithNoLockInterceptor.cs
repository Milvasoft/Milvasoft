using Fody;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Ef.Builder;
using System.Transactions;

namespace Milvasoft.Interception.Ef.WithNoLock;

/// <summary>
/// Interceptor that adds "WITH(NOLOCK)" hint to select queries made in methods marked with <see cref="WithNoLockAttribute"/>.
/// </summary>
/// <remarks>
/// If no DbContext can be found a <see cref="Core.Exceptions.MilvaDeveloperException"/> is thrown and the method is not run.
/// </remarks>
[ConfigureAwait(false)]
public partial class WithNoLockInterceptor(IServiceProvider serviceProvider) : IMilvaInterceptor
{
    /// <summary>
    /// Interception call order.
    /// </summary>
    public int InterceptionOrder { get; set; } = -997;

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IWithNoLockInterceptionOptions _withNoLockInterceptionOptions = serviceProvider.GetService<IWithNoLockInterceptionOptions>();
    private static readonly TransactionOptions _transactionOptions = new()
    {
        IsolationLevel = IsolationLevel.ReadUncommitted,
    };

    /// <summary>
    /// Starts db with(nolock) operation.
    /// </summary>
    /// <param name="call"></param>
    /// <returns></returns>
    public Task OnInvoke(Call call)
    {
        var withNoLockAttribute = call.GetInterceptorAttribute<WithNoLockAttribute>();

        DbContext context = InterceptedDbContextResolver.Resolve(call,
                                                                 withNoLockAttribute?.GetDbContextFromServiceProvider ?? true,
                                                                 _withNoLockInterceptionOptions,
                                                                 _serviceProvider,
                                                                 nameof(WithNoLockInterceptor));

        var executionStrategy = context.Database.CreateExecutionStrategy();

        return executionStrategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, _transactionOptions, TransactionScopeAsyncFlowOption.Enabled);

            await call.NextAsync();

            transactionScope.Complete();
        });
    }
}
