using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Milvasoft.Core.Helpers;
using Milvasoft.UnitTests.CoreTests.HelperTests.ExpressionTests.Fixtures;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.ExpressionTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class ExpressionHelperTests
{
    #region Append

    /// <summary>
    /// left expression , right expression , expression append type , expected result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> InvalidExpressionsAndTypesForAppendMethod()
    {
        Expression<Func<int, bool>> nullLeft = null;
        Expression<Func<int, bool>> nullRight = null;
        Expression<Func<int, bool>> left = x => x > 5;
        Expression<Func<int, bool>> right = x => x < 10;
        Expression<Func<int, bool>> orElseNullLeftExpected = x => false || x < 10;
        Expression<Func<int, bool>> andAlsoNullLeftExpected = x => true && x < 10;

        yield return new object[] { nullLeft, nullRight, ExpressionType.OrElse, nullLeft };
        yield return new object[] { nullLeft, nullRight, ExpressionType.AndAlso, nullLeft };
        yield return new object[] { nullLeft, nullRight, ExpressionType.AndAssign, nullLeft };
        yield return new object[] { left, nullRight, ExpressionType.OrElse, left };
        yield return new object[] { left, nullRight, ExpressionType.AndAlso, left };
        yield return new object[] { left, nullRight, ExpressionType.AndAssign, left };
        yield return new object[] { nullLeft, right, ExpressionType.OrElse, orElseNullLeftExpected };
        yield return new object[] { nullLeft, right, ExpressionType.AndAlso, andAlsoNullLeftExpected };
        yield return new object[] { nullLeft, right, ExpressionType.AndAssign, null };
    }

    [Theory]
    [MemberData(nameof(InvalidExpressionsAndTypesForAppendMethod))]
    public void Append_WithNullLeftOrRightWithDifferentExpressionTypes_ShouldReturnExpectedExpression(Expression<Func<int, bool>> left, Expression<Func<int, bool>> right, ExpressionType expressionType, Expression<Func<int, bool>> expected)
    {
        // Arrange

        // Act
        Expression<Func<int, bool>> result = left.Append(right, expressionType);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expected, result);
        equality.Should().BeTrue();
    }

    [Fact]
    public void Append_WithOrElseExpressionType_ShouldReturnCorrectExpression()
    {
        // Arrange
        Expression<Func<int, bool>> left = x => x > 5;
        Expression<Func<int, bool>> right = x => x < 10;
        ExpressionType expressionAppendType = ExpressionType.OrElse;
        Expression<Func<int, bool>> expected = x => x > 5 || x < 10;

        // Act
        Expression<Func<int, bool>> result = left.Append(right, expressionAppendType);

        // Assert
        result.Body.NodeType.Should().Be(expressionAppendType);
        var equality = ExpressionEqualityComparer.Instance.Equals(expected, result);
        equality.Should().BeTrue();
    }

    [Fact]
    public void Append_WithAndAlsoExpressionType_ShouldReturnCorrectExpression()
    {
        // Arrange
        Expression<Func<int, bool>> left = x => x > 5;
        Expression<Func<int, bool>> right = x => x < 10;
        ExpressionType expressionAppendType = ExpressionType.AndAlso;
        Expression<Func<int, bool>> expected = x => x > 5 && x < 10;

        // Act
        Expression<Func<int, bool>> result = left.Append(right, expressionAppendType);

        // Assert
        result.Body.NodeType.Should().Be(expressionAppendType);
        var equality = ExpressionEqualityComparer.Instance.Equals(expected, result);
        equality.Should().BeTrue();
    }

    #endregion

    #region AndAlso

    /// <summary>
    /// left expression , right expression , expected result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> InvalidExpressionsAndTypesForAndAlsoMethod()
    {
        Expression<Func<int, bool>> nullLeft = null;
        Expression<Func<int, bool>> nullRight = null;
        Expression<Func<int, bool>> left = x => x > 5;
        Expression<Func<int, bool>> right = x => x < 10;

        yield return new object[] { nullLeft, nullRight, nullRight };
        yield return new object[] { left, nullRight, left };
        yield return new object[] { nullLeft, right, right };
    }

    [Theory]
    [MemberData(nameof(InvalidExpressionsAndTypesForAndAlsoMethod))]
    public void AndAlso_WithInvalidExpressions_ShouldReturnCorrectExpression(Expression<Func<int, bool>> left, Expression<Func<int, bool>> right, Expression<Func<int, bool>> expected)
    {
        // Arrange

        // Act
        Expression<Func<int, bool>> result = left.AndAlso(right);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expected, result);
        equality.Should().BeTrue();
    }

    [Fact]
    public void AndAlso_WithValidExpressions_ShouldReturnCorrectExpression()
    {
        // Arrange
        Expression<Func<int, bool>> left = x => x > 5;
        Expression<Func<int, bool>> right = x => x < 10;
        Expression<Func<int, bool>> expected = x => x > 5 && x < 10;

        // Act
        Expression<Func<int, bool>> result = left.AndAlso(right);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expected, result);
        equality.Should().BeTrue();
    }

    #endregion

    #region OrElse

    /// <summary>
    /// left expression , right expression , expected result
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> InvalidExpressionsAndTypesForOrElseMethod() => InvalidExpressionsAndTypesForAndAlsoMethod();

    [Theory]
    [MemberData(nameof(InvalidExpressionsAndTypesForOrElseMethod))]
    public void OrElse_WithInvalidExpressions_ShouldReturnCorrectExpression(Expression<Func<int, bool>> left, Expression<Func<int, bool>> right, Expression<Func<int, bool>> expected)
    {
        // Arrange

        // Act
        Expression<Func<int, bool>> result = left.OrElse(right);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expected, result);
        equality.Should().BeTrue();
    }

    [Fact]
    public void OrElse_WithValidExpressions_ShouldReturnCorrectExpression()
    {
        // Arrange
        Expression<Func<int, bool>> left = x => x > 5;
        Expression<Func<int, bool>> right = x => x < 10;
        Expression<Func<int, bool>> expected = x => x > 5 || x < 10;

        // Act
        Expression<Func<int, bool>> result = left.OrElse(right);

        // Assert
        var equality = ExpressionEqualityComparer.Instance.Equals(expected, result);
        equality.Should().BeTrue();
    }

    #endregion

    #region GetPropertyName

    [Fact]
    public void GetPropertyName_WithNullExpression_ShouldReturnNull()
    {
        // Arrange
        Expression<Func<GetPropertyNameTestModelFixture, byte>> expression = null;
        string expected = null;

        // Act
        var result = expression.GetPropertyName();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetPropertyName_WithInvalidExpressionType_ShouldReturnNull()
    {
        // Arrange
        Expression<Func<GetPropertyNameTestModelFixture, bool>> expression = p => p.Priority > 10;
        string expected = null;

        // Act
        var result = expression.GetPropertyName();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetPropertyName_WithValidExpression_ShouldReturnCorrectPropertyName()
    {
        // Arrange
        Expression<Func<GetPropertyNameTestModelFixture, byte>> expression = p => p.Priority;
        var expected = nameof(GetPropertyNameTestModelFixture.Priority);

        // Act
        var result = expression.GetPropertyName();

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
