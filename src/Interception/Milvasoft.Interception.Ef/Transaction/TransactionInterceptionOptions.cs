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
    public bool RollbackOnUnsuccessfulResponse { get; set; }
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
    /// When true the transaction is rolled back when the intercepted method returns an IResponse whose IsSuccess is false.
    /// Default is false: the work is committed and only exceptions roll back.
    /// </summary>
    /// <remarks>
    /// Off by default because a common pattern writes on purpose before returning an error response
    /// (e.g. incrementing the failed login counter and setting the lockout, then returning "invalid credentials").
    /// Rolling those writes back silently disables such logic. Enable it only when no method in the scope relies on that pattern.
    /// </remarks>
    public bool RollbackOnUnsuccessfulResponse { get; set; }
}