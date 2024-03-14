namespace Milvasoft.Interception.Decorator;

/// <summary>
/// <para>Marks a method to be intercepted by a given <see cref="IMilvaInterceptor"/>.</para>
/// <para>Can be used in both interface and class methods within a hierarchy.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class DecorateAttribute : Attribute
{
    /// <summary>
    /// Gets the <see cref="Type"/> represting the registered decorator.
    /// </summary>
    public Type DecoratorType { get; }

    /// <summary>
    /// Creates an attribute decorating method with a single type of <see cref="IMilvaInterceptor"/>.
    /// </summary>
    /// <param name="decoratorType">A <see cref="Type"/> assignable to <see cref="IMilvaInterceptor"/>.</param>
    public DecorateAttribute(Type decoratorType)
    {
        ArgumentNullException.ThrowIfNull(decoratorType);

        if (!typeof(IMilvaInterceptor).IsAssignableFrom(decoratorType))
        {
            throw new ArgumentException($"Type '{decoratorType.Name}' does not implement interface '{nameof(IMilvaInterceptor)}'", nameof(decoratorType));
        }

        DecoratorType = decoratorType;
    }
}
