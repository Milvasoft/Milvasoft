using ExpressionBuilder.Internal;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionBuilder.Operations;

/// <summary>
/// Matches values that are in the given list. The value must be a list, such as <see cref="List{T}"/> or an array.
/// A null item in the list matches null properties.
/// </summary>
public sealed class In() : OperationBase(1)
{
    private static readonly MethodInfo _containsMethod = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                                           .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => BuildContains(this, member, value, path);

    /// <summary>
    /// Builds <c>list.Contains(member)</c>. EF Core sends the list as one parameter, which PostgreSQL reads as <c>= ANY (@list)</c>.
    /// </summary>
    internal static Expression BuildContains(OperationBase operation, MemberExpression member, object value, string path)
    {
        if (value is null or string or not IEnumerable)
            throw new ArgumentException($"'{operation.Name}' requires a list of values to filter '{path}'.");

        var listType = typeof(List<>).MakeGenericType(member.Type);
        var list = (IList)Activator.CreateInstance(listType);
        var itemsCanBeNull = FilterValues.CanBeNull(member.Type);

        foreach (var item in (IEnumerable)value)
        {
            if (item is null && !itemsCanBeNull)
                throw new ArgumentException($"The list of '{operation.Name}' cannot have null items for '{path}'.");

            list.Add(FilterValues.Convert(item, member.Type, path));
        }

        return Expression.Call(_containsMethod.MakeGenericMethod(member.Type), FilterValues.ToExpression(list, listType), member);
    }
}

/// <summary>
/// Matches values that are not in the given list. The value must be a list, such as <see cref="List{T}"/> or an array.
/// Null properties match unless the list has a null item.
/// </summary>
public sealed class NotIn() : OperationBase(1)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => Expression.Not(In.BuildContains(this, member, value, path));
}
