using System.Linq.Expressions;

namespace Milvasoft.Core.Utils.ExpressionVisitors;

/// <summary>
/// Removes ToList() method from the expression tree.
/// </summary>
public class ToListRemoverVisitor : ExpressionVisitor
{
    /// <summary>
    /// Removes ToList() method from the expression tree.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Check ToList() method and remove it
        if (node.Method.Name == nameof(Enumerable.ToList) && node.Method.DeclaringType == typeof(Enumerable))
        {
            // Before ToList() method, get the method
            var source = node.Arguments[0];

            // Remove ToList() method   
            return Visit(source);
        }

        return base.VisitMethodCall(node);
    }
}