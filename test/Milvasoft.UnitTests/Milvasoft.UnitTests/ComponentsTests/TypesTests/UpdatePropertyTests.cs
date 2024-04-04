using FluentAssertions;
using Milvasoft.Types.Structs;

namespace Milvasoft.UnitTests.ComponentsTests.TypesTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "<Pending>")]
public class UpdatePropertyTests
{
    [Fact]
    public void UpdateProperty_WithValueIsNull_ShouldIsUpdatedIsFalse()
    {
        // Arrange
        var updateProperty = new UpdateProperty<int>();

        // Act

        // Assert
        updateProperty.IsUpdated.Should().BeFalse();
    }

    [Fact]
    public void UpdateProperty_WithValue_ShouldReturnValue()
    {
        // Arrange
        var initialValue = 10;
        var updateProperty = new UpdateProperty<int>(initialValue);

        // Act
        var value = updateProperty.Value;

        // Assert
        value.Should().Be(initialValue);
    }

    [Fact]
    public void UpdateProperty_WithValue_ShouldBeUpdated()
    {
        // Arrange
        var initialValue = 10;
        var updatedValue = 20;
        var updateProperty = new UpdateProperty<int>(initialValue);

        // Act
        updateProperty.Value = updatedValue;

        // Assert
        updateProperty.Value.Should().Be(updatedValue);
    }

    [Fact]
    public void UpdateProperty_WithValue_ShouldReturnTrueInitially()
    {
        // Arrange
        var initialValue = 10;
        var updateProperty = new UpdateProperty<int>(initialValue);

        // Act
        var isUpdated = updateProperty.IsUpdated;

        // Assert
        isUpdated.Should().BeTrue();
    }

    [Fact]
    public void UpdateProperty_WithValueUpdate_ShouldBeTrueAfterValueUpdate()
    {
        // Arrange
        var initialValue = 10;
        var updatedValue = 20;
        var updateProperty = new UpdateProperty<int>(initialValue);
        updateProperty.IsUpdated = false;

        // Act
        updateProperty.Value = updatedValue;

        // Assert
        updateProperty.IsUpdated.Should().BeTrue();
    }

    [Fact]
    public void GetValue_ShouldReturnValueAsObject()
    {
        // Arrange
        var initialValue = 10;
        var updateProperty = new UpdateProperty<int>(initialValue);

        // Act
        var value = updateProperty.GetValue();

        // Assert
        value.Should().Be(initialValue);
    }

    [Fact]
    public void GetHashCode_ShouldReturnCombinedHashCode()
    {
        // Arrange
        var initialValue = 10;
        var updateProperty = new UpdateProperty<int>(initialValue);
        var expected = HashCode.Combine(initialValue, true);

        // Act
        var hashCode = updateProperty.GetHashCode();

        // Assert
        hashCode.Should().Be(expected.GetHashCode());
    }

    [Fact]
    public void Equals_WithComparingSamInstances_ShouldReturnTrue()
    {
        // Arrange
        var initialValue = 10;
        var updateProperty = new UpdateProperty<int>(initialValue);

        // Act
        var result = updateProperty.Equals(updateProperty);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithComparingEqualInstances_ShouldReturnTrue()
    {
        // Arrange
        var initialValue = 10;
        var updateProperty1 = new UpdateProperty<int>(initialValue);
        var updateProperty2 = new UpdateProperty<int>(initialValue);

        // Act
        var result = updateProperty1.Equals(updateProperty2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithComparingDifferentInstances_ShouldReturnFalse()
    {
        // Arrange
        var initialValue1 = 10;
        var initialValue2 = 20;
        var updateProperty1 = new UpdateProperty<int>(initialValue1);
        var updateProperty2 = new UpdateProperty<int>(initialValue2);

        // Act
        var result = updateProperty1.Equals(updateProperty2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateProperty_ImplicitConversionFromUpdatePropertyToValue_ShouldWorkCorrectly()
    {
        // Arrange
        var initialValue = 10;
        var updateProperty = new UpdateProperty<int>(initialValue);

        // Act
        int value = updateProperty;

        // Assert
        value.Should().Be(initialValue);
    }

    [Fact]
    public void ImplicitConversion_FromValueToUpdateProperty_ShouldWorkCorrectly()
    {
        // Arrange
        var initialValue = 10;

        // Act
        UpdateProperty<int> updateProperty = initialValue;

        // Assert
        updateProperty.Value.Should().Be(initialValue);
    }

    [Fact]
    public void EqualityOperator_WithValuesAreEqual_ShouldReturnTrue()
    {
        // Arrange
        var initialValue = 10;
        var updateProperty1 = new UpdateProperty<int>(initialValue);
        var updateProperty2 = new UpdateProperty<int>(initialValue);

        // Act
        var result = updateProperty1 == updateProperty2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithValuesAreNotEqual_ShouldReturnFalse()
    {
        // Arrange
        var initialValue1 = 10;
        var initialValue2 = 20;
        var updateProperty1 = new UpdateProperty<int>(initialValue1);
        var updateProperty2 = new UpdateProperty<int>(initialValue2);

        // Act
        var result = updateProperty1 == updateProperty2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void NotEqualOperator_WithValuesAreNotEqual_ShouldReturnTrue()
    {
        // Arrange
        var initialValue1 = 10;
        var initialValue2 = 20;
        var updateProperty1 = new UpdateProperty<int>(initialValue1);
        var updateProperty2 = new UpdateProperty<int>(initialValue2);

        // Act
        var result = updateProperty1 != updateProperty2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void NotEqualOperator_WithValuesAreEqual_ShouldReturnFalse()
    {
        // Arrange
        var initialValue = 10;
        var updateProperty1 = new UpdateProperty<int>(initialValue);
        var updateProperty2 = new UpdateProperty<int>(initialValue);

        // Act
        var result = updateProperty1 != updateProperty2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnStringRepresentationOfUpdateProperty()
    {
        // Arrange
        var initialValue = 10;
        var isUpdated = true;
        var updateProperty = new UpdateProperty<int>(initialValue) { IsUpdated = isUpdated };

        // Act
        var result = updateProperty.ToString();

        // Assert
        result.Should().Be($"Value : {initialValue} / Is Updated : {isUpdated}");
    }
}
