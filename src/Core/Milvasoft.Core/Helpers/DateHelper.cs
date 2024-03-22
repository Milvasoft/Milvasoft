using System.Linq.Expressions;

namespace Milvasoft.Core.Helpers;

/// <summary>
/// Helper for filtering.
/// </summary>
public static partial class CommonHelper
{
    /// <summary>
    /// <para> Creates expression by <paramref name="endDate"/> and <paramref name="startDate"/> values. </para>
    /// </summary>
    /// 
    /// <remarks>
    /// 
    /// <para><b>Remarks: </b></para>
    /// 
    /// <para> If a selection has been made between two dates, it will return those between the two dates. </para>
    /// <para> If only the <paramref name="endDate"/> value exists, it returns those larger than the <paramref name="startDate"/> value. </para>
    /// <para> If only the <paramref name="startDate"/> value exists, it returns those smaller than the <paramref name="endDate"/> value. </para>
    /// 
    /// </remarks>
    /// 
    /// <typeparam name="T"></typeparam>
    /// <param name="endDate"></param>
    /// <param name="startDate"></param>
    /// <param name="datePropertySelector"></param>
    /// <returns> 
    /// 
    /// Example Return when both values has value :
    /// 
    /// <code> i => i.<paramref name="datePropertySelector"/> <b>biggerThanOrEqual</b> <paramref name="startDate"/> <b>AND</b> i.<paramref name="datePropertySelector"/> <b>lessThanOrEqual</b> <paramref name="endDate"/> </code>
    /// 
    /// </returns>
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
    /// Checks whether the given date's time is between the specified start time and end time.
    /// 
    /// <remarks>
    /// If <paramref name="startTime"/> greater than equal to <paramref name="endTime"/> this means <paramref name="endTime"/> belongs to next day. The comparison will made according to this criteria.
    /// </remarks>
    /// 
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <param name="convertTimesToUtc">Indicates whether to convert times to UTC before comparison. Default is true.</param>
    /// <returns>True if the date's is between the start time and end time, otherwise false.</returns>
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
    /// Removes <paramref name="date"/>'s hour, second and milisecond then adds <paramref name="compareTime"/> to <paramref name="date"/> 
    /// and compares <paramref name="date"/> for whether between <paramref name="startTime"/> and <paramref name="endTime"/>. 
    /// </summary>
    /// 
    /// <remarks>
    /// This is a time comparison not a date comparison.
    /// </remarks>
    /// 
    /// <param name="date"></param>
    /// <param name="compareTime"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="convertTimesToUtc"></param>
    /// <returns></returns>
    public static bool IsBetween(this DateTime date, TimeSpan compareTime, TimeSpan startTime, TimeSpan endTime, bool convertTimesToUtc = true)
    {
        if (convertTimesToUtc)
            compareTime = compareTime.ConvertToUtc();

        return date.WithTime(compareTime).IsBetween(startTime, endTime, convertTimesToUtc);
    }

    /// <summary>
    /// Compares <paramref name="date"/> for whether between <paramref name="startDate"/> and <paramref name="endDate"/>. 
    /// </summary>
    /// <param name="date"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    public static bool IsBetween(this DateTime date, DateTime startDate, DateTime endDate) => date >= startDate && date <= endDate;

    /// <summary>
    /// Removes <paramref name="date"/>'s hour, second and milisecond then adds <paramref name="compareTime"/> to <paramref name="date"/> 
    /// and compares <paramref name="date"/> for whether between <paramref name="startDate"/> and <paramref name="endDate"/>. 
    /// </summary>
    /// <param name="date"></param>
    /// <param name="compareTime"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="convertTimeToUtc"></param>
    /// <returns></returns>
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
    /// <returns>A new DateTime object with the same date as the original <paramref name="date"/> and the specified <paramref name="time"/>.</returns>
    public static DateTime WithTime(this DateTime date, TimeSpan time) => new(date.Year,
                                                                              date.Month,
                                                                              date.Day,
                                                                              time.Hours,
                                                                              time.Minutes,
                                                                              time.Seconds,
                                                                              time.Milliseconds,
                                                                              time.Microseconds);

    /// <summary>
    /// Converts timespan to universal time.
    /// 
    /// <remarks>
    /// Be careful when using this method. Each time you use it, it will repeat the time difference process according to the time zone you are in.
    /// </remarks>
    /// 
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static TimeSpan ConvertToUtc(this TimeSpan timeSpan) => TimeSpan.FromTicks(new DateTime(timeSpan.Ticks, DateTimeKind.Unspecified).ToUniversalTime().Ticks);
}