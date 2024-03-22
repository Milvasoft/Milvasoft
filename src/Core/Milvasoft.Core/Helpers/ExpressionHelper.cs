using Milvasoft.Core.Utils;
using System.Linq.Expressions;

namespace Milvasoft.Core.Helpers;

/// <summary>
/// Expression extensions.
/// </summary>
public static partial class CommonHelper
{
    /// <summary>
    /// Appends <paramref name="right"/> to <paramref name="left"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="expressionAppendType"></param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> Append<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right, ExpressionType expressionAppendType)
    {
        if (right == null)
            return left;

        Expression<Func<T, bool>> result = null;

        switch (expressionAppendType)
        {
            case ExpressionType.OrElse:

                //the initial case starts off with a left expression as null. If that's the case,
                //then give the short-circuit operator something to trigger on for the right expression
                left ??= x => false;

                result = left.OrElse(right);

                break;
            case ExpressionType.AndAlso:

                left ??= x => true;

                result = left.AndAlso(right);

                break;
            default:
                break;
        }

        return result;
    }

    /// <summary>
    /// Creates a lambda expression that represents a conditional AND operation
    /// </summary>
    /// <param name="left">An expression to set the left property of the binary expression</param>
    /// <param name="right">An expression to set the right property of the binary expression</param>
    /// <returns>A binary expression that has the node type property equal to AndAlso, 
    /// and the left and right properties set to the specified values</returns>
    public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        if (left == null)
            return right;

        if (right == null)
            return left;

        var combined = Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left.Body, new ExpressionParameterReplacer(right.Parameters, left.Parameters).Visit(right.Body)), left.Parameters);

        return combined;
    }

    /// <summary>
    /// Creates a lambda expression that represents a conditional OR operation
    /// </summary>
    /// <param name="left">An expression to set the left property of the binary expression</param>
    /// <param name="right">An expression to set the right property of the binary expression</param>
    /// <returns>A binary expression that has the node type property equal to OrElse, 
    /// and the left and right properties set to the specified values</returns>
    public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        if (left == null)
            return right;

        if (right == null)
            return left;

        var combined = Expression.Lambda<Func<T, bool>>(Expression.OrElse(left.Body, new ExpressionParameterReplacer(right.Parameters, left.Parameters).Visit(right.Body)), left.Parameters);

        return combined;
    }

    /// <summary>
    /// Determines the right casting and pets property name from expression with this right casting.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TPropertyType"></typeparam>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static string GetPropertyName<T, TPropertyType>(this Expression<Func<T, TPropertyType>> expression)
    {
        if (expression == null)
            return null;

        try
        {
            if (expression.Body is MemberExpression tempExpression)
            {
                return tempExpression.Member.Name;
            }
            else
            {
                var op = ((UnaryExpression)expression.Body).Operand;

                return ((MemberExpression)op).Member.Name;
            }
        }
        catch (InvalidCastException)
        {
            return null;
        }
    }
}
