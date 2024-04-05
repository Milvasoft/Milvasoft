using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

public class AttributeLocationTests
{
    [Fact]
    public void Method_DecoratedInInterface_ShouldCallDecoratorOnce()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var decoratedClass = services.GetService<ISomeInterface>();

        // Act
        decoratedClass.DecoratedInInterface();

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(1);
    }

    [Fact]
    public void Method_DecoratedInClass_ShouldCallDecoratorOnce()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var decoratedClass = services.GetService<ISomeInterface>();

        // Act
        decoratedClass.DecoratedInClass();

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(1);
    }

    [Fact]
    public void Method_DecoratedInInterfaceAndClassWithSame_ShouldCallDecoratorOnce()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var decoratedClass = services.GetService<ISomeInterface>();

        // Act
        decoratedClass.DecoratedInInterfaceAndClassWithSame();

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(1);
    }

    [Fact]
    public void Method_DecoratedInInterfaceAndClassWithDifferent_ShouldCallEachDecoratorOnce()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var anotherDecorator = services.GetService<AnotherTestDecorator>();
        var decoratedClass = services.GetService<ISomeInterface>();

        // Act
        decoratedClass.DecoratedInInterfaceAndClassWithDifferent();

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(1);
        anotherDecorator.CallCountBefore.Should().Be(1);
        anotherDecorator.CallCountAfter.Should().Be(1);
    }

    [Fact]
    public void Method_WithNoAttribute_ShouldNotCallDecorMethods()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var anotherDecorator = services.GetService<AnotherTestDecorator>();
        var sut = services.GetService<ISomeInterface>();

        // Act
        sut.NotDecoratedMethod();

        // Assert
        decorator.CallCountBefore.Should().Be(0);
        decorator.CallCountAfter.Should().Be(0);
        anotherDecorator.CallCountBefore.Should().Be(0);
        anotherDecorator.CallCountAfter.Should().Be(0);
    }

    #region Setup
    public class TestDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 0;
        public int CallCountBefore { get; set; }
        public int CallCountAfter { get; set; }

        public async Task OnInvoke(Call call)
        {
            CallCountBefore++;
            await call.NextAsync();
            CallCountAfter++;
        }
    }

    public class AnotherTestDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 0;
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
        [Decorate(typeof(TestDecorator))]
        void DecoratedInInterface();

        void DecoratedInClass();

        [Decorate(typeof(TestDecorator))]
        void DecoratedInInterfaceAndClassWithSame();

        [Decorate(typeof(TestDecorator))]
        void DecoratedInInterfaceAndClassWithDifferent();

        void NotDecoratedMethod();
    }

    public class SomeClass : ISomeInterface
    {
        public void DecoratedInInterface() { }

        [Decorate(typeof(TestDecorator))]
        public void DecoratedInClass() { }

        [Decorate(typeof(TestDecorator))]
        public void DecoratedInInterfaceAndClassWithSame() { }

        [Decorate(typeof(AnotherTestDecorator))]
        public void DecoratedInInterfaceAndClassWithDifferent() { }

        public void NotDecoratedMethod() { }
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<TestDecorator>();
        builder.Services.AddScoped<AnotherTestDecorator>();
        builder.Services.AddTransient<ISomeInterface, SomeClass>();
        builder.Services.AddTransient<SomeClass>();

        builder.Services.AddMilvaInterception([typeof(ISomeInterface), typeof(SomeClass)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
