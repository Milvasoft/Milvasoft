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
using Milvasoft.UnitTests.InteceptionTests.Fixtures;

namespace Milvasoft.UnitTests.InteceptionTests;

[Trait("Interceptors Unit Tests", "Unit tests for Milvasoft.Interception project interceptors.")]
public class CacheInterceptorTests
{
    [Fact]
    public void Method_WithCacheInterceptor_ShouldCacheCorrectlyAndMethodCalledOnce()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<ISomeInterface>();
        var cacheAccessor = services.GetService<ICacheAccessor<TestCacheAccessorFixture>>();

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
        var cacheAccessor = services.GetService<ICacheAccessor<TestCacheAccessorFixture>>();

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
    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<ISomeInterface, SomeClass>();

        builder.Services.AddMilvaCaching()
                        .WithAccessor<TestCacheAccessorFixture, InMemoryCacheOptions>(new InMemoryCacheOptions
                        {
                            AccessorLifetime = ServiceLifetime.Scoped,
                        });

        builder.Services.AddMilvaInterception([typeof(ISomeInterface)])
                        .WithCacheInterceptor(opt =>
                        {
                            opt.InterceptorLifetime = ServiceLifetime.Scoped;
                            opt.IncludeRequestHeadersWhenCaching = false;
                            opt.CacheAccessorType = typeof(TestCacheAccessorFixture);
                        });

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
