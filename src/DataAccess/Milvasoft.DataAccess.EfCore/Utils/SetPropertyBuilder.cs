using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace Milvasoft.DataAccess.EfCore.Utils;

/// <summary>
/// <see cref="UpdateSettersBuilder{TSource}"/> expression builder for ExecuteUpdate
/// </summary>
/// <typeparam name="TSource"></typeparam>
public class SetPropertyBuilder<TSource>
{
    internal bool AuditCallsAdded { get; set; }

    private readonly List<string> _setPropertyCallsLog = [];

    /// <summary>
    /// <see cref="SetPropertyValue{TProperty}(Expression{Func{TSource, TProperty}}, TProperty)"/> method info for reflection calls.
    /// </summary>
    public static MethodInfo SetPropertyValueMethodInfo { get; } = Array.Find(typeof(SetPropertyBuilder<TSource>).GetMethods(), mi => mi.Name == nameof(SetPropertyValue));

    /// <summary>
    /// Gets accumulated setter calls for UpdateSettersBuilder (EF Core 10+ uses actions instead of expression trees)
    /// </summary>
    public Action<UpdateSettersBuilder<TSource>> UpdateSettersBuilder { get; private set; } = b => { };

    /// <summary>
    /// Recorded property names that were added to the builder (for testing/inspection).
    /// </summary>
    public IReadOnlyList<string> SetPropertyCallsLog => _setPropertyCallsLog;

    /// <summary>
    /// Appends expressions to <see cref="UpdateSettersBuilder"/>
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="propertyExpression"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public SetPropertyBuilder<TSource> SetPropertyValue<TProperty>(Expression<Func<TSource, TProperty>> propertyExpression, TProperty value)
    {
        if (propertyExpression == null)
            return this;

        LogPropertyCall(propertyExpression);

        UpdateSettersBuilder += builder => builder.SetProperty(propertyExpression, value);

        return this;
    }

    /// <summary>
    /// Appends expressions to <see cref="UpdateSettersBuilder"/>
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="propertyExpression"></param>
    /// <param name="valueExpression"></param>
    /// <returns></returns>
    public SetPropertyBuilder<TSource> SetProperty<TProperty>(Expression<Func<TSource, TProperty>> propertyExpression, Expression<Func<TSource, TProperty>> valueExpression)
    {
        if (propertyExpression == null || valueExpression == null)
            return this;

        LogPropertyCall(propertyExpression);

        // Direct generic call (no reflection), so EF Core overload changes are caught at compile time.
        void action(UpdateSettersBuilder<TSource> builder) => builder.SetProperty(propertyExpression, valueExpression);

        // Append the new action so multiple SetProperty calls are executed in order.
        UpdateSettersBuilder += action;

        return this;
    }

    private void LogPropertyCall<TProperty>(Expression<Func<TSource, TProperty>> propertyExpression)
    {
        var propertyName = propertyExpression.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression { Operand: MemberExpression member } => member.Member.Name,
            _ => null
        };

        if (!string.IsNullOrWhiteSpace(propertyName))
            _setPropertyCallsLog.Add(propertyName);
    }
}