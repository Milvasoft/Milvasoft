using FluentAssertions;
using Milvasoft.Core.Utils.JsonConverters;
using Milvasoft.UnitTests.CoreTests.UtilTests.JsonConverterTests.Fixtures;
using Milvasoft.UnitTests.CoreTests.UtilTests.JsonConverterTests.Helpers;

namespace Milvasoft.UnitTests.CoreTests.UtilTests.JsonConverterTests;

public class InterfaceConverterTests
{
    private readonly InterfaceConverter<ClassImplementsInterfaceFixture, IInterfaceFixture> _converter = new();

    [Fact]
    public void CanConvert_WithValidTypes_ShouldReturnsTrue()
    {
        // Arrange

        // Act
        var result = _converter.CanConvert(typeof(ClassImplementsInterfaceFixture));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanConvert_WithInvalidTypes_ShouldReturnsFalse()
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
    public void Write_WithValidInput_ShouldReturnsEmtpyString()
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
}
