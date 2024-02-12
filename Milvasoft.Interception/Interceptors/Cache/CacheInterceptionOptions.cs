namespace Milvasoft.Interception.Interceptors.Cache;

public class CacheInterceptionOptions : ICacheInterceptionOptions
{
    public Type CacheAccessorType { get; set; }
}


public interface ICacheInterceptionOptions
{
    public Type CacheAccessorType { get; set; }
}