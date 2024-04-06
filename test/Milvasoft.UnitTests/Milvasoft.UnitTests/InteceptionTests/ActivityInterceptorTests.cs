using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Interceptors.ActivityScope;

namespace Milvasoft.UnitTests.InteceptionTests;

public class ActivityInterceptorTests
{
    [Fact]
    public void Method_WithDecorator_ShouldStartActivity()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<SomeClass>();

        // Act & Assert
        sut.Invoking(x =>
        {
            var result = x.MethodWithActivity();
            result.Should().Be("TestActivity");
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
            var result = x.MethodWithNoActivity();
            result.Should().Be("No Activity");
        }).Should().NotThrow();
    }

    #region Setup

    public class SomeClass : IInterceptable
    {
        [ActivityStarter("TestActivity")]
        public virtual string MethodWithActivity() => ActivityHelper.OperationName;

        public virtual string MethodWithNoActivity() => ActivityHelper.OperationName;
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddTransient<SomeClass>();

        builder.Services.AddMilvaInterception([typeof(SomeClass)])
                        .WithActivityInterceptor();

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
