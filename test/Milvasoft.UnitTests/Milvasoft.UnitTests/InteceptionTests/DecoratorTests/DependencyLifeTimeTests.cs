using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

public class DependencyLifeTimeTests
{
    [Fact]
    public void I_can_decorate_with_decorator_that_has_scoped_dependency()
    {
        // Arrange
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<Dependency>();
        builder.Services.AddTransient<TestDecoratorWithDependency>();
        builder.Services.AddScoped<ISomeInterface, SomeClass>();

        builder.Services.AddMilvaInterception([typeof(ISomeInterface)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        // Act & Assert
        serviceProvider.GetRequiredService<ISomeInterface>();
    }

    #region Setup
    public class Dependency { }

    public class TestDecoratorWithDependency : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;

        public Dependency Dependency { get; }

        public TestDecoratorWithDependency(Dependency dependency)
        {
            Dependency = dependency;
        }

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
