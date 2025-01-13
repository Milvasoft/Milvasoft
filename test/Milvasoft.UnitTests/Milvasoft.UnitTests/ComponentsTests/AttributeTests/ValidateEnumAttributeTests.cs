using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Validation;
using Milvasoft.Core.Abstractions.Localization;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace Milvasoft.UnitTests.ComponentsTests.AttributeTests;

[Trait("Rest Components Unit Tests", "Milvasoft.Components.Rest project unit tests.")]
public class ValidateEnumAttributeTests
{
    private enum TestEnumFixture
    {
        Value1,
        Value2,
        Value3
    }

    private class TestModel
    {
        [ValidateEnum(typeof(TestEnumFixture), LocalizeErrorMessages = true, LocalizerKey = "TestKey", FullMessage = false)]
        public TestEnumFixture? EnumProperty { get; set; }
    }

    [Fact]
    public void ValidateEnumAttribute_ShouldReturnSuccess_ForValidEnumValue()
    {
        // Arrange
        var model = new TestModel { EnumProperty = TestEnumFixture.Value1 };
        var validationContext = CreateValidationContext(model);

        var attribute = new ValidateEnumAttribute(typeof(TestEnumFixture));

        // Act
        var result = attribute.GetValidationResult(model.EnumProperty, validationContext);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void ValidateEnumAttribute_ShouldReturnError_ForInvalidEnumValue()
    {
        // Arrange
        var model = new TestModel { EnumProperty = (TestEnumFixture)999 };
        var validationContext = CreateValidationContext(model);

        var attribute = new ValidateEnumAttribute(typeof(TestEnumFixture));

        // Act
        var result = attribute.GetValidationResult(model.EnumProperty, validationContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Please enter a valid .", result.ErrorMessage);
    }

    [Fact]
    public void ValidateEnumAttribute_ShouldReturnError_WhenValueIsNotEnum()
    {
        // Arrange
        var model = new { EnumProperty = 999 }; // Not a valid enum
        var validationContext = CreateValidationContext(model);

        var attribute = new ValidateEnumAttribute(typeof(TestEnumFixture));

        // Act
        var result = attribute.GetValidationResult(model.EnumProperty, validationContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Please enter a valid .", result.ErrorMessage);
    }

    [Fact]
    public void ValidateEnumAttribute_ShouldUseLocalizedErrorMessage_WhenLocalizationIsEnabled()
    {
        // Arrange
        var mockLocalizer = new Mock<IMilvaLocalizer>();
        mockLocalizer.Setup(l => l["TestKey"]).Returns(new Types.Classes.LocalizedValue("TestKey", "Localized error message"));

        var serviceProvider = new ServiceCollection()
            .AddSingleton(mockLocalizer.Object)
            .BuildServiceProvider();

        var validationContext = new ValidationContext(new object(), serviceProvider, null);
        var attribute = new ValidateEnumAttribute(typeof(TestEnumFixture))
        {
            LocalizeErrorMessages = true,
            LocalizerKey = "TestKey",
            FullMessage = true,
        };

        // Act
        var result = attribute.GetValidationResult((TestEnumFixture)999, validationContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Localized error message", result.ErrorMessage);
    }

    [Fact]
    public void ValidateEnumAttribute_ShouldReturnSuccess_WhenValueIsNull()
    {
        // Arrange
        var model = new TestModel { EnumProperty = null };
        var validationContext = CreateValidationContext(model);

        var attribute = new ValidateEnumAttribute(typeof(TestEnumFixture));

        // Act
        var result = attribute.GetValidationResult(model.EnumProperty, validationContext);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    private static ValidationContext CreateValidationContext(object model)
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        return new ValidationContext(model, serviceProvider, null);
    }
}
