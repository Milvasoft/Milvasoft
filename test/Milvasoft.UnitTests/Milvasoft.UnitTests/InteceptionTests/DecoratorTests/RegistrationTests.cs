using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

public class RegistrationTests
{
    [Fact]
    public void Decorate_TypeDescriptor_ShouldBeDecorated()
    {
        // Arrange
        var services = GetTypeServices();
        var decorator = services.GetService<TestDecorator>();
        var someClass = services.GetService<SomeClass>();

        // Act
        someClass.Method();

        // Assert
        decorator.CallCount.Should().Be(1);
    }

    [Fact]
    public void Decorate_InstanceDescriptor_ShouldBeDecorated()
    {
        // Arrange
        var services = GetInstanceServices();
        var decorator = services.GetService<TestDecorator>();
        var someClass = services.GetService<SomeClass>();

        // Act
        someClass.Method();

        // Assert
        decorator.CallCount.Should().Be(1);
    }

    [Fact]
    public void Decorate_FactoryDescriptor_ShouldBeDecorated()
    {
        // Arrange
        var services = GetFactoryServices();
        var decorator = services.GetService<TestDecorator>();
        var someClass = services.GetService<SomeClass>();

        // Act
        someClass.Method();

        // Assert
        decorator.CallCount.Should().Be(1);
    }

    #region Setup
    public class TestDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;
        public int CallCount { get; set; }

        public async Task OnInvoke(Call call)
        {
            CallCount++;
            await call.NextAsync();
        }
    }

    public interface ISomeInterface : IInterceptable
    {
        void Method();
    }

    public class SomeClass : ISomeInterface
    {
        [Decorate(typeof(TestDecorator))]
        public virtual void Method() { }
    }

    private static ServiceProvider GetTypeServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<TestDecorator>();
        builder.Services.AddTransient<SomeClass>();
        builder.Services.AddTransient<ISomeInterface, SomeClass>();

        builder.Services.AddMilvaInterception([typeof(ISomeInterface), typeof(SomeClass)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    private static ServiceProvider GetInstanceServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        var instance = new SomeClass();
        builder.Services.AddScoped<TestDecorator>();
        builder.Services.AddTransient<SomeClass>();
        builder.Services.AddSingleton(instance);
        builder.Services.AddTransient<ISomeInterface, SomeClass>();

        builder.Services.AddMilvaInterception([typeof(ISomeInterface), typeof(SomeClass)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    private static ServiceProvider GetFactoryServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<TestDecorator>();
        builder.Services.AddTransient<ISomeInterface>(_ => new SomeClass());
        builder.Services.AddTransient(_ => new SomeClass());

        builder.Services.AddMilvaInterception([typeof(ISomeInterface), typeof(SomeClass)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
