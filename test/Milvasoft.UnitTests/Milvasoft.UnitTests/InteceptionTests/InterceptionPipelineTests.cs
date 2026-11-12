using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Caching.Builder;
using Milvasoft.Caching.InMemory;
using Milvasoft.Caching.InMemory.Accessor;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Abstractions.Cache;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Interceptors.ActivityScope;
using Milvasoft.Interception.Interceptors.Cache;
using Milvasoft.Interception.Interceptors.Logging;
using Milvasoft.Interception.Interceptors.Runner;
using Milvasoft.UnitTests.InteceptionTests.Fixtures;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Milvasoft.UnitTests.InteceptionTests;

/// <summary>
/// Regression tests for the interception pipeline: async detection by return type, attribute lookup, registrations, logging and caching behavior.
/// </summary>
[Trait("Interceptors Unit Tests", "Unit tests for Milvasoft.Interception project interceptors.")]
public class InterceptionPipelineTests
{
    #region Async handling

    [Fact]
    public async Task TaskMethodWithoutAsyncKeyword_ShouldBeObservedByInterceptorAfterItsWorkCompletes()
    {
        var services = GetServices();
        var sut = services.GetRequiredService<IAsyncTarget>();
        var sink = services.GetRequiredService<ObservationSink>();

        await sut.NonAsyncTaskMethod();

        sink.Observations.Should().ContainSingle().Which.Should().Be("done:True:");
    }

    [Fact]
    public async Task TaskOfTMethodWithoutAsyncKeyword_ShouldExposeAwaitedResultToInterceptor()
    {
        var services = GetServices();
        var sut = services.GetRequiredService<IAsyncTarget>();
        var sink = services.GetRequiredService<ObservationSink>();

        var result = await sut.NonAsyncTaskOfTMethod();

        result.Should().Be(42);
        sink.Observations.Should().ContainSingle().Which.Should().Be("done:True:42");
    }

    [Fact]
    public async Task ValueTaskMethod_ShouldBeObservedByInterceptorAfterItsWorkCompletes()
    {
        var services = GetServices();
        var sut = services.GetRequiredService<IAsyncTarget>();
        var sink = services.GetRequiredService<ObservationSink>();

        await sut.ValueTaskMethod();

        sink.Observations.Should().ContainSingle().Which.Should().Be("done:True:");
    }

    [Fact]
    public async Task ValueTaskOfTMethod_ShouldExposeAwaitedResultToInterceptor()
    {
        var services = GetServices();
        var sut = services.GetRequiredService<IAsyncTarget>();
        var sink = services.GetRequiredService<ObservationSink>();

        var result = await sut.ValueTaskOfTMethod();

        result.Should().Be(7);
        sink.Observations.Should().ContainSingle().Which.Should().Be("done:True:7");
    }

    [Fact]
    public async Task FaultedTaskMethodWithoutAsyncKeyword_ShouldPropagateExceptionThroughInterceptor()
    {
        var services = GetServices();
        var sut = services.GetRequiredService<IAsyncTarget>();
        var sink = services.GetRequiredService<ObservationSink>();

        var act = () => sut.FailingNonAsyncTaskMethod();

        await act.Should().ThrowAsync<InvalidOperationException>();
        sink.Observations.Should().ContainSingle().Which.Should().Be("error:InvalidOperationException");
    }

    [Fact]
    public void SyncValueTypeMethod_WhenInterceptorSkipsInvocation_ShouldReturnDefault()
    {
        var services = GetServices();
        var sut = services.GetRequiredService<IAsyncTarget>();

        sut.SkippedSyncMethod().Should().Be(0);
    }

    #endregion

    #region Attributes and registrations

    [Fact]
    public void AttributeOnInterfaceMethodOnly_ShouldBeFoundByInterceptor()
    {
        var services = new ServiceCollection();
        services.AddScoped<IInterfaceAttributeTarget, InterfaceAttributeTarget>();
        services.AddMilvaInterception([typeof(IInterfaceAttributeTarget)]).WithActivityInterceptor();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IInterfaceAttributeTarget>().CurrentActivityName().Should().Be("InterfaceActivity");
    }

