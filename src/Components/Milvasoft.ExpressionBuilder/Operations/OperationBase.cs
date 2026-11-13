using ExpressionBuilder.Interfaces;
using ExpressionBuilder.Internal;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionBuilder.Operations;

/// <summary>
/// Base class of the built-in operations.
/// </summary>
public abstract class OperationBase : IOperation
{
    private static readonly MethodInfo _trimMethod = typeof(string).GetMethod(nameof(string.Trim), Type.EmptyTypes);

    private protected OperationBase(int numberOfValues, bool matchesMissingParent = false)
    {
        Name = GetType().Name;
        NumberOfValues = numberOfValues;
        MatchesMissingParent = matchesMissingParent;
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public int NumberOfValues { get; }

    /// <summary>
    /// True when a missing reference on the property path satisfies the operation, as it does for <see cref="IsNull"/>.
    /// </summary>
    internal bool MatchesMissingParent { get; }

    /// <summary>
    /// Builds the predicate for <paramref name="member"/>.
    /// </summary>
    /// <param name="member">The filtered property.</param>
    /// <param name="value">First value. Ignored when <see cref="NumberOfValues"/> is 0.</param>
    /// <param name="value2">Second value. Used only when <see cref="NumberOfValues"/> is 2.</param>
    /// <param name="path">Property path, for error messages.</param>
    internal abstract Expression Build(MemberExpression member, object value, object value2, string path);

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>
    /// Builds a comparison of <paramref name="member"/> with the values converted to its type.
    /// When the member is nullable and every value is given, the member is unwrapped behind a <c>HasValue</c> check,
    /// so rows with null never match. When a value is null the member is compared with null as it is.
    /// </summary>
    private protected static Expression Compare(MemberExpression member, string path, Func<Expression, Expression[], Expression> comparison, params object[] values)
    {
        var underlyingType = Nullable.GetUnderlyingType(member.Type);

        if (values.Any(v => v is null))
        {
            if (!FilterValues.CanBeNull(member.Type))
                throw new ArgumentException($"A value is required to filter '{path}'.");

            return comparison(member, [.. values.Select(v => FilterValues.ToExpression(FilterValues.Convert(v, member.Type, path), member.Type))]);
        }

        var targetType = underlyingType ?? member.Type;
        var operands = values.Select(v => FilterValues.ToExpression(FilterValues.Convert(v, targetType, path), targetType)).ToArray();

        if (underlyingType is null)
            return comparison(member, operands);

        return Expression.AndAlso(Expression.Property(member, nameof(Nullable<int>.HasValue)),
                                  comparison(Expression.Property(member, nameof(Nullable<int>.Value)), operands));
    }

    /// <summary>
    /// Builds <c>member != null &amp;&amp; body(member.Trim())</c> for string properties.
    /// </summary>
    private protected Expression OnTrimmedText(MemberExpression member, string path, Func<Expression, Expression> body)
    {
        EnsureText(member, path);

        return Expression.AndAlso(NotEqualsNull(member), body(Trim(member)));
    }

    /// <summary>
    /// Converts the value to a trimmed string. Null becomes an empty string.
    /// </summary>
    private protected static Expression TextOperand(object value, string path)
        => FilterValues.ToExpression(((string)FilterValues.Convert(value ?? string.Empty, typeof(string), path)).Trim(), typeof(string));

    private protected static Expression Trim(Expression member) => Expression.Call(member, _trimMethod);

    private protected static Expression NotEqualsNull(Expression member) => Expression.NotEqual(member, Expression.Constant(null, member.Type));

    private protected static Expression EqualsNull(Expression member) => Expression.Equal(member, Expression.Constant(null, member.Type));

    private protected void EnsureText(MemberExpression member, string path)
    {
        if (member.Type != typeof(string))
            throw new InvalidOperationException($"'{Name}' can be used only with string properties, but '{path}' is '{member.Type.Name}'.");
    }
}
