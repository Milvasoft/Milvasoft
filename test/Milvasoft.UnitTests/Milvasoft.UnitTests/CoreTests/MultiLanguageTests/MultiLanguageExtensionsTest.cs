using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Milvasoft.Core.MultiLanguage;
using Milvasoft.UnitTests.CoreTests.MultiLanguageTests.Fixtures;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.CoreTests.MultiLanguageTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1860:Avoid using 'Enumerable.Any()' extension method", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1125:Boolean literals should not be redundant", Justification = "<Pending>")]
[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class MultiLanguageExtensionsTest
{
    #region CreateProjectionExpression

    [Fact]
    public void CreateProjectionExpression_WithMainAndTranslationEntityPropertyNamesIsNull_ShouldReturnDefaultExpression()
    {
        // Arrange
        IEnumerable<string> mainEntityPropNames = null;
        IEnumerable<string> translationEntityPropNames = null;
        Expression<Func<HasTranslationEntityFixture, HasTranslationEntityFixture>> expected = c => c;

        // Act
        var result = MultiLanguageExtensions.CreateProjectionExpression<HasTranslationEntityFixture, TranslationEntityFixture>(mainEntityPropNames, translationEntityPropNames);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expected, result);
        equality.Should().BeTrue();
    }

    [Fact]
    public void CreateProjectionExpression_WithMainEntityPropertyNamesIsValidAndTranslationEntityPropertyNamesIsNull_ShouldReturnCorrectExpression()
    {
        // Arrange
        IEnumerable<string> mainEntityPropNames = [nameof(HasTranslationEntityFixture.Priority)];
        IEnumerable<string> translationEntityPropNames = null;
        Expression<Func<HasTranslationEntityFixture, HasTranslationEntityFixture>> expected = c => new HasTranslationEntityFixture
        {
            Priority = c.Priority
        };

        // Act
        var result = MultiLanguageExtensions.CreateProjectionExpression<HasTranslationEntityFixture, TranslationEntityFixture>(mainEntityPropNames, translationEntityPropNames);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expected, result);
        equality.Should().BeTrue();
        result.ToString().Should().Be(expected.ToString());
    }

    [Fact]
    public void CreateProjectionExpression_WithMainEntityPropertyNamesNullAndTranslationEntityPropertyNamesIsValid_ShouldReturnCorrectExpression()
    {
        // Arrange
        IEnumerable<string> mainEntityPropNames = null;
        IEnumerable<string> translationEntityPropNames = [nameof(TranslationEntityFixture.Name)];
        Expression<Func<HasTranslationEntityFixture, HasTranslationEntityFixture>> expected = c => new HasTranslationEntityFixture
        {
            Translations = (c.Translations.Any() == false ? null : c.Translations
                                                                    .Select(t => new TranslationEntityFixture
                                                                    {
                                                                        Name = t.Name,
                                                                        LanguageId = t.LanguageId
                                                                    }).ToList())
        };

        // Act
        var result = MultiLanguageExtensions.CreateProjectionExpression<HasTranslationEntityFixture, TranslationEntityFixture>(mainEntityPropNames, translationEntityPropNames);

        // Assert
        result.ToString().Should().Be(expected.ToString());
    }

    [Fact]
    public void CreateProjectionExpression_WithMainEntityAndTranslationEntityPropertyNames_ShouldReturnCorrectExpression()
    {
        // Arrange
        IEnumerable<string> mainEntityPropNames = [nameof(HasTranslationEntityFixture.Priority)];
        IEnumerable<string> translationEntityPropNames = [nameof(TranslationEntityFixture.Name)];
        Expression<Func<HasTranslationEntityFixture, HasTranslationEntityFixture>> expected = c => new HasTranslationEntityFixture
        {
            Priority = c.Priority,
            Translations = (c.Translations.Any() == false ? null : c.Translations
                                                                                                    .Select(t => new TranslationEntityFixture
                                                                                                    {
                                                                                                        Name = t.Name,
                                                                                                        LanguageId = t.LanguageId
                                                                                                    }).ToList())
        };

        // Act
        var result = MultiLanguageExtensions.CreateProjectionExpression<HasTranslationEntityFixture, TranslationEntityFixture>(mainEntityPropNames, translationEntityPropNames);

        // Assert
        result.ToString().Should().Be(expected.ToString());
    }

    [Fact]
    public void CreateTranslationMapExpression_WithReturnedExpressionUsedWithEmptyTranslations_ShouldCorrectExpression()
    {
        // Arrange
        List<HasTranslationEntityFixture> entities =
        [
            new HasTranslationEntityFixture
            {
                Id = 1,
                Priority = 1,
                Translations = []
            }
        ];
        IEnumerable<string> mainEntityPropNames = [nameof(HasTranslationEntityFixture.Priority)];
        IEnumerable<string> translationEntityPropNames = [nameof(TranslationEntityFixture.Name)];
        Expression<Func<HasTranslationEntityFixture, HasTranslationEntityFixture>> expectedExpression = c => new HasTranslationEntityFixture
        {
            Priority = c.Priority,
            Translations = c.Translations == null ? null : c.Translations
                                                            .Select(t => new TranslationEntityFixture
                                                            {
                                                                Name = t.Name,
                                                                LanguageId = t.LanguageId
                                                            }).ToList()
        };
        var expected = entities.AsQueryable().Select(expectedExpression).ToList();

        // Act
        var resultExpression = MultiLanguageExtensions.CreateProjectionExpression<HasTranslationEntityFixture, TranslationEntityFixture>(mainEntityPropNames, translationEntityPropNames);
        var result = entities.AsQueryable().Select(resultExpression).ToList();

        // Assert
        result[0].Priority.Should().Be(expected[0].Priority);
    }

    [Fact]
    public void CreateTranslationMapExpression_WithReturnedExpressionUsedWithTranslationsIsValid_ShouldCorrectExpression()
    {
        // Arrange
        List<HasTranslationEntityFixture> entities =
        [
            new HasTranslationEntityFixture
            {
                Id = 1,
                Priority = 1,
                Translations =
                [
                    new TranslationEntityFixture{
                        Id = 1,
                        Name = "First",
                        Description = "First",
                        EntityId = 1,
                        LanguageId = 3
                    },
                    new TranslationEntityFixture{
                        Id = 2,
                        Name = "İlk",
                        Description = "İlk",
                        EntityId = 1,
                        LanguageId = 4
                    }
                ]
            }
        ];
        IEnumerable<string> mainEntityPropNames = [nameof(HasTranslationEntityFixture.Priority)];
        IEnumerable<string> translationEntityPropNames = [nameof(TranslationEntityFixture.Name)];
        Expression<Func<HasTranslationEntityFixture, HasTranslationEntityFixture>> expectedExpression = c => new HasTranslationEntityFixture
        {
            Priority = c.Priority,
            Translations = c.Translations == null ? null : c.Translations
                                                            .Select(t => new TranslationEntityFixture
                                                            {
                                                                Name = t.Name,
                                                                LanguageId = t.LanguageId
                                                            }).ToList()
        };
        var expected = entities.AsQueryable().Select(expectedExpression).ToList();

        // Act
        var resultExpression = MultiLanguageExtensions.CreateProjectionExpression<HasTranslationEntityFixture, TranslationEntityFixture>(mainEntityPropNames, translationEntityPropNames);
        var result = entities.AsQueryable().Select(resultExpression).ToList();

        // Assert
        result[0].Priority.Should().Be(expected[0].Priority);
        result[0].Translations.Should().HaveCount(2);
    }

    #endregion

    #region GetTranslations

    [Fact]
    public void GetTranslations_WithInputListIsNull_ShouldReturnEmptyList()
    {
        // Arrange
        IEnumerable<TranslationEntityFixture> input = null;

        // Act
        var result = input.GetTranslations<HasTranslationEntityFixture, TranslationEntityFixture, TranslationDtoFixture>();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTranslations_WithDtoTypeParamNotContainsMatchingProperty_ShouldReturnDtoListWithDefaultValues()
    {
        // Arrange
        IEnumerable<TranslationEntityFixture> input =
        [
            new TranslationEntityFixture
            {
                Name = "First",
                Description = "First",
                LanguageId = 1,
                EntityId = 1
            },
            new TranslationEntityFixture
            {
                Name = "İlk",
                Description = "İlk",
                LanguageId = 2,
                EntityId = 1
            }
        ];

        // Act
        var result = input.GetTranslations<HasTranslationEntityFixture, TranslationEntityFixture, InvalidTranslationDtoFixture>();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(i => i.Priority == 0);
    }

    [Fact]
    public void GetTranslations_WithDtoTypeParamNotIsValid_ShouldReturnMappedDtoList()
    {
        // Arrange
        IEnumerable<TranslationEntityFixture> input =
        [
            new TranslationEntityFixture
            {
                Name = "First",
                Description = "First",
                LanguageId = 1,
                EntityId = 1
            },
            new TranslationEntityFixture
            {
                Name = "İlk",
                Description = "İlk",
                LanguageId = 2,
                EntityId = 1
            }
        ];

        // Act
        var result = input.GetTranslations<HasTranslationEntityFixture, TranslationEntityFixture, TranslationDtoFixture>();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(i => i.Name == "First");
        result.Should().Contain(i => i.Name == "İlk");
        result.Should().Contain(i => i.LanguageId == 1);
        result.Should().Contain(i => i.LanguageId == 2);
        result.Should().OnlyContain(i => i.EntityId == 1);
    }

    #endregion
}
