using FluentAssertions;
using Milvasoft.DataAccess.EfCore.Utils.LookupModels;

namespace Milvasoft.UnitTests.PropertyTests.DataAccess.EfCore;

[Trait("Property Getter Setters Unit Tests", "Library's models property getter setter unit tests.")]
public class LookupModelsTests
{
    [Fact]
    public void LookupModel_PropertiesGetterSetter_ShouldReturnCorrectValue()
    {
        // Arrange
        var somePropName = "somepropname";
        object someValue = 12;

        // Act
        LookupModel sut = new LookupModel
        {
            PropertyName = somePropName,
            PropertyValue = someValue
        };

        // Assert
        sut.PropertyName.Should().Be(somePropName);
        sut.PropertyValue.Should().Be(someValue);
    }

    [Fact]
    public void LookupRequest_UpdateFilterByForTranslationPropertyNames_PropertiesGetterSetter_ShouldReturnCorrectValue()
    {
        // Arrange
        var filterBy = "TranslationPropName";
        var sut = new LookupRequest
        {
            Parameters =
            [
                new()
                {
                    EntityName = nameof(LookupRequest),
                    Filtering = new Components.Rest.Request.FilterRequest{
                        Criterias =
                        [
                            new(){
                                 FilterBy = filterBy
                            }
                        ]
                    }
                }
            ]
        };

        // Act
        sut.Parameters[0].UpdateFilterByForTranslationPropertyNames([filterBy]);

        // Assert
        sut.Parameters[0].Filtering.Criterias[0].FilterBy.Should().Be($"Translations[{filterBy}]");
    }
}
