using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Caching.Builder;
using Milvasoft.Caching.InMemory;
using Milvasoft.Caching.InMemory.Accessor;
using Milvasoft.Caching.InMemory.Options;

namespace Milvasoft.UnitTests.CachingTests;

[Trait("Caching Unit Tests", "Milvasoft.Caching.InMemory project unit tests.")]
public class MemoryCacheAccessorTests
{
    [Fact]
    public void WithInMemoryAccessor_WithOptionsButNullMemoryCacheOptions_ShouldResolveCache()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMilvaCaching().WithInMemoryAccessor(new InMemoryCacheOptions { MemoryCacheOptions = null });

        // Act
        using var provider = services.BuildServiceProvider();
        var act = () => provider.GetRequiredService<IMemoryCacheAccessor>();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Set_WithSizeLimitConfigured_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMilvaCaching().WithInMemoryAccessor(new InMemoryCacheOptions { MemoryCacheOptions = new MemoryCacheOptions { SizeLimit = 10 } });
        using var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IMemoryCacheAccessor>();

        // Act
        var act = () =>
        {
            accessor.Set("a", "value");
            accessor.Set("b", new List<int> { 1 });
            accessor.Set("c", (object)"value", TimeSpan.FromMinutes(1));
        };

        // Assert
        act.Should().NotThrow();
        accessor.Get("a").Should().Be("value");
    }

    [Fact]
    public async Task GetAsync_WithValueTypeReturnType_ShouldReturnValue()
    {
        // Arrange
        var accessor = CreateAccessor();
        accessor.Set("number", (object)5, null);

        // Act
        var result = await accessor.GetAsync("number", typeof(int));
        var mismatched = await accessor.GetAsync("number", typeof(string));

        // Assert
        result.Should().Be(5);
        mismatched.Should().BeNull();
    }

    [Fact]
    public async Task BulkMembers_ShouldBeImplemented()
    {
        // Arrange
        var accessor = CreateAccessor();
        accessor.Set("a", "1");
        accessor.Set("b", "2");

        // Act
        var values = accessor.Get<string>(["a", "b", "missing"]);
        var asyncValues = await accessor.GetAsync<string>(["a", "missing"]);
        var removedCount = await accessor.RemoveAsync(["a", "b", "missing"]);

        // Assert
        values.Should().BeEquivalentTo("1", "2");
        asyncValues.Should().BeEquivalentTo("1");
        removedCount.Should().Be(2);
        accessor.KeyExists("a").Should().BeFalse();
    }

    [Fact]
    public void KeyExpire_WithNullDate_ShouldMakeEntryPersistentAndReturnTrue()
    {
        // Arrange
        var accessor = CreateAccessor();
        accessor.Set("a", (object)"1", TimeSpan.FromMilliseconds(500));

        // Act
        var result = accessor.KeyExpire("a", (DateTime?)null);
        Thread.Sleep(700);

        // Assert
        result.Should().BeTrue();
        accessor.Get("a").Should().Be("1");
    }

    [Fact]
    public void KeyExpire_WithUnspecifiedKindDate_ShouldThrowArgumentException()
    {
        // Arrange
        var accessor = CreateAccessor();
        accessor.Set("a", "1");

        // Act
        var act = () => accessor.KeyExpire("a", new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    private static MemoryCacheAccessor CreateAccessor() => new(new MemoryCache(new MemoryCacheOptions()));
}
