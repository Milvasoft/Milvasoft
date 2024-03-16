using System.Linq.Expressions;

namespace Milvasoft.Core.Extensions.Helpers;

/// <summary>
/// Expression parameter replacer for expression builder.
/// </summary>
public class ExpressionParameterReplacer : ExpressionVisitor
{
    private Dictionary<ParameterExpression, ParameterExpression> ParameterReplacements { get; set; }

    /// <summary>
    /// Constructor of <see cref="ExpressionParameterReplacer"/>
    /// </summary>
    /// <param name="fromParameters"></param>
    /// <param name="toParameters"></param>
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
    /// <param name="node"> The expression to visit. </param>
    /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (ParameterReplacements.TryGetValue(node, out var replacement))
        {
            node = replacement;
        }

        return base.VisitParameter(node);
    }
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