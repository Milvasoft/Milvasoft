using System.Linq.Expressions;

namespace Milvasoft.Core.Helpers;

/// <summary>
/// Helper for filtering.
/// </summary>
public static partial class CommonHelper
{
    /// <summary>
    /// Gets the current date and time. 
    /// </summary>
    /// <param name="useUtc"></param>
    /// <returns></returns>
    public static DateTime GetNow(bool useUtc) => useUtc ? DateTime.UtcNow : DateTime.Now;

    /// <summary>
    /// Gets the current date and time. 
    /// </summary>
    /// <param name="useUtc"></param>
    /// <returns></returns>
    public static DateTimeOffset GetDateTimeOffsetNow(bool useUtc) => useUtc ? DateTimeOffset.UtcNow : DateTimeOffset.Now;

    /// <summary>
    /// Creates an expression based on the provided <paramref name="endDate"/> and <paramref name="startDate"/> values.
    /// </summary>
    /// <remarks>
    /// If both <paramref name="startDate"/> and <paramref name="endDate"/> values are provided, it returns an expression that checks if the date property specified by <paramref name="datePropertySelector"/> is greater than or equal to <paramref name="startDate"/> and less than or equal to <paramref name="endDate"/>.
    /// If only the <paramref name="endDate"/> value is provided, it returns an expression that checks if the date property specified by <paramref name="datePropertySelector"/> is less than or equal to <paramref name="endDate"/>.
    /// If only the <paramref name="startDate"/> value is provided, it returns an expression that checks if the date property specified by <paramref name="datePropertySelector"/> is greater than or equal to <paramref name="startDate"/>.
    /// </remarks>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="endDate">The end date value.</param>
    /// <param name="startDate">The start date value.</param>
    /// <param name="datePropertySelector">The selector expression for the date property.</param>
    /// <returns>The expression representing the date search criteria.</returns>
    public static Expression<Func<T, bool>> CreateDateSearchExpression<T>(Expression<Func<T, DateTime?>> datePropertySelector, DateTime? startDate, DateTime? endDate)
    {
        if (datePropertySelector == null || (!startDate.HasValue && !endDate.HasValue))
            return null;

        Expression<Func<T, bool>> mainExpression = null;

        var entityType = typeof(T);

        var propertyName = datePropertySelector.GetPropertyName();

        var parameterExpression = Expression.Parameter(entityType, "i");

        Expression propertyExpression = Expression.Property(parameterExpression, propertyName);

        var prop = Expression.Lambda(Expression.Convert(propertyExpression, typeof(DateTime?)), parameterExpression);

        Expression<Func<T, bool>> greaterThanOrEqualExpression = null;
        Expression<Func<T, bool>> lessThanOrEqualExpression = null;

        if (startDate.HasValue)
        {
            Expression<Func<DateTime?>> dateLowerExpression = () => startDate;
            var greaterThanOrEqualBinaryExpression = Expression.GreaterThanOrEqual(prop.Body, dateLowerExpression.Body);
            greaterThanOrEqualExpression = Expression.Lambda<Func<T, bool>>(Expression.Convert(greaterThanOrEqualBinaryExpression, typeof(bool)), parameterExpression);
        }

        if (endDate.HasValue)
        {
            Expression<Func<DateTime?>> dateTopExpression = () => endDate;
            var lessThanOrEqualBinaryExpression = Expression.LessThanOrEqual(prop.Body, dateTopExpression.Body);
            lessThanOrEqualExpression = Expression.Lambda<Func<T, bool>>(Expression.Convert(lessThanOrEqualBinaryExpression, typeof(bool)), parameterExpression);
        }

        //If a selection has been made between two dates, it will return those between the two dates.
        if (startDate.HasValue && endDate.HasValue)
        {
            var predicate = greaterThanOrEqualExpression.Append(lessThanOrEqualExpression, ExpressionType.AndAlso);

            mainExpression = mainExpression.Append(predicate, ExpressionType.AndAlso);
        }
        // If only the endDate value exists, it returns those larger than the startDate value.
        else if (startDate.HasValue && !endDate.HasValue)
        {
            mainExpression = mainExpression.Append(greaterThanOrEqualExpression, ExpressionType.AndAlso);
        }
        //If only the startDate value exists, it returns those smaller than the endDate value.
        else if (!startDate.HasValue && endDate.HasValue)
        {
            mainExpression = mainExpression.Append(lessThanOrEqualExpression, ExpressionType.AndAlso);
        }

        return mainExpression;
    }

