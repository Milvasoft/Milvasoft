using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core;

namespace Milvasoft.UnitTests.CoreTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMilvaLazy_ShouldAddCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMilvaLazy();
        services.AddScoped<SomeDependency>();
        services.AddScoped<SomeClass>();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var sut = serviceProvider.GetService<SomeClass>();

        // Assert
        sut.Should().NotBeNull();
        sut.SomeDependency.IsValueCreated.Should().BeFalse();

        var res = sut.CallLazy();

        res.Should().Be(1);
        sut.SomeDependency.IsValueCreated.Should().BeTrue();
    }

    #region Setup

    private class SomeDependency
    {
        public int Id { get; set; } = 1;
    }

    private class SomeClass(Lazy<SomeDependency> someDependency)
    {
        public Lazy<SomeDependency> SomeDependency { get; set; } = someDependency;

        public int CallLazy() => SomeDependency.Value.Id;
    }

    #endregion
}
