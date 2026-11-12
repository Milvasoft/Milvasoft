using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Interception.Interceptors.Response;
using Milvasoft.Types.Classes;
using Milvasoft.UnitTests.InteceptionTests.SystemLikeNamespaceFixtures;

namespace Milvasoft.UnitTests.InteceptionTests;

/// <summary>
/// Hide/mask rules must be applied for every collection shape and for types whose namespace merely contains "System".
/// </summary>
[Trait("Interceptors Unit Tests", "Unit tests for Milvasoft.Interception project interceptors.")]
public class ResponseMetadataRulesTests
{
    [Fact]
    public async Task EnumerableResponse_ShouldApplyHideAndMaskRulesToEveryItem()
    {
        var generator = new ResponseMetadataGenerator(GetServices());
        IEnumerable<SecretDto> data = new HashSet<SecretDto> { new() { Name = "a", Secret = "top-secret-1", Masked = "masked-value" } };
        var response = Response<IEnumerable<SecretDto>>.Success(data);

        await generator.GenerateMetadataAsync(response);

        response.Data.Single().Secret.Should().BeNull();
        response.Data.Single().Masked.Should().NotBe("masked-value");
        response.Data.Single().Name.Should().Be("a");
    }

    [Fact]
    public void ArrayResponse_ShouldApplyHideRulesToEveryItem()
    {
        var generator = new ResponseMetadataGenerator(GetServices());
        var response = Response<SecretDto[]>.Success([new() { Secret = "top-secret-1" }, new() { Secret = "top-secret-2" }]);

        generator.GenerateMetadata(response);

        response.Data.Should().AllSatisfy(d => d.Secret.Should().BeNull());
        response.Metadatas.Should().Contain(m => m.Name == "name");
    }

    [Fact]
    public void NestedReadOnlyCollection_ShouldApplyHideRulesToEveryItem()
    {
        var generator = new ResponseMetadataGenerator(GetServices());
        var response = Response<ParentDto>.Success(new ParentDto { Children = [new() { Secret = "top-secret-1" }] });

        generator.GenerateMetadata(response);

        response.Data.Children.Single().Secret.Should().BeNull();
    }

    [Fact]
    public void TypeInNamespaceContainingSystem_ShouldNotBeTreatedAsPrimitive()
    {
        var generator = new ResponseMetadataGenerator(GetServices());
        var response = Response<FileSystemSecretDto>.Success(new FileSystemSecretDto { Secret = "top-secret" });

        generator.GenerateMetadata(response);

        response.Data.Secret.Should().BeNull();
        response.Metadatas.Should().Contain(m => m.Name == "path");
    }

    [Fact]
    public void NonStringClientDefaultValue_WithTranslation_ShouldBeKeptAsIs()
    {
        var generator = new ResponseMetadataGenerator(GetServices());
        var response = Response<TranslatedDto>.Success(new TranslatedDto { Count = 3 });

        generator.GenerateMetadata(response);

        response.Metadatas.Single(m => m.Name == "count").DefaultValue.Should().Be(5);
    }

    [Fact]
    public void ApplyLocalization_WhenNoResourceFound_ShouldFallBackToPropertyName()
    {
        var localizedName = ResponseMetadataGenerator.ApplyLocalization(null, new NotFoundLocalizer(), typeof(TranslatedDto), "Count");

        localizedName.Should().Be("Count");
    }

    #region Setup

    public class SecretDto
    {
        public string Name { get; set; }

        [HideByRole("Hide")]
        public string Secret { get; set; }

        [MaskByRole("NotAllowed")]
        public string Masked { get; set; }
    }

    public class ParentDto
    {
        public IReadOnlyList<SecretDto> Children { get; set; }
    }

    [Translate]
    public class TranslatedDto
    {
        [ClientDefaultValue(5)]
        public int Count { get; set; }
    }

    public class TestLocalizer : IMilvaLocalizer
    {
        public LocalizedValue this[string key] => new(key, $"localized_{key}");
        public LocalizedValue this[string key, params object[] arguments] => new(key, $"localized_{key}");
        public IEnumerable<LocalizedValue> GetAllStrings(bool includeParentCultures) => [];
        public LocalizedValue GetWithCulture(string key, string culture) => this[key];
        public LocalizedValue GetWithCulture(string key, string culture, params object[] arguments) => this[key];
    }

    public class NotFoundLocalizer : IMilvaLocalizer
    {
        public LocalizedValue this[string key] => new(key, key, resourceFound: false);
        public LocalizedValue this[string key, params object[] arguments] => this[key];
        public IEnumerable<LocalizedValue> GetAllStrings(bool includeParentCultures) => [];
        public LocalizedValue GetWithCulture(string key, string culture) => this[key];
        public LocalizedValue GetWithCulture(string key, string culture, params object[] arguments) => this[key];
    }

    private static ServiceProvider GetServices()
    {
        var services = new ServiceCollection();

        services.AddScoped<IMilvaLocalizer, TestLocalizer>();
        services.AddSingleton<IResponseInterceptionOptions>(new ResponseInterceptionOptions
        {
            HideByRoleFunc = (_, attribute) => attribute.Roles.Contains("Hide"),
            MaskByRoleFunc = (_, attribute) => attribute.Roles.Contains("NotAllowed"),
        });

        return services.BuildServiceProvider();
    }

    #endregion
}
