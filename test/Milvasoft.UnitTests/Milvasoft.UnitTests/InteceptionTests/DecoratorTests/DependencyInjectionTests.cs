﻿using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

[Trait("Interception Unit Tests", "Unit tests for Milvasoft.Interception project.")]
public class DependencyInjectionTests
{
    [Fact]
    public void Method_WithDecoratorWithDependencies_ShouldInjectDependencies()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<SomeClass>();

        // Act
        sut.Method();

        // Assert
        var decorator = services.GetService<TestDecoratorWithDependencies>();
        var expectedDependency = services.GetService<SomeDependency>();
        decorator.CallCount.Should().Be(1);
        decorator.SomeDependency.Should().Be(expectedDependency);
    }

    [Fact]
    public void Method_WithClassWithDependencies_ShouldInjectDependencies()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<ISomeClassWithDependencies>();

        // Act
        sut.Method();

        // Assert
        var decorator = services.GetService<TestDecoratorWithDependencies>();
        var expectedDependency = services.GetService<SomeDependency>();
        decorator.CallCount.Should().Be(1);
        sut.SomeDependency.Should().Be(expectedDependency);
    }

    #region Setup

    public class SomeDependency;

    public class TestDecoratorWithDependencies(SomeDependency someDependency) : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;
        public SomeDependency SomeDependency { get; } = someDependency;
        public int CallCount { get; set; }

        public Task OnInvoke(Call call)
        {
            CallCount++;

            return call.NextAsync();
        }
    }

    public class SomeClass : IInterceptable
    {
        [Decorate(typeof(TestDecoratorWithDependencies))]
        public virtual void Method() { }
    }

    public interface ISomeClassWithDependencies : IInterceptable
    {
        SomeDependency SomeDependency { get; }

        void Method();
    }

    public class SomeClassWithDependencies(SomeDependency someDependency) : ISomeClassWithDependencies
    {
        public SomeDependency SomeDependency { get; } = someDependency;

        [Decorate(typeof(TestDecoratorWithDependencies))]
        public virtual void Method() { }
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<SomeDependency>();
        builder.Services.AddScoped<TestDecoratorWithDependencies>();
        builder.Services.AddTransient<SomeClass>();
        builder.Services.AddTransient<ISomeClassWithDependencies, SomeClassWithDependencies>();

        builder.Services.AddMilvaInterception([typeof(SomeClass), typeof(ISomeClassWithDependencies)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
