using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

[Trait("Interception Unit Tests", "Unit tests for Milvasoft.Interception project.")]
public class SingleDecoratorInstanceTests
{
    [Fact]
    public void Method_CalledFirstTime_ShouldTakeLessThan300Ms()
    {
        // Arrange
        var services = GetServices();
        var decoratedObject = services.GetService<ISomeInterface>();

        // Act
        var actual1 = decoratedObject.Method1();
        var actual2 = decoratedObject.Method2();

        // Assert
        actual1.Should().BeEquivalentTo(actual2, options => options.RespectingRuntimeTypes());
    }

    #region Setup
    public class TestDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;

        public async Task OnInvoke(Call call)
        {
            await call.NextAsync();
            call.ReturnValue = this;
        }
    }

    public interface ISomeInterface : IInterceptable
    {
        [Decorate(typeof(TestDecorator))]
        IMilvaInterceptor Method1();

        [Decorate(typeof(TestDecorator))]
        IMilvaInterceptor Method2();
    }

    public class SomeClass : ISomeInterface
    {
        public IMilvaInterceptor Method1() => default;
        public IMilvaInterceptor Method2() => default;
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddTransient<TestDecorator>();
        builder.Services.AddTransient<ISomeInterface, SomeClass>();

        builder.Services.AddMilvaInterception([typeof(ISomeInterface)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
