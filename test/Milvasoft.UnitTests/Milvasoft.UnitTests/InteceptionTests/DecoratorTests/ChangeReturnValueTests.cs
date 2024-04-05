using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

public class ChangeReturnValueTests
{
    [Theory, AutoData]
    public void SyncMethod_WithDecorator_ShouldOverrideReturnValue(int expectedValue)
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        decorator.ExpectedReturnValue = expectedValue;
        var someClass = services.GetService<SomeClass>();

        // Act
        var actual = someClass.SyncMethod();

        // Assert
        actual.Should().Be(expectedValue);
    }

    [Theory, AutoData]
    public async Task AsyncMethod_WithDecorator_ShouldOverrideReturnValue(int expectedValue)
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        decorator.ExpectedReturnValue = expectedValue;
        var someClass = services.GetService<SomeClass>();

        // Act
        var actual = await someClass.AsyncMethod();

        // Assert
        actual.Should().Be(expectedValue);
    }

    [Theory, AutoData]
    public void GenericMethod_WithDecorator_ShouldOverrideReturnValue(int expectedValue)
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        decorator.ExpectedReturnValue = expectedValue;
        var someClass = services.GetService<SomeClass>();

        // Act
        var actual = someClass.GenericMethod<int>();

        // Assert
        actual.Should().Be(expectedValue);
    }

    #region Setup
    public class TestDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;
        public int ExpectedReturnValue { get; set; }

        public async Task OnInvoke(Call call)
        {
            await call.NextAsync();
            call.ReturnValue = ExpectedReturnValue;
        }
    }

    public class SomeClass : IInterceptable
    {
        [Decorate(typeof(TestDecorator))]
        public virtual int SyncMethod() => default;

        [Decorate(typeof(TestDecorator))]
        public virtual async Task<int> AsyncMethod()
        {
            await Task.Delay(100);

            return default;
        }

        [Decorate(typeof(TestDecorator))]
        public virtual T GenericMethod<T>() => default;
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
