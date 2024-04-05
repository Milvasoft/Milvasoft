using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

public class DependencyLifeTimeTests
{
    [Fact]
    public void AddMilvaInterception_CanDecorate_WithDecoratorThatHasScopedDependency()
    {
        // Arrange
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<Dependency>();
        builder.Services.AddTransient<TestDecoratorWithDependency>();
        builder.Services.AddScoped<ISomeInterface, SomeClass>();

        builder.Services.AddMilvaInterception([typeof(ISomeInterface)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        // Act
        var service = serviceProvider.GetService<ISomeInterface>();

        // Assert
        service.Should().NotBeNull();
    }

    #region Setup

#pragma warning disable S2094 // Classes should not be empty
    public class Dependency { }
#pragma warning restore S2094 // Classes should not be empty

    public class TestDecoratorWithDependency(Dependency dependency) : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;
        public Dependency Dependency { get; } = dependency;

        public Task OnInvoke(Call call) => Task.CompletedTask;
    }

    public interface ISomeInterface : IInterceptable
    {
        void SomeMethod();
    }

    public class SomeClass : ISomeInterface
    {
        [Decorate(typeof(TestDecoratorWithDependency))]
        public void SomeMethod() { }
    }

    #endregion
}
