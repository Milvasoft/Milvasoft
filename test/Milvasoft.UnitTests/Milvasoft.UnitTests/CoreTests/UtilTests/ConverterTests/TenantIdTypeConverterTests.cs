using FluentAssertions;
using Milvasoft.Core.Utils.Converters;

namespace Milvasoft.UnitTests.CoreTests.UtilTests.ConverterTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class TenantIdTypeConverterTests
{
    private readonly TenantIdTypeConverter _converter = new();

    [Fact]
    public void CanConvertTo_WithComplexType_ShouldReturnsTrue()
    {
        // Arrange

        // Act
        var result = _converter.CanConvertTo(typeof(Exception));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanConvertFrom_WithComplexType_ShouldReturnsTrue()
    {
        // Arrange

        // Act
        var result = _converter.CanConvertFrom(typeof(Exception));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanConvertTo_WithObjectType_ShouldReturnsFalse()
    {
        // Arrange

        // Act
        var result = _converter.CanConvertTo(typeof(object));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanConvertFrom_WithObjectType_ShouldReturnsFalse()
    {
        // Arrange

        // Act
        var result = _converter.CanConvertFrom(typeof(object));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanConvertTo_WithStringType_ShouldReturnsTrue()
    {
        // Arrange

        // Act
        var result = _converter.CanConvertTo(typeof(string));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanConvertFrom_WithStringType_ShouldReturnsTrue()
    {
        // Arrange

        // Act
        var result = _converter.CanConvertFrom(typeof(string));

        // Assert
        result.Should().BeTrue();
    }
}
