using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

[Trait("Interception Unit Tests", "Unit tests for Milvasoft.Interception project.")]
public class CallMethodMultipleTimesTests
{
    [Fact]
    public void Method_WithRetryDecorator_ShouldCallTargetTwice()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();
        var followingDecorator = services.GetService<CountingDecorator>();

        // Act
        someClass.Method();

        // Assert
        someClass.UnwrapDecorated().CallCount.Should().Be(2);
        followingDecorator.CallCount.Should().Be(2);
    }

    #region Setup

    public class TestDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = int.MaxValue;

        public async Task OnInvoke(Call call)
        {
            try
            {
                await call.NextAsync();
            }
            catch (Exception)
            {
                try
                {
                    await call.NextAsync();
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }
        }
    }

    public class CountingDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = int.MaxValue;
        public int CallCount { get; set; }

        public Task OnInvoke(Call call)
        {
            CallCount++;
            return call.NextAsync();
        }
    }

    public class SomeClass : IInterceptable
    {
        public int CallCount { get; set; }

        [Decorate(typeof(TestDecorator))]
        [Decorate(typeof(CountingDecorator))]
        public virtual void Method()
        {
            CallCount++;
            throw new Exception();
        }
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<TestDecorator>();
        builder.Services.AddScoped<CountingDecorator>();
        builder.Services.AddTransient<SomeClass>();

        builder.Services.AddMilvaInterception([typeof(SomeClass)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
