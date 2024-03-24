using FluentAssertions;
using Milvasoft.Core.Utils.JsonConverters;
using Milvasoft.UnitTests.CoreTests.UtilTests.JsonConverterTests.Fixtures;
using System.Text.Json;

namespace Milvasoft.UnitTests.CoreTests.UtilTests.JsonConverterTests;

public class InterfaceConverterFactoryTests
{
    [Fact]
    public void CanConvert_WithTypeToConvertIsInterfaceType_ShouldReturnTrue()
    {
        // Arrange
        var implementationType = typeof(ClassImplementsInterfaceFixture);
        var interfaceType = typeof(IInterfaceFixture);
        var converterFactory = new InterfaceConverterFactory(implementationType, interfaceType);
        var typeToConvert = interfaceType;

        // Act
        var result = converterFactory.CanConvert(typeToConvert);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanConvert_WithTypeToConvertIsAssignableToInterfaceTypeAndIsGenericType_ShouldReturnTrue()
    {
        // Arrange
        var implementationType = typeof(GenericClassImplementsGenericInterfaceFixture<>);
        var interfaceType = typeof(IGenericInterfaceFixture<>);
        var q = implementationType.IsAssignableTo(interfaceType);
        var converterFactory = new InterfaceConverterFactory(implementationType, interfaceType);
        var typeToConvert = typeof(GenericClassImplementsGenericInterfaceFixture<ClassFixture>);

        // Act
        var result = converterFactory.CanConvert(typeToConvert);

        // Assert
        result.Should().BeTrue();
    }



    [Fact]
    public void CanConvert_WithTypeToConvertIsNotAssignableToInterfaceType_ShouldReturnFalse()
    {
        // Arrange
        var implementationType = typeof(ClassImplementsInterfaceFixture);
        var interfaceType = typeof(IInterfaceFixture);
        var converterFactory = new InterfaceConverterFactory(implementationType, interfaceType);
        var typeToConvert = typeof(ClassFixture);

        // Act
        var result = converterFactory.CanConvert(typeToConvert);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CreateConverter_WithTypeToConvertIsGenericTypeAndImplementationTypeIsGenericType_ShouldReturnInterfaceConverter()
    {
        // Arrange
        var implementationType = typeof(ClassImplementsInterfaceFixture);
        var interfaceType = typeof(IInterfaceFixture);
        var converterFactory = new InterfaceConverterFactory(implementationType, interfaceType);
        var typeToConvert = typeof(GenericClassImplementsGenericInterfaceFixture<>).MakeGenericType(interfaceType);
        var options = new JsonSerializerOptions();

        // Act
        var converter = converterFactory.CreateConverter(typeToConvert, options);

        // Assert
        converter.Should().BeOfType<InterfaceConverter<ClassImplementsInterfaceFixture, IInterfaceFixture>>();
    }

    [Fact]
    public void CreateConverter_WithTypeToConvertIsNotGenericTypeOrImplementationTypeIsNotGenericType_ShouldReturnInterfaceConverter()
    {
        // Arrange
        var implementationType = typeof(ClassImplementsInterfaceFixture);
        var interfaceType = typeof(IInterfaceFixture);
        var converterFactory = new InterfaceConverterFactory(implementationType, interfaceType);
        var typeToConvert = interfaceType;
        var options = new JsonSerializerOptions();

        // Act
        var converter = converterFactory.CreateConverter(typeToConvert, options);

        // Assert
        converter.Should().BeOfType<InterfaceConverter<ClassImplementsInterfaceFixture, IInterfaceFixture>>();
    }
}
