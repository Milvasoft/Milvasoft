﻿using System.Linq.Expressions;

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
        // Check if the method is Enumerable.ToList()
        if (node.Method.Name == nameof(Enumerable.ToList) && node.Method.DeclaringType == typeof(Enumerable) && node.Arguments.Count == 1)
        {
            var nodeArgument = node.Arguments[0];

            // If Where(x => x.IsDeleted == false) exists, do nothing
            if (ContainsIsDeletedFalseFilter(nodeArgument))
                return base.VisitMethodCall(node);

            // If the argument is a collection of ISoftDeletable, return the source expression directly
            if (typeof(ISoftDeletable).IsAssignableFrom(nodeArgument.Type) || (nodeArgument.Type.GetGenericArguments().Length != 0 && typeof(ISoftDeletable).IsAssignableFrom(nodeArgument.Type.GetGenericArguments()[0])))
            {
                // Visit the source expression before ToList()
                var source = Visit(node.Arguments[0]);

                // If the source type matches, return it directly
                if (source != null && node.Method.GetGenericArguments().Length == 1)
                {
                    var elementType = node.Method.GetGenericArguments()[0];

                    // Ensure type compatibility before returning
                    if (typeof(IEnumerable<>).MakeGenericType(elementType).IsAssignableFrom(source.Type))
                        return source;
                }
            }
        }

        return base.VisitMethodCall(node);
    }

    private bool ContainsIsDeletedFalseFilter(Expression expression)
    {
        if (expression is MethodCallExpression methodCall && methodCall.Method.Name == nameof(Enumerable.Where) && methodCall.Arguments.Count == 2)
        {
            var lambda = StripQuotes(methodCall.Arguments[1]) as LambdaExpression;

            if (lambda?.Body is BinaryExpression binaryExpr && binaryExpr.NodeType == ExpressionType.Equal && IsIsDeletedAccess(binaryExpr.Left) && IsBooleanConstantFalse(binaryExpr.Right))
                return true;

            if (lambda?.Body is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Not && IsIsDeletedAccess(unaryExpr.Operand))
                return true;
        }

        if (expression is MethodCallExpression callExpr)
            return callExpr.Arguments.Any(ContainsIsDeletedFalseFilter);

        return false;
    }

    private static Expression StripQuotes(Expression expr)
    {
        while (expr.NodeType == ExpressionType.Quote)
            expr = ((UnaryExpression)expr).Operand;

        return expr;
    }

    private static bool IsIsDeletedAccess(Expression expr) => expr is MemberExpression member && member.Member.Name == "IsDeleted";

    private static bool IsBooleanConstantFalse(Expression expr) => expr is ConstantExpression constant && constant.Type == typeof(bool) && constant.Value?.Equals(false) == true;
}