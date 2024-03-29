using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.PropertyTests.Fixtures;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.PropertyTests;

public class PropertyHelperTests
{
    #region PropertyExists

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void PropertyExists_ForOverloadWithCollection_WithNullOrEmptyOrWhiteSpacePropertyNameInput_ShouldReturnFalse(string propertyName)
    {
        // Arrange
        IQueryable<PropertyExistsTestModelFixture> inputList = null;

        // Act
        var result = inputList.PropertyExists(propertyName);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("poco", true)]
    [InlineData("Poco", true)]
    [InlineData("NotExistsPropName", false)]
    public void PropertyExists_ForOverloadWithCollection_WithValidPropertyNameInput_ShouldReturnExpected(string propertyName, bool expected)
    {
        // Arrange
        IQueryable<PropertyExistsTestModelFixture> inputList = null;

        // Act
        var result = inputList.PropertyExists(propertyName);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void PropertyExists_ForOverloadWithOneGenericParameter_WithNullOrEmptyOrWhiteSpacePropertyNameInput_ShouldReturnFalse(string propertyName)
    {
        // Arrange

        // Act
        var result = CommonHelper.PropertyExists<PropertyExistsTestModelFixture>(propertyName);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("poco", true)]
    [InlineData("Poco", true)]
    [InlineData("NotExistsPropName", false)]
    public void PropertyExists_ForOverloadWithOneGenericParameter_WithValidPropertyNameInput_ShouldReturnExpected(string propertyName, bool expected)
    {
        // Arrange

        // Act
        var result = CommonHelper.PropertyExists<PropertyExistsTestModelFixture>(propertyName);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void PropertyExists_ForOverloadWithObject_WithNullOrEmptyOrWhiteSpacePropertyNameInput_ShouldReturnFalse(string propertyName)
    {
        // Arrange
        object obj = new PropertyExistsTestModelFixture();

        // Act
        var result = obj.PropertyExists(propertyName);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("poco", true)]
    [InlineData("Poco", true)]
    [InlineData("NotExistsPropName", false)]
    public void PropertyExists_ForOverloadWithObject_WithValidPropertyNameInput_ShouldReturnExpected(string propertyName, bool expected)
    {
        // Arrange
        object obj = new PropertyExistsTestModelFixture();

        // Act
        var result = obj.PropertyExists(propertyName);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void PropertyExists_ForOverloadWithType_WithNullOrEmptyOrWhiteSpacePropertyNameInput_ShouldReturnFalse(string propertyName)
    {
        // Arrange
        var type = typeof(PropertyExistsTestModelFixture);

        // Act
        var result = type.PropertyExists(propertyName);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("poco", true)]
    [InlineData("Poco", true)]
    [InlineData("NotExistsPropName", false)]
    public void PropertyExists_ForOverloadWithType_WithValidPropertyNameInput_ShouldReturnExpected(string propertyName, bool expected)
    {
        // Arrange
        var type = typeof(PropertyExistsTestModelFixture);

        // Act
        var result = type.PropertyExists(propertyName);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region ThrowIfPropertyNotExists

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NotExistsPropName")]
    public void ThrowIfPropertyNotExists_ForOverloadWithCollection_WithNullOrEmptyOrWhiteSpaceOrNotExistsPropertyNameInput_ShouldThrowException(string propertyName)
    {
        // Arrange
        IQueryable<PropertyExistsTestModelFixture> inputList = null;

        // Act
        Action act = () => inputList.ThrowIfPropertyNotExists(propertyName);

        // Assert
        act.Should().Throw<MilvaDeveloperException>();
    }

    [Theory]
    [InlineData("poco")]
    [InlineData("Poco")]
    public void ThrowIfPropertyNotExists_ForOverloadWithCollection_WithExistingPropertyNameInput_ShouldReturnCorrectPropertyInfo(string propertyName)
    {
        // Arrange
        IQueryable<PropertyExistsTestModelFixture> inputList = null;
        var expected = typeof(PropertyExistsTestModelFixture).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        // Act
        var result = inputList.ThrowIfPropertyNotExists(propertyName);

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NotExistsPropName")]
    public void ThrowIfPropertyNotExists_ForOverloadWithOneGenericParameter_WithNullOrEmptyOrWhiteSpaceOrNotExistsPropertyNameInput_ShouldThrowException(string propertyName)
    {
        // Arrange

        // Act
        Action act = () => CommonHelper.ThrowIfPropertyNotExists<PropertyExistsTestModelFixture>(propertyName);

        // Assert
        act.Should().Throw<MilvaDeveloperException>();
    }

    [Theory]
    [InlineData("poco")]
    [InlineData("Poco")]
    public void ThrowIfPropertyNotExists_ForOverloadWithOneGenericParameter_WithExistingPropertyNameInput_ShouldReturnCorrectPropertyInfo(string propertyName)
    {
        // Arrange
        var expected = typeof(PropertyExistsTestModelFixture).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        // Act
        var result = CommonHelper.ThrowIfPropertyNotExists<PropertyExistsTestModelFixture>(propertyName);

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NotExistsPropName")]
    public void ThrowIfPropertyNotExists_ForOverloadWithObject_WithNullOrEmptyOrWhiteSpaceOrNotExistsPropertyNameInput_ShouldThrowException(string propertyName)
    {
        // Arrange
        object obj = new PropertyExistsTestModelFixture();

        // Act
        Action act = () => obj.ThrowIfPropertyNotExists(propertyName);

        // Assert
        act.Should().Throw<MilvaDeveloperException>();
    }

    [Theory]
    [InlineData("poco")]
    [InlineData("Poco")]
    public void ThrowIfPropertyNotExists_ForOverloadWithObject_WithExistingPropertyNameInput_ShouldReturnCorrectPropertyInfo(string propertyName)
    {
        // Arrange
        object obj = new PropertyExistsTestModelFixture();
        var expected = typeof(PropertyExistsTestModelFixture).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        // Act
        var result = obj.ThrowIfPropertyNotExists(propertyName);

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NotExistsPropName")]
    public void ThrowIfPropertyNotExists_ForOverloadWithType_WithNullOrEmptyOrWhiteSpaceOrNotExistsPropertyNameInput_ShouldThrowException(string propertyName)
    {
        // Arrange
        var type = typeof(PropertyExistsTestModelFixture);

        // Act
        Action act = () => type.ThrowIfPropertyNotExists(propertyName);

        // Assert
        act.Should().Throw<MilvaDeveloperException>();
    }

    [Theory]
    [InlineData("poco")]
    [InlineData("Poco")]
    public void ThrowIfPropertyNotExists_ForOverloadWithType_WithExistingPropertyNameInput_ShouldReturnCorrectPropertyInfo(string propertyName)
    {
        // Arrange
        var type = typeof(PropertyExistsTestModelFixture);
        var expected = typeof(PropertyExistsTestModelFixture).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        // Act
        var result = type.ThrowIfPropertyNotExists(propertyName);

        // Assert
        result.Should().BeSameAs(expected);
    }

    #endregion

    #region GetPublicPropertyIgnoreCase

    [Fact]
    public void GetPublicPropertyIgnoreCase_WithNullPropertyNameInput_ShouldReturnNull()
    {
        // Arrange
        string propertyName = null;
        var type = typeof(PropertyExistsTestModelFixture);
        PropertyInfo expected = null;

        // Act
        var result = type.GetPublicPropertyIgnoreCase(propertyName);

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NotExistsPropName")]
    public void GetPublicPropertyIgnoreCase_WithEmptyOrWhiteSpaceOrNotExistsPropertyNameInput_ShouldReturnNull(string propertyName)
    {
        // Arrange
        var type = typeof(PropertyExistsTestModelFixture);
        var expected = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        // Act
        var result = type.GetPublicPropertyIgnoreCase(propertyName);

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Theory]
    [InlineData("poco")]
    [InlineData("Poco")]
    public void GetPublicPropertyIgnoreCase_WithExistingPropertyNameInput_ShouldReturnCorrectPropertyInfo(string propertyName)
    {
        // Arrange
        var type = typeof(PropertyExistsTestModelFixture);
        var expected = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        // Act
        var result = type.GetPublicPropertyIgnoreCase(propertyName);

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Theory]
    [InlineData("poco")]
    [InlineData("Poco")]
    public void GetPublicPropertyIgnoreCase_WithHideInheritedMemberInput_ShouldReturnCorrectPropertyInfo(string propertyName)
    {
        // Arrange
        var type = typeof(HideInheritedMemberModelFixture);
        var expected = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        ?? type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        // Act
        var result = type.GetPublicPropertyIgnoreCase(propertyName);

        // Assert
        result.Should().BeSameAs(expected);
    }

    #endregion

    #region CreatePropertySelector

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NotExistsPropName")]
    public void CreatePropertySelector_WithNullOrEmptyOrWhiteSpaceOrNotExistsPropertyNameInput_ShouldReturnNull(string propertyName)
    {
        // Arrange

        // Act
        var result = CommonHelper.CreatePropertySelector<PropertyExistsTestModelFixture, object>(propertyName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CreatePropertySelector_WithExistingPropertyName_ShouldReturnPropertySelectorExpression()
    {
        // Arrange
        var propertyName = "Poco";
        var expectedPropertyInfo = typeof(PropertyExistsTestModelFixture).GetProperty(propertyName);
        Expression<Func<PropertyExistsTestModelFixture, object>> expected = i => (object)i.Poco;

        // Act
        var result = CommonHelper.CreatePropertySelector<PropertyExistsTestModelFixture, object>(propertyName);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expected, result);
        equality.Should().BeTrue();
        result.Should().NotBeNull();
        result.Parameters.Should().HaveCount(1);
        result.Parameters[0].Should().BeAssignableTo(typeof(ParameterExpression));
        result.Body.Should().BeAssignableTo(typeof(UnaryExpression));
        result.Body.NodeType.Should().Be(ExpressionType.Convert);
        result.Body.Type.Should().Be(typeof(object));
        var unaryExpression = (UnaryExpression)result.Body;
        unaryExpression.Operand.Should().BeAssignableTo(typeof(MemberExpression));
        unaryExpression.Operand.NodeType.Should().Be(ExpressionType.MemberAccess);
        var memberExpression = (MemberExpression)unaryExpression.Operand;
        memberExpression.Member.Should().Be(expectedPropertyInfo);
    }

    #endregion

    #region CreateRequiredPropertySelector

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NotExistsPropName")]
    public void CreateRequiredPropertySelector_ForWithTwoGenericParameter_WithNullOrEmptyOrWhiteSpaceOrNotExistsPropertyNameInput_ShouldThrowException(string propertyName)
    {
        // Arrange

        // Act
        Action act = () => CommonHelper.CreateRequiredPropertySelector<PropertyExistsTestModelFixture, object>(propertyName);

        // Assert
        act.Should().Throw<MilvaDeveloperException>();
    }

    [Fact]
    public void CreateRequiredPropertySelector_ForWithTwoGenericParameter_WithExistingPropertyName_ShouldReturnPropertySelectorExpression()
    {
        // Arrange
        var propertyName = "Poco";
        var expectedPropertyInfo = typeof(PropertyExistsTestModelFixture).GetProperty(propertyName);
        Expression<Func<PropertyExistsTestModelFixture, object>> expected = i => (object)i.Poco;

        // Act
        var result = CommonHelper.CreatePropertySelector<PropertyExistsTestModelFixture, object>(propertyName);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expected, result);
        equality.Should().BeTrue();
        result.Should().NotBeNull();
        result.Parameters.Should().HaveCount(1);
        result.Parameters[0].Should().BeAssignableTo(typeof(ParameterExpression));
        result.Body.Should().BeAssignableTo(typeof(UnaryExpression));
        result.Body.NodeType.Should().Be(ExpressionType.Convert);
        result.Body.Type.Should().Be(typeof(object));
        var unaryExpression = (UnaryExpression)result.Body;
        unaryExpression.Operand.Should().BeAssignableTo(typeof(MemberExpression));
        unaryExpression.Operand.NodeType.Should().Be(ExpressionType.MemberAccess);
        var memberExpression = (MemberExpression)unaryExpression.Operand;
        memberExpression.Member.Should().Be(expectedPropertyInfo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NotExistsPropName")]
    public void CreateRequiredPropertySelector_ForWithOneGenericParameter_WithNullOrEmptyOrWhiteSpaceOrNotExistsPropertyNameInput_ShouldThrowException(string propertyName)
    {
        // Arrange

        // Act
        Action act = () => CommonHelper.CreateRequiredPropertySelector<PropertyExistsTestModelFixture>(propertyName);

        // Assert

        // Assert
        act.Should().Throw<MilvaDeveloperException>();
    }

    [Fact]
    public void CreateRequiredPropertySelector_ForWithOneGenericParameter_WithNotExistingPropertyName_ShouldCreateCorrectPropertySelectorExpression()
    {
        // Arrange
        var propertyName = "Poco";
        Expression<Func<PropertyExistsTestModelFixture, object>> expected = i => (object)i.Poco;

        // Act
        var result = CommonHelper.CreateRequiredPropertySelector<PropertyExistsTestModelFixture>(propertyName);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expected, result);
        equality.Should().BeTrue();
        result.Should().NotBeNull();
        result.Parameters.Should().HaveCount(1);
        result.Parameters[0].Should().BeAssignableTo(typeof(ParameterExpression));
        result.Body.Should().BeAssignableTo(typeof(UnaryExpression));
        result.Body.NodeType.Should().Be(ExpressionType.Convert);
        result.Body.Type.Should().Be(typeof(object));
        var unaryExpression = (UnaryExpression)result.Body;
        unaryExpression.Operand.Should().BeAssignableTo(typeof(MemberExpression));
        unaryExpression.Operand.NodeType.Should().Be(ExpressionType.MemberAccess);
        var memberExpression = (MemberExpression)unaryExpression.Operand;
        memberExpression.Member.Name.Should().Be(propertyName);
    }

    #endregion

    #region CreatePropertySelectorFunction

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NotExistsPropName")]
    public void CreatePropertySelectorFunction_WithNullOrEmptyOrWhiteSpaceOrNotExistsPropertyNameInput_ShouldReturnNull(string propertyName)
    {
        // Arrange

        // Act
        var result = CommonHelper.CreatePropertySelector<PropertyExistsTestModelFixture, object>(propertyName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CreatePropertySelectorFunction_WithExistingPropertyName_ShouldReturnPropertySelectorExpression()
    {
        // Arrange
        var propertyName = "Poco";
        byte expected = 1;

        // Act
        var result = CommonHelper.CreatePropertySelectorFunction<PropertyExistsTestModelFixture, object>(propertyName);

        // Assert
        result.Invoke(new PropertyExistsTestModelFixture() { Poco = 1 }).Should().Be(expected);
    }

    #endregion

    #region CreateRequiredPropertySelectorFunction

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("NotExistsPropName")]
    public void CreateRequiredPropertySelectorFunction_WithNullOrEmptyOrWhiteSpaceOrNotExistsPropertyNameInput_ShouldThrowException(string propertyName)
    {
        // Arrange

        // Act
        Action act = () => CommonHelper.CreateRequiredPropertySelectorFuction<PropertyExistsTestModelFixture, object>(propertyName);

        // Assert
        act.Should().Throw<MilvaDeveloperException>();
    }

    [Fact]
    public void CreateRequiredPropertySelectorFunction_WithExistingPropertyName_ShouldReturnPropertySelectorExpression()
    {
        // Arrange
        var propertyName = "Poco";
        byte expected = 1;

        // Act
        var result = CommonHelper.CreateRequiredPropertySelectorFuction<PropertyExistsTestModelFixture, object>(propertyName);

        // Assert
        result.Invoke(new PropertyExistsTestModelFixture() { Poco = 1 }).Should().Be(expected);
    }

    #endregion

    #region DynamicInvokeCreatePropertySelector

    /// <summary>
    /// date , start date , end date  , expected result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> InvalidDataForDynamicInvokeCreatePropertySelectorMethod()
    {
        yield return new object[] { null, typeof(PropertyExistsTestModelFixture), typeof(string), "poco", };
        yield return new object[] { "NotExistsMethodName", typeof(PropertyExistsTestModelFixture), typeof(string), "poco", };
        yield return new object[] { nameof(CommonHelper.CreatePropertySelector), null, typeof(string), "poco", };
        yield return new object[] { nameof(CommonHelper.CreatePropertySelector), typeof(PropertyExistsTestModelFixture), null, "poco", };
        yield return new object[] { nameof(CommonHelper.CreatePropertySelector), typeof(PropertyExistsTestModelFixture), typeof(string), null, };
        yield return new object[] { nameof(CommonHelper.CreatePropertySelector), typeof(PropertyExistsTestModelFixture), typeof(string), "", };
        yield return new object[] { nameof(CommonHelper.CreatePropertySelector), typeof(PropertyExistsTestModelFixture), typeof(string), " ", };
    }

    /// <summary>
    /// date , start date , end date  , expected result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> ValidDataForDynamicInvokeCreatePropertySelectorMethod()
    {
        yield return new object[] { nameof(CommonHelper.CreatePropertySelector), typeof(PropertyExistsTestModelFixture), typeof(byte), "poco", };
        yield return new object[] { nameof(CommonHelper.CreateRequiredPropertySelector), typeof(PropertyExistsTestModelFixture), typeof(byte), "poco", };
        yield return new object[] { nameof(CommonHelper.CreatePropertySelectorFunction), typeof(PropertyExistsTestModelFixture), typeof(byte), "poco", };
        yield return new object[] { nameof(CommonHelper.CreateRequiredPropertySelectorFuction), typeof(PropertyExistsTestModelFixture), typeof(byte), "poco", };
    }

    [Theory]
    [MemberData(nameof(InvalidDataForDynamicInvokeCreatePropertySelectorMethod))]
    public void DynamicInvokeCreatePropertySelector_WithInvalidParameters_ShouldThrowException(string methodName, Type entityType, Type propertyType, string propertyName)
    {
        // Arrange

        // Act
        Action act = () => CommonHelper.DynamicInvokeCreatePropertySelector(methodName, entityType, propertyType, propertyName);

        // Assert
        act.Should().Throw<MilvaDeveloperException>();
    }

    [Theory]
    [InlineData(nameof(CommonHelper.CreatePropertySelector))]
    [InlineData(nameof(CommonHelper.CreateRequiredPropertySelector))]
    [InlineData(nameof(CommonHelper.CreatePropertySelectorFunction))]
    [InlineData(nameof(CommonHelper.CreateRequiredPropertySelectorFuction))]
    public void DynamicInvokeCreatePropertySelector_WithValidParametersButPropertyTypeMismatch_ShouldThrowException(string methodName)
    {
        // Arrange
        var entityType = typeof(PropertyExistsTestModelFixture);
        var propertyType = typeof(string);
        var propertyName = "poco";

        // Act
        Action act = () => CommonHelper.DynamicInvokeCreatePropertySelector(methodName, entityType, propertyType, propertyName);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void DynamicInvokeCreatePropertySelector_WithValidParametersButNotExistsPropertyName_ShouldThrowException()
    {
        // Arrange
        var methodName = nameof(CommonHelper.CreatePropertySelector);
        var entityType = typeof(PropertyExistsTestModelFixture);
        var propertyType = typeof(string);
        var propertyName = "NotExistsPropName";

        // Act
        var result = CommonHelper.DynamicInvokeCreatePropertySelector(methodName, entityType, propertyType, propertyName);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(ValidDataForDynamicInvokeCreatePropertySelectorMethod))]
    public void DynamicInvokeCreatePropertySelector_WithValidParameters_ShouldThrowException(string methodName, Type entityType, Type propertyType, string propertyName)
    {
        // Arrange

        // Act
        var result = CommonHelper.DynamicInvokeCreatePropertySelector(methodName, entityType, propertyType, propertyName);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}
