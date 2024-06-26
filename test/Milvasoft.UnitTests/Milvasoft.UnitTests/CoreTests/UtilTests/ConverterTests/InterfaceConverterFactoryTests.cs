﻿using FluentAssertions;
using Milvasoft.Core.Utils.Converters;
using Milvasoft.UnitTests.CoreTests.UtilTests.ConverterTests.Fixtures;
using System.Text.Json;

namespace Milvasoft.UnitTests.CoreTests.UtilTests.ConverterTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
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
