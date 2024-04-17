using FluentAssertions;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.Core.Utils.Converters;
using Milvasoft.Types.Structs;
using Moq;
using System.ComponentModel;
using System.Globalization;

namespace Milvasoft.UnitTests.CoreTests.UtilTests.ConverterTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class TenantIdTypeConverterTests
{
    private readonly TenantIdTypeConverter _converter = new();

    [Fact]
    public void CanConvertTo_WithNonStringComplexType_ShouldReturnsTrue()
    {
        // Arrange

        // Act
        var result = _converter.CanConvertTo(typeof(Exception));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanConvertFrom_WithNonStringComplexType_ShouldReturnsTrue()
    {
        // Arrange

        // Act
        var result = _converter.CanConvertFrom(typeof(Exception));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanConvertTo_WithNonStringObjectType_ShouldReturnsFalse()
    {
        // Arrange

        // Act
        var result = _converter.CanConvertTo(typeof(object));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanConvertFrom_WithNonStringObjectType_ShouldReturnsFalse()
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

    [Fact]
    public void ConvertFrom_WithNonStringType_ShouldThrowsException()
    {
        // Arrange
        var mockValidator = new Mock<ITypeDescriptorContext>();
        var convertFrom = 1;

        // Act
        Action act = () => _converter.ConvertFrom(mockValidator.Object, CultureInfo.CurrentCulture, convertFrom);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ConvertFrom_WithStringType_ShouldReturnsTrue()
    {
        // Arrange
        var mockValidator = new Mock<ITypeDescriptorContext>();
        var convertFrom = "milvasoft_1";
        var expected = TenantId.Parse(convertFrom);

        // Act
        var result = _converter.ConvertFrom(mockValidator.Object, CultureInfo.CurrentCulture, convertFrom);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertTo_WithNonStringType_ShouldThrowsException()
    {
        // Arrange
        var mockValidator = new Mock<ITypeDescriptorContext>();
        var convertTo = 1;
        var destinationType = typeof(UpdateProperty<string>);

        // Act
        Action act = () => _converter.ConvertTo(mockValidator.Object, CultureInfo.CurrentCulture, convertTo, destinationType);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ConvertTo_WithStringType_ShouldReturnsTrue()
    {
        // Arrange
        var mockValidator = new Mock<ITypeDescriptorContext>();
        var expected = "milvasoft_1";
        var convertTo = new TenantId(expected);
        var destinationType = typeof(string);

        // Act
        var result = _converter.ConvertTo(mockValidator.Object, CultureInfo.CurrentCulture, convertTo, destinationType);

        // Assert
        result.Should().Be(expected);
    }
}
