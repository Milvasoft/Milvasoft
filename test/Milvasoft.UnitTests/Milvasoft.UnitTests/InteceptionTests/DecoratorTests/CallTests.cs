using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

public class CallTests
{
    [Theory, AutoData]
    public void GenericInterfaceMethod_WithDecorator_ShouldHaveFilledCallObject(int expectedReturnValue)
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var someClass = services.GetService<ISomeInterface>();

        // Act
        someClass.GenericMethod(expectedReturnValue);

        // Assert
        decorator.Call.Should().NotBeNull();

        decorator.Call.Method.GetGenericMethodDefinition()
            .Should().BeSameAs(typeof(ISomeInterface).GetMethod(nameof(ISomeInterface.GenericMethod)));

        decorator.Call.MethodImplementation.GetGenericMethodDefinition()
            .Should().BeSameAs(typeof(SomeClass).GetMethod(nameof(SomeClass.GenericMethod)));

        decorator.Call.Object.Should().Be(someClass.UnwrapDecorated());

        decorator.Call.ReturnValue.Should().Be(expectedReturnValue);

        ((int)decorator.Call.Arguments[0]).Should().Be(expectedReturnValue);

        decorator.Call.GenericArguments[0].Should().Be(expectedReturnValue.GetType());
    }

    [Fact]
    public void NonVirtualMethod_WithDecorator_ShouldHaveMethodImplementationSameAsMethod()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var someClass = services.GetService<SomeClass>();

        // Act
        someClass.Method();

        // Assert
        decorator.Call.Method.Should().BeSameAs(decorator.Call.MethodImplementation);
    }

    [Fact]
    public void NonGenericMethod_WithDecorator_ShouldHaveGenericArgumentsEmpty()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var someClass = services.GetService<SomeClass>();

        // Act
        someClass.Method();

        // Assert
        decorator.Call.GenericArguments.Should().BeEmpty();
    }

    [Fact]
    public async Task MethodAsyncWithException_WithDecorator_ShouldThrowException()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var someService = services.GetService<SomeClassThrowingAsyncException>();

        // Act
        await someService.Invoking(x => x.MethodAsyncWithException()).Should().ThrowAsync<Exception>();

        // Assert
        decorator.ExceptionThrown.Should().NotBeNull();
    }

    [Fact]
    public async Task MethodWithException_WithDecorator_ShouldThrowException()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var someService = services.GetService<SomeClassThrowingException>();

        // Act
        await someService.Invoking(x => x.MethodWithException()).Should().ThrowAsync<Exception>();

        // Assert
        decorator.ExceptionThrown.Should().NotBeNull();
    }

    #region Setup
    public class TestDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;
        public Call Call { get; set; }
        public Exception ExceptionThrown { get; set; }

        public async Task OnInvoke(Call call)
        {
            Call = call;
            try
            {
                await call.NextAsync();
            }
            catch (Exception ex)
            {
                ExceptionThrown = ex;
                throw;
            }
        }
    }

    public interface ISomeInterface : IInterceptable
    {
        T GenericMethod<T>(T arg);
    }

    public class SomeClass : ISomeInterface
    {
        [Decorate(typeof(TestDecorator))]
        public T GenericMethod<T>(T arg) => arg;

        [Decorate(typeof(TestDecorator))]
        virtual public void Method() { }
    }

    public class SomeClassThrowingAsyncException : IInterceptable
    {
        [Decorate(typeof(TestDecorator))]
        virtual public async Task MethodAsyncWithException()
        {
            throw new Exception();
        }
    }

    public class SomeClassThrowingException : IInterceptable
    {
        [Decorate(typeof(TestDecorator))]
        virtual public Task MethodWithException()
        {
            throw new Exception();
        }
    }

    private IServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<TestDecorator>();
        builder.Services.AddTransient<ISomeInterface, SomeClass>();
        builder.Services.AddTransient<SomeClassThrowingAsyncException>();
        builder.Services.AddTransient<SomeClassThrowingException>();
        builder.Services.AddTransient<SomeClass>();

        builder.Services.AddMilvaInterception([typeof(ISomeInterface), typeof(SomeClass), typeof(SomeClassThrowingAsyncException), typeof(SomeClassThrowingException)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
