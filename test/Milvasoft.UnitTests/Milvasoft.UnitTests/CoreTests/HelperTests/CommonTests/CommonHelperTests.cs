using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.CommonTests.Fixtures;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.CommonTests;

public partial class CommonHelperTests
{
    #region CreateIsDeletedFalseExpression

    [Fact]
    public void CreateIsDeletedFalseExpression_EntityTypeIsNotSoftDeletable_ShouldReturnsNull()
    {
        // Arrange

        // Act
        var result = CommonHelper.CreateIsDeletedFalseExpression<string>();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CreateIsDeletedFalseExpression_EntityTypeIsSoftDeletable_ShouldReturnsIsDeletedFalseExpression()
    {
        // Arrange
        Expression<Func<SoftDeletableTestEntity, bool>> expected = e => e.IsDeleted == false;

        // Act
        var result = CommonHelper.CreateIsDeletedFalseExpression<SoftDeletableTestEntity>();

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expected, result);
        equality.Should().BeTrue();
    }

    #endregion

    #region GetEnumDesciption

    [Fact]
    public void GetEnumDescription_TypeIsNotEnum_ShouldReturnsNull()
    {
        // Arrange
        int input = 1;

        // Act
        var result = input.GetEnumDesciption();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetEnumDescription_TypeIsEnumButNotContainsDescriptionAttribute_ShouldReturnsEnumValueAsString()
    {
        // Arrange
        var input = TestEnum.Value2;

        // Act
        var result = input.GetEnumDesciption();

        // Assert
        result.Should().Be(TestEnum.Value2.ToString());
    }

    [Fact]
    public void GetEnumDescription_TypeIsEnumAndContainsDescriptionAttribute_ShouldReturnsDescriptionAttributeValue()
    {
        // Arrange
        var input = TestEnum.Value1;

        // Act
        var result = input.GetEnumDesciption();

        // Assert
        result.Should().Be("This is Value1");
    }

    #endregion

    #region IsEnumerableType

    [Theory]
    [InlineData(null, false)]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(string), true)]
    [InlineData(typeof(List<string>), true)]
    [InlineData(typeof(IEnumerable<string>), true)]
    [InlineData(typeof(Dictionary<byte, byte>), true)]
    public void IsEnumerableType_TypeIsNullOrValid_ShouldReturnsExpected(Type input, bool expected)
    {
        // Arrange

        // Act
        var result = input.IsEnumerableType();

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region AssignUpdatedProperties

    [Fact]
    public void AssignUpdatedProperties_EntityIsNull_ShouldReturnsNull()
    {
        // Arrange
        UpdatedPropsTestEntity entity = null;
        UpdatedPropsTestDto dto = new();

        // Act
        var result = entity.AssignUpdatedProperties(dto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void AssignUpdatedProperties_DtoIsNull_ShouldReturnsNull()
    {
        // Arrange
        UpdatedPropsTestEntity entity = new();
        UpdatedPropsTestDto dto = null;

        // Act
        var result = entity.AssignUpdatedProperties(dto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void AssignUpdatedProperties_EntityAndDtoIsNull_ShouldReturnsNull()
    {
        // Arrange
        UpdatedPropsTestEntity entity = null;
        UpdatedPropsTestDto dto = null;

        // Act
        var result = entity.AssignUpdatedProperties(dto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void AssignUpdatedProperties_EntityAndDtoIsValid_ShouldUpdatesMatchingEntityPropertiesAndReturnsUpdatedPropertyInfos()
    {
        // Arrange
        UpdatedPropsTestEntity entity = new()
        {
            Id = 1,
            Name = "test",
            Price = 10M,
            Priority = 1
        };
        UpdatedPropsTestDto dto = new()
        {
            Id = 1,
            Priority = 2,
        };
        var expectedPropertyInfo = dto.GetType().GetProperty("Priority");

        // Act
        var result = entity.AssignUpdatedProperties(dto);

        // Assert
        result.Should().Contain(i => i.Name == "Priority");
        result.Should().HaveCount(1);
        entity.Priority.Should().Be(dto.Priority);
        entity.Name.Should().Be(entity.Name);
        entity.Price.Should().Be(entity.Price);
        entity.Id.Should().Be(entity.Id);
    }

    #endregion
}
