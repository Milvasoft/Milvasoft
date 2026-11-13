namespace ExpressionBuilder.Configuration;

/// <summary>
/// Process wide settings of the expression builder.
/// </summary>
public static class Settings
{
    /// <summary>
    /// When true (the default), <see cref="DateTime"/> filter values are converted to UTC and <see cref="DateTimeOffset"/> values to offset zero.
    /// <see cref="DateTimeKind.Unspecified"/> values are treated as local time. Day boundaries of
    /// <see cref="Operations.DateEqualTo"/> are computed in local time and then converted.
    /// When false, values are used as they are given.
    /// </summary>
    /// <remarks>
    /// Keep this true for PostgreSQL <c>timestamp with time zone</c> columns, which accept only UTC values.
    /// </remarks>
    public static bool UseUtcConversionInDateTypes { get; set; } = true;
}
