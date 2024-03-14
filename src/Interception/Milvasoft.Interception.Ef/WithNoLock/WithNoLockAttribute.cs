using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Ef.WithNoLock;

public class WithNoLockAttribute(bool getDbContextFromServiceProvider = true) : DecorateAttribute(typeof(WithNoLockInterceptor))
{
    /// <summary>
    /// If this value is true, the DbContext instance is fetched from the service collection.
    /// If this value is false, the interceptor checks whether the class to be intercepted implements the ICanRetrieveDbContext interface to access the DbContext instance.
    /// </summary>
    public bool GetDbContextFromServiceProvider { get; set; } = getDbContextFromServiceProvider;
}