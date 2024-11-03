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
    /// Checks if the given <paramref name="date"/> falls between the specified <paramref name="startTime"/> and <paramref name="endTime"/>.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <param name="convertTimesToUtc">Indicates whether to convert times to UTC before comparison. Default is true.</param>
    /// <returns>True if the date falls between the start time and end time, otherwise false.</returns>

    public static bool IsBetween(this DateTime date, TimeSpan startTime, TimeSpan endTime, bool convertTimesToUtc = true)
    {
        if (convertTimesToUtc)
        {
            startTime = startTime.ConvertToUtc();
            endTime = endTime.ConvertToUtc();
        }

        DateTime startDate = date.WithTime(startTime);
        DateTime endDate = date.WithTime(endTime);

        //Check whether the endTime is lesser than startTime
        if (startTime >= endTime)
        {
            //Increase the date if endTime is timespan of the Nextday 
            endDate = endDate.AddDays(1);
        }

        return date >= startDate && date <= endDate;
    }

    /// <summary>
    /// Checks if the given <paramref name="date"/> with the specified <paramref name="compareTime"/> falls between the <paramref name="startTime"/> and <paramref name="endTime"/>.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <param name="compareTime">The time to compare.</param>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <param name="convertTimesToUtc">Indicates whether to convert times to UTC before comparison. Default is true.</param>
    /// <returns>True if the date with the compare time falls between the start time and end time, otherwise false.</returns>
    public static bool IsBetween(this DateTime date, TimeSpan compareTime, TimeSpan startTime, TimeSpan endTime, bool convertTimesToUtc = true)
    {
        if (convertTimesToUtc)
            compareTime = compareTime.ConvertToUtc();

        return date.WithTime(compareTime).IsBetween(startTime, endTime, convertTimesToUtc);
    }

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
    /// <param name="convertTimeToUtc">Indicates whether to convert the compare time to UTC before comparison. Default is true.</param>
    /// <returns>True if the date with the compare time falls between the start date and end date, otherwise false.</returns>
    public static bool IsBetween(this DateTime date, TimeSpan compareTime, DateTime startDate, DateTime endDate, bool convertTimeToUtc = true)
    {
        if (convertTimeToUtc)
            compareTime = compareTime.ConvertToUtc();

        return date.WithTime(compareTime).IsBetween(startDate, endDate);
    }

    /// <summary>
    /// Sets the time component of the given <paramref name="date"/> to the specified <paramref name="time"/>.
    /// </summary>
    /// <param name="date">The date to modify.</param>
    /// <param name="time">The time to set.</param>
    /// <returns>A new DateTime object with the same date as the original date and the specified time.</returns>
#pragma warning disable S6562 // Always set the "DateTimeKind" when creating new "DateTime" instances
    public static DateTime WithTime(this DateTime date, TimeSpan time) => new(date.Year, date.Month, date.Day,
                                                                              time.Hours, time.Minutes, time.Seconds,
                                                                              time.Milliseconds, time.Microseconds);
#pragma warning restore S6562 // Always set the "DateTimeKind" when creating new "DateTime" instances

    /// <summary>
    /// Converts the specified <paramref name="timeSpan"/> to UTC.
    /// </summary>
    /// <param name="timeSpan">The time span to convert.</param>
    /// <returns>The converted time span in UTC.</returns> 
    /// <remarks>
    /// Be careful when using this method. Each time you use it, it will repeat the time difference process according to the time zone you are in.
    /// </remarks>
    public static TimeSpan ConvertToUtc(this TimeSpan timeSpan) => TimeSpan.FromTicks(new DateTime(timeSpan.Ticks, DateTimeKind.Unspecified).ToUniversalTime().Ticks);
}