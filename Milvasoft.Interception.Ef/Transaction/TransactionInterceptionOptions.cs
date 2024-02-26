using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Extensions;

namespace Milvasoft.Interception.Ef.Transaction;

public class TransactionInterceptionOptions : ITransactionInterceptionOptions
{
    private static Type _dbContextType = null;

    public static string SectionName { get; } = $"{MilvaOptionsExtensions.ParentSectionName}:Interception:Transaction";

    public ServiceLifetime InterceptorLifetime { get; set; } = ServiceLifetime.Scoped;

    public string DbContextAssemblyQualifiedName { get; set; }

    public Type GetDbContextType() => _dbContextType ??= Type.GetType(DbContextAssemblyQualifiedName);
}

public interface ITransactionInterceptionOptions : IMilvaOptions
{
    public ServiceLifetime InterceptorLifetime { get; set; }

    public string DbContextAssemblyQualifiedName { get; set; }

    public Type GetDbContextType();
}