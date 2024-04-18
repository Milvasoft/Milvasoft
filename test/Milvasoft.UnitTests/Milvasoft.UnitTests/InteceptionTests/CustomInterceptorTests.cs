using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;
using Milvasoft.UnitTests.InteceptionTests.Fixtures;

namespace Milvasoft.UnitTests.InteceptionTests;

[Trait("Interceptors Unit Tests", "Unit tests for Milvasoft.Interception project interceptors.")]
public class CustomInterceptorTests
{
    [Fact]
    public void Method_WithDecorator_ShouldIntercept()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<SomeClass>();

        // Act & Assert
        sut.Invoking(x =>
        {
            var result = x.MethodWithCustomInterceptor();
            result.Should().Be("intercepted");
        }).Should().NotThrow();
    }

    [Fact]
    public void Method_WithDecoratorAndCustomOptions_ShouldIntercept()
    {
        // Arrange
        var services = GetServicesWithOptions();
        var sut = services.GetService<SomeClass>();

        // Act & Assert
        sut.Invoking(x =>
        {
            var result = x.MethodWithCustomInterceptor();
            result.Should().Be("intercepted with options");
        }).Should().NotThrow();
    }

    [Fact]
    public void Method_WithoutDecorator_ShouldDoNothing()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<SomeClass>();

        // Act & Assert
        sut.Invoking(x =>
        {
            var result = x.MethodWithNoInterceptor();
            result.Should().Be("Not intercepted!");
        }).Should().NotThrow();
    }

    #region Setup

    public class SomeClass : IInterceptable
    {
        [CustomInterceptor]
        public virtual string MethodWithCustomInterceptor() => "Not intercepted!";

        public virtual string MethodWithNoInterceptor() => "Not intercepted!";
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddTransient<SomeClass>();

        builder.Services.AddMilvaInterception([typeof(SomeClass)])
                        .WithInterceptor<CustomInterceptorFixture>();

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    private static ServiceProvider GetServicesWithOptions()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddTransient<SomeClass>();

        builder.Services.AddMilvaInterception([typeof(SomeClass)])
                        .WithInterceptor<CustomInterceptorFixture, ICustomInterceptionOptionsFixture, CustomInterceptionOptionsFixture>(opt =>
                        {
                            opt.InterceptorLifetime = ServiceLifetime.Scoped;
                            opt.SomeOptions = "intercepted with options";
                        });

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
