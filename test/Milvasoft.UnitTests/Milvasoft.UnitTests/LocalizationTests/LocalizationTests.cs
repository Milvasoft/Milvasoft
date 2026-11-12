using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Localization;
using Milvasoft.Localization.Builder;
using Milvasoft.Localization.Resx.ResxManipulator;
using Milvasoft.Types.Classes;
using System.Globalization;

namespace Milvasoft.UnitTests.LocalizationTests;

[Trait("Localization Unit Tests", "Unit tests for Milvasoft.Localization projects.")]
public class LocalizationTests : IDisposable
{
    private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _originalUICulture = CultureInfo.CurrentUICulture;

    public void Dispose()
    {
        // Tests below change the thread cultures; never leak them to other tests.
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUICulture;
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void CultureSwitcher_ShouldNotChangeProcessDefaultsAndShouldRestoreBothCultures()
    {
        var originalDefault = CultureInfo.DefaultThreadCurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

        using (new CultureSwitcher("de-DE"))
        {
            CultureInfo.CurrentUICulture.Name.Should().Be("de-DE");
            CultureInfo.DefaultThreadCurrentCulture.Should().Be(originalDefault);
        }

        CultureInfo.CurrentCulture.Name.Should().Be("tr-TR");
        CultureInfo.CurrentUICulture.Name.Should().Be("en-US");
    }

    [Fact]
    public void GetWithCulture_WhenLookupThrows_ShouldRestoreCulture()
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");

        var localizer = CreateLocalizer(new StubLocalizationManager { Throw = true }, useCache: false);

        var act = () => localizer.GetWithCulture("Key", "en-US");

        act.Should().Throw<InvalidOperationException>();
        CultureInfo.CurrentUICulture.Name.Should().Be("tr-TR");
    }

    [Fact]
    public void Localizer_WithoutCache_ShouldFormatArguments()
    {
        var localizer = CreateLocalizer(new StubLocalizationManager(), useCache: false);

        ((string)localizer["Hello", "World"]).Should().Be("Hello World!");
    }

    [Fact]
    public void Localizer_WithCache_ShouldNotLookUpAMissingKeyAgain()
    {
        var manager = new StubLocalizationManager();
        var localizer = CreateLocalizer(manager, useCache: true);

        var first = localizer["Missing"];
        var second = localizer["Missing"];

        first.ResourceFound.Should().BeFalse();
        ((string)second).Should().Be("Missing");
        manager.LookupCount.Should().Be(1);
    }

    [Fact]
    public void Localizer_WithCache_ShouldFormatArgumentsOfMissingAndFoundKeys()
    {
        var localizer = CreateLocalizer(new StubLocalizationManager(), useCache: true);

        ((string)localizer["Hello", "World"]).Should().Be("Hello World!");
        ((string)localizer["Hello", "Again"]).Should().Be("Hello Again!");
    }

    [Fact]
    public void ResxElement_ShouldBeUsableInHashBasedCollections()
    {
        var set = new HashSet<ResxElement> { new() { Key = "Key" }, new() { Key = "key" } };

        set.Should().HaveCount(1);
    }

    #region Setup

    private static MilvaLocalizer CreateLocalizer(ILocalizationManager manager, bool useCache)
    {
        var services = new ServiceCollection();

        services.AddMemoryCache();
        services.AddSingleton<ILocalizationOptions>(new LocalizationOptions { UseInMemoryCache = useCache });
        services.AddSingleton<ILocalizationMemoryCache>(sp => new LocalizationMemoryCache(sp.GetRequiredService<IMemoryCache>(), sp.GetRequiredService<ILocalizationOptions>()));
        services.AddSingleton(manager);

        return new MilvaLocalizer(services.BuildServiceProvider());
    }

    public class StubLocalizationManager : ILocalizationManager
    {
        private readonly Dictionary<string, string> _values = new() { ["Hello"] = "Hello {0}!" };

        public int LookupCount { get; private set; }

        public bool Throw { get; set; }

        public LocalizedValue this[string key]
        {
            get
            {
                if (Throw)
                    throw new InvalidOperationException();

                LookupCount++;

                return _values.TryGetValue(key, out var value) ? new LocalizedValue(key, value) : new LocalizedValue(key, key, resourceFound: false);
            }
        }

        public LocalizedValue this[string key, params object[] arguments]
        {
            get
            {
                var value = this[key];

                return new LocalizedValue(key, string.Format(value.Value, arguments), value.ResourceFound);
            }
        }

        public IEnumerable<LocalizedValue> GetAllStrings(bool includeParentCultures) => [];
        public void Set(string key, string value) { }
        public void Set(string key, string value, string culture) { }
        public void Remove(string key) { }
        public void Remove(string key, string culture) { }
        public Task SetAsync(string key, string value) => Task.CompletedTask;
        public Task SetAsync(string key, string value, string culture) => Task.CompletedTask;
        public Task RemoveAsync(string key) => Task.CompletedTask;
        public Task RemoveAsync(string key, string culture) => Task.CompletedTask;
    }

    #endregion
}
