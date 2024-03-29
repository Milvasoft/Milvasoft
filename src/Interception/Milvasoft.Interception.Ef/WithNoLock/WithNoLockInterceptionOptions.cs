using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Utils.Constants;

namespace Milvasoft.Interception.Ef.WithNoLock;

public class WithNoLockInterceptionOptions : IWithNoLockInterceptionOptions
{
    private static Type _dbContextType = null;

    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Interception:WithNoLock";

    public ServiceLifetime InterceptorLifetime { get; set; } = ServiceLifetime.Scoped;

    public string DbContextAssemblyQualifiedName { get; set; }

#pragma warning disable S2696 // Instance members should not write to "static" fields
    public Type GetDbContextType() => _dbContextType ??= Type.GetType(DbContextAssemblyQualifiedName);
#pragma warning restore S2696 // Instance members should not write to "static" fields
}

public interface IWithNoLockInterceptionOptions : IMilvaOptions
{
    public ServiceLifetime InterceptorLifetime { get; set; }

    public string DbContextAssemblyQualifiedName { get; set; }

    public Type GetDbContextType();
}