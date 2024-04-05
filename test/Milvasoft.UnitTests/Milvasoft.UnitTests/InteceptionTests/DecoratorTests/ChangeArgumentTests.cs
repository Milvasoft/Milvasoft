using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

public class ChangeArgumentTests
{
    [Theory, AutoData]
    public void GenericMethod_WithDecorator_ShouldOverrideReturnValue(int expectedValue)
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        decorator.ExpectedArgumentValue = expectedValue;
        var someClass = services.GetService<SomeClass>();

        // Act
        var actual = someClass.Method(0); // "0" will be overridden by the decorator.

        // Assert
        actual.Should().Be(expectedValue);
    }

    #region Setup
    public class TestDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;
        public int ExpectedArgumentValue { get; set; }

        public async Task OnInvoke(Call call)
        {
            call.Arguments[0] = ExpectedArgumentValue;
            await call.NextAsync();
        }
    }

    public class SomeClass : IInterceptable
    {
        [Decorate(typeof(TestDecorator))]
        public virtual int Method(int value) => value;
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
