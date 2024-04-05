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

    [Fact]
    public void NotRunningMethod_WithDecorator_ShouldNotThrowException()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<NotProceedDecorator>();
        var someService = services.GetService<SomeClass2>();

        // Act
        someService.NotRunningMethod();

        // Assert
        decorator.Call.ReturnValue.Should().Be(2);
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

    public class NotProceedDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;
        public Call Call { get; set; }
        public Exception ExceptionThrown { get; set; }

        public async Task OnInvoke(Call call)
        {
            call.ReturnValue = 2;
            call.ProceedToOriginalInvocation = false;
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
        public virtual void Method() { }
    }

    public class SomeClassThrowingAsyncException : IInterceptable
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        [Decorate(typeof(TestDecorator))]
        public virtual async Task MethodAsyncWithException() => throw new Exception();
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }

    public class SomeClassThrowingException : IInterceptable
    {
        [Decorate(typeof(TestDecorator))]
        public virtual Task MethodWithException() => throw new Exception();
    }

    public class SomeClass2 : IInterceptable
    {
        [Decorate(typeof(NotProceedDecorator))]
        public virtual int NotRunningMethod() => throw new Exception();
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<TestDecorator>();
        builder.Services.AddScoped<NotProceedDecorator>();
        builder.Services.AddTransient<ISomeInterface, SomeClass>();
        builder.Services.AddTransient<SomeClassThrowingAsyncException>();
        builder.Services.AddTransient<SomeClassThrowingException>();
        builder.Services.AddTransient<SomeClass>();
        builder.Services.AddTransient<SomeClass2>();

        builder.Services.AddMilvaInterception(
        [
            typeof(ISomeInterface),
            typeof(SomeClass),
            typeof(SomeClassThrowingAsyncException),
            typeof(SomeClassThrowingException),
            typeof(SomeClass2)
        ]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
