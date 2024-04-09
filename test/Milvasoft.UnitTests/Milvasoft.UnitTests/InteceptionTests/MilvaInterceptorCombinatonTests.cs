using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Caching.Builder;
using Milvasoft.Caching.InMemory.Options;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Abstractions.Cache;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Interceptors.ActivityScope;
using Milvasoft.Interception.Interceptors.Cache;
using Milvasoft.Interception.Interceptors.Logging;
using System.Text.Json;

namespace Milvasoft.UnitTests.InteceptionTests;

public class MilvaInterceptorCombinatonTests
{
    [Fact]
    public void MethodReturnTypeIsIResponse_WithCacheAndActivityStarterAndLogInterceptor_ShouldCacheCorrectlyAndMethodCalledOnceAndResponseCachedFlagIsTrue()
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
        var firstCallResult = sut.Method();

        var logExistsWithActivityId = TestLogger.Logs.TryGetValue(firstCallResult.Data, out var logEntity);

        logExistsWithActivityId.Should().BeTrue();
        logEntity.IsSuccess.Should().BeFalse();
        logEntity.CacheInfo.Should().Contain("false");

        firstCallResult.IsCachedData.Should().BeFalse();
        cachedValue = cacheAccessor.Get<Response<string>>("");

        // method result should be cached
        cachedValue.Should().Be(firstCallResult);

        // call the method again
        var secondCallResult = sut.Method();

        logEntity = TestLogger.Logs.Last().Value;

        logEntity.IsSuccess.Should().BeFalse();
        logEntity.CacheInfo.Should().Contain("true");
        logEntity.MethodResult.Should().Contain("\"Metadatas\":null");

        sut.CallCount.Should().Be(1);
        secondCallResult.IsCachedData.Should().BeTrue();

    }

    #region Setup

    public interface ISomeInterface : IInterceptable
    {
        public int CallCount { get; set; }
        Response<string> Method();
    }

    public class SomeClass : ISomeInterface
    {
        public int CallCount { get; set; }

        [Cache("Test")]
        [ActivityStarter]
        [Log]
        public virtual Response<string> Method()
        {
            CallCount++;
            return Response<string>.Error(ActivityHelper.TraceId);
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

    public class TestLogEntity
    {
        public string TransactionId { get; set; }
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public string MethodParams { get; set; }
        public string MethodResult { get; set; }
        public int ElapsedMs { get; set; }
        public DateTime UtcLogTime { get; set; }
        public string CacheInfo { get; set; }
        public string Exception { get; set; }
        public bool IsSuccess { get; set; }
        public string ExtraProp { get; set; }
    }

    public class TestLogger : IMilvaLogger
    {
        public static Dictionary<string, TestLogEntity> Logs { get; set; } = [];

        public void Log(string logEntry)
        {
            var logObject = JsonSerializer.Deserialize<TestLogEntity>(logEntry);

            Logs.Add(logObject.TransactionId, logObject);
        }

        public Task LogAsync(string logEntry) => throw new NotImplementedException();
        public void Debug(string message) => throw new NotImplementedException();
        public void Debug(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Debug(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Error(string message) => throw new NotImplementedException();
        public void Error(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Error(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Fatal(string message) => throw new NotImplementedException();
        public void Fatal(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Information(string message) => throw new NotImplementedException();
        public void Information(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Information(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Verbose(string message) => throw new NotImplementedException();
        public void Verbose(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Warning(string message) => throw new NotImplementedException();
        public void Warning(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Warning(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<ISomeInterface, SomeClass>();
        builder.Services.AddSingleton<IMilvaLogger, TestLogger>();

        builder.Services.AddMilvaCaching()
                        .WithAccessor<TestCacheAccessor, InMemoryCacheOptions>(new InMemoryCacheOptions
                        {
                            AccessorLifetime = ServiceLifetime.Singleton,
                        });

        builder.Services.AddMilvaInterception([typeof(ISomeInterface)])
                        .WithActivityInterceptor()
                        .WithLogInterceptor(opt =>
                        {
                            opt.AsyncLogging = false;
                            opt.ExcludeResponseMetadataFromLog = true;
                        })
                        .WithCacheInterceptor(opt =>
                        {
                            opt.IncludeRequestHeadersWhenCaching = false;
                            opt.CacheAccessorType = typeof(TestCacheAccessor);
                        })
                        .WithResponseInterceptor();

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
