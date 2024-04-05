using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

public class CatchExceptionTests
{
    [Fact]
    public void Method_WithExceptionCatchingDecorator_ShouldCatchException()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var someClass = services.GetService<SomeClass>();

        // Act
        someClass.ThrowingMethod();

        // Assert
        decorator.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task MethodAsync_WithExceptionCatchingDecorator_ShouldCatchException()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var someClass = services.GetService<SomeClass>();

        // Act
        await someClass.ThrowingMethodAsync();

        // Assert
        decorator.Exception.Should().NotBeNull();
    }

    #region Setup
    public class TestDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;
        public Exception Exception { get; set; }

        public async Task OnInvoke(Call call)
        {
            try
            {
                await call.NextAsync();
            }
            catch (Exception e)
            {
                Exception = e;
            }
        }
    }

    public class SomeClass : IInterceptable
    {
        [Decorate(typeof(TestDecorator))]
        public virtual void ThrowingMethod() => throw new Exception();

        [Decorate(typeof(TestDecorator))]
        public virtual async Task ThrowingMethodAsync()
        {
            await Task.Delay(100);

            throw new Exception();
        }
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<TestDecorator>();
        builder.Services.AddTransient<SomeClass>();

        builder.Services.AddMilvaInterception([typeof(SomeClass)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
