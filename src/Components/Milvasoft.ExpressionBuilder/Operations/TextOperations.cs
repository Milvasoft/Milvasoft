using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionBuilder.Operations;

/// <summary>
/// Matches strings that contain the given value. Case sensitivity follows the database collation. Both sides are trimmed and null strings never match.
/// </summary>
public sealed class Contains() : OperationBase(1)
{
    internal static readonly MethodInfo ContainsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)]);

    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => OnTrimmedText(member, path, trimmed => Expression.Call(trimmed, ContainsMethod, TextOperand(value, path)));
}

/// <summary>
/// Matches strings that do not contain the given value. Both sides are trimmed and null strings never match.
/// </summary>
public sealed class DoesNotContain() : OperationBase(1)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => OnTrimmedText(member, path, trimmed => Expression.Not(Expression.Call(trimmed, Contains.ContainsMethod, TextOperand(value, path))));
}

/// <summary>
/// Matches strings that start with the given value. Both sides are trimmed and null strings never match.
/// </summary>
public sealed class StartsWith() : OperationBase(1)
{
    private static readonly MethodInfo _startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)]);

    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => OnTrimmedText(member, path, trimmed => Expression.Call(trimmed, _startsWithMethod, TextOperand(value, path)));
}

/// <summary>
/// Matches strings that end with the given value. Both sides are trimmed and null strings never match.
/// </summary>
public sealed class EndsWith() : OperationBase(1)
{
    private static readonly MethodInfo _endsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)]);

    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => OnTrimmedText(member, path, trimmed => Expression.Call(trimmed, _endsWithMethod, TextOperand(value, path)));
}

/// <summary>
/// Matches strings that are empty or white space. Null strings do not match.
/// </summary>
public sealed class IsEmpty() : OperationBase(0)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => OnTrimmedText(member, path, trimmed => Expression.Equal(trimmed, Expression.Constant(string.Empty)));
}

/// <summary>
/// Matches strings that have a character other than white space. Null strings do not match.
/// </summary>
public sealed class IsNotEmpty() : OperationBase(0)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => OnTrimmedText(member, path, trimmed => Expression.NotEqual(trimmed, Expression.Constant(string.Empty)));
}

/// <summary>
/// Matches strings that are null, empty or white space. A missing reference on the property path also matches.
/// </summary>
public sealed class IsNullOrWhiteSpace() : OperationBase(0, matchesMissingParent: true)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
    {
        EnsureText(member, path);

        return Expression.OrElse(EqualsNull(member), Expression.Equal(Trim(member), Expression.Constant(string.Empty)));
    }
}

/// <summary>
/// Matches strings that have a character other than white space.
/// </summary>
public sealed class IsNotNullNorWhiteSpace() : OperationBase(0)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => OnTrimmedText(member, path, trimmed => Expression.NotEqual(trimmed, Expression.Constant(string.Empty)));
}
