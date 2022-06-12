using Milvasoft.Core.Extensions.Helpers;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.Core.Extensions;

/// <summary>
/// Expression extensions.
/// </summary>
public static class Expressions
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
        if (right != null)
        {
            Expression<Func<T, bool>> result = null;

            switch (expressionAppendType)
            {
                case ExpressionType.OrElse:

                    //the initial case starts off with a left expression as null. If that's the case,
                    //then give the short-circuit operator something to trigger on for the right expression
                    if (left == null)
                    {
                        left = model => false;
                    }

                    result = left.OrElse(right);
                    break;
                case ExpressionType.AndAlso:

                    if (left == null)
                    {
                        left = model => true;
                    }

                    result = left.AndAlso(right);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return result;
        }
        else return left;
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
        var combined = Expression.Lambda<Func<T, bool>>(Expression.OrElse(left.Body, new ExpressionParameterReplacer(right.Parameters, left.Parameters).Visit(right.Body)), left.Parameters);

        return combined;
    }

    /// <summary>
    /// Combines two expressions to one.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="selectors"></param>
    /// <returns></returns>
    public static Expression<Func<TEntity, TEntity>> Combine<TEntity>(params Expression<Func<TEntity, TEntity>>[] selectors) where TEntity : class
    {
        var param = Expression.Parameter(typeof(TEntity), "x");

        return Expression.Lambda<Func<TEntity, TEntity>>(Expression.MemberInit(Expression.New(typeof(TEntity).GetConstructor(Type.EmptyTypes)),
                                                         from selector in selectors
                                                         let replace = new ParameterReplaceVisitor(selector.Parameters[0], param)
                                                         from binding in ((MemberInitExpression)selector.Body).Bindings.OfType<MemberAssignment>()
                                                         select Expression.Bind(binding.Member, replace.VisitAndConvert(binding.Expression, "Combine"))),
                                                         param);
    }

    /// <summary>
    /// Combines two expressions to one.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="selectors"></param>
    /// <returns></returns>
    public static Expression<Func<TEntity, object>> Combine<TEntity>(params Expression<Func<TEntity, object>>[] selectors) where TEntity : class
    {
        var param = Expression.Parameter(typeof(TEntity), "x");

        return Expression.Lambda<Func<TEntity, object>>(Expression.MemberInit(Expression.New(typeof(TEntity).GetConstructor(Type.EmptyTypes)),
                                                        from selector in selectors
                                                        let replace = new ParameterReplaceVisitor(selector.Parameters[0], param)
                                                        from binding in ((MemberInitExpression)selector.Body).Bindings.OfType<MemberAssignment>()
                                                        select Expression.Bind(binding.Member, replace.VisitAndConvert(binding.Expression, "Combine"))),
                                                        param);
    }

    /// <summary>
    /// Determines the right casting and pets property from expression with this right casting.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TPropertyType"></typeparam>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static MemberInfo GetPropertyInfo<T, TPropertyType>(this Expression<Func<T, TPropertyType>> expression)
    {
        if (expression.Body is MemberExpression tempExpression)
        {
            return tempExpression.Member;
        }
        else
        {
            var op = ((UnaryExpression)expression.Body).Operand;
            return ((MemberExpression)op).Member;
        }
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

}
