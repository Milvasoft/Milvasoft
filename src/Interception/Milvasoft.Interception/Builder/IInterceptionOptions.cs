using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Interception.Builder;

/// <summary>
/// Represents the options for log interception.
/// </summary>
public interface IInterceptionOptions : IMilvaOptions
{
    /// <summary>
    /// Log interceptor lifetime.
    /// </summary>
    public ServiceLifetime InterceptorLifetime { get; set; }
}