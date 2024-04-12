using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.MultiLanguage.Builder;
using Milvasoft.Core.MultiLanguage.Manager;

namespace Milvasoft.UnitTests.CoreTests;

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

    [Fact]
    public void AddMilvaMultiLanguage_WithDefaultMultiLanguageManager_WithMultiLanguageManagerAlreadyAdded_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddMilvaMultiLanguage()
                              .WithDefaultMultiLanguageManager();

        // Act
        Action act = () => builder.WithDefaultMultiLanguageManager();

        // Assert
        act.Should().Throw<MilvaDeveloperException>().WithMessage("A IMultiLanguageManager manager has already been registered. Please make sure to register only one IMultiLanguageManager manager.");
    }

    [Fact]
    public void AddMilvaMultiLanguage_WithDefaultMultiLanguageManager_ShouldAddCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddMilvaMultiLanguage()
                              .WithDefaultMultiLanguageManager();
        var serviceProvider = builder.Services.BuildServiceProvider();

        // Act
        var sut = serviceProvider.GetService<IMultiLanguageManager>();

        // Assert
        sut.Should().NotBeNull();
        MultiLanguageManager.Languages.Should().HaveCount(15);
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
