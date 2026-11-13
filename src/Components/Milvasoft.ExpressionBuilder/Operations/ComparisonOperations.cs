using ExpressionBuilder.Configuration;
using ExpressionBuilder.Internal;
using System.Linq.Expressions;

namespace ExpressionBuilder.Operations;

/// <summary>
/// Matches values equal to the given value. Strings are compared after trimming both sides, and null strings never match a value.
/// A null value matches null properties.
/// </summary>
public sealed class EqualTo() : OperationBase(1)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
    {
        if (member.Type == typeof(string) && value is not null)
            return OnTrimmedText(member, path, trimmed => Expression.Equal(trimmed, TextOperand(value, path)));

        return Compare(member, path, (m, v) => Expression.Equal(m, v[0]), value);
    }
}

/// <summary>
/// Matches values not equal to the given value. Null properties never match a value.
/// Strings are compared after trimming both sides. A null value matches properties that are not null.
/// </summary>
public sealed class NotEqualTo() : OperationBase(1)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
    {
        if (member.Type == typeof(string) && value is not null)
            return OnTrimmedText(member, path, trimmed => Expression.NotEqual(trimmed, TextOperand(value, path)));

        return Compare(member, path, (m, v) => Expression.NotEqual(m, v[0]), value);
    }
}

/// <summary>
/// Matches values greater than the given value.
/// </summary>
public sealed class GreaterThan() : OperationBase(1)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => Compare(member, path, (m, v) => Expression.GreaterThan(m, v[0]), value);
}

/// <summary>
/// Matches values greater than or equal to the given value.
/// </summary>
public sealed class GreaterThanOrEqualTo() : OperationBase(1)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => Compare(member, path, (m, v) => Expression.GreaterThanOrEqual(m, v[0]), value);
}

/// <summary>
/// Matches values less than the given value.
/// </summary>
public sealed class LessThan() : OperationBase(1)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => Compare(member, path, (m, v) => Expression.LessThan(m, v[0]), value);
}

/// <summary>
/// Matches values less than or equal to the given value.
/// </summary>
public sealed class LessThanOrEqualTo() : OperationBase(1)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => Compare(member, path, (m, v) => Expression.LessThanOrEqual(m, v[0]), value);
}

/// <summary>
/// Matches values between the two given values, both included.
/// </summary>
public sealed class Between() : OperationBase(2)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
        => Compare(member, path, (m, v) => Expression.AndAlso(Expression.GreaterThanOrEqual(m, v[0]), Expression.LessThanOrEqual(m, v[1])), value, value2);
}

/// <summary>
/// Matches dates on the same day as the given value, whatever their time. A null value matches null properties.
/// </summary>
/// <remarks>
/// With <see cref="Settings.UseUtcConversionInDateTypes"/> the day is taken in local time and its bounds are converted to UTC.
/// <see cref="DateTimeOffset"/> values use the day of their own offset.
/// </remarks>
public sealed class DateEqualTo() : OperationBase(1)
{
    internal override Expression Build(MemberExpression member, object value, object value2, string path)
    {
        var type = Nullable.GetUnderlyingType(member.Type) ?? member.Type;

        if (type != typeof(DateTime) && type != typeof(DateTimeOffset))
            throw new InvalidOperationException($"'{Name}' can be used only with DateTime and DateTimeOffset properties, but '{path}' is '{member.Type.Name}'.");

        if (value is null)
            return Compare(member, path, (m, v) => Expression.Equal(m, v[0]), value);

        var (start, end) = type == typeof(DateTime)
            ? DayOf((DateTime)FilterValues.ChangeType(value, type, path))
            : DayOf((DateTimeOffset)FilterValues.ChangeType(value, type, path));

        return Compare(member, path, (m, v) => Expression.AndAlso(Expression.GreaterThanOrEqual(m, v[0]), Expression.LessThan(m, v[1])), start, end);
    }

    private static (object Start, object End) DayOf(DateTime value)
    {
        if (!Settings.UseUtcConversionInDateTypes)
            return (value.Date, value.Date.AddDays(1));

        var local = value.Kind == DateTimeKind.Utc ? value.ToLocalTime() : value;
        var day = DateTime.SpecifyKind(local.Date, DateTimeKind.Local);

        return (day.ToUniversalTime(), day.AddDays(1).ToUniversalTime());
    }

    /// <summary>
    /// Uses the day of the value's own offset. <see cref="OperationBase.Compare"/> converts the bounds to offset zero when UTC conversion is on.
    /// </summary>
    private static (object Start, object End) DayOf(DateTimeOffset value)
    {
        var start = new DateTimeOffset(value.Date, value.Offset);

        return (start, start.AddDays(1));
    }
}
