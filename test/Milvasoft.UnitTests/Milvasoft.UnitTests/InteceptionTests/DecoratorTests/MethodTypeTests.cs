using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

public class MethodTypeTests
{
    [Fact]
    public void SyncMethod_WithOneAttributeInClass_ShouldCallDecoratorOnce()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var decoratedClass = services.GetService<ISomeInterface>();

        // Act
        decoratedClass.Method();

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(1);
    }

    [Fact]
    public async Task AsyncMethod_WithOneAttributeInClass_ShouldCallDecoratorOnce()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var decoratedClass = services.GetService<ISomeInterface>();

        // Act
        await decoratedClass.AsyncMethod();

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(1);
    }

    [Fact]
    public async Task AsyncMethodWithResult_WithOneAttributeInClass_ShouldCallDecoratorOnce()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var decoratedClass = services.GetService<ISomeInterface>();

        // Act
        await decoratedClass.AsyncMethodWithResult();

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(1);
    }

    [Fact]
    public void GenericMethod_WithOneAttributeInClass_ShouldCallDecoratorOnce()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var sut = services.GetService<ISomeInterface>();

        // Act
        sut.GenericMethod<int>();

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(1);
    }

    [Fact]
    public async Task AsyncMethod_WithoutAwaits_ShouldCallDecoratorOnce()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var sut = services.GetService<ISomeInterface>();

        // Act
        await sut.AsyncMethod(shouldAwait: false);

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(1);
    }

    [Fact]
    public async Task AsyncMethodWithResult_WithoutAwaits_ShouldCallDecoratorOnce()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var sut = services.GetService<ISomeInterface>();

        // Act
        await sut.AsyncMethodWithResult(shouldAwait: false);

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(1);
    }

    [Fact]
    public async Task AsyncDependencyMethod_ShouldCallDecorator()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var sut = services.GetService<ISomeInterface>();

        // Act
        await sut.AsyncDependencyMethod();

        // Assert
        decorator.CallCountBefore.Should().BeGreaterThan(0);
        decorator.CallCountAfter.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TaskYieldMethod_ShouldCallDecorator()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var sut = services.GetService<ISomeInterface>();

        // Act
        await sut.TaskYieldMethod();

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(1);
    }

    [Fact]
    public async Task AsyncMethod_WithDelayedAwait_ShouldCallDecorator()
    {
        // Arrange
        var services = GetServices();
        var decorator = services.GetService<TestDecorator>();
        var sut = services.GetService<ISomeInterface>();

        // Act
        var task = sut.AsyncMethod(shouldAwait: true);

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(0);

        // Continue Act
        await task;

        // Assert
        decorator.CallCountBefore.Should().Be(1);
        decorator.CallCountAfter.Should().Be(1);
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
        Getter Getter { get; }

        Task AsyncDependencyMethod();

        Task AsyncMethod(bool shouldAwait);

        Task AsyncMethod();

        Task<int> AsyncMethodWithResult(bool shouldAwait);

        Task<int> AsyncMethodWithResult();

        T GenericMethod<T>();

        void Method();

        [Decorate(typeof(TestDecorator))]
        Task<List<int>> TaskYieldMethod();
    }

    public class SomeClass(Getter getter) : ISomeInterface
    {
        public Getter Getter { get; } = getter;
        public int SomeState { get; set; }

        [Decorate(typeof(TestDecorator))]
        public virtual void Method() { }

        [Decorate(typeof(TestDecorator))]
        public virtual T GenericMethod<T>() => default;

        [Decorate(typeof(TestDecorator))]
        public virtual async Task AsyncMethod()
        {
            await Task.Delay(20);
            await Task.Yield();
        }

        [Decorate(typeof(TestDecorator))]
        public virtual async Task AsyncMethod(bool shouldAwait)
        {
            if (shouldAwait)
            {
                await Task.Delay(20);
                await Task.Yield();
            }
        }

        [Decorate(typeof(TestDecorator))]
        public virtual async Task<int> AsyncMethodWithResult()
        {
            await Task.Delay(20);
            await Task.Yield();
            return 0;
        }

        [Decorate(typeof(TestDecorator))]
        public virtual async Task<int> AsyncMethodWithResult(bool shouldAwait)
        {
            if (shouldAwait)
            {
                await Task.Delay(20);
                await Task.Yield();
            }

            return 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1121:Assignments should not be made from within sub-expressions", Justification = "<Pending>")]
        [Decorate(typeof(TestDecorator))]
        public virtual async Task AsyncDependencyMethod()
        {
            List<int> list;

            while ((list = await Getter.Get()).Count > 0)
            {
                await Task.Delay(30);
                await Task.Yield();

                list.Add(1);
            }
        }

        [Decorate(typeof(TestDecorator))]
        public virtual async Task<List<int>> TaskYieldMethod()
        {
            var list = new List<int>();

            SomeState = 1;
            await Task.Yield();

            list.Add(SomeState + SomeState);
            SomeState = 2;

            await Task.Yield();

            list.Add(SomeState + SomeState);
            SomeState = 3;

            return list;
        }
    }

    public class Getter : IInterceptable
    {
        public Random Random = new();

        private int? _counter;

        [Decorate(typeof(TestDecorator))]
        public virtual async Task<List<int>> Get()
        {
            await Task.Delay(20);
            await Task.Yield();

            if (!_counter.HasValue)
            {
                _counter = Random.Next(1, 5);
            }

            return _counter-- > 0 ? [Random.Next(0, 10)] : [];
        }
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<TestDecorator>();
        builder.Services.AddScoped<Getter>();
        builder.Services.AddTransient<SomeClass>();
        builder.Services.AddTransient<ISomeInterface, SomeClass>();

        builder.Services.AddMilvaInterception([typeof(ISomeInterface), typeof(SomeClass), typeof(Getter)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
