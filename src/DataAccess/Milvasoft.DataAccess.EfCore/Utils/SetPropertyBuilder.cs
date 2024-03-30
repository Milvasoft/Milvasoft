using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.DataAccess.EfCore.Utils;

/// <summary>
/// <see cref="SetPropertyCalls{TSource}"/> expression builder for <see cref="RelationalQueryableExtensions.ExecuteUpdate"/>
/// </summary>
/// <typeparam name="TSource"></typeparam>
public class SetPropertyBuilder<TSource>
{
    /// <summary>
    /// <see cref="SetPropertyValue{TProperty}(Expression{Func{TSource, TProperty}}, TProperty)"/> method info for reflection calls.
    /// </summary>
    public static MethodInfo SetPropertyMethodInfo { get; } = Array.Find(typeof(SetPropertyBuilder<TSource>).GetMethods(), mi => mi.Name == nameof(SetPropertyValue));

    /// <summary>
    /// Gets <see cref="SetPropertyCalls{TSource}"/> expression. 
    /// </summary>
    public Expression<Func<SetPropertyCalls<TSource>, SetPropertyCalls<TSource>>> SetPropertyCalls { get; private set; } = b => b;

    /// <summary>
    /// Appends expressions to <see cref="SetPropertyCalls"/>
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="propertyExpression"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public SetPropertyBuilder<TSource> SetPropertyValue<TProperty>(Expression<Func<TSource, TProperty>> propertyExpression, TProperty value) => SetProperty(propertyExpression, _ => value);

    /// <summary>
    /// Appends expressions to <see cref="SetPropertyCalls"/>
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="propertyExpression"></param>
    /// <param name="valueExpression"></param>
    /// <returns></returns>
    public SetPropertyBuilder<TSource> SetProperty<TProperty>(Expression<Func<TSource, TProperty>> propertyExpression, Expression<Func<TSource, TProperty>> valueExpression)
    {
        SetPropertyCalls = SetPropertyCalls.Update(body: Expression.Call(instance: SetPropertyCalls.Body,
                                                                         methodName: nameof(SetPropertyCalls<TSource>.SetProperty),
                                                                         typeArguments: [typeof(TProperty)],
                                                                         propertyExpression,
                                                                         valueExpression),
                                                  parameters: SetPropertyCalls.Parameters);

        return this;
    }
}