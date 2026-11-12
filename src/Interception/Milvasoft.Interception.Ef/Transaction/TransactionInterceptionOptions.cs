using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Interception.Ef.Builder;

namespace Milvasoft.Interception.Ef.Transaction;

/// <summary>
/// Represents the options for transaction interception.
/// </summary>
public class TransactionInterceptionOptions : HaveDbContextType, ITransactionInterceptionOptions
{
    /// <inheritdoc/>
    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Interception:Transaction";

    /// <inheritdoc/>
    public ServiceLifetime InterceptorLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <inheritdoc/>
    public bool RollbackOnUnsuccessfulResponse { get; set; } = true;
}

/// <summary>
/// Represents the options for transaction interception.
/// </summary>
public interface ITransactionInterceptionOptions : IHaveDbContextType, IMilvaOptions
{
    /// <summary>
    /// Transaction interceptor lifetime.
    /// </summary>
    public ServiceLifetime InterceptorLifetime { get; set; }

    /// <summary>
    /// When true (default) the transaction is rolled back when the intercepted method returns an IResponse whose IsSuccess is false.
    /// </summary>
    public bool RollbackOnUnsuccessfulResponse { get; set; }
}