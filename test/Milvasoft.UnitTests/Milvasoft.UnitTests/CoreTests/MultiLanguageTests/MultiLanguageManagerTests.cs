using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.MultiLanguage.Builder;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.Manager;
using Milvasoft.UnitTests.CoreTests.MultiLanguageTests.Fixtures;
using System.Globalization;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.CoreTests.MultiLanguageTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3358:Ternary operators should not be nested", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1860:Avoid using 'Enumerable.Any()' extension method", Justification = "<Pending>")]
[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
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
    public void CreateTranslationMapExpression_WithTranslationsIsEmptyAndCurrentOrDefaultAndCurrentLanguageAndDefaultLanguageIsDifferent_ShouldCorrectExpression()
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
                Translations = []
            }
        ];
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        var manager = new MilvaMultiLanguageManager();
        Expression<Func<HasTranslationEntityFixture, string>> expectedExpression = src => src.Translations.Any()
                                                                                                ? (!src.Translations.Any(i => i.LanguageId == 2)
                                                                                                    ? src.Translations.Any(i => i.LanguageId == 1)
                                                                                                            ? src.Translations.FirstOrDefault(i => i.LanguageId == 1).Name
                                                                                                            : src.Translations.Any()
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
        Expression<Func<HasTranslationEntityFixture, string>> expectedExpression = src => src.Translations.Any()
                                                                                            ? !src.Translations.Any(i => i.LanguageId == 2)
                                                                                                ? src.Translations.Any(i => i.LanguageId == 1)
                                                                                                        ? src.Translations.FirstOrDefault(i => i.LanguageId == 1).Name
                                                                                                        : src.Translations.Any()
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
        Expression<Func<HasTranslationEntityFixture, string>> expectedExpression = src => (src.Translations.Any()
                                                                                            ? !src.Translations.Any(i => i.LanguageId == 2)
                                                                                                ? src.Translations.Any(i => i.LanguageId == 1)
                                                                                                        ? src.Translations.FirstOrDefault(i => i.LanguageId == 1).Name
                                                                                                        : src.Translations.Any()
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
    public void CreateTranslationMapExpression_WithTranslationsIsEmptyAndCurrentOrDefaultAndCurrentLanguageAndDefaultLanguageIsSame_ShouldCorrectExpression()
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
                Translations = []
            }
        ];
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        var manager = new MilvaMultiLanguageManager();
        Expression<Func<HasTranslationEntityFixture, string>> expectedExpression = src => src.Translations.Any()
                                                                                            ? src.Translations.Any(i => i.LanguageId == 1)
                                                                                                ? src.Translations.FirstOrDefault(i => i.LanguageId == 1).Name
                                                                                                : src.Translations.Any()
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
        Expression<Func<HasTranslationEntityFixture, string>> expectedExpression = src => src.Translations.Any()
                                                                                            ? src.Translations.Any(i => i.LanguageId == 1)
                                                                                                ? src.Translations.FirstOrDefault(i => i.LanguageId == 1).Name
                                                                                                : src.Translations.Any()
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
        Expression<Func<HasTranslationEntityFixture, string>> expectedExpression = src => src.Translations.Any()
                                                                                            ? src.Translations.Any(i => i.LanguageId == 1)
                                                                                                ? src.Translations.FirstOrDefault(i => i.LanguageId == 1).Name
                                                                                                : src.Translations.Any()
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

    #region GetTranslationPropertyValue

    [Fact]
    public void GetTranslationPropertyValue_WithObjectIsNull_ShouldReturnEmptyString()
    {
        // Arrange
        object obj = null;
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(obj, nameof(TranslationEntityFixture.Name));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTranslationPropertyValue_WithObjectNotImplementsIHasTranslationsInterface_ShouldReturnEmptyString()
    {
        // Arrange
        object obj = new { Translations = new List<TranslationEntityFixture>() };
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(obj, nameof(TranslationEntityFixture.Name));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTranslationPropertyValue_WithValidObjectButTranslationsIsNull_ShouldReturnEmptyString()
    {
        // Arrange
        HasTranslationEntityFixture obj = new()
        {
            Translations = null
        };
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(obj, nameof(TranslationEntityFixture.Name));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTranslationPropertyValue_WithValidObjectButTranslationsIsEmpty_ShouldReturnEmptyString()
    {
        // Arrange
        HasTranslationEntityFixture obj = new()
        {
            Translations = new List<TranslationEntityFixture>()
        };
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(obj, nameof(TranslationEntityFixture.Name));

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetTranslationPropertyValue_WithValidObjectButPropetyNameIsNullOrEmptyOrWhiteSpace_ShouldReturnEmptyString(string propertyName)
    {
        // Arrange
        var obj = new HasTranslationEntityFixture
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
        };
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(obj, propertyName);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("name")]
    public void GetTranslationPropertyValue_WithValidObjectAndPropertyName_ShouldReturnCorrectTranslation(string propertyName)
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
        var obj = new HasTranslationEntityFixture
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
        };
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(obj, propertyName);

        // Assert
        result.Should().Be("First");
    }

    #endregion

    #region GetTranslation

    [Fact]
    public void GetTranslation_WithTranslationsIsNull_ShouldReturnEmptyString()
    {
        // Arrange
        List<TranslationEntityFixture> translations = null;
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, nameof(TranslationEntityFixture.Name));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTranslation_WithTranslationsIsEmpty_ShouldReturnEmptyString()
    {
        // Arrange
        var translations = new List<TranslationEntityFixture>();
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, nameof(TranslationEntityFixture.Name));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTranslation_WithEntityNotImplementsITranslationEntityInterface_ShouldReturnEmptyString()
    {
        // Arrange
        var translations = new List<NonTranslationEntityFixture>()
        {
            new()
            {
                 Id = 1,
                 Name = "First",
                 Description = "First"
            }
        };
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, nameof(NonTranslationEntityFixture.Name));

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetTranslation_WithValidTranslationsButPropetyNameIsNullOrEmptyOrWhiteSpace_ShouldReturnEmptyString(string propertyName)
    {
        // Arrange
        var translations = new List<TranslationEntityFixture>
        {
            new(){
                Id = 1,
                Name = "First",
                Description = "First",
                EntityId = 1,
                LanguageId = 1
            },
            new()
            {
                Id = 2,
                Name = "İlk",
                Description = "İlk",
                EntityId = 1,
                LanguageId = 2
            }
        };
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, propertyName);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("name")]
    public void GetTranslation_WithValidTranslationsAndPropertyName_ShouldReturnCorrectTranslation(string propertyName)
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
        var translations = new List<TranslationEntityFixture>
        {
            new(){
                Id = 1,
                Name = "First",
                Description = "First",
                EntityId = 1,
                LanguageId = 1
            },
            new()
            {
                Id = 2,
                Name = "İlk",
                Description = "İlk",
                EntityId = 1,
                LanguageId = 2
            }
        };
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, propertyName);

        // Assert
        result.Should().Be("First");
    }

    [Fact]
    public void GetTranslation_WithTranslationsOnlyContainsDefaultLanguageAndCurrentAndDefaultLanguageIsDifferent_ShouldReturnCorrectTranslation()
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
        var translations = new List<TranslationEntityFixture>
        {
            new(){
                Id = 1,
                Name = "First",
                Description = "First",
                EntityId = 1,
                LanguageId = 1
            }
        };
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, nameof(TranslationEntityFixture.Name));

        // Assert
        result.Should().Be("First");
    }

    [Fact]
    public void GetTranslation_WithTranslationsNotContainsCurrentOrDefaultLanguageAndCurrentAndDefaultLanguageIsDifferent_ShouldReturnCorrectTranslation()
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
        var translations = new List<TranslationEntityFixture>
        {
            new(){
                Id = 1,
                Name = "соответствие",
                Description = "соответствие",
                EntityId = 1,
                LanguageId = 4
            }
        };
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, nameof(TranslationEntityFixture.Name));

        // Assert
        result.Should().Be("соответствие");
    }

    [Fact]
    public void GetTranslation_WithTranslationsOnlyContainsDefaultLanguageAndCurrentAndDefaultLanguageIsSame_ShouldReturnCorrectTranslation()
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
        var translations = new List<TranslationEntityFixture>
        {
            new(){
                Id = 1,
                Name = "First",
                Description = "First",
                EntityId = 1,
                LanguageId = 1
            }
        };
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, nameof(TranslationEntityFixture.Name));

        // Assert
        result.Should().Be("First");
    }

    [Fact]
    public void GetTranslation_WithTranslationsNotContainsCurrentOrDefaultLanguageAndCurrentAndDefaultLanguageIsSame_ShouldReturnCorrectTranslation()
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
        var translations = new List<TranslationEntityFixture>
        {
            new(){
                Id = 1,
                Name = "соответствие",
                Description = "соответствие",
                EntityId = 1,
                LanguageId = 4
            }
        };
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, nameof(TranslationEntityFixture.Name));

        // Assert
        result.Should().Be("соответствие");
    }

    #endregion

    #region GetTranslationValue

    [Fact]
    public void GetTranslationValue_WithTranslationsIsNull_ShouldReturnEmptyString()
    {
        // Arrange
        List<TranslationEntityFixture> translations = null;
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, i => i.Name);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTranslationValue_WithTranslationsIsEmpty_ShouldReturnEmptyString()
    {
        // Arrange
        var translations = new List<TranslationEntityFixture>();
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, i => i.Name);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTranslationValue_WithEntityNotImplementsITranslationEntityInterface_ShouldReturnEmptyString()
    {
        // Arrange
        var translations = new List<NonTranslationEntityFixture>()
        {
            new()
            {
                 Id = 1,
                 Name = "First",
                 Description = "First"
            }
        };
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, i => i.Name);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTranslationValue_WithValidTranslationsButPropetyExpressionIsNull_ShouldReturnEmptyString()
    {
        // Arrange
        var translations = new List<TranslationEntityFixture>
        {
            new(){
                Id = 1,
                Name = "First",
                Description = "First",
                EntityId = 1,
                LanguageId = 1
            },
            new()
            {
                Id = 2,
                Name = "İlk",
                Description = "İlk",
                EntityId = 1,
                LanguageId = 2
            }
        };
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, (string)null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTranslationValue_WithValidTranslationsAndPropertyExpression_ShouldReturnCorrectTranslation()
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
        var translations = new List<TranslationEntityFixture>
        {
            new(){
                Id = 1,
                Name = "First",
                Description = "First",
                EntityId = 1,
                LanguageId = 1
            },
            new()
            {
                Id = 2,
                Name = "İlk",
                Description = "İlk",
                EntityId = 1,
                LanguageId = 2
            }
        };
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, i => i.Name);

        // Assert
        result.Should().Be("First");
    }

    [Fact]
    public void GetTranslationValue_WithTranslationsOnlyContainsDefaultLanguageAndCurrentAndDefaultLanguageIsDifferent_ShouldReturnCorrectTranslation()
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
        var translations = new List<TranslationEntityFixture>
        {
            new(){
                Id = 1,
                Name = "First",
                Description = "First",
                EntityId = 1,
                LanguageId = 1
            }
        };
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, i => i.Name);

        // Assert
        result.Should().Be("First");
    }

    [Fact]
    public void GetTranslationValue_WithTranslationsNotContainsCurrentOrDefaultLanguageAndCurrentAndDefaultLanguageIsDifferent_ShouldReturnCorrectTranslation()
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
        var translations = new List<TranslationEntityFixture>
        {
            new(){
                Id = 1,
                Name = "соответствие",
                Description = "соответствие",
                EntityId = 1,
                LanguageId = 4
            }
        };
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, i => i.Name);

        // Assert
        result.Should().Be("соответствие");
    }

    [Fact]
    public void GetTranslationValue_WithTranslationsOnlyContainsDefaultLanguageAndCurrentAndDefaultLanguageIsSame_ShouldReturnCorrectTranslation()
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
        var translations = new List<TranslationEntityFixture>
        {
            new(){
                Id = 1,
                Name = "First",
                Description = "First",
                EntityId = 1,
                LanguageId = 1
            }
        };
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, i => i.Name);

        // Assert
        result.Should().Be("First");
    }

    [Fact]
    public void GetTranslationValue_WithTranslationsNotContainsCurrentOrDefaultLanguageAndCurrentAndDefaultLanguageIsSame_ShouldReturnCorrectTranslation()
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
        var translations = new List<TranslationEntityFixture>
        {
            new(){
                Id = 1,
                Name = "соответствие",
                Description = "соответствие",
                EntityId = 1,
                LanguageId = 4
            }
        };
        MultiLanguageManager.UpdateLanguagesList(languages);
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        var manager = new MilvaMultiLanguageManager();

        // Act
        var result = manager.GetTranslation(translations, i => i.Name);

        // Assert
        result.Should().Be("соответствие");
    }

    #endregion

    #region Builder Tests

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

    #endregion
}
