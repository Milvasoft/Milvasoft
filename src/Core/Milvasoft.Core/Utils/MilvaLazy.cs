using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Core.Utils;

/// <summary>
/// Represents a custom implementation of the <see cref="Lazy{T}"/> class.
/// </summary>
/// <typeparam name="T">The type of the lazily initialized value.</typeparam>
/// <remarks>
/// This class provides a constructor for creating an instance of <see cref="MilvaLazy{T}"/>.
/// </remarks>
/// <param name="serviceProvider">The service provider used to resolve the lazily initialized value.</param>
public class MilvaLazy<T>(IServiceProvider serviceProvider) : Lazy<T>(() => serviceProvider.GetRequiredService<T>())
{
}
