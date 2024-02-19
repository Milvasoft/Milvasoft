using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Abstractions.Cache;
using Milvasoft.Core.Extensions;

namespace Milvasoft.Interception.Interceptors.Cache;

public class CacheInterceptionOptions : ICacheInterceptionOptions
{
    private static Type _accessorType = null;

    public static string SectionName { get; } = $"{MilvaOptionsExtensions.ParentSectionName}:Interception:Cache";
    public ServiceLifetime InterceptorLifetime { get; set; } = ServiceLifetime.Scoped;
    public string CacheAccessorAssemblyQualifiedName { get; set; }

    /// <summary>
    /// Gets generic accessor type as <see cref="ICacheAccessor{TAccessor}"/>
    /// </summary>
    /// <returns></returns>
    public Type GetAccessorType() => _accessorType ??= typeof(ICacheAccessor<>).MakeGenericType([Type.GetType(CacheAccessorAssemblyQualifiedName)]);
}


public interface ICacheInterceptionOptions : IMilvaOptions
{
    public ServiceLifetime InterceptorLifetime { get; set; }
    public string CacheAccessorAssemblyQualifiedName { get; set; }

    public Type GetAccessorType();
}