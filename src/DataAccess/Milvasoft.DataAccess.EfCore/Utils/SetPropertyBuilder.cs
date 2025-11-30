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
    public SetPropertyBuilder<TSource> SetPropertyValue<TProperty>(Expression<Func<TSource, TProperty>> propertyExpression, TProperty value) => SetProperty(propertyExpression, _ => value);

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

        // Extract property name for logging
        string propName = null;
        if (propertyExpression.Body is MemberExpression m)
            propName = m.Member.Name;
        else if (propertyExpression.Body is UnaryExpression u && u.Operand is MemberExpression um)
            propName = um.Member.Name;

        if (!string.IsNullOrWhiteSpace(propName))
            _setPropertyCallsLog.Add(propName);

        // Find the generic method definition on UpdateSettersBuilder<TSource>
        var methodDef = typeof(UpdateSettersBuilder<TSource>)
                        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .First(mi => mi.Name == "SetProperty" && mi.IsGenericMethodDefinition && mi.GetParameters().Length == 2);

        var genericMethod = methodDef.MakeGenericMethod(typeof(TProperty));

        // Create an action that invokes the generic SetProperty on the builder instance using reflection.
        void action(UpdateSettersBuilder<TSource> builder) => genericMethod.Invoke(builder, [propertyExpression, valueExpression]);

        // Append the new action so multiple SetProperty calls are executed in order.
        UpdateSettersBuilder += action;

        return this;
    }
}