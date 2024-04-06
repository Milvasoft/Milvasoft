namespace Milvasoft.Interception.Interceptors.Cache;

public class CacheInterceptionPostConfigureOptions
{
    /// <summary>
    /// When a return value is to be cached, a cache key is created with the method name and the values of the method request parameters. 
    /// In cases where HttpRequestHeaders may cause differences in the requests made, the value returned by this delegate is appended to the cache key.
    /// </summary>
    public Func<IServiceProvider, string> CacheKeyConfigurator { get; set; }

    public Type CacheAccessorType { get; set; }
}