using Milvasoft.Core.Utils;
using System.Linq.Expressions;

namespace Milvasoft.Core.Helpers;

/// <summary>
/// Expression extensions.
/// </summary>
public static partial class CommonHelper
{
    /// <summary>
    /// Appends the specified right expression to the left expression using the specified expression append type.
    /// </summary>
    /// <typeparam name="T">The type of the parameter in the expressions.</typeparam>
    /// <param name="left">The left expression.</param>
    /// <param name="right">The right expression.</param>
    /// <param name="expressionAppendType">The expression append type.</param>
    /// <returns>The combined expression.</returns>
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
    /// Creates a lambda expression that represents a conditional AND operation.
    /// </summary>
    /// <typeparam name="T">The type of the parameter in the expression.</typeparam>
    /// <param name="left">The left expression.</param>
    /// <param name="right">The right expression.</param>
    /// <returns>The combined expression.</returns>
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
    /// Creates a lambda expression that represents a conditional OR operation.
    /// </summary>
    /// <typeparam name="T">The type of the parameter in the expression.</typeparam>
    /// <param name="left">The left expression.</param>
    /// <param name="right">The right expression.</param>
    /// <returns>The combined expression.</returns>
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
    /// Gets the property name from the specified expression.
    /// </summary>
    /// <typeparam name="T">The type of the parameter in the expression.</typeparam>
    /// <typeparam name="TPropertyType">The type of the property.</typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns>The property name.</returns>
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