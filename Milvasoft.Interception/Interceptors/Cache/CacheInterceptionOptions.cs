namespace Milvasoft.Interception.Interceptors.Logging;

public class CacheInterceptionOptions : ICacheInterceptionOptions
{
    public Type CacheAccessorType { get; set; }
}


public interface ICacheInterceptionOptions
{
    public Type CacheAccessorType { get; set; }
}