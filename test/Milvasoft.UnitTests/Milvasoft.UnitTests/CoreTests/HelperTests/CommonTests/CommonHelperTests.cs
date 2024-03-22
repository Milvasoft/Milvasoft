using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.CommonTests.Fixtures;
using Moq;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.CommonTests;

public partial class CommonHelperTests
{
    #region CreateIsDeletedFalseExpression

    [Fact]
    public void CreateIsDeletedFalseExpression_WithEntityTypeIsNotSoftDeletable_ShouldReturnNull()
    {
        // Arrange

        // Act
        var result = CommonHelper.CreateIsDeletedFalseExpression<string>();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CreateIsDeletedFalseExpression_WithEntityTypeIsSoftDeletable_ShouldReturnIsDeletedFalseExpression()
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
    public void GetEnumDescription_WithTypeIsNotEnum_ShouldReturnNull()
    {
        // Arrange
        int input = 1;

        // Act
        var result = input.GetEnumDesciption();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetEnumDescription_WithTypeIsEnumButNotContainsDescriptionAttribute_ShouldReturnEnumValueAsString()
    {
        // Arrange
        var input = TestEnum.Value2;

        // Act
        var result = input.GetEnumDesciption();

        // Assert
        result.Should().Be(TestEnum.Value2.ToString());
    }

    [Fact]
    public void GetEnumDescription_WithTypeIsEnumAndContainsDescriptionAttribute_ShouldReturnDescriptionAttributeValue()
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
    public void IsEnumerableType_WithTypeIsNullOrValid_ShouldReturnExpected(Type input, bool expected)
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
    public void AssignUpdatedProperties_WithEntityIsNull_ShouldReturnNull()
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
    public void AssignUpdatedProperties_WithDtoIsNull_ShouldReturnNull()
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
    public void AssignUpdatedProperties_WithEntityAndDtoIsNull_ShouldReturnNull()
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
    public void AssignUpdatedProperties_WithEntityAndDtoIsValid_ShouldUpdatesUpdatablePropertiesAndReturnsUpdatedPropertyInfos()
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

    #region FindUpdatablePropertiesAndAct

    [Fact]
    public void FindUpdatablePropertiesAndAct_WithDtoIsNull_ShouldDoNothing()
    {
        // Arrange
        var mockValidatorForAction = new Mock<Action<PropertyInfo, object>>();

        // Act
        CommonHelper.FindUpdatablePropertiesAndAct<UpdatedPropsTestEntity, UpdatedPropsTestDto>(null, mockValidatorForAction.Object);

        // Assert
        mockValidatorForAction.Verify(m => m.Invoke(null, null), Times.Never());
    }

    [Fact]
    public void FindUpdatablePropertiesAndAct_WithActionIsNull_ShouldDoNothing()
    {
        // Arrange
        var mockValidator = new Mock<UpdatedPropsTestDto>();

        // Act
        CommonHelper.FindUpdatablePropertiesAndAct<UpdatedPropsTestEntity, UpdatedPropsTestDto>(mockValidator.Object, null);

        // Assert
        mockValidator.Verify(m => m.GetUpdatableProperties(), Times.Never());
    }

    [Fact]
    public void FindUpdatablePropertiesAndAct_WithDtoNotContainsAnyUpdatableProperties_ShouldDoNothing()
    {
        // Arrange
        var mockValidatorForDto = new Mock<UpdatedPropsTestInvalidDto>();
        var mockValidatorForAction = new Mock<Action<PropertyInfo, object>>();

        // Act
        CommonHelper.FindUpdatablePropertiesAndAct<UpdatedPropsTestEntity, UpdatedPropsTestInvalidDto>(mockValidatorForDto.Object, mockValidatorForAction.Object);

        // Assert
        mockValidatorForDto.Verify(m => m.GetUpdatableProperties(), Times.Once());
        mockValidatorForAction.Verify(m => m.Invoke(null, null), Times.Never());
    }

    [Fact]
    public void FindUpdatablePropertiesAndAct_WithDtoAndActionIsValidButUpdatedPropertyNotExistsInEntity_ShouldFindUpdatablePropertiesAndNotInvokesInputAction()
    {
        // Arrange
        var mockValidatorForAction = new Mock<Action<PropertyInfo, object>>();
        UpdatedPropsTestDto dto = new()
        {
            Id = 1,
            Type = 1,
        };

        // Act
        CommonHelper.FindUpdatablePropertiesAndAct<UpdatedPropsTestEntity, UpdatedPropsTestDto>(dto, mockValidatorForAction.Object);

        // Assert
        mockValidatorForAction.Verify(m => m.Invoke(null, null), Times.Never());
    }

    [Fact]
    public void FindUpdatablePropertiesAndAct_WithDtoAndActionIsValidButPropertiesNotUpdated_ShouldFindUpdatablePropertiesAndNotInvokesInputAction()
    {
        // Arrange
        var mockValidatorForDto = new Mock<UpdatedPropsTestDto>();
        var mockValidatorForAction = new Mock<Action<PropertyInfo, object>>();

        // Act
        CommonHelper.FindUpdatablePropertiesAndAct<UpdatedPropsTestEntity, UpdatedPropsTestDto>(mockValidatorForDto.Object, mockValidatorForAction.Object);

        // Assert
        mockValidatorForDto.Verify(m => m.GetUpdatableProperties(), Times.Once());
        mockValidatorForAction.Verify(m => m.Invoke(null, null), Times.Never());
    }

    [Fact]
    public void FindUpdatablePropertiesAndAct_WithDtoAndActionIsValidAndOneUpdatedPropertyExists_ShouldFindUpdatablePropertiesAndInvokesInputAction()
    {
        // Arrange
        UpdatedPropsTestDto dto = new()
        {
            Id = 1,
            Name = "test",
        };

        PropertyInfo resultMatchingEntityProp = null;
        object resultValue = null;

        // Act
        CommonHelper.FindUpdatablePropertiesAndAct<UpdatedPropsTestEntity, UpdatedPropsTestDto>(dto, (matchingEntityProp, dtoPropertyValue) =>
        {
            resultMatchingEntityProp = matchingEntityProp;
            resultValue = dtoPropertyValue;
        });

        // Assert
        resultMatchingEntityProp.Name.Should().Be("Name");
        resultValue.Should().Be("test");
    }

    #endregion
}
