using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Cache;

public class CacheAttribute : DecorateAttribute
{
    /// <summary>
    /// Determines whether <see cref="CacheInterceptor"/> process action once.
    /// </summary>
    public bool IsProcessedOnce { get; set; }

    /// <summary>
    /// Cache key.
    /// </summary>
    public string Key { get; set; } = null;

    /// <summary>
    /// Timeout as second. Default is 300
    /// </summary>
    public int? Timeout { get; set; } = 300;

    public CacheAttribute(string key) : base(typeof(CacheInterceptor))
    {
        Key = key;
    }

    public CacheAttribute(string key, int timeout) : base(typeof(CacheInterceptor))
    {
        Key = key;
        Timeout = timeout;
    }
}