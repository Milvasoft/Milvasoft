using FluentAssertions;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.ConversionTests.Fixtures;
using System.Reflection;
using System.Text.Json;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.ConversionTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public partial class ConversionHelperTests
{
    #region ToGuid

    [Fact]
    public void ToGuid_WithValidInputWithDefaultJsonOptions_ShouldReturnCorrectGuid()
    {
        // Arrange
        var input = 1;

        // Act
        var result = input.ToGuid();

        // Assert
        result.Should().Be("00000001-0000-0000-0000-000000000000");
    }

    #endregion

    #region ToJson

    [Fact]
    public void ToJson_WithNullObjectInputWithDefaultJsonOptions_ShouldReturnInputString()
    {
        // Arrange
        ToJsonTestModelFixture input = null;

        // Act
        var result = input.ToJson();

        // Assert
        result.Should().Be("null");
    }

    [Fact]
    public void ToJson_WithSimpleObjectInputWithDefaultJsonOptions_ShouldReturnCorrectJsonString()
    {
        // Arrange
        var input = new ToJsonTestModelFixture()
        {
            Name = "test",
            Priority = 1,
        };

        // Act
        var result = input.ToJson();

        // Assert
        result.Should().Be("{\"Name\":\"test\",\"Priority\":1}");
    }

    #endregion

    #region ToObject

    [Fact]
    public void ToObject_ForOverloadWithGenericParameter_WithInvalidJsonWithDefaultJsonOptions_ShouldThrowsException()
    {
        // Arrange
        var input = "\"Name\":\"test\",\"Name\":2}";

        // Act
        Action act = () => input.ToObject<ToJsonTestModelFixture>();

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void ToObject_ForOverloadWithGenericParameter_WithNullOrEmptyOrWhitespacedStringInputWithDefaultJsonOptions_ShouldReturnNull(string input)
    {
        // Arrange

        // Act
        var result = input.ToObject<ToJsonTestModelFixture>();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ToObject_ForOverloadWithGenericParameter_WithValidJsonButPropertyNameMismatchWithDefaultJsonOptions_ShouldReturnObjectWithDefaultProperties()
    {
        // Arrange
        var input = "{\"InvalidProp1\":\"test\",\"InvalidProp2\":1}";

        // Act
        var result = input.ToObject<ToJsonTestModelFixture>();

        // Assert
        result.Name.Should().BeNull();
        result.Priority.Should().Be(0);
    }

    [Fact]
    public void ToObject_ForOverloadWithGenericParameter_WithValidJsonWithValidTypeWithDefaultJsonOptions_ShouldReturnMappedObject()
    {
        // Arrange
        var input = "{\"Name\":\"test\",\"Priority\":1}";

        // Act
        var result = input.ToObject<ToJsonTestModelFixture>();

        // Assert
        result.Name.Should().Be("test");
        result.Priority.Should().Be(1);
    }

    [Fact]
    public void ToObject_WithInvalidJsonWithDefaultJsonOptions_ShouldReturnDefaultObject()
    {
        // Arrange
        var input = "\"Name\":\"test\",\"Name\":2}";

        // Act
        Action act = () => input.ToObject(typeof(ToJsonTestModelFixture));

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void ToObject_WithNullOrEmptyOrWhitespacedStringInputWithDefaultJsonOptions_ShouldReturnNull(string input)
    {
        // Act & Arrange
        var result = input.ToObject(typeof(ToJsonTestModelFixture));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ToObject_WithValidJsonButPropertyNameMismatchWithDefaultJsonOptions_ShouldReturnObjectWithDefaultProperties()
    {
        // Arrange
        var input = "{\"InvalidProp1\":\"test\",\"InvalidProp2\":1}";

        // Act
        var result = (ToJsonTestModelFixture)input.ToObject(typeof(ToJsonTestModelFixture));

        // Assert
        result.Name.Should().BeNull();
        result.Priority.Should().Be(0);
    }

    [Fact]
    public void ToObject_WithValidJsonWithValidTypeWithDefaultJsonOptions_ShouldReturnMappedObject()
    {
        // Arrange
        var input = "{\"Name\":\"test\",\"Priority\":1}";

        // Act
        var result = (ToJsonTestModelFixture)input.ToObject(typeof(ToJsonTestModelFixture));

        // Assert
        result.Name.Should().Be("test");
        result.Priority.Should().Be(1);
    }

    #endregion

    #region Deserialize

    [Fact]
    public void Deserialize_WithInvalidTypeInput_ShouldThrowsException()
    {
        // Arrange
        var json = "{\"Name\":\"test\"}";

        var input = JsonSerializer.Deserialize<DeserializeTestModelFixture>(json);

        var valueKindObject = (JsonElement)input.Name;

        // Act
        Action act = () => valueKindObject.Deserialize(typeof(int));

        // Assert
        act.Should().Throw<TargetInvocationException>();
    }

    [Fact]
    public void Deserialize_WithValidTypeInput_ShouldReturnCorrectValue()
    {
        // Arrange
        var json = "{\"Name\":\"test\"}";

        var input = JsonSerializer.Deserialize<DeserializeTestModelFixture>(json);

        var valueKindObject = (JsonElement)input.Name;

        // Act
        var result = valueKindObject.Deserialize(typeof(string));

        // Assert
        result.GetType().Should().Be<string>();
        result.Should().Be("test");
    }

    #endregion
}
