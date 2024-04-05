using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S4144:Methods should not have identical implementations", Justification = "<Pending>")]
public class ExceptionTests
{
    [Fact]
    public void Method_WithThrowException_ShouldCatchSameException()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();

        // Act & Assert
        someClass.Invoking(x => x.MethodThrow())
            .Should().ThrowExactly<ExpectedException>();
    }

    [Fact]
    public void MethodAsync_WithThrowImmediately_ShouldCatchSameException()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();

        // Act & Assert
        someClass.Invoking(async x => await x.MethodAsyncThrowImmediately())
            .Should().ThrowExactlyAsync<ExpectedException>();
    }

    [Fact]
    public void MethodAsyncResult_WithThrowImmediately_ShouldCatchSameException()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();

        // Act & Assert
        someClass.Invoking(async x => await x.MethodAsyncResultThrowImmediately())
            .Should().ThrowExactlyAsync<ExpectedException>();
    }

    [Fact]
    public void MethodAsync_WithThrowAfterAwait_ShouldCatchSameException()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();

        // Act & Assert
        someClass.Invoking(async x => await x.MethodAsyncThrowAfterAwait())
            .Should().ThrowExactlyAsync<ExpectedException>();
    }

    [Fact]
    public void MethodAsyncResult_WithThrowAfterAwait_ShouldCatchSameException()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();

        // Act & Assert
        someClass.Invoking(async x => await x.MethodAsyncResultThrowAfterAwait())
            .Should().ThrowExactlyAsync<ExpectedException>();
    }

    [Fact]
    public void Method_WithThrowExceptionNotDecorated_ShouldCatchSameException()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();

        // Act & Assert
        someClass.Invoking(x => x.MethodThrowNotDecorated())
            .Should().ThrowExactly<ExpectedException>();
    }

    [Fact]
    public void MethodAsync_WithThrowAfterAwaitNotDecorated_ShouldCatchSameException()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();

        // Act & Assert
        someClass.Invoking(async x => await x.MethodAsyncThrowAfterAwaitNotDecorated())
            .Should().ThrowExactlyAsync<ExpectedException>();
    }

    [Fact]
    public void MethodAsync_WithThrowImmediatelyNotDecorated_ShouldCatchSameException()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();

        // Act & Assert
        someClass.Invoking(async x => await x.MethodAsyncThrowImmediatelyNotDecorated())
            .Should().ThrowExactlyAsync<ExpectedException>();
    }

    [Fact]
    public void MethodAsyncResult_WithThrowAfterAwaitNotDecorated_ShouldCatchSameException()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();

        // Act & Assert
        someClass.Invoking(async x => await x.MethodAsyncResultThrowAfterAwaitNotDecorated())
            .Should().ThrowExactlyAsync<ExpectedException>();
    }

    [Fact]
    public void MethodAsyncResult_WithThrowImmediatelyNotDecorated_ShouldCatchSameException()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();

        // Act & Assert
        someClass.Invoking(async x => await x.MethodAsyncResultThrowImmediatelyNotDecorated())
            .Should().ThrowExactlyAsync<ExpectedException>();
    }

    [Fact]
    public void MethodDecoratedToThrow_WithThrowingDecorator_ShouldCatchSameException()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();

        // Act & Assert
        someClass.Invoking(x => x.MethodDecoratedToThrow())
            .Should().ThrowExactly<ExpectedException>();
    }

    [Fact]
    public void MethodDecoratedToThrowAsync_WithThrowingDecorator_ShouldCatchSameException()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();

        // Act & Assert
        someClass.Invoking(async x => await x.MethodDecoratedToThrowAsync())
            .Should().ThrowExactlyAsync<ExpectedException>();
    }

    [Fact]
    public void MethodDecoratedToThrowAsyncResult_WithThrowingDecorator_ShouldCatchSameException()
    {
        // Arrange
        var services = GetServices();
        var someClass = services.GetService<SomeClass>();

        // Act & Assert
        someClass.Invoking(async x => await x.MethodDecoratedToThrowAsyncResult())
            .Should().ThrowExactlyAsync<ExpectedException>();
    }

    #region Setup
    public class TestDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;
        public async Task OnInvoke(Call call) => await call.NextAsync();
    }

    public class ThrowingDecorator : IMilvaInterceptor
    {
        public int InterceptionOrder { get; set; } = 1;

        public Task OnInvoke(Call call) => throw new ExpectedException();
    }

    public class ExpectedException : Exception { }

    public class SomeClass : IInterceptable
    {
        public int CallCount { get; set; }

        [Decorate(typeof(TestDecorator))]
        public virtual void MethodThrow() => throw new ExpectedException();

        [Decorate(typeof(TestDecorator))]
        public virtual Task MethodAsyncThrowImmediately() => throw new ExpectedException();

        [Decorate(typeof(TestDecorator))]
        public virtual async Task MethodAsyncThrowAfterAwait()
        {
            await Task.Yield();
            throw new ExpectedException();
        }

        [Decorate(typeof(TestDecorator))]
        public virtual Task<int> MethodAsyncResultThrowImmediately() => throw new ExpectedException();

        [Decorate(typeof(TestDecorator))]
        public virtual async Task<int> MethodAsyncResultThrowAfterAwait()
        {
            await Task.Yield();
            throw new ExpectedException();
        }

        public virtual void MethodThrowNotDecorated() => throw new ExpectedException();

        public virtual Task MethodAsyncThrowImmediatelyNotDecorated() => throw new ExpectedException();

        public virtual async Task MethodAsyncThrowAfterAwaitNotDecorated()
        {
            await Task.Yield();
            throw new ExpectedException();
        }

        public virtual Task<int> MethodAsyncResultThrowImmediatelyNotDecorated() => throw new ExpectedException();

        public virtual async Task<int> MethodAsyncResultThrowAfterAwaitNotDecorated()
        {
            await Task.Yield();
            throw new ExpectedException();
        }

        [Decorate(typeof(ThrowingDecorator))]
        public virtual void MethodDecoratedToThrow() { }

        [Decorate(typeof(ThrowingDecorator))]
        public virtual async Task MethodDecoratedToThrowAsync() => await Task.Yield();

        [Decorate(typeof(ThrowingDecorator))]
        public virtual async Task<int> MethodDecoratedToThrowAsyncResult() => await Task.FromResult(0);
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddScoped<TestDecorator>();
        builder.Services.AddScoped<ThrowingDecorator>();
        builder.Services.AddTransient<SomeClass>();

        builder.Services.AddMilvaInterception([typeof(SomeClass)]);

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
