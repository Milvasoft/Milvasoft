using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Interception.Ef.Builder;

namespace Milvasoft.Interception.Ef.WithNoLock;

/// <summary>
/// Represents the options for no lock interception.
/// </summary>
public class WithNoLockInterceptionOptions : HaveDbContextType, IWithNoLockInterceptionOptions
{
    /// <inheritdoc/>
    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Interception:WithNoLock";

    /// <inheritdoc/>
    public ServiceLifetime InterceptorLifetime { get; set; } = ServiceLifetime.Scoped;
}

/// <summary>
/// Represents the options for no lock interception.
/// </summary>
public interface IWithNoLockInterceptionOptions : IHaveDbContextType, IMilvaOptions
{
    /// <summary>
    /// With no lock interception lifetime.
    /// </summary>
    public ServiceLifetime InterceptorLifetime { get; set; }
}