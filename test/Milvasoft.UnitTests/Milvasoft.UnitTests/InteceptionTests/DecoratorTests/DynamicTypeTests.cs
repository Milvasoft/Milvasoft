using AutoFixture.Xunit3;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

[Trait("Interception Unit Tests", "Unit tests for Milvasoft.Interception project.")]
public class DynamicTypeTests
{
    [Fact]
    public async Task SomeMethod_WithDynamicSut_ShouldCallDecorator()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        dynamic sut = services.GetService<ISomeInterface>();

        // Act
        await sut.SomeMethod();

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(1);
    }

    [Theory, AutoData]
    public async Task SomeMethod_WithDynamicSutAndParameter_ShouldCallDecorator(int expected)
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        dynamic sut = services.GetService<ISomeInterface>();

        // Act
        var actual = await sut.SomeMethodWithParameterAndResult((dynamic)expected);

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(1);
        ((int)actual).Should().Be(expected);
    }

    #region Setup
    public class TestDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;
        public int CallCountBefore { get; set; }
        public int CallCountAfter { get; set; }

        public async Task OnInvoke(Call call)
        {
            CallCountBefore++;
            await call.NextAsync();
            CallCountAfter++;
        }
    }

    public interface ISomeInterface : IInterceptable
    {
        Task SomeMethod();
        Task<int> SomeMethodWithParameterAndResult(int argument);
    }

    public class SomeClass : ISomeInterface
    {
        [Decorate(typeof(TestDecorator))]
        public async Task SomeMethod() => await Task.Yield();

        [Decorate(typeof(TestDecorator))]
        public async Task<int> SomeMethodWithParameterAndResult(int argument)
        {
            await Task.Yield();
            return argument;
        }
    }
    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<TestDecorator>();
        builder.Services.AddTransient<ISomeInterface, SomeClass>();

        builder.Services.AddMilvaInterception([typeof(ISomeInterface)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
