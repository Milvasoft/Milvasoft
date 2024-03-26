using FluentAssertions;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.Manager;
using Milvasoft.UnitTests.CoreTests.MultiLanguageTests.Fixtures;
using System.Globalization;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.CoreTests.MultiLanguageTests;

public class MultiLanguageManagerTests
{
    #region UpdateLanguagesList

    [Fact]
    public void UpdateLanguagesList_WithInputListIsNull_ShouldClearLanguagesList()
    {
        // Arrange
        List<ILanguage> input = null;

        // Act
        MultiLanguageManager.UpdateLanguagesList(input);

        // Assert
        MultiLanguageManager.Languages.Should().BeEmpty();
    }

    [Fact]
    public void UpdateLanguagesList_WithInputListIsEmpty_ShouldClearLanguagesList()
    {
        // Arrange
        List<ILanguage> input = [];

        // Act
        MultiLanguageManager.UpdateLanguagesList(input);

        // Assert
        MultiLanguageManager.Languages.Should().BeEmpty();
    }

    [Fact]
    public void UpdateLanguagesList_WithInputListIsNotEmpty_ShouldUpdateLanguagesAsInput()
    {
        // Arrange
        List<ILanguage> input =
        [
            new LanguageModelFixture
            {
                Id = 1,
                Code = "tr-TR",
                IsDefault = true,
                Name ="Turkish",
                Supported = true,
            }
        ];

        // Act
        MultiLanguageManager.UpdateLanguagesList(input);

        // Assert
        MultiLanguageManager.Languages.Should().HaveCount(1);
        MultiLanguageManager.Languages.Should().Contain(input[0]);
    }

    #endregion

    #region GetDefaultLanguageId

