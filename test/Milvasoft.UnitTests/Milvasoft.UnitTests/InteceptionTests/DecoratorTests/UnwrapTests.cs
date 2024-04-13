using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

[Trait("Interception Unit Tests", "Unit tests for Milvasoft.Interception project.")]
public class UnwrapTests
{
    [Fact]
    public void UnwrapDecorate_WithDecoratedObject_ShouldReturnUnderlyingObject()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<ISomeInterface>();

        // Act
        var actual = sut.UnwrapDecorated();

        // Assert
        actual.GetType().Should().Be(typeof(SomeDecoratedClass));
    }

    [Fact]
    public void UnwrapDecorate_WithNotDecoratedObject_ShouldReturnSameObject()
    {
        // Arrange
        var expected = new SomeNotDecoratedClass();

        // Act
        var actual = expected.UnwrapDecorated();

        // Assert
        actual.Should().Be(expected);
    }

    [Theory, AutoData]
    public void SomeMethod_WithUnwrappedDecorated_ShouldNotCallDecorator(int expected)
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var sut = services.GetService<ISomeInterface>().UnwrapDecorated();

        // Act
        var actual = sut.SomeMethod(expected);

        // Assert
        actual.Should().Be(expected);
        decorator.CallBeforeCount.Should().Be(0);
        decorator.CallAfterCount.Should().Be(0);
    }

    #region Setup
    public class TestDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;
        public int CallBeforeCount { get; set; }
        public int CallAfterCount { get; set; }

        public async Task OnInvoke(Call call)
        {
            CallBeforeCount++;
            await call.NextAsync();
            CallAfterCount++;
        }
    }

    public interface ISomeInterface : IInterceptable
    {
        T SomeMethod<T>(T argument);
    }

    public class SomeDecoratedClass : ISomeInterface
    {
        [Decorate(typeof(TestDecorator))]
        public T SomeMethod<T>(T argument) => argument;
    }

#pragma warning disable S2094 // Classes should not be empty
    public class SomeNotDecoratedClass { }
#pragma warning restore S2094 // Classes should not be empty

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<TestDecorator>();
        builder.Services.AddTransient<ISomeInterface, SomeDecoratedClass>();

        builder.Services.AddMilvaInterception([typeof(ISomeInterface)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