    /// <summary>
    /// Checks if the time of day of <paramref name="date"/> falls in the daily window from <paramref name="startTime"/> to <paramref name="endTime"/> (both inclusive).
    /// </summary>
    /// <remarks>
    /// Only the time of day parts of <paramref name="startTime"/> and <paramref name="endTime"/> are used.
    /// If <paramref name="endTime"/> is less than or equal to <paramref name="startTime"/>, the window crosses midnight (e.g. 22:00 - 02:00).
    /// </remarks>
    /// <param name="date">The date to check.</param>
    /// <param name="startTime">The start time of the daily window.</param>
    /// <param name="endTime">The end time of the daily window.</param>
    /// <param name="convertTimesToUtc">
    /// If true, <paramref name="startTime"/> and <paramref name="endTime"/> are local (<see cref="TimeZoneInfo.Local"/>) wall clock times
    /// and <paramref name="date"/> is treated as UTC unless its kind is <see cref="DateTimeKind.Local"/>; the comparison is done in local time.
    /// If false, all values are compared as they are. Default is true.
    /// </param>
    /// <returns>True if the date falls between the start time and end time, otherwise false.</returns>
    public static bool IsBetween(this DateTime date, TimeSpan startTime, TimeSpan endTime, bool convertTimesToUtc = true)
    {
        var timeOfDay = convertTimesToUtc ? ToLocalTime(date).TimeOfDay : date.TimeOfDay;

        return IsTimeOfDayBetween(timeOfDay, startTime, endTime);
    }

    /// <summary>
    /// Checks if <paramref name="compareTime"/> falls in the daily window from <paramref name="startTime"/> to <paramref name="endTime"/> (both inclusive).
    /// </summary>
    /// <remarks>
    /// All times must be expressed in the same time zone; only their time of day parts are used.
    /// If <paramref name="endTime"/> is less than or equal to <paramref name="startTime"/>, the window crosses midnight (e.g. 22:00 - 02:00).
    /// </remarks>
    /// <param name="date">The date the times belong to. It does not affect the result.</param>
    /// <param name="compareTime">The time to compare.</param>
    /// <param name="startTime">The start time of the daily window.</param>
    /// <param name="endTime">The end time of the daily window.</param>
    /// <returns>True if the compare time falls between the start time and end time, otherwise false.</returns>
    public static bool IsBetween(this DateTime date, TimeSpan compareTime, TimeSpan startTime, TimeSpan endTime)
        => IsTimeOfDayBetween(compareTime, startTime, endTime);

    /// <summary>
    /// Checks if the given <paramref name="date"/> falls between the specified <paramref name="startDate"/> and <paramref name="endDate"/>.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>True if the date falls between the start date and end date, otherwise false.</returns>
    public static bool IsBetween(this DateTime date, DateTime startDate, DateTime endDate) => date >= startDate && date <= endDate;

    /// <summary>
    /// Checks if the given <paramref name="date"/> with the specified <paramref name="compareTime"/> falls between the <paramref name="startDate"/> and <paramref name="endDate"/>.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <param name="compareTime">The time to compare.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="convertTimeToUtc">
    /// If true, <paramref name="compareTime"/> is a local (<see cref="TimeZoneInfo.Local"/>) wall clock time on the day of <paramref name="date"/>
    /// and it is converted to UTC (the day may change) before comparing with <paramref name="startDate"/> and <paramref name="endDate"/>. Default is true.
    /// </param>
    /// <returns>True if the date with the compare time falls between the start date and end date, otherwise false.</returns>
    public static bool IsBetween(this DateTime date, TimeSpan compareTime, DateTime startDate, DateTime endDate, bool convertTimeToUtc = true)
    {
        var dateTime = DateTime.SpecifyKind(date.Date.Add(GetTimeOfDay(compareTime)), DateTimeKind.Unspecified);

        if (convertTimeToUtc)
            dateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo.Local);

        return dateTime.IsBetween(startDate, endDate);
    }

    /// <summary>
    /// Sets the time component of the given <paramref name="date"/> to the specified <paramref name="time"/>.
    /// </summary>
    /// <param name="date">The date to modify.</param>
    /// <param name="time">The time to set.</param>
    /// <param name="kind"></param>
    /// <returns>A new DateTime object with the same date as the original date and the specified time.</returns>
    public static DateTime WithTime(this DateTime date, TimeSpan time, DateTimeKind kind = DateTimeKind.Unspecified)
        => new(date.Year, date.Month, date.Day,
               time.Hours, time.Minutes, time.Seconds,
               time.Milliseconds, time.Microseconds, kind: kind);

    private static bool IsTimeOfDayBetween(TimeSpan time, TimeSpan startTime, TimeSpan endTime)
    {
        time = GetTimeOfDay(time);
        startTime = GetTimeOfDay(startTime);
        endTime = GetTimeOfDay(endTime);

        if (startTime < endTime)
            return time >= startTime && time <= endTime;

        // Window crosses midnight (or covers the whole day when start equals end).
        return time >= startTime || time <= endTime;
    }

    private static TimeSpan GetTimeOfDay(TimeSpan time)
    {
        var ticks = time.Ticks % TimeSpan.TicksPerDay;

        return TimeSpan.FromTicks(ticks < 0 ? ticks + TimeSpan.TicksPerDay : ticks);
    }

    private static DateTime ToLocalTime(DateTime date) => date.Kind == DateTimeKind.Local
                                                            ? date
                                                            : TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(date, DateTimeKind.Utc), TimeZoneInfo.Local);
}