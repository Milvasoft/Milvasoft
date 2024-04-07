using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Utils.Constants;

namespace Milvasoft.Interception.Ef.WithNoLock;

/// <summary>
/// Represents the options for no lock interception.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "This value will be assigned only at application startup and acts as a singleton. Therefore, there is no harm in this use.")]
public class WithNoLockInterceptionOptions : IWithNoLockInterceptionOptions
{
    private static Type _dbContextType = null;
    private string _dbContextAssemblyQualifiedName;

    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Interception:WithNoLock";

    public ServiceLifetime InterceptorLifetime { get; set; } = ServiceLifetime.Scoped;

    public string DbContextAssemblyQualifiedName
    {
        get => _dbContextAssemblyQualifiedName;
        set
        {
            _dbContextAssemblyQualifiedName = value;
            _dbContextType = Type.GetType(_dbContextAssemblyQualifiedName);
        }
    }

    public Type DbContextType
    {
        get => _dbContextType;
        set
        {
            _dbContextType = value;
            _dbContextAssemblyQualifiedName = value.AssemblyQualifiedName;
        }
    }

    public Type GetDbContextType() => _dbContextType ??= Type.GetType(_dbContextAssemblyQualifiedName);
}

/// <summary>
/// Represents the options for no lock interception.
/// </summary>
public interface IWithNoLockInterceptionOptions : IMilvaOptions
{
    /// <summary>
    /// With no lock interception lifetime.
    /// </summary>
    public ServiceLifetime InterceptorLifetime { get; set; }

    /// <summary>
    /// DbContext assembly qualified name for configuring options from configuration file.
    /// For example 'SomeNamespace.SomeContext, SomeNamespace, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null'
    /// </summary>
    public string DbContextAssemblyQualifiedName { get; set; }

    /// <summary>
    /// DbContext type. For example typeof(SomeContext)
    /// </summary>
    public Type DbContextType { get; set; }

    /// <summary>
    /// Returns the dbcontext type to be applied to the transaction.
    /// </summary>
    /// <returns>Returns the dbcontext type to be applied to the transaction.</returns>
    public Type GetDbContextType();
}