    [Fact]
    public void GetDefaultLanguageId_WithLanguagesListIsEmpty_ShouldReturnZero()
    {
        // Arrange
        MultiLanguageManager.Languages.Clear();
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetDefaultLanguageId();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetDefaultLanguageId_WithLanguagesListNotContainsDefaultLanguage_ShouldReturnZero()
    {
        // Arrange
        List<ILanguage> languages =
        [
            new LanguageModelFixture
            {
                Id = 1,
                Code = "tr-TR",
                IsDefault = false,
                Name ="Turkish",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 2,
                Code = "en-US",
                IsDefault = false,
                Name ="English",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 3,
                Code = "de-DE",
                IsDefault = false,
                Name ="Deutsche",
                Supported = true,
            }
        ];
        MultiLanguageManager.UpdateLanguagesList(languages);
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetDefaultLanguageId();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetDefaultLanguageId_WithLanguagesListContainsMultipleDefaultLanguage_ShouldReturnFirstsId()
    {
        // Arrange
        List<ILanguage> languages =
        [
            new LanguageModelFixture
            {
                Id = 2,
                Code = "en-US",
                IsDefault = true,
                Name ="English",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 1,
                Code = "tr-TR",
                IsDefault = true,
                Name ="Turkish",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 3,
                Code = "de-DE",
                IsDefault = false,
                Name ="Deutsche",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 4,
                Code = "nl-NL",
                IsDefault = true,
                Name ="Nederlands",
                Supported = true,
            }
        ];
        MultiLanguageManager.UpdateLanguagesList(languages);
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetDefaultLanguageId();

        // Assert
        result.Should().Be(4);
    }

    #endregion

    #region GetCurrentLanguageId

    [Fact]
    public void GetCurrentLanguageId_WithLanguagesListIsEmpty_ShouldReturnZero()
    {
        // Arrange
        MultiLanguageManager.Languages.Clear();
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetCurrentLanguageId();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetCurrentLanguageId_WithLanguagesListNotContainsCurrentAndDefaultLanguage_ShouldReturnZero()
    {
        // Arrange
        List<ILanguage> languages =
        [
            new LanguageModelFixture
            {
                Id = 1,
                Code = "en-US",
                IsDefault = false,
                Name ="English",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 2,
                Code = "tr-TR",
                IsDefault = false,
                Name ="Turkish",
                Supported = true,
            },
        ];
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("nl-NL");
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetCurrentLanguageId();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetCurrentLanguageId_WithLanguagesListNotContainsCurrentLanguage_ShouldReturnDefaultLanguageId()
    {
        // Arrange
        List<ILanguage> languages =
        [
            new LanguageModelFixture
            {
                Id = 1,
                Code = "en-US",
                IsDefault = true,
                Name ="English",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 2,
                Code = "tr-TR",
                IsDefault = false,
                Name ="Turkish",
                Supported = true,
            },
        ];
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("nl-NL");
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetCurrentLanguageId();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void GetCurrentLanguageId_WithLanguagesListContainsCurrentLanguage_ShouldReturnCurrentLanguageId()
    {
        // Arrange
        List<ILanguage> languages =
        [
            new LanguageModelFixture
            {
                Id = 1,
                Code = "en-US",
                IsDefault = true,
                Name ="English",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 2,
                Code = "tr-TR",
                IsDefault = false,
                Name ="Turkish",
                Supported = true,
            },
        ];
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetCurrentLanguageId();

        // Assert
        result.Should().Be(2);
    }

    #endregion

    #region CreateTranslationMapExpression

    [Fact]
    public void CreateTranslationMapExpression_WithPropertyExpressionIsNull_ShouldReturnNull()
    {
        // Arrange
        List<ILanguage> languages =
        [
            new LanguageModelFixture
            {
                Id = 1,
                Code = "en-US",
                IsDefault = true,
                Name ="English",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 2,
                Code = "tr-TR",
                IsDefault = false,
                Name ="Turkish",
                Supported = true,
            },
        ];
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        var manager = new MilvaMultiLanguageManager();
        Expression<Func<HasTranslationDtoFixture, string>> input = null;

        // Act
        var result = manager.CreateTranslationMapExpression<HasTranslationEntityFixture, HasTranslationDtoFixture, TranslationEntityFixture>(input);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CreateTranslationMapExpression_WithTranslationsIsNullAndCurrentOrDefaultAndCurrentLanguageAndDefaultLanguageIsDifferent_ShouldCorrectExpression()
    {
        // Arrange
        List<ILanguage> languages =
        [
            new LanguageModelFixture
            {
                Id = 1,
                Code = "en-US",
                IsDefault = true,
                Name ="English",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 2,
                Code = "tr-TR",
                IsDefault = false,
                Name ="Turkish",
                Supported = true,
            },
        ];
        List<HasTranslationEntityFixture> entities =
        [
            new HasTranslationEntityFixture
            {
                Id = 1,
                Translations = null
            }
        ];
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        var manager = new MilvaMultiLanguageManager();
        Expression<Func<HasTranslationEntityFixture, string>> expectedExpression = src => src.Translations != null
                                                                                                ? (null == src.Translations.FirstOrDefault(i => i.LanguageId == 2)
                                                                                                    ? src.Translations.FirstOrDefault(i => i.LanguageId == 1) != null
                                                                                                            ? src.Translations.FirstOrDefault(i => i.LanguageId == 1).Name
                                                                                                            : src.Translations.FirstOrDefault().Name != null
                                                                                                                ? src.Translations.FirstOrDefault().Name
                                                                                                                : null
                                                                                                    : src.Translations.FirstOrDefault(i => i.LanguageId == 2).Name)
                                                                                                : null;

        // Act
        var resultExpression = manager.CreateTranslationMapExpression<HasTranslationEntityFixture, HasTranslationDtoFixture, TranslationEntityFixture>(e => e.Name);

        // Assert
        var resultWithExpectedExpression = entities.AsQueryable().Select(expectedExpression).First();
        var resultWithResultExpression = entities.AsQueryable().Select(resultExpression).First();

        resultWithResultExpression.Should().Be(resultWithExpectedExpression);
    }

    [Fact]
    public void CreateTranslationMapExpression_WithTranslationsNotContainCurrentOrDefaultAndCurrentLanguageAndDefaultLanguageIsDifferent_ShouldCorrectExpression()
    {
        // Arrange
        List<ILanguage> languages =
        [
            new LanguageModelFixture
            {
                Id = 1,
                Code = "en-US",
                IsDefault = true,
                Name ="English",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 2,
                Code = "tr-TR",
                IsDefault = false,
                Name ="Turkish",
                Supported = true,
            },
        ];
        List<HasTranslationEntityFixture> entities =
        [
            new HasTranslationEntityFixture
            {
                Id = 1,
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
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        var manager = new MilvaMultiLanguageManager();
        Expression<Func<HasTranslationEntityFixture, string>> expectedExpression = src => src.Translations != null
                                                                                            ? null == src.Translations.FirstOrDefault(i => i.LanguageId == 2)
                                                                                                ? src.Translations.FirstOrDefault(i => i.LanguageId == 1) != null
                                                                                                        ? src.Translations.FirstOrDefault(i => i.LanguageId == 1).Name
                                                                                                        : src.Translations.FirstOrDefault().Name != null
                                                                                                            ? src.Translations.FirstOrDefault().Name
                                                                                                            : null
                                                                                                : src.Translations.FirstOrDefault(i => i.LanguageId == 2).Name
                                                                                             : null;

        // Act
        var resultExpression = manager.CreateTranslationMapExpression<HasTranslationEntityFixture, HasTranslationDtoFixture, TranslationEntityFixture>(e => e.Name);

        // Assert
        var resultWithExpectedExpression = entities.AsQueryable().Select(expectedExpression).First();
        var resultWithResultExpression = entities.AsQueryable().Select(resultExpression).First();

        resultWithResultExpression.Should().Be(resultWithExpectedExpression);
    }

    [Fact]
    public void CreateTranslationMapExpression_WithTranslationsContainCurrentOrDefaultAndCurrentLanguageAndDefaultLanguageIsDifferent_ShouldCorrectExpression()
    {
        // Arrange
        List<ILanguage> languages =
        [
            new LanguageModelFixture
            {
                Id = 1,
                Code = "en-US",
                IsDefault = true,
                Name ="English",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 2,
                Code = "tr-TR",
                IsDefault = false,
                Name ="Turkish",
                Supported = true,
            },
        ];
        List<HasTranslationEntityFixture> entities =
        [
            new HasTranslationEntityFixture
            {
                Id = 1,
                Translations =
                [
                    new TranslationEntityFixture{
                        Id = 1,
                        Name = "First",
                        Description = "First",
                        EntityId = 1,
                        LanguageId = 1
                    },
                    new TranslationEntityFixture{
                        Id = 2,
                        Name = "İlk",
                        Description = "İlk",
                        EntityId = 1,
                        LanguageId = 2
                    }
                ]
            }
        ];
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        var manager = new MilvaMultiLanguageManager();
        Expression<Func<HasTranslationEntityFixture, string>> expectedExpression = src => (src.Translations != null
                                                                                            ? src.Translations.FirstOrDefault(i => i.LanguageId == 2) == null
                                                                                                ? src.Translations.FirstOrDefault(i => i.LanguageId == 1) != null
                                                                                                        ? src.Translations.FirstOrDefault(i => i.LanguageId == 1).Name
                                                                                                        : src.Translations.FirstOrDefault().Name != null
                                                                                                            ? src.Translations.FirstOrDefault().Name
                                                                                                            : null
                                                                                                : src.Translations.FirstOrDefault(i => i.LanguageId == 2).Name
                                                                                            : null);

        // Act
        var resultExpression = manager.CreateTranslationMapExpression<HasTranslationEntityFixture, HasTranslationDtoFixture, TranslationEntityFixture>(e => e.Name);

        // Assert
        var resultWithExpectedExpression = entities.AsQueryable().Select(expectedExpression).First();
        var resultWithResultExpression = entities.AsQueryable().Select(resultExpression).First();

        resultWithResultExpression.Should().Be(resultWithExpectedExpression);
    }

    [Fact]
    public void CreateTranslationMapExpression_WithTranslationsIsNullAndCurrentOrDefaultAndCurrentLanguageAndDefaultLanguageIsSame_ShouldCorrectExpression()
    {
        // Arrange
        List<ILanguage> languages =
        [
            new LanguageModelFixture
            {
                Id = 1,
                Code = "en-US",
                IsDefault = true,
                Name ="English",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 2,
                Code = "tr-TR",
                IsDefault = false,
                Name ="Turkish",
                Supported = true,
            },
        ];
        List<HasTranslationEntityFixture> entities =
        [
            new HasTranslationEntityFixture
            {
                Id = 1,
                Translations = null
            }
        ];
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        var manager = new MilvaMultiLanguageManager();
        Expression<Func<HasTranslationEntityFixture, string>> expectedExpression = src => src.Translations != null
                                                                                            ? src.Translations.FirstOrDefault(i => i.LanguageId == 1) != null
                                                                                                ? src.Translations.FirstOrDefault(i => i.LanguageId == 1).Name
                                                                                                : src.Translations.FirstOrDefault().Name != null
                                                                                                    ? src.Translations.FirstOrDefault().Name
                                                                                                    : null
                                                                                            : null;

        // Act
        var resultExpression = manager.CreateTranslationMapExpression<HasTranslationEntityFixture, HasTranslationDtoFixture, TranslationEntityFixture>(e => e.Name);

        // Assert
        var resultWithExpectedExpression = entities.AsQueryable().Select(expectedExpression).First();
        var resultWithResultExpression = entities.AsQueryable().Select(resultExpression).First();

        resultWithResultExpression.Should().Be(resultWithExpectedExpression);
    }

    [Fact]
    public void CreateTranslationMapExpression_WithTranslationsNotContainCurrentOrDefaultAndCurrentLanguageAndDefaultLanguageIsSame_ShouldCorrectExpression()
    {
        // Arrange
        List<ILanguage> languages =
        [
            new LanguageModelFixture
            {
                Id = 1,
                Code = "en-US",
                IsDefault = true,
                Name ="English",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 2,
                Code = "tr-TR",
                IsDefault = false,
                Name ="Turkish",
                Supported = true,
            },
        ];
        List<HasTranslationEntityFixture> entities =
        [
            new HasTranslationEntityFixture
            {
                Id = 1,
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
                        LanguageId = 3
                    }
                ]
            }
        ];
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        var manager = new MilvaMultiLanguageManager();
        Expression<Func<HasTranslationEntityFixture, string>> expectedExpression = src => src.Translations != null
                                                                                            ? src.Translations.FirstOrDefault(i => i.LanguageId == 1) != null
                                                                                                ? src.Translations.FirstOrDefault(i => i.LanguageId == 1).Name
                                                                                                : src.Translations.FirstOrDefault().Name != null
                                                                                                    ? src.Translations.FirstOrDefault().Name
                                                                                                    : null
                                                                                            : null;

        // Act
        var resultExpression = manager.CreateTranslationMapExpression<HasTranslationEntityFixture, HasTranslationDtoFixture, TranslationEntityFixture>(e => e.Name);

        // Assert
        var resultWithExpectedExpression = entities.AsQueryable().Select(expectedExpression).First();
        var resultWithResultExpression = entities.AsQueryable().Select(resultExpression).First();

        resultWithResultExpression.Should().Be(resultWithExpectedExpression);
    }

    [Fact]
    public void CreateTranslationMapExpression_WithTranslationsContainCurrentOrDefaultAndCurrentLanguageAndDefaultLanguageIsSame_ShouldCorrectExpression()
    {
        // Arrange
        List<ILanguage> languages =
        [
            new LanguageModelFixture
            {
                Id = 1,
                Code = "en-US",
                IsDefault = true,
                Name ="English",
                Supported = true,
            },
            new LanguageModelFixture
            {
                Id = 2,
                Code = "tr-TR",
                IsDefault = false,
                Name ="Turkish",
                Supported = true,
            },
        ];
        List<HasTranslationEntityFixture> entities =
        [
            new HasTranslationEntityFixture
            {
                Id = 1,
                Translations =
                [
                    new TranslationEntityFixture{
                        Id = 1,
                        Name = "First",
                        Description = "First",
                        EntityId = 1,
                        LanguageId = 1
                    },
                    new TranslationEntityFixture{
                        Id = 2,
                        Name = "İlk",
                        Description = "İlk",
                        EntityId = 1,
                        LanguageId = 2
                    }
                ]
            }
        ];
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        var manager = new MilvaMultiLanguageManager();
        Expression<Func<HasTranslationEntityFixture, string>> expectedExpression = src => src.Translations != null
                                                                                            ? src.Translations.FirstOrDefault(i => i.LanguageId == 1) != null
                                                                                                ? src.Translations.FirstOrDefault(i => i.LanguageId == 1).Name
                                                                                                : src.Translations.FirstOrDefault().Name != null
                                                                                                    ? src.Translations.FirstOrDefault().Name
                                                                                                    : null
                                                                                            : null;

        // Act
        var resultExpression = manager.CreateTranslationMapExpression<HasTranslationEntityFixture, HasTranslationDtoFixture, TranslationEntityFixture>(e => e.Name);

        // Assert
        var resultWithExpectedExpression = entities.AsQueryable().Select(expectedExpression).First();
        var resultWithResultExpression = entities.AsQueryable().Select(resultExpression).First();

        resultWithResultExpression.Should().Be(resultWithExpectedExpression);
    }

    #endregion
}
