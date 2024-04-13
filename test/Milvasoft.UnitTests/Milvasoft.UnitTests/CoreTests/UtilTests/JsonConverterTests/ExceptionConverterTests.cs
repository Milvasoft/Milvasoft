using FluentAssertions;
using Milvasoft.Core.Utils.JsonConverters;
using Milvasoft.UnitTests.CoreTests.UtilTests.JsonConverterTests.Helpers;

namespace Milvasoft.UnitTests.CoreTests.UtilTests.JsonConverterTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class ExceptionConverterTests
{
    private readonly ExceptionConverter<Exception> _converter = new();

    [Fact]
    public void CanConvert_WithExceptionType_ShouldReturnsTrue()
    {
        // Arrange

        // Act
        var result = _converter.CanConvert(typeof(Exception));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanConvert_WithNonExceptionType_ShouldReturnsFalse()
    {
        // Arrange

        // Act
        var result = _converter.CanConvert(typeof(object));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Write_WithNullException_ShouldReturnsEmptyString()
    {
        // Arrange
        var exception = new Exception("Test Exception");
        exception.Data["CustomData"] = "Custom Value";
        exception.HelpLink = "https://example.com";
        exception.Source = "Test Source";

        // Act
        var result = _converter.Write(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Write_WithValidException_ShouldReturnsSerializedExceptionWithOnlySerializablePropertiesJsonString()
    {
        // Arrange
        var exception = new Exception("Test Exception");
        exception.Data["CustomData"] = "Custom Value";
        exception.HelpLink = "https://example.com";
        exception.Source = "Test Source";

        // Act
        var result = _converter.Write(exception);

        // Assert
        result.Should().Be("{\"Message\":\"Test Exception\",\"InnerException\":null,\"StackTrace\":null}");
    }

    [Fact]
    public void Read_WithNullInput_ShouldReturnsCorrectResult()
    {
        // Arrange
        string input = "{}";

        // Act
        var result = _converter.Read(input);

        // Assert
        result.Should().BeOfType<Exception>();
        result.Source.Should().BeNull();
    }

    [Fact]
    public void Read_WithValidInput_ShouldReturnsCorrectResult()
    {
        // Arrange
        var input = "{\"Message\":\"Test Exception\",\"HelpLink\":\"Test Exception\",\"StackTrace\":null}";

        // Act
        var result = _converter.Read(input);

        // Assert
        result.Should().BeOfType<Exception>();
        result.HelpLink.Should().Be("Test Exception");
    }
}
