using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Ef.WithNoLock;

/// <summary>
/// This attribute ensures that the "with(nolock)" expression is added to the select queries made within the methods marked with this attribute.
/// </summary>
public class WithNoLockAttribute(bool getDbContextFromServiceProvider = true) : DecorateAttribute(typeof(WithNoLockInterceptor))
{
    /// <summary>
    /// If this value is true, the DbContext instance is fetched from the service collection.
    /// If this value is false, the interceptor checks whether the class to be intercepted implements the ICanRetrieveDbContext interface to access the DbContext instance.
    /// </summary>
    public bool GetDbContextFromServiceProvider { get; set; } = getDbContextFromServiceProvider;
}