using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Utils;
using Milvasoft.UnitTests.CoreTests.UtilTests.Fixtures;

namespace Milvasoft.UnitTests.CoreTests.UtilTests;

public partial class MilvaLazyTests
{
    [Fact]
    public void MilvaLazy_ShouldResolveInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<MilvaLazyTestModelFixture>();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var lazyInstance = new MilvaLazy<MilvaLazyTestModelFixture>(serviceProvider);

        // Assert
        lazyInstance.Value.Should().NotBeNull();
        lazyInstance.Value.Should().BeOfType<MilvaLazyTestModelFixture>();
    }

    [Fact]
    public void MilvaLazy_ShouldResolveSameInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<MilvaLazyTestModelFixture>();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var lazyInstance1 = new MilvaLazy<MilvaLazyTestModelFixture>(serviceProvider);
        var lazyInstance2 = new MilvaLazy<MilvaLazyTestModelFixture>(serviceProvider);

        // Assert
        lazyInstance1.Value.Should().BeSameAs(lazyInstance2.Value);
    }
}
