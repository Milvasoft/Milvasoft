using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;
using System.Diagnostics;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

[Trait("Interception Unit Tests", "Unit tests for Milvasoft.Interception project.")]
public class PerformanceTests(ITestOutputHelper output)
{
    public ITestOutputHelper Output { get; } = output;

    /// <summary>
    /// .Net Framework 4.6.1 seems to be a bit slower and needs ~600ms, while .Net Core fits in ~200 ms.
    /// </summary>
    [Fact(DisplayName = "<200 ms on first call.")]
    public void Method_CalledFirstTime_ShouldTakeLessThanSpecifiedDuration()
    {
        // Arrange
        var services = GetServices();
        services.GetService<TestDecorator>(); // Warm up ServiceProvider
        var stopwatch = Stopwatch.StartNew();

        // Act
        services.GetService<ISomeInterface>().Method();

        // Assert
        var actualTime = stopwatch.ElapsedMilliseconds;
        Output.WriteLine($"Initial call took '{actualTime}' ms.");
        actualTime.Should().BeLessThan(200);
    }

    [Fact(DisplayName = "<=2 ms after first call.")]
    public void Method_CalledAfterTheFirstTime_ShouldTakeLessThanSpecifiedDuration()
    {
        // Arrange
        var services = GetServices();
        services.GetService<ISomeInterface>().Method(); // Warm up dynamic proxy
        var stopwatch = Stopwatch.StartNew();

        // Act
        services.GetService<ISomeInterface>().Method();

        // Assert
        var actualTime = stopwatch.ElapsedMilliseconds;
        Output.WriteLine($"Subsequent call took '{actualTime}' ms.");
        actualTime.Should().BeLessThan(3);
    }

    #region Setup
    public class TestDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;
        public int CallCount { get; set; }

        public Task OnInvoke(Call call)
        {
            CallCount++;
            return call.NextAsync();
        }
    }

    public interface ISomeInterface : IInterceptable
    {
        [Decorate(typeof(TestDecorator))]
        void Method();
    }

    public class SomeClass : ISomeInterface
    {
        public void Method() { }
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<TestDecorator>();
        builder.Services.AddTransient<ISomeInterface, SomeClass>();

        builder.Services.AddMilvaInterception([typeof(ISomeInterface)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
