using ExpressionBuilder.Configuration;
using System.Globalization;
using System.Linq.Expressions;

namespace ExpressionBuilder.Internal;

/// <summary>
/// Converts filter values to property types and turns them into expressions.
/// </summary>
internal static class FilterValues
{
    /// <summary>
    /// Returns an expression that reads <paramref name="value"/> as <paramref name="type"/>.
    /// Non null values are read from a holder object, so EF Core sends them as query parameters instead of inlining them into SQL.
    /// </summary>
    public static Expression ToExpression(object value, Type type)
    {
        if (value is null)
            return Expression.Constant(null, type);

        var holder = Activator.CreateInstance(typeof(ValueHolder<>).MakeGenericType(type), value);

        return Expression.Field(Expression.Constant(holder), nameof(ValueHolder<object>.Value));
    }

    /// <summary>
    /// Converts <paramref name="value"/> to the underlying type of <paramref name="targetType"/> and normalizes dates.
    /// </summary>
    /// <exception cref="ArgumentException">When the value cannot be converted.</exception>
    public static object Convert(object value, Type targetType, string propertyPath)
    {
        if (value is null)
            return null;

        var type = Nullable.GetUnderlyingType(targetType) ?? targetType;

        return NormalizeDate(ChangeType(value, type, propertyPath));
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <paramref name="type"/> without touching dates.
    /// </summary>
    /// <exception cref="ArgumentException">When the value cannot be converted.</exception>
    public static object ChangeType(object value, Type type, string propertyPath)
    {
        if (type.IsInstanceOfType(value))
            return value;

        try
        {
            if (type.IsEnum)
                return value is string name ? Enum.Parse(type, name, ignoreCase: true) : Enum.ToObject(type, value);

            if (type == typeof(Guid) && value is string guid)
                return Guid.Parse(guid);

            if (type == typeof(DateTimeOffset) && value is DateTime dateTime)
                return new DateTimeOffset(dateTime);

            if (type == typeof(string))
                return System.Convert.ToString(value, CultureInfo.InvariantCulture);

            if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(type))
                return System.Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException or ArgumentException)
        {
            throw MismatchException(value, type, propertyPath, ex);
        }

        throw MismatchException(value, type, propertyPath, null);
    }

    /// <summary>
    /// Applies <see cref="Settings.UseUtcConversionInDateTypes"/> to <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values.
    /// </summary>
    public static object NormalizeDate(object value)
    {
        if (!Settings.UseUtcConversionInDateTypes)
            return value;

        return value switch
        {
            DateTime { Kind: DateTimeKind.Utc } => value,
            DateTime dateTime => dateTime.ToUniversalTime(),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToOffset(TimeSpan.Zero),
            _ => value
        };
    }

    public static bool CanBeNull(Type type) => !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

    private static ArgumentException MismatchException(object value, Type type, string propertyPath, Exception inner)
        => new($"A value of type '{value.GetType().Name}' cannot be used to filter '{propertyPath}' of type '{type.Name}'.", inner);

    /// <summary>
    /// Holds a filter value. A field read on a constant is what C# closures produce, and EF Core turns it into a query parameter.
    /// </summary>
    private sealed class ValueHolder<T>(T value)
    {
        public readonly T Value = value;
    }
}
