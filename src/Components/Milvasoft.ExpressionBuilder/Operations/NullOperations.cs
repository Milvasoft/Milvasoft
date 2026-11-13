using System.Linq.Expressions;

namespace ExpressionBuilder.Operations;

/// <summary>
/// Matches null values. A missing reference on the property path also matches. Never matches a non nullable value type.
/// </summary>
public sealed class IsNull() : OperationBase(0, matchesMissingParent: true)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => Internal.FilterValues.CanBeNull(member.Type) ? EqualsNull(member) : Expression.Constant(false);
}

/// <summary>
/// Matches values that are not null. Always matches a non nullable value type.
/// </summary>
public sealed class IsNotNull() : OperationBase(0)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => Internal.FilterValues.CanBeNull(member.Type) ? NotEqualsNull(member) : Expression.Constant(true);
}
