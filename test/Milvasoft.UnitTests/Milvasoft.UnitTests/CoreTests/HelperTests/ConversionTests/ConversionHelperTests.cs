using FluentAssertions;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.ConversionTests.Fixtures;
using System.Reflection;
using System.Text.Json;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.CommonTests;

public partial class ConversionHelperTests
{
    #region ToGuid

    [Fact]
    public void ToGuid_ValidInputWithDefaultJsonOptions_ShouldReturnsCorrectGuid()
    {
        // Arrange
        int input = 1;

        // Act
        var result = input.ToGuid();

        // Assert
        result.Should().Be("00000001-0000-0000-0000-000000000000");
    }

    #endregion

    #region ToJson

    [Fact]
    public void ToJson_NullObjectInputWithDefaultJsonOptions_ShouldReturnsInputString()
    {
        // Arrange
        ToJsonTestModelFixture input = null;

        // Act
        string result = input.ToJson();

        // Assert
        result.Should().Be("null");
    }

    [Fact]
    public void ToJson_SimpleObjectInputWithDefaultJsonOptions_ShouldReturnsCorrectJsonString()
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
    public void ToObject_WithGenericParameter_InvalidJsonWithDefaultJsonOptions_ShouldThrowsException()
    {
        // Arrange
        var input = "\"Name\":\"test\",\"Name\":1}";

        // Act
        Action act = () => input.ToObject<ToJsonTestModelFixture>();

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void ToObject_WithGenericParameter_NullOrEmptyOrWhitespacedStringInputWithDefaultJsonOptions_ShouldReturnsNull(string input)
    {
        // Arrange

        // Act
        var result = input.ToObject<ToJsonTestModelFixture>();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ToObject_WithGenericParameter_ValidJsonButPropertyNameMismatchWithDefaultJsonOptions_ShouldReturnsObjectWithDefaultProperties()
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
    public void ToObject_WithGenericParameter_ValidJsonWithValidTypeWithDefaultJsonOptions_ShouldReturnsMappedObject()
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
    public void ToObject_InvalidJsonWithDefaultJsonOptions_ShouldReturnsDefaultObject()
    {
        // Arrange
        var input = "\"Name\":\"test\",\"Name\":1}";

        // Act
        Action act = () => input.ToObject(typeof(ToJsonTestModelFixture));

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void ToObject_NullOrEmptyOrWhitespacedStringInputWithDefaultJsonOptions_ShouldReturnsNull(string input)
    {
        // Arrange

        // Act
        var result = input.ToObject(typeof(ToJsonTestModelFixture));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ToObject_ValidJsonButPropertyNameMismatchWithDefaultJsonOptions_ShouldReturnsObjectWithDefaultProperties()
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
    public void ToObject_ValidJsonWithValidTypeWithDefaultJsonOptions_ShouldReturnsMappedObject()
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
    public void Deserialize_InvalidTypeInput_ShouldThrowsException()
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
    public void Deserialize_ValidTypeInput_ShouldReturnsCorrectValue()
    {
        // Arrange
        var json = "{\"Name\":\"test\"}";

        var input = JsonSerializer.Deserialize<DeserializeTestModelFixture>(json);

        var valueKindObject = (JsonElement)input.Name;

        // Act
        var result = valueKindObject.Deserialize(typeof(string));

        // Assert
        result.GetType().Should().Be(typeof(string));
        result.Should().Be("test");
    }

    #endregion
}
