﻿using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

public class CallOrderTests
{
    // These tests will be useful if 'Order' property is added to DecorateAttribute.
    [Fact]
    public void Method_WithAscendingDecorators_ShouldCallInAscendingOrder()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();
        var decorator1 = services.GetService<Decorator1>();
        var decorator2 = services.GetService<Decorator2>();

        // Act
        someClass.AscendingMethod();

        // Assert
        decorator1.CallTime.Should().BeBefore(decorator2.CallTime);
    }

    [Fact]
    public void Method_WithDescendingDecorators_ShouldCallInDescendingOrder()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();
        var decorator1 = services.GetService<Decorator1>();
        var decorator2 = services.GetService<Decorator2>();

        // Act
        someClass.DescendingMethod();

        // Assert
        decorator1.CallTime.Should().BeBefore(decorator2.CallTime);
    }

    #region Setup
    public class Decorator1 : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;

        public DateTime CallTime { get; set; }

        public async Task OnInvoke(Call call)
        {
            CallTime = DateTime.UtcNow;
            await call.NextAsync();
        }
    }

    public class Decorator2 : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 2;

        public DateTime CallTime { get; set; }

        public async Task OnInvoke(Call call)
        {
            CallTime = DateTime.UtcNow;
            await call.NextAsync();
        }
    }

    public class SomeClass : IInterceptable
    {
        [Decorate(typeof(Decorator1))]
        [Decorate(typeof(Decorator2))]
        public virtual void AscendingMethod() { }

        [Decorate(typeof(Decorator2))]
        [Decorate(typeof(Decorator1))]
        public virtual void DescendingMethod() { }
    }

    private IServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<Decorator1>();
        builder.Services.AddScoped<Decorator2>();
        builder.Services.AddTransient<SomeClass>();

        builder.Services.AddMilvaInterception([typeof(SomeClass)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
