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
using Milvasoft.UnitTests.InteceptionTests.Fixtures;
using System.Text.Json;

namespace Milvasoft.UnitTests.InteceptionTests;

[Trait("Interceptors Unit Tests", "Unit tests for Milvasoft.Interception project interceptors.")]
public class MilvaInterceptorCombinatonTests
{
    [Fact]
    public void MethodReturnTypeIsIResponse_WithCacheAndActivityStarterAndLogInterceptor_ShouldCacheCorrectlyAndMethodCalledOnceAndResponseCachedFlagIsTrue()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<ISomeInterface>();
        var cacheAccessor = services.GetService<ICacheAccessor<TestCacheAccessorFixture>>();

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
        public void Debug(string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Debug(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Error(string message) => throw new NotImplementedException();
        public void Error(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Error(string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Error(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Fatal(string message) => throw new NotImplementedException();
        public void Fatal(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Fatal(string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Information(string message) => throw new NotImplementedException();
        public void Information(string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Information(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Information(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Verbose(string message) => throw new NotImplementedException();
        public void Verbose(string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Verbose(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Warning(string message) => throw new NotImplementedException();
        public void Warning(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Warning(string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Warning(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddSingleton<ISomeInterface, SomeClass>();
        builder.Services.AddSingleton<IMilvaLogger, TestLogger>();

        builder.Services.AddMilvaCaching()
                        .WithAccessor<TestCacheAccessorFixture, InMemoryCacheOptions>(new InMemoryCacheOptions
                        {
                            AccessorLifetime = ServiceLifetime.Singleton,
                        });

        builder.Services.AddMilvaInterception([typeof(ISomeInterface)])
                        .WithActivityInterceptor()
                        .WithLogInterceptor(opt =>
                        {
                            opt.InterceptorLifetime = ServiceLifetime.Singleton;
                            opt.AsyncLogging = false;
                            opt.ExcludeResponseMetadataFromLog = true;
                        })
                        .WithCacheInterceptor(opt =>
                        {
                            opt.InterceptorLifetime = ServiceLifetime.Singleton;
                            opt.IncludeRequestHeadersWhenCaching = false;
                            opt.CacheAccessorType = typeof(TestCacheAccessorFixture);
                        })
                        .WithResponseInterceptor();

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
