using System.Linq.Expressions;

namespace Milvasoft.Core.Utils;

/// <summary>
/// Replaces the parameters in an expression for the expression builder.
/// </summary>
public class ExpressionParameterReplacer : ExpressionVisitor
{
    private Dictionary<ParameterExpression, ParameterExpression> ParameterReplacements { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionParameterReplacer"/> class.
    /// </summary>
    /// <param name="fromParameters">The original parameters.</param>
    /// <param name="toParameters">The replacement parameters.</param>
    public ExpressionParameterReplacer(IList<ParameterExpression> fromParameters, IList<ParameterExpression> toParameters)
    {
        ParameterReplacements = [];

        for (var i = 0; i != fromParameters.Count && i != toParameters.Count; i++)
        {
            ParameterReplacements.Add(fromParameters[i], toParameters[i]);
        }
    }

    /// <summary>
    /// Visits the <see cref="ParameterExpression"/>.
    /// </summary>
    /// <param name="node">The expression to visit.</param>
    /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (ParameterReplacements.TryGetValue(node, out var replacement))
        {
            node = replacement;
        }

        return base.VisitParameter(node);
    }
}