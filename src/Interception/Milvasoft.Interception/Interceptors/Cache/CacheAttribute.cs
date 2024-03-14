using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Cache;

public class CacheAttribute : DecorateAttribute
{
    /// <summary>
    /// Cache key.
    /// </summary>
    public string Key = null;

    /// <summary>
    /// Timeout as ms.
    /// </summary>
    public int? Timeout = null;

    /// <summary>
    /// Determines whether <see cref="CacheInterceptor"/> process action once.
    /// </summary>
    public bool IsProcessedOnce { get; set; }

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