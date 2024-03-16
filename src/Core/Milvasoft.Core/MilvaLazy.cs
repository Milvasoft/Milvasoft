using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Core;

/// <summary>
/// Custom <see cref="Lazy{T}"/> class.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Constructor of <see cref="MilvaLazy{T}"/>.
/// </remarks>
/// <param name="serviceProvider"></param>
public class MilvaLazy<T>(IServiceProvider serviceProvider) : Lazy<T>(() => serviceProvider.GetRequiredService<T>())
{
}
