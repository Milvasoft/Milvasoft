using System.Linq.Expressions;

namespace Milvasoft.Core.Utils.ExpressionVisitors;

/// <summary>
/// Adds IsDeleted property mapping to the MemberInitExpression.
/// </summary>
public class IsDeletedMappingVisitor : ExpressionVisitor
{
    /// <summary>
    /// Adds IsDeleted property mapping to the MemberInitExpression.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
        // Visit the bindings first to process any nested MemberInitExpressions
        var bindings = node.Bindings.Select(VisitBinding).Where(i => i != null).ToList();

        var type = node.Type;

        // Check if the type implements ISoftDeletable
        if (typeof(ISoftDeletable).IsAssignableFrom(type))
        {
            // Check if IsDeleted property is already mapped
            var isDeletedAlreadyMapped = bindings
                .OfType<MemberAssignment>()
                .Any(b => b.Member.Name == nameof(ISoftDeletable.IsDeleted));

            if (!isDeletedAlreadyMapped)
            {
                // Attempt to find the source expression for IsDeleted
                var isDeletedBinding = CreateIsDeletedBinding(node);

                if (isDeletedBinding != null)
                {
                    bindings.Add(isDeletedBinding);
                }
            }
        }

        return Expression.MemberInit(node.NewExpression, bindings);
    }

    private MemberAssignment CreateIsDeletedBinding(MemberInitExpression node)
    {
        // Attempt to find a source expression from which to get the IsDeleted value
        var sourceExpression = FindSourceExpression(node);

        if (sourceExpression != null)
        {
            // Create the IsDeleted property binding
            var isDeletedProperty = node.Type.GetProperty(nameof(ISoftDeletable.IsDeleted));
            try
            {
                var isDeletedExpression = Expression.Property(sourceExpression, isDeletedProperty);

                return Expression.Bind(isDeletedProperty, isDeletedExpression);
            }
            catch (Exception)
            {
                return null;
            }
        }

        return null;
    }

    private Expression FindSourceExpression(MemberInitExpression node)
    {
        // Try to find a source expression from the member assignments
        foreach (var bindingExpression in node.Bindings.OfType<MemberAssignment>().Select(i => i.Expression))
        {
            if (bindingExpression is MemberExpression memberExpr)
            {
                // For expressions like u.SomeNavigation.Id, return u.SomeNavigation
                return memberExpr.Expression;
            }
            else if (bindingExpression is MethodCallExpression methodCallExpr)
            {
                // For method calls, we might get the source from the first argument
                return methodCallExpr.Arguments.FirstOrDefault();
            }
            else if (bindingExpression is ConditionalExpression conditionalExpr)
            {
                // For conditional expressions, check the true branch
                var source = FindSourceExpressionFromConditional(conditionalExpr);
                if (source != null)
                    return source;
            }
            else if (bindingExpression is MemberInitExpression initExpr)
            {
                // Recursively find the source expression
                var source = FindSourceExpression(initExpr);
                if (source != null)
                    return source;
            }
        }

        return null;
    }

    private Expression FindSourceExpressionFromConditional(ConditionalExpression conditionalExpr)
    {
        // Check the true branch for a MemberInitExpression
        if (conditionalExpr.IfTrue is MemberInitExpression trueInit)
        {
            return FindSourceExpression(trueInit);
        }
        // Alternatively, check the test part
        else if (conditionalExpr.Test is BinaryExpression binaryExpr && binaryExpr.Left is MemberExpression memberExpr)
        {
            return memberExpr.Expression;
        }

        return null;
    }

    private MemberBinding VisitBinding(MemberBinding node)
    {
        // Override this method to process nested bindings
        if (node is MemberAssignment assignment)
        {
            var expression = Visit(assignment.Expression);

            return Expression.Bind(assignment.Member, expression);
        }

        return null;
    }
}