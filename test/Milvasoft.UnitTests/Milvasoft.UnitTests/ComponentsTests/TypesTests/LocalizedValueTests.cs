using FluentAssertions;
using Milvasoft.Types.Classes;

namespace Milvasoft.UnitTests.ComponentsTests.TypesTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "<Pending>")]
[Trait("Types Components Unit Tests", "Milvasoft.Components.Types project unit tests.")]
public class LocalizedValueTests
{
    [Fact]
    public void LocalizedValue_WithNullKeyImplicitConversionToString_ShouldReturnStringValue()
    {
        // Arrange

        // Act
        Action act = () => new LocalizedValue(null, "value");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void LocalizedValue_WithImplicitConversionToString_ShouldReturnStringValue()
    {
        // Arrange
        var localizedValue = new LocalizedValue("key", "value");

        // Act
        string result = localizedValue;

        // Assert
        result.Should().Be(localizedValue.Value);
    }

    [Fact]
    public void LocalizedValue_WithToString_ShouldReturnStringValue()
    {
        // Arrange
        var localizedValue = new LocalizedValue("key", "value");

        // Act
        string result = localizedValue.ToString();

        // Assert
        result.Should().Be(localizedValue.Value);
    }
}
