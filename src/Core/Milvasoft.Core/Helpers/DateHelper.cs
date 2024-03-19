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
    /// <param name="dateProperty"></param>
    /// <returns> 
    /// 
    /// Example Return when both values has value :
    /// 
    /// <code> i => i.<paramref name="dateProperty"/> <b>biggerThanOrEqual</b> <paramref name="startDate"/> <b>AND</b> i.<paramref name="dateProperty"/> <b>lessThanOrEqual</b> <paramref name="endDate"/> </code>
    /// 
    /// </returns>
    public static Expression<Func<T, bool>> CreateDateSearchExpression<T>(Expression<Func<T, DateTime?>> dateProperty, DateTime? startDate, DateTime? endDate)
    {
        Expression<Func<T, bool>> mainExpression = null;

        var entityType = typeof(T);
        var propertyName = dateProperty.GetPropertyName();

        var parameterExpression = Expression.Parameter(entityType, "i");
        Expression orderByProperty = Expression.Property(parameterExpression, propertyName);

        var prop = Expression.Lambda<Func<T, DateTime>>(Expression.Convert(Expression.Property(parameterExpression, propertyName), typeof(DateTime)), parameterExpression);

        Expression<Func<DateTime>> dateLowerExpression = () => startDate.Value;
        var greaterThanOrEqualBinaryExpression = Expression.GreaterThanOrEqual(prop.Body, dateLowerExpression.Body);
        var greaterThanOrEqualExpression = Expression.Lambda<Func<T, bool>>(Expression.Convert(greaterThanOrEqualBinaryExpression, typeof(bool)), parameterExpression);

        Expression<Func<DateTime>> dateTopExpression = () => endDate.Value;
        var lessThanOrEqualBinaryExpression = Expression.LessThanOrEqual(prop.Body, dateTopExpression.Body);

        var lessThanOrEqualExpression = Expression.Lambda<Func<T, bool>>(Expression.Convert(lessThanOrEqualBinaryExpression, typeof(bool)), parameterExpression);

        var predicate = greaterThanOrEqualExpression.Append(lessThanOrEqualExpression, ExpressionType.AndAlso);

        //If a selection has been made between two dates, it will return those between the two dates.
        if (startDate.HasValue && endDate.HasValue)
            mainExpression = mainExpression.Append(predicate, ExpressionType.AndAlso);

        // If only the endDate value exists, it returns those larger than the startDate value.
        else if (startDate.HasValue && !endDate.HasValue)
        {
            mainExpression = mainExpression.Append(greaterThanOrEqualExpression, ExpressionType.AndAlso);
        }

        //If only the startDate value exists, it returns those smaller than the endDate value.
        else if (!startDate.HasValue && endDate.HasValue)
            mainExpression = mainExpression.Append(lessThanOrEqualExpression, ExpressionType.AndAlso);

        return mainExpression;
    }

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
    /// <param name="dateProperty"></param>
    /// <returns> 
    /// 
    /// Example Return when both values has value :
    /// 
    /// <code> i => i.<paramref name="dateProperty"/> <b>biggerThanOrEqual</b> <paramref name="startDate"/> <b>AND</b> i.<paramref name="dateProperty"/> <b>lessThanOrEqual</b> <paramref name="endDate"/> </code>
    /// 
    /// </returns>
    public static Expression<Func<T, bool>> CreateDateSearchExpression<T>(Expression<Func<T, DateTime>> dateProperty, DateTime? startDate, DateTime? endDate)
    {
        var expression = Expression.Lambda<Func<T, DateTime?>>(Expression.Convert(dateProperty, typeof(Func<T, DateTime?>)));

        return CreateDateSearchExpression(expression, startDate, endDate);
    }

    /// <summary>
    /// Compares <paramref name="date"/> for whether between <paramref name="startTime"/> and <paramref name="endTime"/>. 
    /// </summary>
    /// 
    /// <remarks>
    /// This is a time comparison not a date comparison.
    /// </remarks>
    /// 
    /// <param name="date"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="convertTimesToUtc"></param>
    /// <returns></returns>
    public static bool IsBetween(this DateTime date, TimeSpan startTime, TimeSpan endTime, bool convertTimesToUtc = true)
    {
        if (convertTimesToUtc)
        {
            startTime = startTime.ConvertToUtc();
            endTime = endTime.ConvertToUtc();
        }

        DateTime startDate = new(date.Year, date.Month, date.Day);
        DateTime endDate = startDate;

        //Check whether the endTime is lesser than startTime
        if (startTime >= endTime)
        {
            //Increase the date if endTime is timespan of the Nextday 
            endDate = endDate.AddDays(1);
        }

        //Assign the startTime and endTime to the Dates
        startDate = startDate.Date + startTime;
        endDate = endDate.Date + endTime;

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
        {
            compareTime = compareTime.ConvertToUtc();
            startTime = startTime.ConvertToUtc();
            endTime = endTime.ConvertToUtc();
        }

        date = new DateTime(date.Year, date.Month, date.Day).Date + compareTime;

        return date.IsBetween(startTime, endTime);
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
    /// Compares <paramref name="date"/> for whether between <paramref name="startDate"/> and <paramref name="endDate"/>. 
    /// </summary>
    /// <param name="date"></param>
    /// <param name="time"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="convertTimeToUtc"></param>
    /// <returns></returns>
    public static bool IsBetween(this DateTime date, TimeSpan time, DateTime startDate, DateTime endDate, bool convertTimeToUtc = true)
    {
        if (convertTimeToUtc)
            time = time.ConvertToUtc();

        date = new DateTime(date.Year, date.Month, date.Day).Date + time;

        return date >= startDate && date <= endDate;
    }

    /// <summary>
    /// Converts timespan to universal time.
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static TimeSpan ConvertToUtc(this TimeSpan timeSpan) => TimeSpan.FromTicks(new DateTime(timeSpan.Ticks).ToUniversalTime().Ticks);
}
