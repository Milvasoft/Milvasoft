using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Caching.Builder;
using Milvasoft.Caching.InMemory.Options;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Abstractions.Cache;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Interceptors.Cache;

namespace Milvasoft.UnitTests.InteceptionTests;

public class CacheInterceptorTests
{
    [Fact]
    public void Method_WithCacheInterceptor_ShouldCacheCorrectlyAndMethodCalledOnce()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<ISomeInterface>();
        var cacheAccessor = services.GetService<ICacheAccessor<TestCacheAccessor>>();

        // Act & Assert
        var cachedValue = cacheAccessor.Get<string>("");

        // cache should be null before the method run
        cachedValue.Should().BeNull();

        // call the method
        var result = sut.Method();

        cachedValue = cacheAccessor.Get<string>("");

        // method result should be cached
        cachedValue.Should().Be(result);

        // call the method again
        sut.Method();

        sut.CallCount.Should().Be(1);

    }

    [Fact]
    public void MethodReturnTypeIsIResponse_WithCacheInterceptor_ShouldCacheCorrectlyAndMethodCalledOnceAndResponseCachedFlagIsTrue()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<ISomeInterface>();
        var cacheAccessor = services.GetService<ICacheAccessor<TestCacheAccessor>>();

        // Act & Assert
        var cachedValue = cacheAccessor.Get<Response<string>>("");

        // cache should be null before the method run
        cachedValue.Should().BeNull();

        // call the method
        var result = sut.MethodReturnTypeIsIResponse();

        result.IsCachedData.Should().BeFalse();
        cachedValue = cacheAccessor.Get<Response<string>>("");

        // method result should be cached
        cachedValue.Should().Be(result);

        // call the method again
        sut.MethodReturnTypeIsIResponse();

        sut.CallCount.Should().Be(1);
        result.IsCachedData.Should().BeTrue();

    }

    #region Setup

    public interface ISomeInterface : IInterceptable
    {
        public int CallCount { get; set; }

        string Method();
        Response<string> MethodReturnTypeIsIResponse();
    }

    public class SomeClass : ISomeInterface
    {
        public int CallCount { get; set; }

        public SomeClass()
        {

        }

        [Cache("Test")]
        public virtual string Method()
        {
            CallCount++;
            return "Cached return value";
        }

        [Cache("Test")]
        public virtual Response<string> MethodReturnTypeIsIResponse()
        {
            CallCount++;
            return Response<string>.Success("Cached return value");
        }
    }

    public class TestCacheAccessor : ICacheAccessor<TestCacheAccessor>
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public Dictionary<string, object> Cache { get; set; } = [];

        public async Task<bool> SetAsync(string key, object value) => Cache.TryAdd("Test", value);
        public async Task<bool> SetAsync(string key, object value, TimeSpan? expiration) => Cache.TryAdd("Test", value);
        public async Task<T> GetAsync<T>(string key) where T : class => Cache.Count != 0 ? (T)Cache["Test"] : null;
        public async Task<object> GetAsync(string key, Type returnType) => Cache.Count != 0 ? Cache["Test"] : null;
        public async Task<string> GetAsync(string key) => throw new NotImplementedException();
        public Task<IEnumerable<T>> GetAsync<T>(IEnumerable<string> keys) => throw new NotImplementedException();
        public T Get<T>(string key) where T : class => Cache.Count != 0 ? (T)Cache["Test"] : null;
        public string Get(string key) => throw new NotImplementedException();
        public IEnumerable<T> Get<T>(IEnumerable<string> keys) => throw new NotImplementedException();
        public bool IsConnected() => throw new NotImplementedException();
        public bool KeyExists(string key) => throw new NotImplementedException();
        public Task<bool> KeyExistsAsync(string key) => throw new NotImplementedException();
        public bool KeyExpire(string key, TimeSpan expiration) => throw new NotImplementedException();
        public bool KeyExpire(string key, DateTime? expiration) => throw new NotImplementedException();
        public Task<bool> KeyExpireAsync(string key, TimeSpan expiration) => throw new NotImplementedException();
        public Task<bool> KeyExpireAsync(string key, DateTime? expiration) => throw new NotImplementedException();
        public bool Remove(string key) => throw new NotImplementedException();
        public Task<bool> RemoveAsync(string key) => throw new NotImplementedException();
        public Task<long> RemoveAsync(IEnumerable<string> keys) => throw new NotImplementedException();
        public bool Set(string key, string value) => throw new NotImplementedException();
        public bool Set<T>(string key, T value) where T : class => throw new NotImplementedException();
        public bool Set(string key, object value, TimeSpan? expiration) => throw new NotImplementedException();
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<ISomeInterface, SomeClass>();

        builder.Services.AddMilvaCaching()
                        .WithAccessor<TestCacheAccessor, InMemoryCacheOptions>(new InMemoryCacheOptions
                        {
                            AccessorLifetime = ServiceLifetime.Scoped,
                        });

        builder.Services.AddMilvaInterception([typeof(ISomeInterface)])
                        .WithCacheInterceptor(opt =>
                        {
                            opt.IncludeRequestHeadersWhenCaching = false;
                            opt.CacheAccessorType = typeof(TestCacheAccessor);
                        });

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
