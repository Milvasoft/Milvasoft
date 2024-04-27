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
}