using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

[Trait("Interception Unit Tests", "Unit tests for Milvasoft.Interception project.")]
public class ReturnValueTests
{
    [Theory, AutoData]
    public void Method_WithValueTypeReturn_ShouldReturnTheSame(int expectedReturnValue)
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<SomeClass>();

        // Act
        var actualReturnValue = sut.IntReturnMethod(expectedReturnValue);

        // Assert
        actualReturnValue.Should().Be(expectedReturnValue);
    }

    [Theory, AutoData]
    public void Method_WithReferenceTypeReturn_ShouldReturnTheSame(string expectedReturnValue)
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<SomeClass>();

        // Act
        var actualReturnValue = sut.StringReturnMethod(expectedReturnValue);

        // Assert
        actualReturnValue.Should().Be(expectedReturnValue);
    }

    [Fact]
    public void Method_WithTaskReturn_ShouldReturnUntouchedTask()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<SomeClass>();

        // Act
        var actualReturnValue = sut.TaskReturnMethod();

        // Assert
        actualReturnValue.IsCompleted.Should().Be(false);
        actualReturnValue.Status.Should().Be(TaskStatus.WaitingForActivation);
    }

    [Theory, AutoData]
    public void Method_WithGenericValueTypeReturn_ShouldReturnTheSame(int expectedReturnValue)
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<SomeClass>();

        // Act
        var actualReturnValue = sut.GenericReturnMethod(expectedReturnValue);

        // Assert
        actualReturnValue.Should().Be(expectedReturnValue);
    }

    [Theory, AutoData]
    public void Method_WithGenericReferenceTypeReturn_ShouldReturnTheSame(string expectedReturnValue)
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<SomeClass>();

        // Act
        var actualReturnValue = sut.GenericReturnMethod(expectedReturnValue);

        // Assert
        actualReturnValue.Should().Be(expectedReturnValue);
    }

    [Theory, AutoData]
    public async Task Method_WithGenericAsyncReturn_ShouldReturnTheSame(string expectedReturnValue)
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<SomeClass>();

        // Act
        var actualReturnValue = await sut.GenericAsyncReturnMethod(expectedReturnValue);

        // Assert
        actualReturnValue.Should().Be(expectedReturnValue);
    }

    [Fact]
    public async Task GenericAsyncMethod_WithNullReturn_ShouldReturnTheSame()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<SomeClass>();

        // Act
        var actualReturnValue = await sut.GenericAsyncReturnMethod<string>(null);

        // Assert
        actualReturnValue.Should().BeNull();
    }

    [Fact]
    public void Method_WithNullTask_ShouldReturnNull()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<SomeClass>();

        // Act
        var actual = sut.NullTaskMethod();

        // Assert
        actual.Should().BeNull();
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

    public class SomeClass
    {
        [Decorate(typeof(TestDecorator))]
        public virtual int IntReturnMethod(int expected) => expected;

        [Decorate(typeof(TestDecorator))]
        public virtual string StringReturnMethod(string expected) => expected;

        [Decorate(typeof(TestDecorator))]
        public virtual Task TaskReturnMethod() => Task.Delay(100);

        [Decorate(typeof(TestDecorator))]
        public virtual T GenericReturnMethod<T>(T expected) => expected;

        [Decorate(typeof(TestDecorator))]
        public virtual async Task<T> GenericAsyncReturnMethod<T>(T expected)
        {
            await Task.Delay(20);
            return expected;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Bug", "S4586:Non-async \"Task/Task<T>\" methods should not return null", Justification = "<Pending>")]
        [Decorate(typeof(TestDecorator))]
        public virtual Task NullTaskMethod() => null;
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<TestDecorator>();
        builder.Services.AddTransient<SomeClass>();

        builder.Services.AddMilvaInterception([typeof(SomeClass)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
