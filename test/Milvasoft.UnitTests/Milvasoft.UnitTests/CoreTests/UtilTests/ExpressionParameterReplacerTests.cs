using FluentAssertions;
using Milvasoft.Core.Utils;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.CoreTests.UtilTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public partial class ExpressionParameterReplacerTests
{
    [Fact]
    public void VisitParameter_WithMatchingReplacement_ShouldReplaceParameter()
    {
        // Arrange
        var fromParameter = Expression.Parameter(typeof(int), "x");
        var toParameter = Expression.Parameter(typeof(int), "y");
        var expression = Expression.Add(fromParameter, Expression.Constant(5));

        var replacer = new ExpressionParameterReplacer([fromParameter], [toParameter]);

        // Act
        var result = replacer.Visit(expression);

        // Assert
        result.Should().BeEquivalentTo(Expression.Add(toParameter, Expression.Constant(5)));
    }

    [Fact]
    public void VisitParameter_WithNoMatchingReplacement_ShouldNotReplaceParameter()
    {
        // Arrange
        var fromParameter = Expression.Parameter(typeof(int), "x");
        var toParameter = Expression.Parameter(typeof(int), "y");
        var expression = Expression.Add(toParameter, Expression.Constant(5));

        var replacer = new ExpressionParameterReplacer([fromParameter], [toParameter]);

        // Act
        var result = replacer.Visit(expression);

        // Assert
        result.Should().BeEquivalentTo(expression);
    }

    [Fact]
    public void VisitParameter_WithMultipleMatchingReplacements_ShouldReplaceParameters()
    {
        // Arrange
        var fromParameter1 = Expression.Parameter(typeof(int), "x");
        var fromParameter2 = Expression.Parameter(typeof(int), "y");
        var toParameter1 = Expression.Parameter(typeof(int), "a");
        var toParameter2 = Expression.Parameter(typeof(int), "b");
        var expression = Expression.Add(Expression.Add(fromParameter1, fromParameter2), Expression.Constant(5));

        var replacer = new ExpressionParameterReplacer([fromParameter1, fromParameter2], [toParameter1, toParameter2]);

        // Act
        var result = replacer.Visit(expression);

        // Assert
        result.Should().BeEquivalentTo(Expression.Add(Expression.Add(toParameter1, toParameter2), Expression.Constant(5)));
    }

    /// <summary>
    /// Expression parameter replacer for expression builder.
    /// </summary>
    /// <remarks>
    /// Constructor of <see cref="ParameterReplaceVisitor"/>
    /// </remarks>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public class ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        private readonly ParameterExpression _from = from, _to = to;

        /// <summary>
        /// Visits the System.Linq.Expressions.ParameterExpression.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitParameter(ParameterExpression node) => node == _from ? _to : base.VisitParameter(node);
    }
}
