﻿using FluentAssertions;
using Milvasoft.Core.Utils.JsonConverters;
using Milvasoft.UnitTests.CoreTests.UtilTests.JsonConverterTests.Fixtures;
using Milvasoft.UnitTests.CoreTests.UtilTests.JsonConverterTests.Helpers;

namespace Milvasoft.UnitTests.CoreTests.UtilTests.JsonConverterTests;

public class InterfaceConverterTests
{
    private readonly InterfaceConverter<ClassImplementsInterfaceFixture, IInterfaceFixture> _converter = new();
    private readonly InterfaceConverter<GenericClassImplementsGenericInterfaceFixture<ClassFixture>, IGenericInterfaceFixture<ClassFixture>> _genericConverter = new();

    [Theory]
    [InlineData(typeof(ClassImplementsInterfaceFixture))]
    [InlineData(typeof(ClassFixture))]
    public void CanConvert_WithConcreteType_ShouldReturnsFalse(Type typeToConvert)
    {
        // Arrange

        // Act
        var result = _converter.CanConvert(typeToConvert);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(typeof(IInterfaceFixture), true)]
    [InlineData(typeof(IInterfaceNotImplementedFixture), false)]
    public void CanConvert_WithInterfaceType_ShouldReturnsExpected(Type typeToConvert, bool expected)
    {
        // Arrange

        // Act
        var result = _converter.CanConvert(typeToConvert);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(typeof(GenericClassImplementsGenericInterfaceFixture<>))]
    [InlineData(typeof(GenericClassImplementsGenericInterfaceFixture<ClassFixture>))]
    public void CanConvert_WithGenericConverterConcreteType_ShouldReturnsFalse(Type typeToConvert)
    {
        // Arrange

        // Act
        var result = _genericConverter.CanConvert(typeToConvert);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(typeof(IGenericInterfaceFixture<>), false)]
    [InlineData(typeof(IGenericInterfaceFixture<ClassFixture>), true)]
    public void CanConvert_WithGenericConverterInterfaceType_ShouldReturnsExpected(Type typeToConvert, bool expected)
    {
        // Arrange

        // Act
        var result = _genericConverter.CanConvert(typeToConvert);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CanConvert_WithObjectType_ShouldReturnsFalse()
    {
        // Arrange

        // Act
        var result = _converter.CanConvert(typeof(object));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Write_WithNullInput_ShouldReturnsEmtpyString()
    {
        // Arrange
        IInterfaceFixture input = null;

        // Act
        var result = _converter.Write(input);

        // Assert
        result.Should().Be("null");
    }

    [Fact]
    public void Write_WithValidInput_ShouldReturnsCorrectResult()
    {
        // Arrange
        IInterfaceFixture input = new ClassImplementsInterfaceFixture
        {
            Name = "John"
        };

        // Act
        var result = _converter.Write(input);

        // Assert
        result.Should().Be("{\"Name\":\"John\"}");
    }

    [Fact]
    public void Read_WithNullInput_ShouldReturnsCorrectResult()
    {
        // Arrange
        var input = "{}";

        // Act
        IInterfaceFixture result = _converter.Read(input);

        // Assert
        result.Should().BeOfType<ClassImplementsInterfaceFixture>();
        result.Name.Should().BeNull();
    }

    [Fact]
    public void Read_WithValidInput_ShouldReturnsCorrectResult()
    {
        // Arrange
        var input = "{\"Name\":\"John\"}";

        // Act
        IInterfaceFixture result = _converter.Read(input);

        // Assert
        result.Should().BeOfType<ClassImplementsInterfaceFixture>();
        result.Name.Should().Be("John");
    }

    [Fact]
    public void Read_WithValidGenericInput_ShouldReturnsCorrectResult()
    {
        // Arrange
        var input = "{\"Name\":\"John\",\"Class\":{\"Priority\": 1}}";

        // Act
        IGenericInterfaceFixture<ClassFixture> result = _genericConverter.Read(input);

        // Assert
        result.Should().BeOfType<GenericClassImplementsGenericInterfaceFixture<ClassFixture>>();
        result.Name.Should().Be("John");
    }
}
