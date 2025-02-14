﻿using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Milvasoft.Core.Helpers;
using Milvasoft.Core.MultiLanguage.Manager;
using Milvasoft.DataAccess.EfCore.Utils;
using Milvasoft.UnitTests.CoreTests.HelperTests.CommonTests.Fixtures;
using System.Linq.Expressions;
using static Milvasoft.UnitTests.CoreTests.HelperTests.CommonTests.Fixtures.IsAssignableToTypeModelFixtures;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.CommonTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1125:Boolean literals should not be redundant", Justification = "Expression equality")]
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
        var input = TestEnumFixture.Value2;

        // Act
        var result = input.GetEnumDesciption();

        // Assert
        result.Should().Be(TestEnumFixture.Value2.ToString());
    }

    [Fact]
    public void GetEnumDescription_WithTypeIsEnumAndContainsDescriptionAttribute_ShouldReturnDescriptionAttributeValue()
    {
        // Arrange
        var input = TestEnumFixture.Value1;

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

    #region CanAssignableTo

    [Fact]
    public void CanAssignableTo_WithNullTypeParameters_ShouldReturnFalse()
    {
        // Arrange
        Type type = null;
        Type targetType = null;

        // Act
        var result = type.CanAssignableTo(targetType);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAssignableTo_WithNonGenericTypeAndNonInterfaceTargetType_ShouldReturnTrueIfTypeIsAssignable()
    {
        // Arrange
        Type type = typeof(string);
        Type targetType = typeof(object);

        // Act
        var result = type.CanAssignableTo(targetType);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanAssignableTo_WithNonGenericTypeAndNonInterfaceTargetType_ShouldReturnFalseIfTypeIsNotAssignable()
    {
        // Arrange
        Type type = typeof(int);
        Type targetType = typeof(string);

        // Act
        var result = type.CanAssignableTo(targetType);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAssignableTo_WithNonGenericTypeAndInterfaceTargetType_ShouldReturnTrueIfTypeImplementsTargetInterface()
    {
        // Arrange
        Type type = typeof(List<int>);
        Type targetType = typeof(IEnumerable<int>);

        // Act
        var result = type.CanAssignableTo(targetType);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanAssignableTo_WithNonGenericTypeAndInterfaceTargetType_ShouldReturnFalseIfTypeDoesNotImplementTargetInterface()
    {
        // Arrange
        Type type = typeof(List<int>);
        Type targetType = typeof(IDictionary<int, string>);

        // Act
        var result = type.CanAssignableTo(targetType);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAssignableTo_WithGenericTypeAndNonInterfaceTargetType_ShouldReturnTrueIfTypeIsAssignable()
    {
        // Arrange
        Type type = typeof(List<int>);
        Type targetType = typeof(List<>);

        // Act
        var result = type.CanAssignableTo(targetType);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanAssignableTo_WithGenericTypeAndNonInterfaceTargetType_ShouldReturnFalseIfTypeIsNotAssignable()
    {
        // Arrange
        Type type = typeof(List<int>);
        Type targetType = typeof(Dictionary<,>);

        // Act
        var result = type.CanAssignableTo(targetType);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAssignableTo_WithGenericTypeAndInterfaceTargetType_ShouldReturnTrueIfTypeImplementsTargetInterface()
    {
        // Arrange
        Type type = typeof(List<int>);
        Type targetType = typeof(IEnumerable<>);

        // Act
        var result = type.CanAssignableTo(targetType);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanAssignableTo_WithGenericTypeAndInterfaceTargetType_ShouldReturnFalseIfTypeDoesNotImplementTargetInterface()
    {
        // Arrange
        Type type = typeof(List<int>);
        Type targetType = typeof(ICollection<>);

        // Act
        var result = type.CanAssignableTo(targetType);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(typeof(ClassImplementation), typeof(GenericClassImplementation<>), false)]
    [InlineData(typeof(ClassImplementation), typeof(IInterface), false)]
    [InlineData(typeof(IInterfaceImplementsIInterface), typeof(IInterface), true)]
    [InlineData(typeof(IInterface), typeof(IInterface), true)]
    [InlineData(typeof(IInterface), typeof(ClassImplementationWithInterface), false)]
    [InlineData(typeof(IGenericInterface<>), typeof(IGenericInterface<>), true)]
    [InlineData(typeof(IGenericInterfaceImplementsInterface<>), typeof(IInterface), true)]
    [InlineData(typeof(ClassImplementationWithInterface), typeof(IInterface), true)]
    [InlineData(typeof(GenericClassImplementationWithGenericInterface<>), typeof(IGenericInterface<>), true)]
    [InlineData(typeof(GenericClassImplementationWithInterface<>), typeof(IInterface), true)]
    [InlineData(typeof(ClassImplementsClassImplementation), typeof(ClassImplementation), true)]
    [InlineData(typeof(GenericClassImplementsClassImplementation<>), typeof(ClassImplementation), true)]
    [InlineData(typeof(GenericClassImplementsGenericClassImplementation<int>), typeof(GenericClassImplementation<>), true)]
    [InlineData(typeof(ClassImplementsClassImplementationWithInterface), typeof(ClassImplementationWithInterface), true)]
    [InlineData(typeof(ClassImplementsClassImplementationWithInterface), typeof(IInterface), true)]
    [InlineData(typeof(GenericClassImplementsGenericClassImplementationWithTwoArgument<,>), typeof(GenericClassImplementation<>), true)]
    [InlineData(typeof(GenericClassImplementsGenericClassImplementationWithTwoArgument<int, string>), typeof(GenericClassImplementation<>), true)]
    [InlineData(typeof(GenericClassImplementsGenericClassImplementationWithTwoArgument<int, string>), typeof(GenericClassImplementation<int>), true)]
    [InlineData(typeof(GenericClassImplementsGenericClassImplementationWithTwoArgument<int, string>), typeof(GenericClassImplementation<string>), false)]
    public void CanAssignableTo_WithValidTypes_ShouldReturnExpected(Type type, Type targetType, bool expected)
    {
        // Arrange

        // Act
        var result = type.CanAssignableTo(targetType);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region GetGenericMethod

    [Fact]
    public void GetGenericMethod_WithClassNotContainsGenericMethod_ShouldReturnNull()
    {
        // Arrange
        var type = typeof(CommonHelperTests);

        // Act
        var result = type.GetGenericMethod(nameof(GetGenericMethod_WithClassNotContainsGenericMethod_ShouldReturnNull), 1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetGenericMethod_WithClassContainsGenericMethodButParametersMismatch_ShouldReturnNull()
    {
        // Arrange
        var type = typeof(CommonHelper);

        // Act
        var result = type.GetGenericMethod(nameof(CommonHelper.GetEnumDesciption), 2, typeof(string));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetGenericMethod_WithClassContainsGenericMethodButParameterCountMismatch_ShouldReturnNull()
    {
        // Arrange
        var type = typeof(CommonHelper);

        // Act
        var result = type.GetGenericMethod(nameof(CommonHelper.CreateIsDeletedFalseExpression), 1, typeof(string));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetGenericMethod_WithClassContainsGenericMethodButParameterTypesMismatch_ShouldReturnNull()
    {
        // Arrange
        var type = typeof(CommonHelper);

        // Act
        var result = type.GetGenericMethod(nameof(MilvaEfExtensions.FindUpdatablePropertiesAndAct), 2, typeof(string));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetGenericMethod_WithClassContainsGenericMethodAndParametersAreGenericType_ShouldReturnCorrectMethodInfo()
    {
        // Arrange
        var type = typeof(CommonHelper);

        // Act
        var result = type.GetGenericMethod(nameof(CommonHelper.GetEnumDesciption), 1);

        // Assert
        result.Name.Should().Be(nameof(CommonHelper.GetEnumDesciption));
        result.GetParameters().Should().HaveCount(1);
        result.IsGenericMethod.Should().BeTrue();
    }

    [Fact]
    public void GetGenericMethod_WithClassContainsGenericMethodAndParametersAreValid_ShouldReturnCorrectMethodInfo()
    {
        // Arrange
        var type = typeof(MultiLanguageManager);

        // Act
        var result = type.GetGenericMethod(nameof(MultiLanguageManager.GetTranslation), 1, typeof(IEnumerable<>), typeof(string));

        // Assert
        result.Name.Should().Be(nameof(MultiLanguageManager.GetTranslation));
        result.GetParameters().Should().HaveCount(2);
        result.IsGenericMethod.Should().BeTrue();
    }

    #endregion

    #region IsNonNullableValueType

    [Theory]
    [InlineData(null, false)]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(int?), false)]
    public void IsNonNullableValueType_WithTypeInput_ShouldReturnCorrectResult(Type input, bool expected)
    {
        // Arrange

        // Act
        var result = input.IsNonNullableValueType();

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