    [Fact]
    public void KeyedService_ShouldBeIntercepted()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ObservationSink>();
        services.AddKeyedScoped<IAsyncTarget, AsyncTarget>("key");
        services.AddMilvaInterception([typeof(IAsyncTarget)]).WithInterceptor<ObservingInterceptor>().WithInterceptor<SkippingInterceptor>();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredKeyedService<IAsyncTarget>("key").SyncMethod().Should().Be(5);
        provider.GetRequiredService<ObservationSink>().Observations.Should().ContainSingle().Which.Should().Be("done:False:5");
    }

    [Fact]
    public void AddMilvaInterception_WithUnrelatedKeyedRegistrations_ShouldNotThrow()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<object>("unrelated", (_, _) => new object());
        services.AddKeyedScoped<IDisposable, MemoryStream>("stream");
        services.AddScoped<IAsyncTarget, AsyncTarget>();

        var act = () => services.AddMilvaInterception([typeof(IAsyncTarget)]);

        act.Should().NotThrow();
    }

    [Fact]
    public void InterceptingTwice_ShouldNotRunInterceptorsTwice()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ObservationSink>();
        services.AddScoped<IAsyncTarget, AsyncTarget>();
        services.AddMilvaInterception([typeof(IAsyncTarget)])
                .WithInterceptor<ObservingInterceptor>()
                .WithInterceptor<SkippingInterceptor>()
                .Intercept<IAsyncTarget>();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IAsyncTarget>().SyncMethod();

        provider.GetRequiredService<ObservationSink>().Observations.Should().HaveCount(1);
    }

    [Fact]
    public void DisposableImplementation_ShouldBeDisposedWithTheScope()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ObservationSink>();
        services.AddScoped<IDisposableTarget, DisposableTarget>();
        services.AddMilvaInterception([typeof(IDisposableTarget)]).WithInterceptor<ObservingInterceptor>();

        using var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IDisposableTarget>().Method();
        }

        provider.GetRequiredService<ObservationSink>().Observations.Should().Contain("disposed");
    }

    [Fact]
    public void SingletonService_WithScopedInterceptor_ShouldResolveInterceptorPerCallInsteadOfCapturingIt()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ObservationSink>();
        services.AddSingleton<IAsyncTarget, AsyncTarget>();
        services.AddMilvaInterception([typeof(IAsyncTarget)]).WithInterceptor<ObservingInterceptor>(ServiceLifetime.Scoped).WithInterceptor<SkippingInterceptor>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        var sut = provider.GetRequiredService<IAsyncTarget>();

        sut.SyncMethod();
        sut.SyncMethod();

        var sink = provider.GetRequiredService<ObservationSink>();
        sink.Observations.Should().HaveCount(2);
        sink.InterceptorInstances.Distinct().Should().HaveCount(2);
    }

    #endregion

    #region Logging

    [Fact]
    public void InterceptorRunner_WithCapturedVariableArgument_ShouldLogArgumentValueAndReturnResult()
    {
        var services = GetLogServices();
        var runner = services.GetRequiredService<IInterceptorRunner>();
        var logger = services.GetRequiredService<RecordingLoggerFixture>();
        var target = new RunnerTarget();
        var value = 7;

        var result = runner.InterceptWithLog(() => target.Echo(value));

        result.Should().Be(7);
        logger.Last.GetProperty("MethodName").GetString().Should().Be(nameof(RunnerTarget.Echo));
        logger.Last.GetProperty("MethodParams").GetString().Should().Be("[7]");
    }

    [Fact]
    public void InterceptorRunner_WithNonMethodCallBody_ShouldNotFail()
    {
        var services = GetLogServices();
        var runner = services.GetRequiredService<IInterceptorRunner>();
        var target = new RunnerTarget();

        var result = runner.InterceptWithLog(() => target.Value);

        result.Should().Be(3);
    }

    [Fact]
    public void LoggerFailure_ShouldNotChangeTheResultOfTheCall()
    {
        var services = GetLogServices();
        var logger = services.GetRequiredService<RecordingLoggerFixture>();
        logger.ThrowOnLog = true;
        var sut = services.GetRequiredService<ILoggedTarget>();

        sut.Add(2, 3).Should().Be(5);
    }

    [Fact]
    public async Task LoggerFailure_ShouldNotReplaceTheExceptionOfTheCall()
    {
        var services = GetLogServices();
        services.GetRequiredService<RecordingLoggerFixture>().ThrowOnLog = true;
        var sut = services.GetRequiredService<ILoggedTarget>();

        var act = () => sut.FailAsync();

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void ExtraLoggingProperty_WithSameNameAsDefaultProperty_ShouldNotFailAndShouldOverrideIt()
    {
        var services = GetLogServices(opt => opt.ExtraLoggingPropertiesSelector = _ => new { TransactionId = "custom" });
        var logger = services.GetRequiredService<RecordingLoggerFixture>();
        var sut = services.GetRequiredService<ILoggedTarget>();

        sut.Add(1, 1).Should().Be(2);
        logger.Last.GetProperty("TransactionId").GetString().Should().Be("custom");
    }

    [Fact]
    public void QueryableResult_ShouldNotBeEnumeratedByLogging()
    {
        var services = GetLogServices();
        var logger = services.GetRequiredService<RecordingLoggerFixture>();
        var sut = services.GetRequiredService<ILoggedTarget>();

        var query = sut.Query();

        sut.EnumerationCount.Should().Be(0);
        logger.Last.GetProperty("MethodResult").GetString().Should().Contain("not serialized");
        query.Should().HaveCount(2);
    }

    [Fact]
    public void LogMasked_ShouldMaskNonStringPropertiesInheritedPropertiesAndParameters()
    {
        var services = GetLogServices();
        var logger = services.GetRequiredService<RecordingLoggerFixture>();
        var sut = services.GetRequiredService<ILoggedTarget>();

        var result = sut.Login(new DerivedCredentials { UserName = "john", Pin = 4815162342, Secret = "secret-value" }, "p@ssw0rd");

        result.Pin.Should().Be(4815162342);
        var entry = logger.Entries.Last();
        entry.Should().Contain("john");
        entry.Should().NotContain("4815162342");
        entry.Should().NotContain("secret-value");
        entry.Should().NotContain("p@ssw0rd");
    }

    #endregion

    #region Cache

    [Fact]
    public void CacheInterceptionOptions_ShouldNotShareAccessorTypeBetweenInstances()
    {
        var first = new CacheInterceptionOptions { CacheAccessorType = typeof(MemoryCacheAccessor) };
        var second = new CacheInterceptionOptions();

        second.CacheAccessorType.Should().BeNull();
        second.GetAccessorType().Should().BeNull();
        first.GetAccessorType().Should().Be(typeof(ICacheAccessor<MemoryCacheAccessor>));
    }

    [Fact]
    public void CacheInterceptionOptions_AssemblyQualifiedName_ShouldResolveGenericAccessorType()
    {
        var options = new CacheInterceptionOptions { CacheAccessorAssemblyQualifiedName = typeof(MemoryCacheAccessor).AssemblyQualifiedName };

        options.CacheAccessorType.Should().Be(typeof(MemoryCacheAccessor));
        options.GetAccessorType().Should().Be(typeof(ICacheAccessor<MemoryCacheAccessor>));
    }

    [Fact]
    public void CacheKey_ShouldNotCollideBetweenMethodsWithTheSameNameInDifferentClasses()
    {
        var services = new ServiceCollection();
        services.AddMilvaCaching().WithInMemoryAccessor(cachingOptions: null);
        services.AddScoped<ICachedTargetA, CachedTargetA>();
        services.AddScoped<ICachedTargetB, CachedTargetB>();
        services.AddMilvaInterception([typeof(ICachedTargetA), typeof(ICachedTargetB)])
                .WithCacheInterceptor(opt => opt.CacheAccessorType = typeof(MemoryCacheAccessor));

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<ICachedTargetA>().Get().Should().Be("A");
        provider.GetRequiredService<ICachedTargetB>().Get().Should().Be("B");
    }

    #endregion

    #region Setup

    private static ServiceProvider GetServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ObservationSink>();
        services.AddScoped<IAsyncTarget, AsyncTarget>();
        services.AddMilvaInterception([typeof(IAsyncTarget)])
                .WithInterceptor<ObservingInterceptor>()
                .WithInterceptor<SkippingInterceptor>();

        return services.BuildServiceProvider();
    }

    private static ServiceProvider GetLogServices(Action<ILogInterceptionOptions> configure = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<RecordingLoggerFixture>();
        services.AddSingleton<IMilvaLogger>(sp => sp.GetRequiredService<RecordingLoggerFixture>());
        services.AddScoped<ILoggedTarget, LoggedTarget>();
        services.AddMilvaInterception([typeof(ILoggedTarget)])
                .WithLogInterceptor(opt =>
                {
                    opt.AsyncLogging = false;
                    configure?.Invoke(opt);
                });

        return services.BuildServiceProvider();
    }

    public class ObservationSink
    {
        public ConcurrentQueue<string> Observations { get; } = new();
        public ConcurrentQueue<object> InterceptorInstances { get; } = new();
    }

    public class ObserveAttribute() : DecorateAttribute(typeof(ObservingInterceptor));

    public class SkipAttribute() : DecorateAttribute(typeof(SkippingInterceptor));

    public class ObservingInterceptor(ObservationSink sink) : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; }

        public async Task OnInvoke(Call call)
        {
            sink.InterceptorInstances.Enqueue(this);

            try
            {
                await call.NextAsync();

                var completed = call.Object is IAsyncTarget target && target.Completed;

                sink.Observations.Enqueue($"done:{completed}:{call.ReturnValue}");
            }
            catch (Exception ex)
            {
                sink.Observations.Enqueue($"error:{ex.GetType().Name}");
                throw;
            }
        }
    }

    public class SkippingInterceptor : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; }

        public Task OnInvoke(Call call)
        {
            call.ProceedToOriginalInvocation = false;
            return call.NextAsync();
        }
    }

    public interface IAsyncTarget : IInterceptable
    {
        bool Completed { get; }
        Task NonAsyncTaskMethod();
        Task<int> NonAsyncTaskOfTMethod();
        ValueTask ValueTaskMethod();
        ValueTask<int> ValueTaskOfTMethod();
        Task FailingNonAsyncTaskMethod();
        int SyncMethod();
        int SkippedSyncMethod();
    }

    public class AsyncTarget : IAsyncTarget
    {
        public bool Completed { get; private set; }

        [Observe]
        public Task NonAsyncTaskMethod() => Task.Delay(30).ContinueWith(_ => Completed = true, TaskScheduler.Default);

        [Observe]
        public Task<int> NonAsyncTaskOfTMethod() => Task.Delay(30).ContinueWith(_ =>
        {
            Completed = true;
            return 42;
        }, TaskScheduler.Default);

        [Observe]
        public async ValueTask ValueTaskMethod()
        {
            await Task.Delay(30);
            Completed = true;
        }

        [Observe]
        public ValueTask<int> ValueTaskOfTMethod() => new(Task.Delay(30).ContinueWith(_ =>
        {
            Completed = true;
            return 7;
        }, TaskScheduler.Default));

        [Observe]
        public Task FailingNonAsyncTaskMethod() => Task.Delay(10).ContinueWith(_ => throw new InvalidOperationException(), TaskScheduler.Default);

        [Observe]
        public int SyncMethod() => 5;

        [Skip]
        public int SkippedSyncMethod() => 99;
    }

    public interface IInterfaceAttributeTarget : IInterceptable
    {
        [ActivityStarter("InterfaceActivity")]
        string CurrentActivityName();
    }

    public class InterfaceAttributeTarget : IInterfaceAttributeTarget
    {
        public string CurrentActivityName() => Activity.Current?.OperationName;
    }

    public interface IDisposableTarget : IInterceptable
    {
        void Method();
    }

    public class DisposableTarget(ObservationSink sink) : IDisposableTarget, IDisposable
    {
        [Observe]
        public void Method() { }

        public void Dispose()
        {
            sink.Observations.Enqueue("disposed");
            GC.SuppressFinalize(this);
        }
    }

    public class RunnerTarget
    {
        public int Value => 3;

        public int Echo(int value) => value;
    }

    public class Credentials
    {
        public string UserName { get; set; }

        [LogMasked]
        public long Pin { get; set; }

        [LogMasked]
        public virtual string Secret { get; set; }
    }

    public class DerivedCredentials : Credentials
    {
        public override string Secret { get; set; }
    }

    public interface ILoggedTarget : IInterceptable
    {
        int EnumerationCount { get; }
        int Add(int a, int b);
        Task FailAsync();
        IQueryable<int> Query();
        Credentials Login(Credentials credentials, [LogMasked] string password);
    }

    public class LoggedTarget : ILoggedTarget
    {
        public int EnumerationCount { get; private set; }

        [Log]
        public int Add(int a, int b) => a + b;

        [Log]
        public Task FailAsync() => Task.FromException(new ArgumentException("failed"));

        [Log]
        public IQueryable<int> Query() => Enumerate().AsQueryable();

        [Log]
        public Credentials Login(Credentials credentials, [LogMasked] string password) => credentials;

        private IEnumerable<int> Enumerate()
        {
            EnumerationCount++;
            yield return 1;
            yield return 2;
        }
    }

    public interface ICachedTargetA : IInterceptable
    {
        string Get();
    }

    public interface ICachedTargetB : IInterceptable
    {
        string Get();
    }

    public class CachedTargetA : ICachedTargetA
    {
        [Cache(null)]
        public string Get() => "A";
    }

    public class CachedTargetB : ICachedTargetB
    {
        [Cache(null)]
        public string Get() => "B";
    }

    #endregion
}
